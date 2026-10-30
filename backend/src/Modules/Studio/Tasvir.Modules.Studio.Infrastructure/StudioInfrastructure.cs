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

internal sealed class MaterialRepository(StudioDbContext db) : IMaterialRepository
{
    public Task<List<Material>> ListAsync(Guid userId, CancellationToken ct) =>
        db.Materials.AsNoTracking().Where(m => m.UserId == userId).OrderByDescending(m => m.UpdatedAt).ToListAsync(ct);

    public Task<int> CountAsync(Guid userId, CancellationToken ct) =>
        db.Materials.CountAsync(m => m.UserId == userId, ct);

    public Task<Material?> FindAsync(Guid userId, Guid id, CancellationToken ct) =>
        db.Materials.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId, ct);

    public void Add(Material material) => db.Materials.Add(material);

    public void Remove(Material material) => db.Materials.Remove(material);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}

public static class DependencyInjection
{
    /// <summary>Sozlamalar: "ConnectionStrings:Studio", "Database:Provider".</summary>
    public static IServiceCollection AddStudioInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "Sqlite";
        var connectionString = configuration.GetConnectionString("Studio")
            ?? throw new InvalidOperationException("ConnectionStrings:Studio topilmadi.");

        services.AddDbContext<StudioDbContext>(options =>
        {
            switch (provider.ToLowerInvariant())
            {
                case "postgres":
                    options.UseNpgsql(connectionString,
                        npgsql => npgsql.MigrationsHistoryTable("__ef_migrations", StudioDbContext.Schema));
                    break;
                case "sqlite":
                    options.UseSqlite(connectionString);
                    break;
                default:
                    throw new InvalidOperationException($"Noma’lum baza provayderi: {provider}");
            }
        });

        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IMaterialRepository, MaterialRepository>();
        return services;
    }

    public static async Task InitializeStudioDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<StudioDbContext>();

        // TODO: ishlab chiqarishga chiqishdan oldin EnsureCreated o‘rniga EF Core migratsiyalariga o‘tish.
        await db.Database.EnsureCreatedAsync(cancellationToken);
    }
}
