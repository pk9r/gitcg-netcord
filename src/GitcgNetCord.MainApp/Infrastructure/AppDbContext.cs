using GitcgNetCord.MainApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace GitcgNetCord.MainApp.Infrastructure;

public class AppDbContext : DbContext
{
    public DbSet<DiscordUser> DiscordUsers { get; set; } = null!;
    public DbSet<HoyolabAccount> HoyolabAccounts { get; set; } = null!;
    public DbSet<ActiveHoyolabAccount> ActiveHoyolabAccounts { get; set; } = null!;

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
        });

        builder.Entity<ActiveHoyolabAccount>(config =>
        {
            config.HasKey(x => x.DiscordUserId);
            config.Property(x => x.DiscordUserId)
                .ValueGeneratedNever();
        });


        base.OnModelCreating(builder);
    }
}
