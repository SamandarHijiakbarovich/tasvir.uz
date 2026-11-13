using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Tasvir.Api;
using Tasvir.Modules.Academy.Infrastructure;
using Tasvir.Modules.Academy.Infrastructure.Persistence;
using Tasvir.Modules.Classroom.Infrastructure;
using Tasvir.Modules.Content.Infrastructure;
using Tasvir.Modules.Content.Infrastructure.Persistence;
using Tasvir.Modules.Identity.Infrastructure;
using Tasvir.Modules.Identity.Infrastructure.Persistence;
using Tasvir.Modules.Payments.Infrastructure;
using Tasvir.Modules.Studio.Infrastructure;

namespace Tasvir.Modules.Identity.Tests;

/// <summary>
/// Serverda (Neon) baza PostgreSQL. Bu test bazaga ulanmaydi: har modul modeli Postgres uchun jadval yaratish skriptiga
/// to‘g‘ri aylanishini va har modul o‘z sxemasida turishini tekshiradi.
/// </summary>
public sealed class PostgresModelTests
{
    private static ServiceProvider BuildServices()
    {
        var configuration = new ConfigurationManager();
        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Database:PostgresUrl"] = "postgresql://user:secret@db.example.com/neondb?sslmode=require",
            ["Identity:Jwt:SigningKey"] = new string('k', 48),
        });
        HostingSetup.UsePostgresUrl(configuration);

        var services = new ServiceCollection();
        services.AddLogging();
        var environment = new HostingEnvironment { EnvironmentName = Environments.Development };
        services
            .AddContentInfrastructure(configuration)
            .AddIdentityInfrastructure(configuration, environment)
            .AddAcademyInfrastructure(configuration)
            .AddStudioInfrastructure(configuration)
            .AddClassroomInfrastructure(configuration)
            .AddPaymentsInfrastructure(configuration);
        return services.BuildServiceProvider();
    }

    public static TheoryData<Type, string, string> Contexts => new()
    {
        { typeof(ContentDbContext), "content", "tasvir_content" },
        { typeof(IdentityModuleDbContext), "identity", "tasvir_identity" },
        { typeof(AcademyDbContext), "academy", "tasvir_academy" },
        { typeof(StudioDbContext), "studio", "tasvir_studio" },
        { typeof(ClassroomDbContext), "classroom", "tasvir_classroom" },
        { typeof(PaymentsDbContext), "payments", "tasvir_payments" },
    };

    [Theory]
    [MemberData(nameof(Contexts))]
    public void Model_generates_postgres_schema(Type contextType, string schema, string database)
    {
        using var services = BuildServices();
        using var scope = services.CreateScope();
        var db = (DbContext)scope.ServiceProvider.GetRequiredService(contextType);

        Assert.True(db.Database.IsNpgsql());
        Assert.Contains($"Database={database}", db.Database.GetConnectionString());

        var script = db.Database.GenerateCreateScript();
        Assert.Contains($"CREATE SCHEMA {schema};", script);
        Assert.Contains($"CREATE TABLE {schema}.", script);
        Assert.DoesNotContain("TEXT NOT NULL CONSTRAINT", script);
    }
}
