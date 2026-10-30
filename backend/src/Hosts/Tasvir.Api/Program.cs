using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using Tasvir.Modules.Academy.Application;
using Tasvir.Modules.Academy.Infrastructure;
using Tasvir.Modules.Academy.Presentation;
using Tasvir.Modules.Content.Application;
using Tasvir.Modules.Content.Infrastructure;
using Tasvir.Modules.Content.Presentation;
using Tasvir.Modules.Identity.Application;
using Tasvir.Modules.Identity.Infrastructure;
using Tasvir.Modules.Identity.Presentation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

// Foydalanuvchi IP manzili: brauzer → Next.js → Gateway → API.
// Faqat ishonchli proksilar (lokalda loopback) yuborgan sarlavhalar hisobga olinadi; serverda "KnownProxies" sozlanadi.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 2;
    foreach (var proxy in builder.Configuration.GetSection("KnownProxies").Get<string[]>() ?? [])
    {
        options.KnownProxies.Add(IPAddress.Parse(proxy));
    }
});

// Modullar
builder.Services
    .AddContentApplication()
    .AddContentInfrastructure(builder.Configuration);
builder.Services
    .AddIdentityApplication()
    .AddIdentityInfrastructure(builder.Configuration, builder.Environment)
    .AddIdentityPresentation(builder.Configuration);
builder.Services
    .AddAcademyApplication()
    .AddAcademyInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await app.Services.InitializeContentDatabaseAsync();
await app.Services.InitializeIdentityDatabaseAsync();
await app.Services.InitializeAcademyDatabaseAsync();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapContentEndpoints();
app.MapIdentityEndpoints();
app.MapAcademyEndpoints();

app.Run();

// Integratsion testlar (WebApplicationFactory) uchun
public partial class Program;
