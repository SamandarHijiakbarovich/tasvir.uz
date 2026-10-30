using Tasvir.Modules.Content.Application;
using Tasvir.Modules.Content.Infrastructure;
using Tasvir.Modules.Content.Presentation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

// Modullar
builder.Services
    .AddContentApplication()
    .AddContentInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await app.Services.InitializeContentDatabaseAsync();

app.MapHealthChecks("/health");
app.MapContentEndpoints();

app.Run();
