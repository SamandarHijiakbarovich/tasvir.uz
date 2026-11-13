using Microsoft.Extensions.Configuration;
using Tasvir.Api;

namespace Tasvir.Modules.Identity.Tests;

public sealed class HostingSetupTests
{
    [Fact]
    public void Neon_url_is_converted_to_npgsql_connection()
    {
        var (connection, database) = HostingSetup.ParsePostgresUrl(
            "postgresql://neondb_owner:p%40ss'w0rd@ep-cool-name-123.eu-central-1.aws.neon.tech/neondb?sslmode=require&channel_binding=require");

        Assert.Equal("neondb", database);
        Assert.Equal(
            "Host=ep-cool-name-123.eu-central-1.aws.neon.tech;Port=5432;Username='neondb_owner';Password='p@ss''w0rd';SSL Mode=Require",
            connection);
    }

    [Fact]
    public void Every_module_gets_its_own_database()
    {
        var configuration = new ConfigurationManager();
        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Database:PostgresUrl"] = "postgres://u:p@db.example.com:6543/main",
            ["ConnectionStrings:Identity"] = "Data Source=tasvir-identity.db",
        });

        HostingSetup.UsePostgresUrl(configuration);

        Assert.Equal("Postgres", configuration["Database:Provider"]);
        Assert.Equal("main", configuration["Database:AdminDatabase"]);
        Assert.EndsWith("Database=tasvir_identity", configuration.GetConnectionString("Identity"));
        Assert.Contains("Port=6543", configuration.GetConnectionString("Payments"));
        Assert.EndsWith("Database=tasvir_payments", configuration.GetConnectionString("Payments"));
    }

    [Fact]
    public void Without_url_nothing_changes()
    {
        var configuration = new ConfigurationManager();
        configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:Identity"] = "Data Source=x.db" });

        HostingSetup.UsePostgresUrl(configuration);

        Assert.Null(configuration["Database:Provider"]);
        Assert.Equal("Data Source=x.db", configuration.GetConnectionString("Identity"));
    }
}
