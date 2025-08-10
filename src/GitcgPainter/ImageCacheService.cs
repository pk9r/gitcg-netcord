using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Hybrid;
using SixLabors.ImageSharp;

namespace GitcgPainter;

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

    public async Task<Image> LoadHttpImageAsync(string uri)
    {
        var cachedBytes = await cache.GetOrCreateAsync(uri,
            factory: FetchImageAsync,
            _hybridCacheEntryOptions
        );

        using var cachedStream = new MemoryStream(cachedBytes);
        var cachedImage = await Image.LoadAsync(cachedStream);
        return cachedImage;

        async ValueTask<byte[]> FetchImageAsync(
            CancellationToken cancel
        )
        {
            await using var stream = await httpClient
                .GetStreamAsync(uri, cancellationToken: cancel);

            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(
                memoryStream,
                cancellationToken: cancel
            );

            return memoryStream.GetBuffer();
        }
    }

    public async Task<Image?> LoadCachedImageAsync(string key)
    {
        var emptyBytes = Array.Empty<byte>();

        var cachedBytes = await cache.GetOrCreateAsync(
            key: key,
            factory: _ => ValueTask.FromResult(emptyBytes)
        );

        if (cachedBytes.Length == 0)
            return null;

        using var cachedStream = new MemoryStream(cachedBytes);
        var cachedImage = await Image.LoadAsync(cachedStream);
        return cachedImage;
    }

    public async Task<Image> CacheImageAsync(string key, Image image)
    {
        using var stream = new MemoryStream();
        await image.SaveAsPngAsync(stream);

        stream.Position = 0;
        var pngBytes = stream.ToArray();

        await cache.SetAsync(key, pngBytes, _hybridCacheEntryOptions);

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