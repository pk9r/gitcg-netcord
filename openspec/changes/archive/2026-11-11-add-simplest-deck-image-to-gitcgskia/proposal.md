# Proposal

## Why

Deck image rendering in the project is transitioning from SixLabors.ImageSharp (`GitcgPainter`) to SkiaSharp (`GitcgSkia`) for better cross-platform performance and consistency. While `GameBackground` and `Genshincards` deck image creators have been ported to `GitcgSkia`, the `Simplest` layout is currently missing. As a result, the `img-simplest` option in `GitcgNetCord.MainApp`'s `/deck` slash command is disabled. Completing `SimplestDeckImageCreator` in `GitcgSkia` allows users to generate the simple 30-card deck overview and fully operationalizes `GitcgSkia` for all deck image formats.

## What Changes

- Add `SimplestDeckImageOptions` to `GitcgSkia` providing configuration for grid columns (`ActionCardColumns`, default 10) and background color (`BackgroundColor`, default `#00000000`).
- Add `SimplestDeckImageCreator` to `GitcgSkia` implementing `IDeckImageCreator<SimplestDeckImageOptions>` and `IDeckImageCreationService` using SkiaSharp.
- Render 3 role cards centered horizontally at the top, and all 30 action cards in a grid below (10 columns x 3 rows by default) with proper card borders and spacing.
- Extend `ImageCacheServiceExtension` in `GitcgSkia` to load, resize, and cache bordered card icons cleanly.
- Register `SimplestDeckImageCreator` in `GitcgSkia.Extensions.ServiceCollectionExtensions`.
- Re-enable `SimplestDeckImageCreator` in `GitcgNetCord.MainApp.Models.DeckImageCreatorCollection` and link `SharingDecodeType.ImageSimplest` in `DeckSlashCommand.cs`.
- Preserve `GitcgPainter` and `RolesImageCreator` intact for existing archival and reference purposes.

## Capabilities

### New Capabilities
- `deck-image-rendering`: Defines requirements and scenarios for generating Genius Invokation TCG deck visual overviews, specifically supporting the Simplest deck image layout alongside existing Skia-based creators.

### Modified Capabilities
None.

## Impact

- `GitcgSkia`: New types `SimplestDeckImageOptions`, `SimplestDeckImageCreator`, and updated `ServiceCollectionExtensions` and `ImageCacheServiceExtension`.
- `GitcgNetCord.MainApp`: Uncomments `Simplest` property in `DeckImageCreatorCollection` and handles `SharingDecodeType.ImageSimplest` in `DeckSlashCommand.cs`.
- Non-breaking change: Restores the `img-simplest` slash command feature for Discord bot users.
