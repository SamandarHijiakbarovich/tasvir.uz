using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tasvir.Modules.Payments.Application;
using Tasvir.Modules.Payments.Domain;

namespace Tasvir.Modules.Payments.Infrastructure;

public sealed class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : DbContext(options)
{
    public const string Schema = "payments";

    public DbSet<PaymentOrder> Orders => Set<PaymentOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // SQLite sxemalarni qo‘llab-quvvatlamaydi
        if (!Database.IsSqlite())
        {
            modelBuilder.HasDefaultSchema(Schema);
        }

        modelBuilder.Entity<PaymentOrder>(order =>
        {
            order.ToTable("payment_orders");
            order.Property(o => o.Id).ValueGeneratedNever();
            order.Property(o => o.PlanKey).HasMaxLength(40).IsRequired();
            order.Property(o => o.PlanName).HasMaxLength(80).IsRequired();
            order.Property(o => o.Amount).HasPrecision(14, 2);
            order.Property(o => o.Provider).HasMaxLength(10).IsRequired();
            order.Property(o => o.Status).HasMaxLength(12).IsRequired();
            order.Property(o => o.PaymeTransactionId).HasMaxLength(64);
            order.Ignore(o => o.AmountInTiyin);
            order.Ignore(o => o.IsPending);
            order.HasIndex(o => o.UserId);
            order.HasIndex(o => o.PaymeTransactionId).IsUnique();
            order.HasIndex(o => o.PaymeCreateTime);
        });
    }
}

internal sealed class PaymentRepository(PaymentsDbContext db) : IPaymentRepository
{
    public Task<PaymentOrder?> FindAsync(Guid id, CancellationToken ct) =>
        db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<PaymentOrder?> FindByPaymeTransactionAsync(string transactionId, CancellationToken ct) =>
        db.Orders.FirstOrDefaultAsync(o => o.PaymeTransactionId == transactionId, ct);

    public async Task<List<PaymentOrder>> ListPaymeCreatedBetweenAsync(long fromMs, long toMs, CancellationToken ct)
    {
        var orders = await db.Orders.AsNoTracking()
            .Where(o => o.PaymeTransactionId != null && o.PaymeCreateTime >= fromMs && o.PaymeCreateTime <= toMs)
            .ToListAsync(ct);
        return orders.OrderBy(o => o.PaymeCreateTime).ToList();
    }

    public async Task<List<PaymentOrder>> ListByUserAsync(Guid userId, CancellationToken ct)
    {
        var orders = await db.Orders.AsNoTracking().Where(o => o.UserId == userId).ToListAsync(ct);
        return orders.OrderByDescending(o => o.CreatedAt).ToList();
    }

    public Task<int> CountPendingAsync(Guid userId, DateTime since, CancellationToken ct) =>
        db.Orders.CountAsync(o => o.UserId == userId && o.Status == OrderStatus.Pending && o.CreatedAt >= since, ct);

    public void Add(PaymentOrder order) => db.Orders.Add(order);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}

public static class DependencyInjection
{
    /// <summary>Sozlamalar: "ConnectionStrings:Payments", "Database:Provider".</summary>
    public static IServiceCollection AddPaymentsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "Sqlite";
        var connectionString = configuration.GetConnectionString("Payments")
            ?? throw new InvalidOperationException("ConnectionStrings:Payments topilmadi.");

        services.AddDbContext<PaymentsDbContext>(options =>
        {
            switch (provider.ToLowerInvariant())
            {
                case "postgres":
                    options.UseNpgsql(connectionString,
                        npgsql => npgsql.UseAdminDatabase(configuration["Database:AdminDatabase"] ?? "postgres").MigrationsHistoryTable("__ef_migrations", PaymentsDbContext.Schema));
                    break;
                case "sqlite":
                    options.UseSqlite(connectionString);
                    break;
                default:
                    throw new InvalidOperationException($"Noma’lum baza provayderi: {provider}");
            }
        });

        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        return services;
    }

    public static async Task InitializePaymentsDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();

        // TODO: ishlab chiqarishga chiqishdan oldin EnsureCreated o‘rniga EF Core migratsiyalariga o‘tish.
        await db.Database.EnsureCreatedAsync(cancellationToken);
    }
}
