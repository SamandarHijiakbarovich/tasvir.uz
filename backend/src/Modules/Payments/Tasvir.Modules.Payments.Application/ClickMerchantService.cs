using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Tasvir.Modules.Payments.Domain;

namespace Tasvir.Modules.Payments.Application;

/// <summary>Click SHOP API so‘rovi (form-urlencoded). Imzo hisoblash uchun qiymatlar Click yuborgan ko‘rinishida saqlanadi.</summary>
public sealed record ClickRequest(
    string? ClickTransId,
    string? ServiceId,
    string? ClickPaydocId,
    string? MerchantTransId,
    string? MerchantPrepareId,
    string? Amount,
    string? Action,
    string? Error,
    string? ErrorNote,
    string? SignTime,
    string? SignString);

/// <summary>
/// Click SHOP API: to‘lov ikki bosqichda keladi. Prepare (action=0) — buyurtma bor va summasi to‘g‘riligini tekshirish,
/// Complete (action=1) — to‘lov yakuni. Har bir so‘rov md5 imzo bilan keladi, imzoda maxfiy kalit (SecretKey) ishtirok etadi.
/// </summary>
public sealed class ClickMerchantService(IPaymentRepository repository, PaymentOptions options, TimeProvider clock)
{
    public static class Codes
    {
        public const int Success = 0;
        public const int SignFailed = -1;
        public const int InvalidAmount = -2;
        public const int ActionNotFound = -3;
        public const int AlreadyPaid = -4;
        public const int OrderNotFound = -5;
        public const int TransactionNotFound = -6;
        public const int UpdateFailed = -7;
        public const int BadRequest = -8;
        public const int Cancelled = -9;
    }

    public const string ActionPrepare = "0";
    public const string ActionComplete = "1";

    public async Task<Dictionary<string, object?>> PrepareAsync(ClickRequest request, CancellationToken ct)
    {
        if (Validate(request, ActionPrepare) is { } invalid)
        {
            return Reply(request, invalid.Code, invalid.Note);
        }

        var order = await FindOrderAsync(request, ct);
        if (order is null)
        {
            return Reply(request, Codes.OrderNotFound, "Order not found");
        }

        if (Closed(order) is { } closed)
        {
            return Reply(request, closed.Code, closed.Note);
        }

        if (!AmountMatches(request, order))
        {
            return Reply(request, Codes.InvalidAmount, "Incorrect parameter amount");
        }

        // Foydalanuvchi to‘lovni qayta boshlasa, Click yangi click_trans_id bilan yana Prepare yuboradi: oxirgisi amal qiladi.
        var prepareId = order.ClickTransId == long.Parse(request.ClickTransId!, CultureInfo.InvariantCulture) && order.ClickPrepareId is { } existing
            ? existing
            : RandomNumberGenerator.GetInt32(1, int.MaxValue);
        order.ClickPrepare(
            long.Parse(request.ClickTransId!, CultureInfo.InvariantCulture),
            long.TryParse(request.ClickPaydocId, CultureInfo.InvariantCulture, out var paydoc) ? paydoc : 0,
            prepareId);
        await repository.SaveChangesAsync(ct);
        return Reply(request, Codes.Success, "Success", ("merchant_prepare_id", prepareId));
    }

    public async Task<Dictionary<string, object?>> CompleteAsync(ClickRequest request, CancellationToken ct)
    {
        if (Validate(request, ActionComplete) is { } invalid)
        {
            return Reply(request, invalid.Code, invalid.Note);
        }

        var order = await FindOrderAsync(request, ct);
        if (order is null)
        {
            return Reply(request, Codes.OrderNotFound, "Order not found");
        }

        if (order.ClickPrepareId is null
            || order.ClickPrepareId != long.Parse(request.MerchantPrepareId!, CultureInfo.InvariantCulture)
            || order.ClickTransId != long.Parse(request.ClickTransId!, CultureInfo.InvariantCulture))
        {
            return Reply(request, Codes.TransactionNotFound, "Transaction does not exist");
        }

        if (Closed(order) is { } closed)
        {
            return Reply(request, closed.Code, closed.Note);
        }

        // Click to‘lov o‘tmaganini bildirsa (error < 0), buyurtma yopiladi.
        if (int.TryParse(request.Error, CultureInfo.InvariantCulture, out var clickError) && clickError < 0)
        {
            order.Cancel(clock.GetUtcNow().UtcDateTime);
            await repository.SaveChangesAsync(ct);
            return Reply(request, Codes.Cancelled, "Transaction cancelled");
        }

        if (!AmountMatches(request, order))
        {
            return Reply(request, Codes.InvalidAmount, "Incorrect parameter amount");
        }

        order.MarkPaid(clock.GetUtcNow().UtcDateTime);
        await repository.SaveChangesAsync(ct);
        return Reply(request, Codes.Success, "Success", ("merchant_confirm_id", order.ClickPrepareId));
    }

    /// <summary>
    /// Prepare: md5(click_trans_id + service_id + SECRET_KEY + merchant_trans_id + amount + action + sign_time).
    /// Complete: merchant_trans_id dan keyin merchant_prepare_id qo‘shiladi.
    /// </summary>
    public static string Sign(ClickRequest r, string secretKey)
    {
        var source = r.Action == ActionComplete
            ? $"{r.ClickTransId}{r.ServiceId}{secretKey}{r.MerchantTransId}{r.MerchantPrepareId}{r.Amount}{r.Action}{r.SignTime}"
            : $"{r.ClickTransId}{r.ServiceId}{secretKey}{r.MerchantTransId}{r.Amount}{r.Action}{r.SignTime}";
        return Convert.ToHexStringLower(MD5.HashData(Encoding.UTF8.GetBytes(source)));
    }

    private (int Code, string Note)? Validate(ClickRequest r, string action)
    {
        var required = action == ActionComplete
            ? new[] { r.ClickTransId, r.ServiceId, r.MerchantTransId, r.MerchantPrepareId, r.Amount, r.Action, r.SignTime, r.SignString }
            : [r.ClickTransId, r.ServiceId, r.MerchantTransId, r.Amount, r.Action, r.SignTime, r.SignString];
        if (!options.Click.IsConfigured
            || required.Any(string.IsNullOrWhiteSpace)
            || !long.TryParse(r.ClickTransId, CultureInfo.InvariantCulture, out _)
            || (action == ActionComplete && !long.TryParse(r.MerchantPrepareId, CultureInfo.InvariantCulture, out _)))
        {
            return (Codes.BadRequest, "Error in request from click");
        }

        var expected = Encoding.ASCII.GetBytes(Sign(r, options.Click.SecretKey!));
        var actual = Encoding.ASCII.GetBytes(r.SignString!.Trim().ToLowerInvariant());
        if (!CryptographicOperations.FixedTimeEquals(expected, actual))
        {
            return (Codes.SignFailed, "SIGN CHECK FAILED!");
        }

        if (r.Action != action)
        {
            return (Codes.ActionNotFound, "Action not found");
        }

        if (r.ServiceId != options.Click.ServiceId)
        {
            return (Codes.BadRequest, "Error in request from click");
        }

        return null;
    }

    private async Task<PaymentOrder?> FindOrderAsync(ClickRequest r, CancellationToken ct)
    {
        var order = Guid.TryParse(r.MerchantTransId, out var id) ? await repository.FindAsync(id, ct) : null;
        return order?.Provider == PaymentProvider.Click ? order : null;
    }

    private static (int Code, string Note)? Closed(PaymentOrder order) => order.Status switch
    {
        OrderStatus.Paid or OrderStatus.Refunded => (Codes.AlreadyPaid, "Already paid"),
        OrderStatus.Cancelled => (Codes.Cancelled, "Transaction cancelled"),
        _ => null,
    };

    private static bool AmountMatches(ClickRequest r, PaymentOrder order) =>
        decimal.TryParse(r.Amount, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount)
        && Math.Abs(amount - order.Amount) < 0.01m;

    private static Dictionary<string, object?> Reply(ClickRequest r, int code, string note, (string Key, object? Value)? extra = null)
    {
        var response = new Dictionary<string, object?>
        {
            ["click_trans_id"] = long.TryParse(r.ClickTransId, CultureInfo.InvariantCulture, out var transId) ? transId : null,
            ["merchant_trans_id"] = r.MerchantTransId,
        };
        if (extra is { } e)
        {
            response[e.Key] = e.Value;
        }

        response["error"] = code;
        response["error_note"] = note;
        return response;
    }
}
