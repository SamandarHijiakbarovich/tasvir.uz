using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tasvir.Modules.Studio.Application;
using Tasvir.Modules.Studio.Domain;

namespace Tasvir.Modules.Studio.Infrastructure;

public sealed class StudioDbContext(DbContextOptions<StudioDbContext> options) : DbContext(options)
{
    public const string Schema = "studio";

    public DbSet<Material> Materials => Set<Material>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // SQLite sxemalarni qo‘llab-quvvatlamaydi
        if (!Database.IsSqlite())
        {
            modelBuilder.HasDefaultSchema(Schema);
        }

        modelBuilder.Entity<Material>(material =>
        {
            material.ToTable("materials");
            material.Property(m => m.Id).ValueGeneratedNever();
            material.Property(m => m.TemplateKey).HasMaxLength(40).IsRequired();
            material.Property(m => m.Title).HasMaxLength(120).IsRequired();
            material.Property(m => m.Subject).HasMaxLength(40);
            material.Property(m => m.ContentJson).IsRequired();
            material.HasIndex(m => new { m.UserId, m.UpdatedAt });
        });
    }
}

/// <summary>O‘qituvchining kutubxonasi: yuklangan fayllar, havolalar va ochiq ulashish havolalari.</summary>
public sealed class StudioLibraryDbContext(DbContextOptions<StudioLibraryDbContext> options) : DbContext(options)
{
    public DbSet<LibraryItem> Items => Set<LibraryItem>();
    public DbSet<LibraryFile> Files => Set<LibraryFile>();
    public DbSet<ShareLink> Shares => Set<ShareLink>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (!Database.IsSqlite())
        {
            modelBuilder.HasDefaultSchema(StudioDbContext.Schema);
        }

        modelBuilder.Entity<LibraryItem>(item =>
        {
            item.ToTable("library_items");
            item.Property(i => i.Id).ValueGeneratedNever();
            item.Property(i => i.Kind).HasMaxLength(10).IsRequired();
            item.Property(i => i.Title).HasMaxLength(120).IsRequired();
            item.Property(i => i.Subject).HasMaxLength(40);
            item.Property(i => i.Topic).HasMaxLength(80);
            item.Property(i => i.Url).HasMaxLength(500);
            item.Property(i => i.FileName).HasMaxLength(160);
            item.Property(i => i.ContentType).HasMaxLength(100);
            item.HasIndex(i => new { i.UserId, i.UpdatedAt });
        });

        modelBuilder.Entity<LibraryFile>(file =>
        {
            file.ToTable("library_files");
            file.Property(f => f.Id).ValueGeneratedNever();
            file.Property(f => f.Data).IsRequired();
            file.HasOne<LibraryItem>().WithOne().HasForeignKey<LibraryFile>(f => f.Id).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ShareLink>(share =>
        {
            share.ToTable("share_links");
            share.HasKey(s => s.Token);
            share.Property(s => s.Token).HasMaxLength(32);
            share.Property(s => s.TargetKind).HasMaxLength(10).IsRequired();
            share.HasIndex(s => new { s.TargetKind, s.TargetId }).IsUnique();
            share.HasIndex(s => s.UserId);
        });
    }
}

internal sealed class LibraryRepository(StudioLibraryDbContext db) : ILibraryRepository
{
    public Task<List<LibraryItem>> ListAsync(Guid userId, CancellationToken ct) =>
        db.Items.AsNoTracking().Where(i => i.UserId == userId).OrderByDescending(i => i.UpdatedAt).ToListAsync(ct);

    public Task<int> CountAsync(Guid userId, CancellationToken ct) => db.Items.CountAsync(i => i.UserId == userId, ct);

    // Bo‘sh to‘plamda yig‘indi null bo‘ladi
    public async Task<long> TotalSizeAsync(Guid userId, CancellationToken ct) =>
        await db.Items.Where(i => i.UserId == userId).SumAsync(i => (long?)i.SizeBytes, ct) ?? 0;

    public Task<LibraryItem?> FindAsync(Guid userId, Guid id, CancellationToken ct) =>
        db.Items.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId, ct);

    public Task<LibraryItem?> FindAnyAsync(Guid id, CancellationToken ct) =>
        db.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id, ct);

    public Task<byte[]?> ReadFileAsync(Guid id, CancellationToken ct) =>
        db.Files.AsNoTracking().Where(f => f.Id == id).Select(f => f.Data).FirstOrDefaultAsync(ct);

    public void Add(LibraryItem item, LibraryFile? file)
    {
        db.Items.Add(item);
        if (file is not null)
        {
            db.Files.Add(file);
        }
    }

    public async Task RemoveAsync(LibraryItem item, CancellationToken ct)
    {
        // Fayl mazmunini xotiraga yuklamasdan o‘chirish
        await db.Files.Where(f => f.Id == item.Id).ExecuteDeleteAsync(ct);
        db.Items.Remove(item);
    }

    public Task<List<ShareLink>> ListSharesAsync(Guid userId, CancellationToken ct) =>
        db.Shares.AsNoTracking().Where(s => s.UserId == userId).ToListAsync(ct);

    public Task<ShareLink?> FindShareAsync(string targetKind, Guid targetId, CancellationToken ct) =>
        db.Shares.FirstOrDefaultAsync(s => s.TargetKind == targetKind && s.TargetId == targetId, ct);

    public Task<ShareLink?> FindShareByTokenAsync(string token, CancellationToken ct) =>
        db.Shares.AsNoTracking().FirstOrDefaultAsync(s => s.Token == token, ct);

    public void AddShare(ShareLink share) => db.Shares.Add(share);

    public void RemoveShare(ShareLink share) => db.Shares.Remove(share);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}

internal sealed class MaterialRepository(StudioDbContext db) : IMaterialRepository
{
    public Task<List<Material>> ListAsync(Guid userId, CancellationToken ct) =>
        db.Materials.AsNoTracking().Where(m => m.UserId == userId).OrderByDescending(m => m.UpdatedAt).ToListAsync(ct);

    public Task<int> CountAsync(Guid userId, CancellationToken ct) =>
        db.Materials.CountAsync(m => m.UserId == userId, ct);

    public Task<Material?> FindAsync(Guid userId, Guid id, CancellationToken ct) =>
        db.Materials.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId, ct);

    public Task<Material?> FindAnyAsync(Guid id, CancellationToken ct) =>
        db.Materials.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, ct);

    public void Add(Material material) => db.Materials.Add(material);

    public void Remove(Material material) => db.Materials.Remove(material);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}

public static class DependencyInjection
{
    /// <summary>Sozlamalar: "ConnectionStrings:Studio", "ConnectionStrings:StudioLibrary", "Database:Provider".</summary>
    public static IServiceCollection AddStudioInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "Sqlite";
        var connectionString = configuration.GetConnectionString("Studio")
            ?? throw new InvalidOperationException("ConnectionStrings:Studio topilmadi.");

        // Kutubxona (yuklangan fayllar, havolalar, ulashish) alohida bazada: mavjud bazaga EnsureCreated yangi jadval qo‘shmaydi
        var libraryConnectionString = configuration.GetConnectionString("StudioLibrary")
            ?? throw new InvalidOperationException("ConnectionStrings:StudioLibrary topilmadi.");

        services.AddDbContext<StudioDbContext>(options => Configure(options, provider, connectionString, configuration));
        services.AddDbContext<StudioLibraryDbContext>(options => Configure(options, provider, libraryConnectionString, configuration));

        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IMaterialRepository, MaterialRepository>();
        services.AddScoped<ILibraryRepository, LibraryRepository>();
        return services;
    }

    private static void Configure(DbContextOptionsBuilder options, string provider, string connectionString, IConfiguration configuration)
    {
        switch (provider.ToLowerInvariant())
        {
            case "postgres":
                options.UseNpgsql(connectionString,
                    npgsql => npgsql.UseAdminDatabase(configuration["Database:AdminDatabase"] ?? "postgres").MigrationsHistoryTable("__ef_migrations", StudioDbContext.Schema));
                break;
            case "sqlite":
                options.UseSqlite(connectionString);
                break;
            default:
                throw new InvalidOperationException($"Noma’lum baza provayderi: {provider}");
        }
    }

    public static async Task InitializeStudioDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<StudioDbContext>();

        // TODO: ishlab chiqarishga chiqishdan oldin EnsureCreated o‘rniga EF Core migratsiyalariga o‘tish.
        await db.Database.EnsureCreatedAsync(cancellationToken);
        await scope.ServiceProvider.GetRequiredService<StudioLibraryDbContext>().Database.EnsureCreatedAsync(cancellationToken);
    }
}
