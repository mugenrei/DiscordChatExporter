# DiscordChatExporter

[![Status](https://img.shields.io/badge/status-maintenance-ffd700.svg)](https://github.com/Tyrrrz/.github/blob/master/docs/project-status.md)
[![Build](https://img.shields.io/github/actions/workflow/status/Tyrrrz/DiscordChatExporter/main.yml?branch=master)](https://github.com/Tyrrrz/DiscordChatExporter/actions)
[![Coverage](https://img.shields.io/codecov/c/github/Tyrrrz/DiscordChatExporter/master)](https://codecov.io/gh/Tyrrrz/DiscordChatExporter)
[![Release](https://img.shields.io/github/release/Tyrrrz/DiscordChatExporter.svg)](https://github.com/Tyrrrz/DiscordChatExporter/releases)
[![Downloads](https://img.shields.io/github/downloads/Tyrrrz/DiscordChatExporter/total.svg)](https://github.com/Tyrrrz/DiscordChatExporter/releases)
[![Pulls](https://img.shields.io/docker/pulls/tyrrrz/discordchatexporter)](https://hub.docker.com/r/tyrrrz/discordchatexporter)
[![Discord](https://img.shields.io/discord/869237470565392384?label=discord)](https://discord.gg/2SUWKFnHSm)

<table>
    <tr>
        <td width="99999" align="center">Development of this project is entirely funded by the community. <b><a href="https://tyrrrz.me/donate">Consider donating to support!</a></b></td>
    </tr>
</table>

<p align="center">
    <img src="favicon.png" alt="Icon" />
</p>

**DiscordChatExporter** is an application that can be used to export message history from any [Discord](https://discord.com) channel to a file.
It works with direct messages, group messages, and server channels, and supports Discord's dialect of markdown as well as most other rich media features.

> ❔ If you have questions or issues, **please refer to the [docs](.docs)**.

> 💬 If you want to chat, **join my [Discord server](https://discord.gg/2SUWKFnHSm)**.

## Download

- **Graphical user interface** (desktop app):
  - 🟢 **[Stable release](https://github.com/Tyrrrz/DiscordChatExporter/releases/latest)**: look for `DiscordChatExporter.*.zip`
  - 🟠 [CI build](https://github.com/Tyrrrz/DiscordChatExporter/actions/workflows/main.yml): look for `DiscordChatExporter.*.zip`
- **Command-line interface** (terminal app):
  - 🟢 **[Stable release](https://github.com/Tyrrrz/DiscordChatExporter/releases/latest)**: look for `DiscordChatExporter.Cli.*.zip`
  - 🟠 [CI build](https://github.com/Tyrrrz/DiscordChatExporter/actions/workflows/main.yml): look for `DiscordChatExporter.Cli.*.zip`
  - 🐋 [Docker](https://hub.docker.com/r/tyrrrz/discordchatexporter): `docker pull tyrrrz/discordchatexporter`
  - 📦 [AUR](https://aur.archlinux.org/packages/discord-chat-exporter-cli): `discord-chat-exporter-cli`
  - 📦 [Nix](https://search.nixos.org/packages?query=discordchatexporter-cli): `discordchatexporter-cli`

> **Note**:
> If you're unsure which build is right for your system, consult with [this page](https://useragent.cc) to determine your OS and CPU architecture.

> **Note**:
> AUR and Nix packages linked above are maintained by the community.
> If you have any issues with them, please contact the corresponding maintainers.

# Command-Line Interface (CLI)

DiscordChatExporter CLI is a terminal application for exporting Discord chat logs.

## Features

- Cross-platform graphical and command-line interfaces
- Authentication via either a user or a bot token
- Multiple output formats: HTML (dark/light), TXT, CSV, JSON
- Support for markdown, attachments, embeds, emoji, and other rich media features
- File partitioning, date ranges, message filtering, and other export options
- Self-contained exports that can be viewed offline

## Commands

| Command    | Description                                   |
|------------|-----------------------------------------------|
| `export`   | Export channel(s)                             |
| `exportall`| Export all accessible channels from a server  |
| `exportdm` | Export all direct message channels            |
| `exportguild` | Export all channels from a server          |
| `channels` | Get a list of accessible channels             |
| `dm`       | Get a list of direct message channels         |
| `guilds`   | Get a list of accessible servers              |

## Global options

| Option      | Description                                 |
|-------------|---------------------------------------------|
| `--help`    | Show command line help                      |
| `--token`   | Discord authentication token                |
| `--bot`     | Specify whether the token belongs to a bot  |

## Export options

| Option           | Description                                                                     |
|------------------|---------------------------------------------------------------------------------|
| `--output`, `-o` | Output file or directory path                                                  |
| `--format`, `-f` | Export format: `PlainText`, `HtmlDark`, `HtmlLight`, `Json`, `Csv`             |
| `--after`        | Only include messages sent after this date or message ID                        |
| `--before`       | Only include messages sent before this date or message ID                       |
| `--partition`    | Split output into partitions, each limited to the specified message count       |
| `--filter`       | Only include messages that satisfy the filter                                   |
| `--parallel`     | Limits how many channels can be exported in parallel                           |
| `--markdown`     | Process markdown, mentions, and other special tokens                            |

## Media download options

| Option                | Description                                                                   |
|----------------------|-------------------------------------------------------------------------------|
| `--media`            | Download assets referenced by the export (avatars, attachments, images, etc.) |
| `--reuse-media`      | Reuse previously downloaded assets to avoid redundant requests                |
| `--media-dir`        | Download assets to this directory                                             |
| `--no-emoji`         | Skip downloading emoji images when using `--media`                            |
| `--no-user-avatar`   | Skip downloading user avatars when using `--media`                            |
| `--no-sticker`       | Skip downloading stickers when using `--media`                                |
| `--no-external`      | Skip downloading all external URLs (like tenor gifs) when using `--media`     |
| `--filter-external`  | Skip downloading external URLs containing specific patterns when using `--media`.<br>Can be specified multiple times, e.g., `--filter-external tenor --filter-external giphy` |

## Formatting options

| Option           | Description                                                          |
|------------------|----------------------------------------------------------------------|
| `--locale`       | Locale to use when formatting dates and numbers                      |
| `--utc`          | Normalize all timestamps to UTC+0                                    |

## Examples

### Export a channel with default settings

```shell
DiscordChatExporter.Cli export --token "your-token" --channel 123456789123456789
```

## Screenshots

![channel list](.assets/list.png)
![rendered output](.assets/output.png)

## See also

- [**Chat Analytics**](https://github.com/mlomb/chat-analytics) — solution for analyzing chat patterns of Discord users, using exports produced by **DiscordChatExporter**.
- [**DiscordChatExporter-frontend**](https://github.com/slatinsky/DiscordChatExporter-frontend) — convenient viewer for exports produced by **DiscordChatExporter**.

## Note on Political Neutrality

In our recent update, we've made the decision to remove political messaging from the application. This change aligns with the principles of the MIT license, which is designed to provide software with minimal restrictions, allowing it to be used by anyone regardless of their background, beliefs, or geographical location.

### Why this change?

1. **Software Accessibility**: We believe software tools should be accessible to all users without imposing political viewpoints. Technical tools should focus on their core functionality.

2. **MIT License Principles**: The MIT license is about freedom to use, modify, and share software without discrimination. Political statements within the software could potentially contradict the spirit of this license by creating an unwelcoming environment for some users.

3. **Focus on Functionality**: DiscordChatExporter is a utility tool with a specific technical purpose. By maintaining political neutrality, we ensure that the focus remains on the tool's functionality rather than peripheral issues.

This change doesn't represent a stance on any particular political issue, but rather a commitment to creating software that serves its technical purpose while respecting the diverse backgrounds and views of all users.