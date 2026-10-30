// API Gateway — frontend uchun yagona kirish nuqtasi.
// Har bir servis (C# yoki boshqa texnologiyada) appsettings.json dagi "ReverseProxy" bo‘limida marshrut sifatida ulanadi.

const string FrontendCors = "frontend";

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options => options.AddPolicy(FrontendCors, policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

builder.Services.AddHealthChecks();
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseCors(FrontendCors);
app.MapHealthChecks("/health");
app.MapReverseProxy();

app.Run();
