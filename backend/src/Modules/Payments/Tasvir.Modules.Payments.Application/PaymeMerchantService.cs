using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Tasvir.Modules.Payments.Domain;

namespace Tasvir.Modules.Payments.Application;

/// <summary>
/// Payme Merchant API (JSON-RPC 2.0). Payme to‘lov jarayonida bizning endpointga oltita usulni chaqiradi:
/// CheckPerformTransaction → CreateTransaction → PerformTransaction, kerak bo‘lsa CancelTransaction,
/// holatni tekshirish uchun CheckTransaction va hisobot uchun GetStatement.
/// Javob har doim HTTP 200, xato "error" maydonida qaytadi. Hisob maydoni: account.order_id.
/// </summary>
public sealed class PaymeMerchantService(IPaymentRepository repository, PaymentOptions options, TimeProvider clock)
{
    /// <summary>Yaratilgan, lekin 12 soat ichida bajarilmagan tranzaksiya bekor qilinadi (Payme talabi).</summary>
    public const long TransactionTimeoutMs = 43_200_000;
    public const int ReasonTimeout = 4;

    public static class Codes
    {
        public const int InsufficientPrivilege = -32504;
        public const int ParseError = -32700;
        public const int InvalidRequest = -32600;
        public const int MethodNotFound = -32601;
        public const int InvalidAmount = -31001;
        public const int TransactionNotFound = -31003;
        public const int CannotCancel = -31007;
        public const int CannotPerform = -31008;
        public const int OrderNotFound = -31050;
        public const int OrderClosed = -31051;
        public const int OrderBusy = -31052;
    }

    private long NowMs => clock.GetUtcNow().ToUnixTimeMilliseconds();
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<JsonObject> HandleAsync(string body, string? authorization, CancellationToken ct)
    {
        JsonNode? id = null;
        try
        {
            if (!IsAuthorized(authorization))
            {
                return Fail(null, Codes.InsufficientPrivilege, "Ruxsat yöq", "Недостаточно привилегий", "Insufficient privilege");
            }

            JsonObject request;
            try
            {
                request = JsonNode.Parse(body)?.AsObject() ?? throw new JsonException();
            }
            catch (Exception e) when (e is JsonException or InvalidOperationException)
            {
                return Fail(null, Codes.ParseError, "JSON xato", "Ошибка разбора JSON", "Parse error");
            }

            id = request["id"]?.DeepClone();
            var parameters = request["params"] as JsonObject ?? [];
            return (string?)request["method"] switch
            {
                "CheckPerformTransaction" => await CheckPerformAsync(id, parameters, ct),
                "CreateTransaction" => await CreateAsync(id, parameters, ct),
                "PerformTransaction" => await PerformAsync(id, parameters, ct),
                "CancelTransaction" => await CancelAsync(id, parameters, ct),
                "CheckTransaction" => await CheckAsync(id, parameters, ct),
                "GetStatement" => await StatementAsync(id, parameters, ct),
                null => Fail(id, Codes.InvalidRequest, "Usul körsatilmagan", "Метод не указан", "Method is missing"),
                _ => Fail(id, Codes.MethodNotFound, "Usul topilmadi", "Метод не найден", "Method not found"),
            };
        }
        catch (Exception e) when (e is FormatException or InvalidOperationException or OverflowException)
        {
            return Fail(id, Codes.InvalidRequest, "Sörov parametrlari xato", "Неверные параметры запроса", "Invalid request parameters");
        }
    }

    /// <summary>Payme "Authorization: Basic base64(Paycom:{kalit})" yuboradi. Kalit vaqt bo‘yicha xavfsiz solishtiriladi.</summary>
    private bool IsAuthorized(string? authorization)
    {
        if (!options.Payme.IsConfigured || authorization is null || !authorization.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string decoded;
        try
        {
            decoded = Encoding.UTF8.GetString(Convert.FromBase64String(authorization[6..].Trim()));
        }
        catch (FormatException)
        {
            return false;
        }

        var expected = Encoding.UTF8.GetBytes($"Paycom:{options.Payme.Key}");
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(decoded), expected);
    }

    private async Task<(PaymentOrder? Order, JsonObject? Error)> FindOrderAsync(JsonNode? id, JsonObject parameters, CancellationToken ct)
    {
        var raw = (string?)parameters["account"]?["order_id"];
        var order = Guid.TryParse(raw, out var orderId) ? await repository.FindAsync(orderId, ct) : null;
        if (order is null || order.Provider != PaymentProvider.Payme)
        {
            return (null, Fail(id, Codes.OrderNotFound, "Buyurtma topilmadi", "Заказ не найден", "Order not found", "order_id"));
        }

        if ((long?)parameters["amount"] != order.AmountInTiyin)
        {
            return (null, Fail(id, Codes.InvalidAmount, "Summa notöğri", "Неверная сумма", "Invalid amount"));
        }

        return (order, null);
    }

    private async Task<JsonObject> CheckPerformAsync(JsonNode? id, JsonObject parameters, CancellationToken ct)
    {
        var (order, error) = await FindOrderAsync(id, parameters, ct);
        if (error is not null)
        {
            return error;
        }

        if (!order!.IsPending)
        {
            return Fail(id, Codes.OrderClosed, "Buyurtma tölangan yoki yopilgan", "Заказ уже оплачен или закрыт", "Order is already paid or closed", "order_id");
        }

        if (order.PaymeState == PaymeState.Created)
        {
            return Fail(id, Codes.OrderBusy, "Buyurtma böyiça tölov kutilmoqda", "По заказу ожидается оплата", "Order is awaiting payment", "order_id");
        }

        return Ok(id, new JsonObject { ["allow"] = true });
    }

    private async Task<JsonObject> CreateAsync(JsonNode? id, JsonObject parameters, CancellationToken ct)
    {
        var transactionId = (string?)parameters["id"] ?? throw new FormatException();
        var existing = await repository.FindByPaymeTransactionAsync(transactionId, ct);
        if (existing is not null)
        {
            if (existing.PaymeState != PaymeState.Created)
            {
                return Fail(id, Codes.CannotPerform, "Amalni bajarib bölmaydi", "Невозможно выполнить операцию", "Unable to perform operation");
            }

            if (await TimedOutAsync(existing, ct))
            {
                return Fail(id, Codes.CannotPerform, "Tranzaksiya muddati tugagan", "Время транзакции истекло", "Transaction timed out");
            }

            return Ok(id, Created(existing));
        }

        var (order, error) = await FindOrderAsync(id, parameters, ct);
        if (error is not null)
        {
            return error;
        }

        if (!order!.IsPending)
        {
            return Fail(id, Codes.OrderClosed, "Buyurtma tölangan yoki yopilgan", "Заказ уже оплачен или закрыт", "Order is already paid or closed", "order_id");
        }

        if (order.PaymeState == PaymeState.Created)
        {
            return Fail(id, Codes.OrderBusy, "Buyurtma böyiça boşqa tölov kutilmoqda", "По заказу ожидается другая оплата", "Another payment is pending for this order", "order_id");
        }

        order.PaymeCreate(transactionId, (long?)parameters["time"] ?? NowMs, NowMs);
        await repository.SaveChangesAsync(ct);
        return Ok(id, Created(order));
    }

    private async Task<JsonObject> PerformAsync(JsonNode? id, JsonObject parameters, CancellationToken ct)
    {
        var order = await repository.FindByPaymeTransactionAsync((string?)parameters["id"] ?? throw new FormatException(), ct);
        if (order is null)
        {
            return Fail(id, Codes.TransactionNotFound, "Tranzaksiya topilmadi", "Транзакция не найдена", "Transaction not found");
        }

        switch (order.PaymeState)
        {
            case PaymeState.Created:
                if (await TimedOutAsync(order, ct))
                {
                    return Fail(id, Codes.CannotPerform, "Tranzaksiya muddati tugagan", "Время транзакции истекло", "Transaction timed out");
                }

                order.PaymePerform(NowMs, Now);
                await repository.SaveChangesAsync(ct);
                return Ok(id, Performed(order));
            case PaymeState.Performed:
                return Ok(id, Performed(order));
            default:
                return Fail(id, Codes.CannotPerform, "Amalni bajarib bölmaydi", "Невозможно выполнить операцию", "Unable to perform operation");
        }
    }

    private async Task<JsonObject> CancelAsync(JsonNode? id, JsonObject parameters, CancellationToken ct)
    {
        var order = await repository.FindByPaymeTransactionAsync((string?)parameters["id"] ?? throw new FormatException(), ct);
        if (order is null)
        {
            return Fail(id, Codes.TransactionNotFound, "Tranzaksiya topilmadi", "Транзакция не найдена", "Transaction not found");
        }

        if (order.PaymeState is PaymeState.Created or PaymeState.Performed)
        {
            order.PaymeCancel((int?)parameters["reason"] ?? 0, NowMs, Now);
            await repository.SaveChangesAsync(ct);
        }

        return Ok(id, new JsonObject
        {
            ["transaction"] = order.Id.ToString("N"),
            ["cancel_time"] = order.PaymeCancelTime,
            ["state"] = order.PaymeState,
        });
    }

    private async Task<JsonObject> CheckAsync(JsonNode? id, JsonObject parameters, CancellationToken ct)
    {
        var order = await repository.FindByPaymeTransactionAsync((string?)parameters["id"] ?? throw new FormatException(), ct);
        return order is null
            ? Fail(id, Codes.TransactionNotFound, "Tranzaksiya topilmadi", "Транзакция не найдена", "Transaction not found")
            : Ok(id, new JsonObject
            {
                ["create_time"] = order.PaymeCreateTime,
                ["perform_time"] = order.PaymePerformTime ?? 0,
                ["cancel_time"] = order.PaymeCancelTime ?? 0,
                ["transaction"] = order.Id.ToString("N"),
                ["state"] = order.PaymeState,
                ["reason"] = order.PaymeReason,
            });
    }

    private async Task<JsonObject> StatementAsync(JsonNode? id, JsonObject parameters, CancellationToken ct)
    {
        var from = (long?)parameters["from"] ?? throw new FormatException();
        var to = (long?)parameters["to"] ?? throw new FormatException();
        var orders = await repository.ListPaymeCreatedBetweenAsync(from, to, ct);
        var transactions = new JsonArray(orders.Select(o => (JsonNode)new JsonObject
        {
            ["id"] = o.PaymeTransactionId,
            ["time"] = o.PaymeTime,
            ["amount"] = o.AmountInTiyin,
            ["account"] = new JsonObject { ["order_id"] = o.Id.ToString("N") },
            ["create_time"] = o.PaymeCreateTime,
            ["perform_time"] = o.PaymePerformTime ?? 0,
            ["cancel_time"] = o.PaymeCancelTime ?? 0,
            ["transaction"] = o.Id.ToString("N"),
            ["state"] = o.PaymeState,
            ["reason"] = o.PaymeReason,
        }).ToArray());
        return Ok(id, new JsonObject { ["transactions"] = transactions });
    }

    private async Task<bool> TimedOutAsync(PaymentOrder order, CancellationToken ct)
    {
        if (NowMs - order.PaymeCreateTime <= TransactionTimeoutMs)
        {
            return false;
        }

        order.PaymeCancel(ReasonTimeout, NowMs, Now);
        await repository.SaveChangesAsync(ct);
        return true;
    }

    private static JsonObject Created(PaymentOrder order) => new()
    {
        ["create_time"] = order.PaymeCreateTime,
        ["transaction"] = order.Id.ToString("N"),
        ["state"] = order.PaymeState,
    };

    private static JsonObject Performed(PaymentOrder order) => new()
    {
        ["transaction"] = order.Id.ToString("N"),
        ["perform_time"] = order.PaymePerformTime,
        ["state"] = order.PaymeState,
    };

    private static JsonObject Ok(JsonNode? id, JsonObject result) => new() { ["jsonrpc"] = "2.0", ["result"] = result, ["id"] = id };

    private static JsonObject Fail(JsonNode? id, int code, string uz, string ru, string en, string? data = null)
    {
        var error = new JsonObject { ["code"] = code, ["message"] = new JsonObject { ["uz"] = uz, ["ru"] = ru, ["en"] = en } };
        if (data is not null)
        {
            error["data"] = data;
        }

        return new JsonObject { ["jsonrpc"] = "2.0", ["error"] = error, ["id"] = id };
    }
}
