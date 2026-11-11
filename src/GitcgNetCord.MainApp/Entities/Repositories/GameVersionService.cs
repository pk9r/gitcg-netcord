using GitcgNetCord.MainApp.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace GitcgNetCord.MainApp.Entities.Repositories;

public class GameVersionService(AppDbContext dbContext)
{
    public async Task<List<GameVersion>> GetGameVersionsAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.GameVersions
            .OrderByDescending(x => x.ReleaseDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<GameVersion?> GetVersionAsync(string versionNumber, CancellationToken cancellationToken = default)
    {
        return await dbContext.GameVersions
            .FirstOrDefaultAsync(x => x.VersionNumber == versionNumber, cancellationToken);
    }

    public async Task<GameVersion?> GetNextVersionAsync(string versionNumber, CancellationToken cancellationToken = default)
    {
        var current = await GetVersionAsync(versionNumber, cancellationToken);
        if (current == null) return null;

        return await dbContext.GameVersions
            .Where(x => x.ReleaseDate > current.ReleaseDate)
            .OrderBy(x => x.ReleaseDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<GameVersion>> GetDueUnprocessedVersionsAsync(DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        return await dbContext.GameVersions
            .Where(x => !x.IsProcessed && x.ReleaseDate <= now)
            .OrderBy(x => x.ReleaseDate)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkVersionAsProcessedAsync(int versionId, CancellationToken cancellationToken = default)
    {
        var version = await dbContext.GameVersions.FindAsync([versionId], cancellationToken);
        if (version != null)
        {
            version.IsProcessed = true;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<GameVersion> AddOrUpdateVersionAsync(
        string versionNumber,
        string? name,
        DateTimeOffset releaseDate,
        CancellationToken cancellationToken = default
    )
    {
        var existing = await dbContext.GameVersions
            .FirstOrDefaultAsync(x => x.VersionNumber == versionNumber, cancellationToken);

        if (existing != null)
        {
            existing.Name = name;
            existing.ReleaseDate = releaseDate;
            await dbContext.SaveChangesAsync(cancellationToken);
            return existing;
        }

        var newVersion = new GameVersion
        {
            VersionNumber = versionNumber,
            Name = name,
            ReleaseDate = releaseDate,
            IsProcessed = releaseDate <= DateTimeOffset.UtcNow
        };

        dbContext.GameVersions.Add(newVersion);
        await dbContext.SaveChangesAsync(cancellationToken);
        return newVersion;
    }
}
