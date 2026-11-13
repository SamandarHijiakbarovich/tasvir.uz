using System.Text;
using System.Text.Json.Nodes;
using Tasvir.Modules.Payments.Application;
using Tasvir.Modules.Payments.Domain;

namespace Tasvir.Modules.Payments.Tests;

internal sealed class ManualClock(DateTimeOffset start) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = start;
    public override DateTimeOffset GetUtcNow() => Now;
    public void Advance(TimeSpan by) => Now += by;
}

internal sealed class MemoryRepository : IPaymentRepository
{
    public List<PaymentOrder> Orders { get; } = [];
    public int Saves { get; private set; }

    public Task<PaymentOrder?> FindAsync(Guid id, CancellationToken ct) => Task.FromResult(Orders.FirstOrDefault(o => o.Id == id));
    public Task<PaymentOrder?> FindByPaymeTransactionAsync(string transactionId, CancellationToken ct) =>
        Task.FromResult(Orders.FirstOrDefault(o => o.PaymeTransactionId == transactionId));
    public Task<List<PaymentOrder>> ListPaymeCreatedBetweenAsync(long fromMs, long toMs, CancellationToken ct) =>
        Task.FromResult(Orders.Where(o => o.PaymeTransactionId != null && o.PaymeCreateTime >= fromMs && o.PaymeCreateTime <= toMs).ToList());
    public Task<List<PaymentOrder>> ListByUserAsync(Guid userId, CancellationToken ct) => Task.FromResult(Orders.Where(o => o.UserId == userId).ToList());
    public Task<int> CountPendingAsync(Guid userId, DateTime since, CancellationToken ct) =>
        Task.FromResult(Orders.Count(o => o.UserId == userId && o.IsPending && o.CreatedAt >= since));
    public void Add(PaymentOrder order) => Orders.Add(order);
    public Task SaveChangesAsync(CancellationToken ct)
    {
        Saves++;
        return Task.CompletedTask;
    }
}

internal static class TestOptions
{
    public const string PaymeKey = "test-payme-key";
    public const string ClickSecret = "test-click-secret";

    public static PaymentOptions Create() => new()
    {
        Plans = [new PlanOptions { Key = "pro-1", Name = "Pro", Months = 1, Price = 49000 }],
        Payme = new PaymeOptions { MerchantId = "test-merchant", Key = PaymeKey },
        Click = new ClickOptions { ServiceId = "111", MerchantId = "222", SecretKey = ClickSecret },
    };
}

public sealed class PaymeMerchantTests
{
    private static readonly string Auth = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"Paycom:{TestOptions.PaymeKey}"));

    private readonly MemoryRepository _repository = new();
    private readonly ManualClock _clock = new(new DateTimeOffset(2026, 11, 13, 9, 0, 0, TimeSpan.Zero));
    private readonly PaymeMerchantService _payme;
    private readonly PaymentOrder _order;

    public PaymeMerchantTests()
    {
        _payme = new PaymeMerchantService(_repository, TestOptions.Create(), _clock);
        _order = new PaymentOrder(Guid.NewGuid(), "pro-1", "Pro", 1, 49000, PaymentProvider.Payme, _clock.GetUtcNow().UtcDateTime);
        _repository.Add(_order);
    }

    private Task<JsonObject> Call(string method, object parameters, string? auth = null) =>
        _payme.HandleAsync(
            new JsonObject { ["jsonrpc"] = "2.0", ["id"] = 7, ["method"] = method, ["params"] = JsonNode.Parse(System.Text.Json.JsonSerializer.Serialize(parameters)) }.ToJsonString(),
            auth ?? Auth, CancellationToken.None);

    private object Account(long? amount = null, string? orderId = null) => new
    {
        amount = amount ?? _order.AmountInTiyin,
        account = new { order_id = orderId ?? _order.Id.ToString("N") },
    };

    private object Create(string transactionId, long? amount = null) => new
    {
        id = transactionId,
        time = _clock.GetUtcNow().ToUnixTimeMilliseconds(),
        amount = amount ?? _order.AmountInTiyin,
        account = new { order_id = _order.Id.ToString("N") },
    };

    private static int? ErrorCode(JsonObject response) => (int?)response["error"]?["code"];

    [Fact]
    public async Task Wrong_key_is_rejected()
    {
        var wrong = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("Paycom:boshqa"));
        Assert.Equal(PaymeMerchantService.Codes.InsufficientPrivilege, ErrorCode(await Call("CheckPerformTransaction", Account(), wrong)));
        Assert.Equal(PaymeMerchantService.Codes.InsufficientPrivilege, ErrorCode(await Call("CheckPerformTransaction", Account(), "")));
    }

    [Fact]
    public async Task Without_configured_key_everything_is_rejected()
    {
        var payme = new PaymeMerchantService(_repository, new PaymentOptions(), _clock);
        var response = await payme.HandleAsync("""{"id":1,"method":"CheckPerformTransaction","params":{}}""", "Basic " + Convert.ToBase64String("Paycom:"u8.ToArray()), CancellationToken.None);
        Assert.Equal(PaymeMerchantService.Codes.InsufficientPrivilege, ErrorCode(response));
    }

    [Fact]
    public async Task Malformed_json_and_unknown_method()
    {
        Assert.Equal(PaymeMerchantService.Codes.ParseError, ErrorCode(await _payme.HandleAsync("{yomon", Auth, CancellationToken.None)));
        var unknown = await Call("Nimadir", new { });
        Assert.Equal(PaymeMerchantService.Codes.MethodNotFound, ErrorCode(unknown));
        Assert.Equal(7, (int?)unknown["id"]);
    }

    [Fact]
    public async Task Check_validates_order_and_amount()
    {
        Assert.True((bool?)(await Call("CheckPerformTransaction", Account()))["result"]?["allow"]);
        Assert.Equal(PaymeMerchantService.Codes.InvalidAmount, ErrorCode(await Call("CheckPerformTransaction", Account(amount: 100))));
        Assert.Equal(PaymeMerchantService.Codes.OrderNotFound, ErrorCode(await Call("CheckPerformTransaction", Account(orderId: Guid.NewGuid().ToString("N")))));
        Assert.Equal(PaymeMerchantService.Codes.OrderNotFound, ErrorCode(await Call("CheckPerformTransaction", Account(orderId: "abc"))));
    }

    [Fact]
    public async Task Click_order_is_not_visible_to_payme()
    {
        var click = new PaymentOrder(Guid.NewGuid(), "pro-1", "Pro", 1, 49000, PaymentProvider.Click, _clock.GetUtcNow().UtcDateTime);
        _repository.Add(click);
        Assert.Equal(PaymeMerchantService.Codes.OrderNotFound, ErrorCode(await Call("CheckPerformTransaction", Account(orderId: click.Id.ToString("N")))));
    }

    [Fact]
    public async Task Full_payment_flow_marks_order_paid()
    {
        var created = await Call("CreateTransaction", Create("tx-1"));
        Assert.Equal(PaymeState.Created, (int?)created["result"]?["state"]);

        // Takroriy CreateTransaction — o‘sha natija
        var again = await Call("CreateTransaction", Create("tx-1"));
        Assert.Equal((long?)created["result"]?["create_time"], (long?)again["result"]?["create_time"]);

        // Boshqa tranzaksiya shu buyurtmaga — band
        Assert.Equal(PaymeMerchantService.Codes.OrderBusy, ErrorCode(await Call("CreateTransaction", Create("tx-2"))));

        _clock.Advance(TimeSpan.FromMinutes(1));
        var performed = await Call("PerformTransaction", new { id = "tx-1" });
        Assert.Equal(PaymeState.Performed, (int?)performed["result"]?["state"]);
        Assert.Equal(OrderStatus.Paid, _order.Status);
        Assert.NotNull(_order.PaidAt);

        // Takroriy Perform — o‘sha natija, vaqt o‘zgarmaydi
        _clock.Advance(TimeSpan.FromMinutes(1));
        var performedAgain = await Call("PerformTransaction", new { id = "tx-1" });
        Assert.Equal((long?)performed["result"]?["perform_time"], (long?)performedAgain["result"]?["perform_time"]);

        var check = await Call("CheckTransaction", new { id = "tx-1" });
        Assert.Equal(PaymeState.Performed, (int?)check["result"]?["state"]);
        Assert.Equal(_order.Id.ToString("N"), (string?)check["result"]?["transaction"]);

        // To‘langan buyurtmaga yangi to‘lov yo‘q
        Assert.Equal(PaymeMerchantService.Codes.OrderClosed, ErrorCode(await Call("CheckPerformTransaction", Account())));
    }

    [Fact]
    public async Task Cancel_before_perform_closes_order()
    {
        await Call("CreateTransaction", Create("tx-1"));
        var cancelled = await Call("CancelTransaction", new { id = "tx-1", reason = 3 });
        Assert.Equal(PaymeState.CancelledBeforePerform, (int?)cancelled["result"]?["state"]);
        Assert.Equal(OrderStatus.Cancelled, _order.Status);
        Assert.Equal(PaymeMerchantService.Codes.CannotPerform, ErrorCode(await Call("PerformTransaction", new { id = "tx-1" })));

        // Takroriy bekor qilish — o‘sha javob
        var again = await Call("CancelTransaction", new { id = "tx-1", reason = 3 });
        Assert.Equal((long?)cancelled["result"]?["cancel_time"], (long?)again["result"]?["cancel_time"]);
    }

    [Fact]
    public async Task Cancel_after_perform_is_refund()
    {
        await Call("CreateTransaction", Create("tx-1"));
        await Call("PerformTransaction", new { id = "tx-1" });
        var refunded = await Call("CancelTransaction", new { id = "tx-1", reason = 5 });
        Assert.Equal(PaymeState.CancelledAfterPerform, (int?)refunded["result"]?["state"]);
        Assert.Equal(OrderStatus.Refunded, _order.Status);
        Assert.False(Subscription.From(_repository.Orders, _clock.GetUtcNow().UtcDateTime).IsActive);
    }

    [Fact]
    public async Task Transaction_times_out_after_12_hours()
    {
        await Call("CreateTransaction", Create("tx-1"));
        _clock.Advance(TimeSpan.FromHours(12) + TimeSpan.FromSeconds(1));
        Assert.Equal(PaymeMerchantService.Codes.CannotPerform, ErrorCode(await Call("PerformTransaction", new { id = "tx-1" })));
        var check = await Call("CheckTransaction", new { id = "tx-1" });
        Assert.Equal(PaymeState.CancelledBeforePerform, (int?)check["result"]?["state"]);
        Assert.Equal(PaymeMerchantService.ReasonTimeout, (int?)check["result"]?["reason"]);
    }

    [Fact]
    public async Task Unknown_transaction()
    {
        Assert.Equal(PaymeMerchantService.Codes.TransactionNotFound, ErrorCode(await Call("PerformTransaction", new { id = "yoq" })));
        Assert.Equal(PaymeMerchantService.Codes.TransactionNotFound, ErrorCode(await Call("CancelTransaction", new { id = "yoq", reason = 1 })));
        Assert.Equal(PaymeMerchantService.Codes.TransactionNotFound, ErrorCode(await Call("CheckTransaction", new { id = "yoq" })));
    }

    [Fact]
    public async Task Statement_lists_transactions_in_range()
    {
        var from = _clock.GetUtcNow().ToUnixTimeMilliseconds();
        await Call("CreateTransaction", Create("tx-1"));
        var statement = await Call("GetStatement", new { from, to = from + 1000 });
        var transactions = statement["result"]?["transactions"]?.AsArray();
        Assert.Single(transactions!);
        Assert.Equal("tx-1", (string?)transactions![0]?["id"]);
        Assert.Equal(_order.AmountInTiyin, (long?)transactions[0]?["amount"]);

        var empty = await Call("GetStatement", new { from = from + 5000, to = from + 9000 });
        Assert.Empty(empty["result"]?["transactions"]?.AsArray()!);
    }
}

public sealed class ClickMerchantTests
{
    private readonly MemoryRepository _repository = new();
    private readonly ManualClock _clock = new(new DateTimeOffset(2026, 11, 13, 9, 0, 0, TimeSpan.Zero));
    private readonly ClickMerchantService _click;
    private readonly PaymentOrder _order;

    public ClickMerchantTests()
    {
        _click = new ClickMerchantService(_repository, TestOptions.Create(), _clock);
        _order = new PaymentOrder(Guid.NewGuid(), "pro-1", "Pro", 1, 49000, PaymentProvider.Click, _clock.GetUtcNow().UtcDateTime);
        _repository.Add(_order);
    }

    private ClickRequest Signed(string action, string transId = "5001", string amount = "49000.00", long? prepareId = null, string error = "0", string? orderId = null)
    {
        var request = new ClickRequest(transId, "111", "9001", orderId ?? _order.Id.ToString("N"), prepareId?.ToString(), amount, action, error, "Success", "2026-11-13 14:00:00", null);
        return request with { SignString = ClickMerchantService.Sign(request, TestOptions.ClickSecret) };
    }

    private static int Code(Dictionary<string, object?> response) => (int)response["error"]!;

    [Fact]
    public void Signature_matches_click_formula()
    {
        // md5("5001" + "111" + SECRET + order + "49000.00" + "0" + sign_time)
        var request = Signed(ClickMerchantService.ActionPrepare);
        var source = $"5001111{TestOptions.ClickSecret}{_order.Id:N}49000.000{request.SignTime}";
        var expected = Convert.ToHexStringLower(System.Security.Cryptography.MD5.HashData(Encoding.UTF8.GetBytes(source)));
        Assert.Equal(expected, request.SignString);
    }

    [Fact]
    public async Task Prepare_then_complete_marks_paid()
    {
        var prepared = await _click.PrepareAsync(Signed(ClickMerchantService.ActionPrepare), CancellationToken.None);
        Assert.Equal(ClickMerchantService.Codes.Success, Code(prepared));
        var prepareId = Convert.ToInt64(prepared["merchant_prepare_id"]);

        var completed = await _click.CompleteAsync(Signed(ClickMerchantService.ActionComplete, prepareId: prepareId), CancellationToken.None);
        Assert.Equal(ClickMerchantService.Codes.Success, Code(completed));
        Assert.Equal(prepareId, Convert.ToInt64(completed["merchant_confirm_id"]));
        Assert.Equal(OrderStatus.Paid, _order.Status);

        // Ikkinchi marta — allaqachon to‘langan
        var again = await _click.CompleteAsync(Signed(ClickMerchantService.ActionComplete, prepareId: prepareId), CancellationToken.None);
        Assert.Equal(ClickMerchantService.Codes.AlreadyPaid, Code(again));
    }

    [Fact]
    public async Task Bad_signature_is_rejected()
    {
        var request = Signed(ClickMerchantService.ActionPrepare) with { Amount = "1.00" };
        Assert.Equal(ClickMerchantService.Codes.SignFailed, Code(await _click.PrepareAsync(request, CancellationToken.None)));
        Assert.Null(_order.ClickTransId);
    }

    [Fact]
    public async Task Wrong_amount_unknown_order_and_wrong_action()
    {
        Assert.Equal(ClickMerchantService.Codes.InvalidAmount, Code(await _click.PrepareAsync(Signed(ClickMerchantService.ActionPrepare, amount: "1000.00"), CancellationToken.None)));
        Assert.Equal(ClickMerchantService.Codes.OrderNotFound, Code(await _click.PrepareAsync(Signed(ClickMerchantService.ActionPrepare, orderId: Guid.NewGuid().ToString("N")), CancellationToken.None)));
        Assert.Equal(ClickMerchantService.Codes.ActionNotFound, Code(await _click.PrepareAsync(Signed(ClickMerchantService.ActionComplete, prepareId: 1), CancellationToken.None)));
    }

    [Fact]
    public async Task Complete_requires_matching_prepare()
    {
        await _click.PrepareAsync(Signed(ClickMerchantService.ActionPrepare), CancellationToken.None);
        var response = await _click.CompleteAsync(Signed(ClickMerchantService.ActionComplete, prepareId: 12345), CancellationToken.None);
        Assert.Equal(ClickMerchantService.Codes.TransactionNotFound, Code(response));
        Assert.Equal(OrderStatus.Pending, _order.Status);
    }

    [Fact]
    public async Task Click_error_cancels_order()
    {
        var prepared = await _click.PrepareAsync(Signed(ClickMerchantService.ActionPrepare), CancellationToken.None);
        var prepareId = Convert.ToInt64(prepared["merchant_prepare_id"]);
        var response = await _click.CompleteAsync(Signed(ClickMerchantService.ActionComplete, prepareId: prepareId, error: "-5017"), CancellationToken.None);
        Assert.Equal(ClickMerchantService.Codes.Cancelled, Code(response));
        Assert.Equal(OrderStatus.Cancelled, _order.Status);
        Assert.Equal(ClickMerchantService.Codes.Cancelled, Code(await _click.PrepareAsync(Signed(ClickMerchantService.ActionPrepare, transId: "5002"), CancellationToken.None)));
    }

    [Fact]
    public async Task Without_keys_everything_is_rejected()
    {
        var click = new ClickMerchantService(_repository, new PaymentOptions(), _clock);
        Assert.Equal(ClickMerchantService.Codes.BadRequest, Code(await click.PrepareAsync(Signed(ClickMerchantService.ActionPrepare), CancellationToken.None)));
    }
}

public sealed class SubscriptionTests
{
    private static readonly DateTime Start = new(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);

    private static PaymentOrder Paid(int months, DateTime at)
    {
        var order = new PaymentOrder(Guid.NewGuid(), "pro", "Pro", months, 49000, PaymentProvider.Click, at);
        order.MarkPaid(at);
        return order;
    }

    [Fact]
    public void No_orders_means_no_subscription()
    {
        Assert.False(Subscription.From([], Start).IsActive);
    }

    [Fact]
    public void Renewal_extends_from_current_end()
    {
        var orders = new[] { Paid(1, Start), Paid(1, Start.AddDays(20)) };
        var subscription = Subscription.From(orders, Start.AddDays(25));
        Assert.True(subscription.IsActive);
        Assert.Equal(Start.AddMonths(2), subscription.ValidUntil);
    }

    [Fact]
    public void Expired_subscription_is_inactive_and_new_purchase_starts_fresh()
    {
        var old = Paid(1, Start);
        Assert.False(Subscription.From([old], Start.AddMonths(2)).IsActive);

        var renewed = Paid(1, Start.AddMonths(3));
        Assert.Equal(Start.AddMonths(4), Subscription.From([old, renewed], Start.AddMonths(3).AddDays(1)).ValidUntil);
    }

    [Fact]
    public void Pending_and_cancelled_orders_do_not_count()
    {
        var pending = new PaymentOrder(Guid.NewGuid(), "pro", "Pro", 1, 49000, PaymentProvider.Payme, Start);
        var cancelled = new PaymentOrder(Guid.NewGuid(), "pro", "Pro", 1, 49000, PaymentProvider.Payme, Start);
        cancelled.Cancel(Start);
        Assert.False(Subscription.From([pending, cancelled], Start.AddDays(1)).IsActive);
    }
}
