

# Table of Contents

1. [🤖 Bots](#1--bots)
1. [⚙️ Core Libraries & APIs](#2-️-core-libraries--apis)
2. [📄 Documentation](#3--documentation)
3. [🩹 Support](#4--support)
4. [📜 License](#5--license)
5. [🛠️ Development](#6-%EF%B8%8F-development)

## 1. 🤖 Bots

- [Fischl TCG Bot](https://discord.com/oauth2/authorize?client_id=1205143575155707934): Host by [@pk9r](https://github.com/pk9r)

## 2. ⚙️ Core Libraries & APIs

- [NetCord](https://github.com/NetCordDev/NetCord): For Discord interaction.
- [SkiaSharp](https://github.com/mono/SkiaSharp): For image processing.

## 3. 📄 Documentation

- [Github Wiki](https://github.com/pk9r/gitcg-netcord/wiki): Comprehensive documentation for the project.

## 4. 🩹 Support

- [Discord Server](https://discord.gg/7ME6bwkQSs): Join for support and discussions.

## 5. 📜 License

This repository is released under the [MIT License](LICENSE.md).

## 6. 🛠️ Development

### AI Agent Environment & Skills Optimization

If developing with Antigravity / Gemini AI coding agents, maintain the optimized skill profile to avoid context budget limits dropping core Aspire and C# capabilities.

```powershell
# Check current plugin and skill budget status
pwsh scripts/optimize-agent-skills.ps1 -Status

# Apply Strategy B: Zero-Truncation (disables 12 unused/auxiliary plugins, keeps 6 core plugins)
pwsh scripts/optimize-agent-skills.ps1 -Apply

# Enable Blazor on-demand when developing web dashboard components
pwsh scripts/optimize-agent-skills.ps1 -EnableBlazor

# Disable Blazor to reclaim context for pure bot backend work
pwsh scripts/optimize-agent-skills.ps1 -DisableBlazor

# Restore all plugins when switching projects
pwsh scripts/optimize-agent-skills.ps1 -Restore
```

### Versioning

Follows a `MAJOR.MINOR.PATCH` versioning scheme:

- MAJOR versions can introduce significant, breaking API changes.
- MINOR versions can add new features and may include limited breaking changes that are unlikely to affect most users.
- PATCH versions contain backwards-compatible bug fixes and improvements.
