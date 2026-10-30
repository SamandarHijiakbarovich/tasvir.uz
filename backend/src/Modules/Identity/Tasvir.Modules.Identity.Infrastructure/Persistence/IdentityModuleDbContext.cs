using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasvir.Modules.Identity.Domain;

namespace Tasvir.Modules.Identity.Infrastructure.Persistence;

public sealed class IdentityModuleDbContext(DbContextOptions<IdentityModuleDbContext> options) : DbContext(options)
{
    public const string Schema = "identity";

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OneTimeToken> OneTimeTokens => Set<OneTimeToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // SQLite sxemalarni qo‘llab-quvvatlamaydi
        if (!Database.IsSqlite())
        {
            modelBuilder.HasDefaultSchema(Schema);
        }

        Configure(modelBuilder.Entity<User>());
        Configure(modelBuilder.Entity<RefreshToken>());
        Configure(modelBuilder.Entity<OneTimeToken>());
    }

    private static void Configure(EntityTypeBuilder<User> user)
    {
        user.ToTable("users");
        user.Property(u => u.FullName).HasMaxLength(100).IsRequired();
        user.Property(u => u.Email).HasMaxLength(254).IsRequired();
        user.Property(u => u.NormalizedEmail).HasMaxLength(254).IsRequired();
        user.Property(u => u.PasswordHash).HasMaxLength(512).IsRequired();
        user.HasIndex(u => u.NormalizedEmail).IsUnique();
    }

    private static void Configure(EntityTypeBuilder<RefreshToken> token)
    {
        token.ToTable("refresh_tokens");
        token.Property(t => t.TokenHash).HasMaxLength(64).IsRequired();
        token.HasIndex(t => t.TokenHash).IsUnique();
        token.HasIndex(t => t.FamilyId);
        token.HasIndex(t => t.UserId);
        token.HasOne<User>().WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
    }

    private static void Configure(EntityTypeBuilder<OneTimeToken> token)
    {
        token.ToTable("one_time_tokens");
        token.Property(t => t.TokenHash).HasMaxLength(64).IsRequired();
        token.Property(t => t.Purpose).HasConversion<int>();
        token.HasIndex(t => t.TokenHash).IsUnique();
        token.HasIndex(t => new { t.UserId, t.Purpose });
        token.HasOne<User>().WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
