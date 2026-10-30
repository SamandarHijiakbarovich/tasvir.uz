using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tasvir.Modules.Content.Application.Abstractions;
using Tasvir.Modules.Content.Infrastructure.Persistence;

namespace Tasvir.Modules.Content.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Baza provayderi konfiguratsiyadan tanlanadi: "Database:Provider" = "Sqlite" (lokal ishlab chiqish) yoki "Postgres".
    /// Ulanish satri: "ConnectionStrings:Content".
    /// </summary>
    public static IServiceCollection AddContentInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "Sqlite";
        var connectionString = configuration.GetConnectionString("Content")
            ?? throw new InvalidOperationException("ConnectionStrings:Content topilmadi.");

        services.AddDbContext<ContentDbContext>(options =>
        {
            switch (provider.ToLowerInvariant())
            {
                case "postgres":
                    options.UseNpgsql(connectionString,
                        npgsql => npgsql.MigrationsHistoryTable("__ef_migrations", ContentDbContext.Schema));
                    break;
                case "sqlite":
                    options.UseSqlite(connectionString);
                    break;
                default:
                    throw new InvalidOperationException($"Noma’lum baza provayderi: {provider}");
            }
        });

        services.AddScoped<ILandingContentRepository, LandingContentRepository>();
        return services;
    }

    /// <summary>Bazani yaratadi va boshlang‘ich ma’lumotlarni yozadi.</summary>
    public static async Task InitializeContentDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ContentDbContext>();

        // TODO: ishlab chiqarishga chiqishdan oldin EnsureCreated o‘rniga EF Core migratsiyalariga o‘tish.
        await db.Database.EnsureCreatedAsync(cancellationToken);
        await ContentSeeder.SeedAsync(db, cancellationToken);
    }
}
