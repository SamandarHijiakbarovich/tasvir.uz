using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tasvir.Modules.Academy.Application;
using Tasvir.Modules.Academy.Infrastructure.Persistence;

namespace Tasvir.Modules.Academy.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Sozlamalar: "ConnectionStrings:Academy", "Database:Provider".</summary>
    public static IServiceCollection AddAcademyInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "Sqlite";
        var connectionString = configuration.GetConnectionString("Academy")
            ?? throw new InvalidOperationException("ConnectionStrings:Academy topilmadi.");

        services.AddDbContext<AcademyDbContext>(options =>
        {
            switch (provider.ToLowerInvariant())
            {
                case "postgres":
                    options.UseNpgsql(connectionString,
                        npgsql => npgsql.MigrationsHistoryTable("__ef_migrations", AcademyDbContext.Schema));
                    break;
                case "sqlite":
                    options.UseSqlite(connectionString);
                    break;
                default:
                    throw new InvalidOperationException($"Noma’lum baza provayderi: {provider}");
            }
        });

        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IAcademyRepository, AcademyRepository>();
        return services;
    }

    public static async Task InitializeAcademyDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AcademyDbContext>();

        // TODO: ishlab chiqarishga chiqishdan oldin EnsureCreated o‘rniga EF Core migratsiyalariga o‘tish.
        await db.Database.EnsureCreatedAsync(cancellationToken);

        // EnsureCreated mavjud bazaga jadval qo‘shmaydi: lokal bazalardagi progress saqlanib qolsin
        if (db.Database.IsSqlite())
        {
            await db.Database.ExecuteSqlRawAsync("""
                CREATE TABLE IF NOT EXISTS step_attempts (
                    Id TEXT NOT NULL CONSTRAINT PK_step_attempts PRIMARY KEY,
                    UserId TEXT NOT NULL,
                    StepId TEXT NOT NULL,
                    Answer TEXT NOT NULL,
                    IsCorrect INTEGER NOT NULL,
                    AttemptedAt TEXT NOT NULL);
                CREATE INDEX IF NOT EXISTS IX_step_attempts_UserId_StepId ON step_attempts (UserId, StepId);
                """, cancellationToken);
        }

        await AcademySeeder.SeedAsync(db, cancellationToken);
    }
}
