using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Tasvir.Modules.Payments.Domain;
using Tasvir.SharedKernel;

namespace Tasvir.Modules.Payments.Application;

// API shartnomasi. Frontenddagi src/features/payments/types.ts bilan mos bo‘lishi kerak.

public sealed record CreateOrderRequest(string? PlanKey, string? Provider);

public sealed record PlanDto(string Key, string Name, string? Description, int Months, decimal? Price, IReadOnlyList<string> Features, bool IsAvailable);
public sealed record ProviderDto(string Key, string Name, bool IsAvailable);
public sealed record CatalogDto(IReadOnlyList<PlanDto> Plans, IReadOnlyList<ProviderDto> Providers);

public sealed record OrderDto(Guid Id, string PlanKey, string PlanName, int Months, decimal Amount, string Provider, string Status, DateTime CreatedAt, DateTime? PaidAt);
public sealed record CreatedOrderDto(OrderDto Order, string PaymentUrl);
/// <summary>ValidUntil null — obuna yo‘q yoki tugagan.</summary>
public sealed record SubscriptionDto(bool IsActive, string? PlanName, DateTime? ValidUntil);
public sealed record MyPaymentsDto(SubscriptionDto Subscription, IReadOnlyList<OrderDto> Orders);

public interface IPaymentRepository
{
    Task<PaymentOrder?> FindAsync(Guid id, CancellationToken ct);
    Task<PaymentOrder?> FindByPaymeTransactionAsync(string transactionId, CancellationToken ct);
    Task<List<PaymentOrder>> ListPaymeCreatedBetweenAsync(long fromMs, long toMs, CancellationToken ct);
    Task<List<PaymentOrder>> ListByUserAsync(Guid userId, CancellationToken ct);
    Task<int> CountPendingAsync(Guid userId, DateTime since, CancellationToken ct);
    void Add(PaymentOrder order);
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed class PaymentService(IPaymentRepository repository, PaymentOptions options, TimeProvider clock)
{
    /// <summary>Bir soatda yaratiladigan to‘lanmagan buyurtmalar chegarasi: tugmani qayta-qayta bosish bazani to‘ldirmasin.</summary>
    public const int MaxPendingPerHour = 20;

    private static readonly Error OrderNotFound = new("order_not_found", "Buyurtma topilmadi.", ErrorKind.NotFound);

    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public CatalogDto GetCatalog() => new(
        options.Plans.Select(ToDto).ToList(),
        [
            new ProviderDto(PaymentProvider.Payme, "Payme", options.Payme.IsConfigured),
            new ProviderDto(PaymentProvider.Click, "Click", options.Click.IsConfigured),
        ]);

    public async Task<Result<CreatedOrderDto>> CreateOrderAsync(Guid userId, CreateOrderRequest request, CancellationToken ct)
    {
        var plan = options.Plans.FirstOrDefault(p => p.Key == request.PlanKey);
        if (plan is null || !IsAvailable(plan))
        {
            return Invalid("planKey", "Bu tarifni hozir sotib bölmaydi.");
        }

        if (!PaymentProvider.IsKnown(request.Provider))
        {
            return Invalid("provider", "Tölov tizimini tanlang.");
        }

        if (!IsConfigured(request.Provider!))
        {
            return new Error("provider_unavailable", "Bu tölov tizimi hali ulanmagan. Boşqasini tanlang yoki keyinroq urinib köring.", ErrorKind.Conflict);
        }

        if (await repository.CountPendingAsync(userId, Now.AddHours(-1), ct) >= MaxPendingPerHour)
        {
            return new Error("too_many_orders", "Juda köp buyurtma yaratildi. Birozdan keyin urinib köring.", ErrorKind.TooManyRequests);
        }

        var order = new PaymentOrder(userId, plan.Key, plan.Name, plan.Months, plan.Price!.Value, request.Provider!, Now);
        repository.Add(order);
        await repository.SaveChangesAsync(ct);
        return new CreatedOrderDto(ToDto(order), PaymentUrl(order));
    }

    public async Task<Result<OrderDto>> GetOrderAsync(Guid userId, Guid orderId, CancellationToken ct)
    {
        var order = await repository.FindAsync(orderId, ct);
        return order is null || order.UserId != userId ? OrderNotFound : ToDto(order);
    }

    public async Task<Result<MyPaymentsDto>> GetMineAsync(Guid userId, CancellationToken ct)
    {
        var orders = await repository.ListByUserAsync(userId, ct);
        return new MyPaymentsDto(Subscription.From(orders, Now), orders.Select(ToDto).ToList());
    }

    /// <summary>To‘lov sahifasi havolasi. Payme: base64 parametrlar, summa tiyinda. Click: so‘rov parametrlari, summa so‘mda.</summary>
    public string PaymentUrl(PaymentOrder order)
    {
        var returnUrl = $"{options.FrontendUrl.TrimEnd('/')}/tolov/natija?buyurtma={order.Id}";
        if (order.Provider == PaymentProvider.Payme)
        {
            var parameters = $"m={options.Payme.MerchantId};ac.order_id={order.Id:N};a={order.AmountInTiyin};c={returnUrl};l=uz";
            return $"{options.Payme.CheckoutUrl.TrimEnd('/')}/{Convert.ToBase64String(Encoding.UTF8.GetBytes(parameters))}";
        }

        return $"{options.Click.PayUrl}?service_id={Uri.EscapeDataString(options.Click.ServiceId!)}&merchant_id={Uri.EscapeDataString(options.Click.MerchantId!)}"
            + $"&amount={order.Amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)}&transaction_param={order.Id:N}&return_url={Uri.EscapeDataString(returnUrl)}";
    }

    private bool IsConfigured(string provider) =>
        provider == PaymentProvider.Payme ? options.Payme.IsConfigured : options.Click.IsConfigured;

    private static bool IsAvailable(PlanOptions plan) => plan.Price is > 0 && plan.Months > 0;

    private static PlanDto ToDto(PlanOptions plan) =>
        new(plan.Key, plan.Name, plan.Description, plan.Months, plan.Price is > 0 ? plan.Price : null, plan.Features, IsAvailable(plan));

    public static OrderDto ToDto(PaymentOrder o) =>
        new(o.Id, o.PlanKey, o.PlanName, o.Months, o.Amount, o.Provider, o.Status, o.CreatedAt, o.PaidAt);

    private static Error Invalid(string field, string message) =>
        new("validation_failed", message, ErrorKind.Validation, new Dictionary<string, string> { [field] = message });
}

/// <summary>
/// Obuna alohida saqlanmaydi, to‘langan buyurtmalardan hisoblanadi: qaytarilgan to‘lov avtomatik hisobdan chiqadi.
/// Amal qilayotgan obuna ustiga sotib olinsa, yangi muddat eskisining oxiridan boshlanadi.
/// </summary>
public static class Subscription
{
    public static SubscriptionDto From(IEnumerable<PaymentOrder> orders, DateTime now)
    {
        DateTime? validUntil = null;
        string? plan = null;
        foreach (var order in orders.Where(o => o.Status == OrderStatus.Paid && o.PaidAt is not null).OrderBy(o => o.PaidAt))
        {
            var start = validUntil is { } until && until > order.PaidAt!.Value ? until : order.PaidAt!.Value;
            validUntil = start.AddMonths(order.Months);
            plan = order.PlanName;
        }

        return validUntil is { } end && end > now ? new SubscriptionDto(true, plan, end) : new SubscriptionDto(false, null, null);
    }
}

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentsApplication(this IServiceCollection services)
    {
        services.AddScoped<PaymentService>();
        services.AddScoped<PaymeMerchantService>();
        services.AddScoped<ClickMerchantService>();
        return services;
    }
}
