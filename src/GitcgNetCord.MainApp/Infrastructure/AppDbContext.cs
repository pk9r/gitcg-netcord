using System.Text.Json;
using GitcgNetCord.MainApp.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace GitcgNetCord.MainApp.Infrastructure;

public class AppDbContext : DbContext
{
    public DbSet<DiscordUser> DiscordUsers { get; set; } = null!;
    public DbSet<HoyolabAccount> HoyolabAccounts { get; set; } = null!;
    public DbSet<ActiveHoyolabAccount> ActiveHoyolabAccounts { get; set; } = null!;
    public DbSet<DiscordCardCodeChannel> DiscordCardCodeChannels { get; set; } = null!;
    public DbSet<CardSnapshot> CardSnapshots { get; set; } = null!;
    public DbSet<GameVersion> GameVersions { get; set; } = null!;
    public DbSet<DiscordDailyReportChannel> DiscordDailyReportChannels { get; set; } = null!;
    public DbSet<DiscordDailyReportThread> DiscordDailyReportThreads { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<DiscordUser>(config =>
        {
            config.Property(x => x.Id)
                .ValueGeneratedNever();

            config.HasOne(x => x.ActiveHoyolabAccount)
                .WithOne(x => x.DiscordUser)
                .HasForeignKey<ActiveHoyolabAccount>(x => x.DiscordUserId);
        });

        builder.Entity<HoyolabAccount>(config =>
        {
            config.Property(x => x.HoyolabUserId).HasMaxLength(10);
            config.Property(x => x.Token).HasMaxLength(255);
            config.Property(x => x.GameRoleId).HasMaxLength(10);
            config.Property(x => x.Region).HasMaxLength(10);
            config.Property(x => x.IsDailySnapshotEnabled).HasDefaultValue(false);
            config.Property(x => x.IsDeleted).HasDefaultValue(false);
            config.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<ActiveHoyolabAccount>(config =>
        {
            config.HasKey(x => x.DiscordUserId);
            config.Property(x => x.DiscordUserId)
                .ValueGeneratedNever();
        });

        builder.Entity<DiscordCardCodeChannel>(config =>
        {
            config.Property(x => x.Id)
                .ValueGeneratedNever();
            config.Property(x => x.SharingDecodeType)
                .HasConversion<string>();
            config.HasIndex(x => x.GuildId);
        });

        builder.Entity<GameVersion>(config =>
        {
            config.Property(x => x.VersionNumber).HasMaxLength(20);
            config.Property(x => x.Name).HasMaxLength(100);
            config.HasIndex(x => x.VersionNumber).IsUnique();
        });

        var cardDataComparer = new ValueComparer<Dictionary<int, CardStatSummary>>(
            (d1, d2) => d1 != null && d2 != null && d1.Count == d2.Count && !d1.Except(d2).Any(),
            d => d.Aggregate(0, (a, p) => HashCode.Combine(a, p.Key.GetHashCode(), p.Value.GetHashCode())),
            d => new Dictionary<int, CardStatSummary>(d)
        );

        builder.Entity<CardSnapshot>(config =>
        {
            config.Property(x => x.Name).HasMaxLength(100);
            config.Property(x => x.SnapshotType)
                .HasConversion<string>();
            config.Property(x => x.CardData)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<int, CardStatSummary>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<int, CardStatSummary>()
                );
            config.Property(x => x.CardData)
                .Metadata
                .SetValueComparer(cardDataComparer);

            config.HasIndex(x => new { x.HoyolabAccountId, x.Name });
            config.HasIndex(x => x.DiscordUserId);

            config.HasOne(x => x.HoyolabAccount)
                .WithMany(x => x.CardSnapshots)
                .HasForeignKey(x => x.HoyolabAccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<DiscordDailyReportChannel>(config =>
        {
            config.Property(x => x.Id)
                .ValueGeneratedNever();
            config.HasIndex(x => x.GuildId);
        });

        builder.Entity<DiscordDailyReportThread>(config =>
        {
            config.HasIndex(x => new { x.ThreadId, x.HoyolabAccountId }).IsUnique();
            config.HasIndex(x => x.GuildId);
            config.HasIndex(x => x.DiscordUserId);
            config.HasIndex(x => x.HoyolabAccountId);

            config.HasOne(x => x.HoyolabAccount)
                .WithMany(x => x.DailyReportThreads)
                .HasForeignKey(x => x.HoyolabAccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(builder);
    }
}
