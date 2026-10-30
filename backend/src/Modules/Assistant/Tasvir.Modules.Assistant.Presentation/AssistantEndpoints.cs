using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tasvir.Modules.Assistant.Application;

namespace Tasvir.Modules.Assistant.Presentation;

public static class AssistantEndpoints
{
    private const string RateLimit = "assistant";
    private const int MaxQuestionLength = 500;

    /// <summary>Sozlama: "Assistant:RateLimit:PermitPerMinute".</summary>
    public static IServiceCollection AddAssistantPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        var permitPerMinute = configuration.GetValue("Assistant:RateLimit:PermitPerMinute", 30);

        services.AddRateLimiter(options => options.AddPolicy(RateLimit, context => RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress ?? IPAddress.None,
            _ => new FixedWindowRateLimiterOptions { PermitLimit = permitPerMinute, Window = TimeSpan.FromMinutes(1) })));

        return services;
    }

    /// <summary>
    /// Yordamchi moduli: /api/assistant/*. Savollar saqlanmaydi va logga yozilmaydi.
    /// Kirmagan foydalanuvchi ham so‘rashi mumkin (bosh sahifada ham ishlaydi).
    /// </summary>
    public static IEndpointRouteBuilder MapAssistantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/assistant").WithTags("Assistant").RequireRateLimiting(RateLimit);

        group.MapPost("/ask", (AskRequest request, IAnswerProvider provider) =>
            {
                var question = request.Question?.Trim() ?? string.Empty;
                if (question.Length is 0 or > MaxQuestionLength)
                {
                    return Results.Problem(
                        title: $"Savol 1–{MaxQuestionLength} belgi oralig‘ida bo‘lsin.",
                        statusCode: StatusCodes.Status400BadRequest,
                        extensions: new Dictionary<string, object?> { ["code"] = "invalid_question" });
                }

                return Results.Ok(provider.Answer(question));
            })
            .WithSummary("Oddiy savolga bilim bazasidan javob");

        return app;
    }
}
