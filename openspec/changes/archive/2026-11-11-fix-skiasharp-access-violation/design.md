# Design

## Context

`GitcgSkia` utilizes SkiaSharp for cross-platform 2D image composition of Genius Invokation TCG decks. SkiaSharp objects (`SKBitmap`, `SKData`, `SKCanvas`) wrap unmanaged C++ pointers that must be deterministically disposed. In `ImageCacheServiceExtension`, `LoadBorderedIconAsync` composites card icons with corresponding card frames (Standard or Arcane Legend). Currently, `LoadArcaneBorderAsync` disposes the resized border bitmap before returning it, resulting in a fatal `0xC0000005` (Access Violation) when native methods access `border.Info`.

## Goals / Non-Goals

**Goals:**
- Eliminate the Access Violation crash when rendering decks containing Arcane Legend cards.
- Establish clean ownership and deterministic disposal for all intermediate `SKBitmap` and `SKData` instances created during rendering.
- Ensure all 33 card bitmaps preloaded in deck creators (`SimplestDeckImageCreator` and `GameBackgroundDeckImageCreator`) are freed after drawing.
- Add null-safety checks to prevent unexpected native null pointer dereferences.

**Non-Goals:**
- Altering visual deck layout, dimensions, or styling.
- Replacing `HybridCache` or redesigning public interfaces of `ImageCacheService`.

## Decisions

### 1. Transfer ownership of resized arcane border
- **Choice**: Remove `using var` on `arcaneBorderResized` inside `LoadArcaneBorderAsync`. The caller (`LoadBorderedIconAsync`) already manages the returned border with `using (border)`.
- **Alternatives considered**:
  - *Copying/Cloning*: Using `arcaneBorderResized.Copy()` and disposing the original. Rejected as redundant allocation.
  - *Static singleton border*: Preloading and retaining a static `SKBitmap` for borders in memory. While efficient, `using (border)` in callers would dispose a shared singleton, causing use-after-free for subsequent calls unless caller code is extensively refactored. Since `LoadCachedImageAsync` decodes a new `SKBitmap` per call, instance-per-call ownership is safest.

### 2. Dispose intermediate bitmaps and native buffers
- **Choice**:
  - In `LoadBorderedIconAsync(card)`: Wrap `var icon = await imageCacheService.LoadIconAsync(card)` in `using (icon)` so the unscaled source icon is freed after `BorderIcon` finishes.
  - In `LoadBorderedIconAsync(card, size)`: Wrap `cachedBorderedIcon` in `using (cachedBorderedIcon)` once resized into `borderedResizedIcon`.
  - In `ImageCacheService.CacheImageAsync`: Wrap `var data = image.Encode(...)` with `using var data` to immediately free the native `SKData` byte buffer.
  - In `SimplestDeckImageCreator` and `GameBackgroundDeckImageCreator`: Dispose all bitmaps in `roleBitmaps` and `actionBitmaps` once canvas drawing is completed.
- **Alternatives considered**:
  - Relying on GC finalizers: Rejected because native Skia memory is allocated outside the CLR heap, leading to high process memory and potential OOM before GC triggers.

### 3. Null safety in `BorderIcon`
- **Choice**: Validate that both `icon` and `border` are non-null and valid before creating `SKBitmap(border.Info)`. Check that `icon.Resize(...)` returns a valid bitmap.
- **Alternatives considered**: Letting it throw standard .NET exceptions. Added guard throws an explicit `InvalidOperationException` with clear context if asset decoding fails.

## Risks / Trade-offs

- **[Risk] Double dispose if an instance is shared** → *Mitigation*: `ImageCacheService` stores byte arrays in `HybridCache` and creates new `SKBitmap` instances via `SKBitmap.Decode()`. Each task owns its separate bitmap instance, preventing cross-task double-free.
- **[Risk] Exception thrown mid-rendering skips disposal in creators** → *Mitigation*: Use `try ... finally` blocks around the drawing operations in `SimplestDeckImageCreator` and `GameBackgroundDeckImageCreator` to guarantee that all preloaded card bitmaps are disposed even if an exception occurs.
