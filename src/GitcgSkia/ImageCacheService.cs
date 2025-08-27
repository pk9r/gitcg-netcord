using Microsoft.Extensions.Caching.Hybrid;
using SkiaSharp;

namespace GitcgSkia;

public class ImageCacheService(
    HttpClient httpClient,
    HybridCache cache
)
{
    private readonly HybridCacheEntryOptions
        _hybridCacheEntryOptions = new()
        {
            Expiration = TimeSpan.FromDays(1) //
        };

    public async ValueTask<SKBitmap> LoadHttpImageAsync(string uri)
    {
        var cachedBytes = await cache
            .GetOrCreateAsync(
                key: uri,
                factory: FetchImageAsync,
                options: _hybridCacheEntryOptions
            );

        var cachedImage = SKBitmap.Decode(cachedBytes);
        return cachedImage;

        async ValueTask<byte[]> FetchImageAsync(
            CancellationToken cancel
        )
        {
            var buffer = await httpClient
                .GetByteArrayAsync(
                    requestUri: uri,
                    cancellationToken: cancel
                );

            return buffer;
        }
    }

    public async ValueTask<SKBitmap?> LoadCachedImageAsync(string key)
    {
        var emptyBytes = Array.Empty<byte>();

        // var emptyBytes = ReadOnlySpan<byte>.Empty; // Not supported

        var cachedBytes = await cache.GetOrCreateAsync(
            key: key,
            factory: _ => ValueTask.FromResult(emptyBytes)
        );

        if (cachedBytes.Length == 0)
            return null;

        var cachedImage = SKBitmap.Decode(cachedBytes);
        return cachedImage;
    }

    public async ValueTask<SKBitmap> CacheImageAsync(
        string key, SKBitmap image
    )
    {
        var data = image.Encode(
            format: SKEncodedImageFormat.Png,
            quality: 100
        );

        await cache.SetAsync(
            key: key,
            value: data.ToArray(),
            options: _hybridCacheEntryOptions
        );

        return image;
    }

    public Task<bool> RemoveCacheAsync(string key)
    {
        throw new NotImplementedException();
        //if (_cache.TryRemove(key, out var image))
        //{
        //    image.Dispose();
        //    return true;
        //}

        //return false;
    }

    public Task ClearCacheAsync()
    {
        throw new NotImplementedException();
        //foreach (var image in _cache.Values)
        //{
        //    image.Dispose();
        //}

        //_cache.Clear();
    }
}