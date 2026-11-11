using GitcgNetCord.MainApp.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace GitcgNetCord.MainApp.Entities.Repositories;

public class CardSnapshotService(AppDbContext dbContext)
{
    public async Task<CardSnapshot> SaveSnapshotAsync(
        Guid hoyolabAccountId,
        ulong discordUserId,
        string name,
        SnapshotType type,
        int totalGames,
        int totalWins,
        Dictionary<int, CardStatSummary> cardData,
        CancellationToken cancellationToken = default
    )
    {
        var existing = await dbContext.CardSnapshots
            .FirstOrDefaultAsync(x => x.HoyolabAccountId == hoyolabAccountId && x.Name == name, cancellationToken);

        if (existing != null)
        {
            existing.SnapshotType = type;
            existing.CreatedAt = DateTimeOffset.UtcNow;
            existing.TotalGames = totalGames;
            existing.TotalWins = totalWins;
            existing.CardData = cardData;
            await dbContext.SaveChangesAsync(cancellationToken);
            return existing;
        }

        var snapshot = new CardSnapshot
        {
            Id = Guid.NewGuid(),
            HoyolabAccountId = hoyolabAccountId,
            DiscordUserId = discordUserId,
            Name = name,
            SnapshotType = type,
            CreatedAt = DateTimeOffset.UtcNow,
            TotalGames = totalGames,
            TotalWins = totalWins,
            CardData = cardData
        };

        dbContext.CardSnapshots.Add(snapshot);
        await dbContext.SaveChangesAsync(cancellationToken);
        return snapshot;
    }

    public async Task<CardSnapshot?> GetSnapshotByNameAsync(
        Guid hoyolabAccountId,
        string name,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext.CardSnapshots
            .FirstOrDefaultAsync(x => x.HoyolabAccountId == hoyolabAccountId && x.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    public async Task<List<CardSnapshot>> GetSnapshotsForAccountAsync(
        Guid hoyolabAccountId,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext.CardSnapshots
            .Where(x => x.HoyolabAccountId == hoyolabAccountId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> DeleteSnapshotAsync(
        Guid hoyolabAccountId,
        string name,
        CancellationToken cancellationToken = default
    )
    {
        var snapshot = await dbContext.CardSnapshots
            .FirstOrDefaultAsync(x => x.HoyolabAccountId == hoyolabAccountId && x.Name.ToLower() == name.ToLower(), cancellationToken);

        if (snapshot == null) return false;

        dbContext.CardSnapshots.Remove(snapshot);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<CardSnapshot?> GetEarliestSnapshotAsync(
        Guid hoyolabAccountId,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext.CardSnapshots
            .Where(x => x.HoyolabAccountId == hoyolabAccountId)
            .OrderBy(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
