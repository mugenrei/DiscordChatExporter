using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Utils;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Core.Exporting;

internal class ExportContext(DiscordClient discord, ExportRequest request)
{
    private readonly Dictionary<Snowflake, Member?> _membersById = new();
    private readonly Dictionary<Snowflake, Channel> _channelsById = new();
    private readonly Dictionary<Snowflake, Role> _rolesById = new();

    private readonly ExportAssetDownloader _assetDownloader = new(
        request.AssetsDirPath,
        request.ShouldReuseAssets
    );

    public DiscordClient Discord { get; } = discord;

    public ExportRequest Request { get; } = request;

    public DateTimeOffset NormalizeDate(DateTimeOffset instant) =>
        Request.IsUtcNormalizationEnabled ? instant.ToUniversalTime() : instant.ToLocalTime();

    public string FormatDate(DateTimeOffset instant, string format = "g") =>
        NormalizeDate(instant).ToString(format, Request.CultureInfo);

    public async ValueTask PopulateChannelsAndRolesAsync(
        CancellationToken cancellationToken = default
    )
    {
        await foreach (
            var channel in Discord.GetGuildChannelsAsync(Request.Guild.Id, cancellationToken)
        )
        {
            _channelsById[channel.Id] = channel;
        }

        await foreach (var role in Discord.GetGuildRolesAsync(Request.Guild.Id, cancellationToken))
        {
            _rolesById[role.Id] = role;
        }
    }

    // Because members cannot be pulled in bulk, we need to populate them on demand
    private async ValueTask PopulateMemberAsync(
        Snowflake id,
        User? fallbackUser,
        CancellationToken cancellationToken = default
    )
    {
        if (_membersById.ContainsKey(id))
            return;

        var member = await Discord.TryGetGuildMemberAsync(Request.Guild.Id, id, cancellationToken);

        // User may have left the guild since they were mentioned.
        // Create a dummy member object based on the user info.
        if (member is null)
        {
            var user = fallbackUser ?? await Discord.TryGetUserAsync(id, cancellationToken);

            // User may have been deleted since they were mentioned
            if (user is not null)
                member = Member.CreateFallback(user);
        }

        // Store the result even if it's null, to avoid re-fetching non-existing members
        _membersById[id] = member;
    }

    public async ValueTask PopulateMemberAsync(
        Snowflake id,
        CancellationToken cancellationToken = default
    ) => await PopulateMemberAsync(id, null, cancellationToken);

    public async ValueTask PopulateMemberAsync(
        User user,
        CancellationToken cancellationToken = default
    ) => await PopulateMemberAsync(user.Id, user, cancellationToken);

    public Member? TryGetMember(Snowflake id) => _membersById.GetValueOrDefault(id);

    public Channel? TryGetChannel(Snowflake id) => _channelsById.GetValueOrDefault(id);

    public Role? TryGetRole(Snowflake id) => _rolesById.GetValueOrDefault(id);

    public IReadOnlyList<Role> GetUserRoles(Snowflake id) =>
        TryGetMember(id)
            ?.RoleIds.Select(TryGetRole)
            .WhereNotNull()
            .OrderByDescending(r => r.Position)
            .ToArray() ?? [];

    public Color? TryGetUserColor(Snowflake id) =>
        GetUserRoles(id).Where(r => r.Color is not null).Select(r => r.Color).FirstOrDefault();

    public async ValueTask<string> ResolveAssetUrlAsync(
        string url,
        CancellationToken cancellationToken = default
    )
    {
        if (!Request.ShouldDownloadAssets)
            return url;

        // Skip emoji assets if the flag is set
        if (Request.ShouldSkipEmoji && IsEmojiUrl(url))
            return url;

        // Skip user avatar assets if the flag is set
        if (Request.ShouldSkipUserAvatars && IsUserAvatarUrl(url))
            return url;

        // Skip sticker assets if the flag is set
        if (Request.ShouldSkipStickers && IsStickerUrl(url))
            return url;

        // Skip all external assets if the flag is set
        if (Request.ShouldSkipExternal && IsExternalUrl(url))
            return url;

        // Skip external assets that match the filters
        if (Request.ExternalFilters.Count > 0 && MatchesExternalFilter(url))
            return url;

        try
        {
            var filePath = await _assetDownloader.DownloadAsync(url, cancellationToken);
            var relativeFilePath = Path.GetRelativePath(Request.OutputDirPath, filePath);

            // Prefer the relative path so that the export package can be copied around without breaking references.
            // However, if the assets directory lies outside the export directory, use the absolute path instead.
            var shouldUseAbsoluteFilePath =
                relativeFilePath.StartsWith(
                    ".." + Path.DirectorySeparatorChar,
                    StringComparison.Ordinal
                )
                || relativeFilePath.StartsWith(
                    ".." + Path.AltDirectorySeparatorChar,
                    StringComparison.Ordinal
                );

            var optimalFilePath = shouldUseAbsoluteFilePath ? filePath : relativeFilePath;

            // For HTML, the path needs to be properly formatted
            if (Request.Format is ExportFormat.HtmlDark or ExportFormat.HtmlLight)
                return Url.EncodeFilePath(optimalFilePath);

            return optimalFilePath;
        }
        // Try to catch only exceptions related to failed HTTP requests
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/332
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/372
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException)
        {
            // We don't want this to crash the exporting process in case of failure.
            // TODO: add logging so we can be more liberal with catching exceptions.
            return url;
        }
    }

    // Checks if the URL is for an emoji image
    private static bool IsEmojiUrl(string url)
    {
        return url.Contains("/emojis/")
            || url.Contains("emoji.gg")
            || url.Contains("emoji-cdn")
            || (url.EndsWith(".svg") && url.Contains("emoji"))
            || url.Contains("twemoji")
            || (url.Contains("discord") && url.Contains("emoji"));
    }

    // Checks if the URL is for a user avatar
    private static bool IsUserAvatarUrl(string url)
    {
        return url.Contains("/avatars/") || (url.Contains("/users/") && url.Contains("avatar"));
    }

    // Checks if the URL is for a sticker image
    private static bool IsStickerUrl(string url)
    {
        return url.Contains("/stickers/");
    }

    // Checks if the URL is from external sources
    private static bool IsExternalUrl(string url)
    {
        return url.Contains("/external/");
    }

    // Checks if an external URL matches any of the filters
    private bool MatchesExternalFilter(string url)
    {
        if (!IsExternalUrl(url))
            return false;

        foreach (var filter in Request.ExternalFilters)
        {
            if (url.Contains(filter, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
