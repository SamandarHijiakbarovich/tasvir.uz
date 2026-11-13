using Tasvir.Modules.Academy.Application;
using Tasvir.Modules.Academy.Infrastructure;
using Tasvir.Modules.Academy.Presentation;
using Tasvir.Modules.Assistant.Application;
using Tasvir.Modules.Assistant.Presentation;
using Tasvir.Api;
using Tasvir.Modules.Classroom.Application;
using Tasvir.Modules.Classroom.Infrastructure;
using Tasvir.Modules.Classroom.Presentation;
using Tasvir.Modules.Content.Application;
using Tasvir.Modules.Content.Infrastructure;
using Tasvir.Modules.Content.Presentation;
using Tasvir.Modules.Identity.Application;
using Tasvir.Modules.Identity.Infrastructure;
using Tasvir.Modules.Identity.Presentation;
using Tasvir.Modules.Payments.Application;
using Tasvir.Modules.Payments.Infrastructure;
using Tasvir.Modules.Payments.Presentation;
using Tasvir.Modules.Studio.Application;
using Tasvir.Modules.Studio.Infrastructure;
using Tasvir.Modules.Studio.Presentation;

var builder = WebApplication.CreateBuilder(args);

// Serverda bitta "Database:PostgresUrl" bilan barcha modul bazalari ulanadi
HostingSetup.UsePostgresUrl(builder.Configuration);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

// Foydalanuvchi IP manzili: faqat ishonchli proksilar yuborgan sarlavhalar hisobga olinadi
builder.Services.Configure<ForwardedHeadersOptions>(options => HostingSetup.ConfigureForwardedHeaders(options, builder.Configuration));

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
builder.Services
    .AddStudioApplication()
    .AddStudioInfrastructure(builder.Configuration);
builder.Services
    .AddAssistantApplication()
    .AddAssistantPresentation(builder.Configuration);
builder.Services
    .AddClassroomApplication()
    .AddClassroomInfrastructure(builder.Configuration)
    .AddClassroomPresentation(builder.Configuration)
    .AddClassroomAdapters();
builder.Services
    .AddPaymentsApplication()
    .AddPaymentsInfrastructure(builder.Configuration)
    .AddPaymentsPresentation(builder.Configuration);

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
await app.Services.InitializeStudioDatabaseAsync();
await app.Services.InitializeClassroomDatabaseAsync();
await app.Services.InitializePaymentsDatabaseAsync();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapContentEndpoints();
app.MapIdentityEndpoints();
app.MapAcademyEndpoints();
app.MapStudioEndpoints();
app.MapAssistantEndpoints();
app.MapClassroomEndpoints();
app.MapPaymentsEndpoints();

app.Run();

// Integratsion testlar (WebApplicationFactory) uchun
public partial class Program;
