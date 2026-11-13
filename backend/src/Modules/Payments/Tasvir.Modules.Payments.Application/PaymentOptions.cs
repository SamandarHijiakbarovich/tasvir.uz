namespace Tasvir.Modules.Payments.Application;

/// <summary>
/// "Payments" sozlamalari. Kalitlar repozitoriyga yozilmaydi: serverda muhit o‘zgaruvchilari yoki user-secrets orqali beriladi
/// (masalan, Payments__Payme__Key). Kalit bo‘lmagan to‘lov tizimi saytda o‘chiq ko‘rinadi.
/// </summary>
public sealed class PaymentOptions
{
    public const string Section = "Payments";

    /// <summary>To‘lovdan keyin qaytiladigan sahifa: "{FrontendUrl}/tolov/natija?buyurtma={id}".</summary>
    public string FrontendUrl { get; set; } = "http://localhost:3000";

    public List<PlanOptions> Plans { get; set; } = [];
    public PaymeOptions Payme { get; set; } = new();
    public ClickOptions Click { get; set; } = new();
}

/// <summary>Price — so‘mda; belgilanmagan (null yoki 0) tarifni sotib bo‘lmaydi.</summary>
public sealed class PlanOptions
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Months { get; set; }
    public decimal? Price { get; set; }
    public List<string> Features { get; set; } = [];
}

/// <summary>Payme Merchant API. Kabinet: merchant.paycom.uz → kassa sozlamalari.</summary>
public sealed class PaymeOptions
{
    /// <summary>Kassa ID (checkout havolasidagi "m").</summary>
    public string? MerchantId { get; set; }
    /// <summary>Kassa kaliti: Payme so‘rovlari "Paycom:{Key}" Basic sarlavhasi bilan keladi. Sinovda test kaliti qo‘yiladi.</summary>
    public string? Key { get; set; }
    /// <summary>Sinov uchun https://checkout.test.paycom.uz.</summary>
    public string CheckoutUrl { get; set; } = "https://checkout.paycom.uz";
    /// <summary>Bo‘sh bo‘lmasa, faqat shu IP manzillardan kelgan so‘rovlar qabul qilinadi.</summary>
    public List<string> AllowedIps { get; set; } = [];

    public bool IsConfigured => !string.IsNullOrWhiteSpace(MerchantId) && !string.IsNullOrWhiteSpace(Key);
}

/// <summary>Click SHOP API. Kabinet: merchant.click.uz → servis sozlamalari.</summary>
public sealed class ClickOptions
{
    public string? ServiceId { get; set; }
    public string? MerchantId { get; set; }
    public string? SecretKey { get; set; }
    public string PayUrl { get; set; } = "https://my.click.uz/services/pay";
    public List<string> AllowedIps { get; set; } = [];

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ServiceId) && !string.IsNullOrWhiteSpace(MerchantId) && !string.IsNullOrWhiteSpace(SecretKey);
}
