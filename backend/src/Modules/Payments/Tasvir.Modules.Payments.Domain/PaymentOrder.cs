using Tasvir.SharedKernel;

namespace Tasvir.Modules.Payments.Domain;

public static class PaymentProvider
{
    public const string Payme = "payme";
    public const string Click = "click";

    public static bool IsKnown(string? provider) => provider is Payme or Click;
}

public static class OrderStatus
{
    public const string Pending = "pending";
    public const string Paid = "paid";
    public const string Cancelled = "cancelled";
    /// <summary>To‘langan, keyin to‘lov tizimi orqali qaytarilgan.</summary>
    public const string Refunded = "refunded";
}

/// <summary>Payme tranzaksiya holatlari (Merchant API hujjatidagi raqamlar).</summary>
public static class PaymeState
{
    public const int Created = 1;
    public const int Performed = 2;
    public const int CancelledBeforePerform = -1;
    public const int CancelledAfterPerform = -2;
}

/// <summary>
/// Tarif uchun buyurtma. Narx, nom va muddat buyurtma yaratilgan paytdagi tarifdan nusxa olinadi: keyin tarif
/// o‘zgarsa ham, to‘langan buyurtma o‘zgarmaydi. Har bir buyurtma bitta to‘lov tizimi orqali to‘lanadi.
/// </summary>
public sealed class PaymentOrder : Entity
{
    public Guid UserId { get; private set; }
    public string PlanKey { get; private set; } = string.Empty;
    public string PlanName { get; private set; } = string.Empty;
    public int Months { get; private set; }
    /// <summary>So‘mda.</summary>
    public decimal Amount { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public string Status { get; private set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }

    // Payme: vaqtlar Unix millisekundlarda, Payme kutganidek
    public string? PaymeTransactionId { get; private set; }
    public long? PaymeTime { get; private set; }
    public long? PaymeCreateTime { get; private set; }
    public long? PaymePerformTime { get; private set; }
    public long? PaymeCancelTime { get; private set; }
    public int? PaymeState { get; private set; }
    public int? PaymeReason { get; private set; }

    // Click
    public long? ClickTransId { get; private set; }
    public long? ClickPaydocId { get; private set; }
    public long? ClickPrepareId { get; private set; }

    private PaymentOrder() { }

    public PaymentOrder(Guid userId, string planKey, string planName, int months, decimal amount, string provider, DateTime now)
    {
        if (!PaymentProvider.IsKnown(provider))
        {
            throw new ArgumentException("Noma’lum to‘lov tizimi.", nameof(provider));
        }

        UserId = userId;
        PlanKey = Guard.NotEmpty(planKey, nameof(planKey));
        PlanName = Guard.NotEmpty(planName, nameof(planName));
        Months = months > 0 ? months : throw new ArgumentOutOfRangeException(nameof(months));
        Amount = amount > 0 ? amount : throw new ArgumentOutOfRangeException(nameof(amount));
        Provider = provider;
        CreatedAt = now;
    }

    /// <summary>Payme summani tiyinda yuboradi.</summary>
    public long AmountInTiyin => (long)(Amount * 100);

    public bool IsPending => Status == OrderStatus.Pending;

    public void PaymeCreate(string transactionId, long paymeTime, long nowMs)
    {
        PaymeTransactionId = Guard.NotEmpty(transactionId, nameof(transactionId));
        PaymeTime = paymeTime;
        PaymeCreateTime = nowMs;
        PaymeState = Domain.PaymeState.Created;
    }

    public void PaymePerform(long nowMs, DateTime now)
    {
        PaymeState = Domain.PaymeState.Performed;
        PaymePerformTime = nowMs;
        MarkPaid(now);
    }

    /// <summary>To‘lovdan oldin bekor qilinsa, buyurtma yopiladi; keyin bekor qilinsa (qaytarish), obuna ham bekor bo‘ladi.</summary>
    public void PaymeCancel(int reason, long nowMs, DateTime now)
    {
        var performed = PaymeState == Domain.PaymeState.Performed;
        PaymeState = performed ? Domain.PaymeState.CancelledAfterPerform : Domain.PaymeState.CancelledBeforePerform;
        PaymeCancelTime = nowMs;
        PaymeReason = reason;
        Status = performed ? OrderStatus.Refunded : OrderStatus.Cancelled;
        ClosedAt = now;
    }

    public void ClickPrepare(long clickTransId, long clickPaydocId, long prepareId)
    {
        ClickTransId = clickTransId;
        ClickPaydocId = clickPaydocId;
        ClickPrepareId = prepareId;
    }

    public void MarkPaid(DateTime now)
    {
        Status = OrderStatus.Paid;
        PaidAt = now;
    }

    public void Cancel(DateTime now)
    {
        Status = OrderStatus.Cancelled;
        ClosedAt = now;
    }
}
