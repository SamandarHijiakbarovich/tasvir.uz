using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace Tasvir.Api;

/// <summary>Serverga joylashtirishda kerak bo‘ladigan sozlamalar (Render, Neon va boshqalar).</summary>
public static class HostingSetup
{
    private static readonly string[] Modules = ["Content", "Identity", "Academy", "Studio", "Classroom", "Payments"];

    /// <summary>
    /// "Database:PostgresUrl" berilsa (Neon kabi xizmatlar beradigan postgresql://... havola), har modul uchun shu serverda
    /// alohida baza ulanishi hosil qilinadi: tasvir_content, tasvir_identity va hokazo. Bazalar birinchi ishga tushishda yaratiladi.
    /// Shunday qilib modullar ajralib qoladi, sozlash esa bitta o‘zgaruvchi bilan bo‘ladi.
    /// </summary>
    public static void UsePostgresUrl(ConfigurationManager configuration)
    {
        var url = configuration["Database:PostgresUrl"];
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        var (baseConnection, database) = ParsePostgresUrl(url);
        var values = new Dictionary<string, string?>
        {
            ["Database:Provider"] = "Postgres",
            // Yangi baza yaratish uchun mavjud bazaga ulaniladi (Neon da "postgres" bazasi bo‘lmasligi mumkin)
            ["Database:AdminDatabase"] = configuration["Database:AdminDatabase"] ?? database,
        };
        foreach (var module in Modules)
        {
            values[$"ConnectionStrings:{module}"] = $"{baseConnection};Database=tasvir_{module.ToLowerInvariant()}";
        }

        configuration.AddInMemoryCollection(values);
    }

    /// <summary>postgresql://user:parol@host:port/baza?sslmode=require → Npgsql ulanish satri (bazasiz) va baza nomi.</summary>
    public static (string Connection, string Database) ParsePostgresUrl(string url)
    {
        var uri = new Uri(url.Trim());
        if (uri.Scheme is not ("postgres" or "postgresql"))
        {
            throw new InvalidOperationException("Database:PostgresUrl postgresql:// bilan boshlanishi kerak.");
        }

        var credentials = uri.UserInfo.Split(':', 2);
        var user = Uri.UnescapeDataString(credentials[0]);
        var password = credentials.Length > 1 ? Uri.UnescapeDataString(credentials[1]) : string.Empty;
        var port = uri.IsDefaultPort || uri.Port < 0 ? 5432 : uri.Port;
        var database = Uri.UnescapeDataString(uri.AbsolutePath.Trim('/'));
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var sslMode = (query["sslmode"] ?? "require").ToLowerInvariant() switch
        {
            "disable" => "Disable",
            "prefer" => "Prefer",
            "verify-ca" => "VerifyCA",
            "verify-full" => "VerifyFull",
            _ => "Require",
        };

        var connection = $"Host={uri.Host};Port={port};Username={Quote(user)};Password={Quote(password)};SSL Mode={sslMode}";
        return (connection, string.IsNullOrEmpty(database) ? "postgres" : database);
    }

    private static string Quote(string value) => $"'{value.Replace("'", "''")}'";

    /// <summary>
    /// Foydalanuvchi IP manzili: brauzer → Next.js → (Gateway) → API. Lokalda faqat loopback proksilarga ishoniladi.
    /// Bulut xostingda (Render) proksilar manzili oldindan noma’lum: "ForwardedHeaders:TrustAll" = true bo‘lsa,
    /// oxirgi ikki bosqich hisobga olinadi (Next.js serveri va xosting balanslovchisi).
    /// </summary>
    public static void ConfigureForwardedHeaders(ForwardedHeadersOptions options, IConfiguration configuration)
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.ForwardLimit = 2;

        if (configuration.GetValue("ForwardedHeaders:TrustAll", false))
        {
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
            return;
        }

        foreach (var proxy in configuration.GetSection("KnownProxies").Get<string[]>() ?? [])
        {
            options.KnownProxies.Add(IPAddress.Parse(proxy));
        }
    }
}
