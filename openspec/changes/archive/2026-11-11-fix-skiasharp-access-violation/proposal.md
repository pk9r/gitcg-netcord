# Proposal

## Why

Rendering Genius Invokation TCG decks containing Arcane Legend action cards causes an immediate application crash with a fatal `0xC0000005` (Access Violation) error in native SkiaSharp (`SkiaApi.sk_bitmap_get_info`). This occurs because `LoadArcaneBorderAsync` prematurely disposes the resized border bitmap before returning it, causing `BorderIcon` to dereference a dangling native pointer. Additionally, multiple intermediate `SKBitmap` and `SKData` instances in the rendering pipeline are not deterministically disposed, leading to unmanaged memory leaks under load.

## What Changes

- Fix premature disposal of the resized arcane border bitmap in `GitcgSkia.Extensions.ImageCacheServiceExtension.LoadArcaneBorderAsync`.
- Ensure deterministic disposal of intermediate bitmaps and native resources across the card icon loading pipeline (`LoadIconAsync`, `LoadBorderedIconAsync`, and `CacheImageAsync`).
- Dispose preloaded role and action card bitmaps after rendering onto the canvas in `SimplestDeckImageCreator` and `GameBackgroundDeckImageCreator`.
- Add test coverage or verification ensuring decks containing Arcane Legend cards render successfully without native memory corruption.

## Capabilities

### New Capabilities

*(None)*

### Modified Capabilities

- `deck-image-rendering`: Update requirements to ensure robust rendering of all card types, including Arcane Legend action cards, without native memory corruption or process crashes.

## Impact

- **Affected Code**: `src/GitcgSkia/Extensions/ImageCacheServiceExtension.cs`, `src/GitcgSkia/ImageCacheService.cs`, `src/GitcgSkia/ImageCreators/Deck/Simplest/SimplestDeckImageCreator.cs`, `src/GitcgSkia/ImageCreators/Deck/GameBackground/GameBackgroundDeckImageCreator.cs`.
- **APIs/Dependencies**: Internal SkiaSharp lifecycle management; public API signatures remain unchanged and backward compatible.
- **Stability**: Prevents fatal Access Violation crashes and stabilizes memory usage during high-volume deck image generation in the Discord bot.
