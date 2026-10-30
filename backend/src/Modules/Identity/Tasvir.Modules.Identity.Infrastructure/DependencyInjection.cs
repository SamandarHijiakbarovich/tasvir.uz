using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Tasvir.Modules.Identity.Application.Abstractions;
using Tasvir.Modules.Identity.Infrastructure.Email;
using Tasvir.Modules.Identity.Infrastructure.Persistence;
using Tasvir.Modules.Identity.Infrastructure.Security;

namespace Tasvir.Modules.Identity.Infrastructure;

public static class DependencyInjection
{
    private const int MinSigningKeyBytes = 32;

    /// <summary>
    /// Sozlamalar: "ConnectionStrings:Identity", "Database:Provider", "Identity:*".
    /// Ishlab chiqarishda "Identity:Jwt:SigningKey" va "Identity:Smtp:Host" majburiy.
    /// </summary>
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var provider = configuration["Database:Provider"] ?? "Sqlite";
        // Har modul o‘z bazasiga ega: EnsureCreated mavjud bazaga jadval qo‘shmaydi, modullar ham bir-biriga bog‘lanmaydi
        var connectionString = configuration.GetConnectionString("Identity")
            ?? throw new InvalidOperationException("ConnectionStrings:Identity topilmadi.");

        services.AddDbContext<IdentityModuleDbContext>(options =>
        {
            switch (provider.ToLowerInvariant())
            {
                case "postgres":
                    options.UseNpgsql(connectionString,
                        npgsql => npgsql.MigrationsHistoryTable("__ef_migrations", IdentityModuleDbContext.Schema));
                    break;
                case "sqlite":
                    options.UseSqlite(connectionString);
                    break;
                default:
                    throw new InvalidOperationException($"Noma’lum baza provayderi: {provider}");
            }
        });

        var identity = configuration.GetSection(IdentityOptions.Section).Get<IdentityOptions>() ?? new IdentityOptions();
        var jwt = configuration.GetSection(JwtOptions.Section).Get<JwtOptions>() ?? new JwtOptions();
        var smtp = configuration.GetSection(SmtpOptions.Section).Get<SmtpOptions>() ?? new SmtpOptions();
        jwt.SigningKey = ResolveSigningKey(jwt.SigningKey, environment);

        services.AddSingleton(identity);
        services.AddSingleton(jwt);
        services.AddSingleton(smtp);
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IPasswordService, PasswordService>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddScoped<IIdentityRepository, IdentityRepository>();

        if (!string.IsNullOrWhiteSpace(smtp.Host))
        {
            services.AddSingleton<IEmailSender, SmtpEmailSender>();
        }
        else if (environment.IsProduction())
        {
            throw new InvalidOperationException("Identity:Smtp:Host sozlanmagan: parolni tiklash xatlari yuborilmaydi.");
        }
        else
        {
            services.AddSingleton<IEmailSender, DevelopmentEmailSender>();
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });
        services.AddAuthorization();

        return services;
    }

    public static async Task InitializeIdentityDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityModuleDbContext>();

        // TODO: ishlab chiqarishga chiqishdan oldin EnsureCreated o‘rniga EF Core migratsiyalariga o‘tish.
        await db.Database.EnsureCreatedAsync(cancellationToken);
    }

    private static string ResolveSigningKey(string configured, IHostEnvironment environment)
    {
        if (Encoding.UTF8.GetByteCount(configured) >= MinSigningKeyBytes)
        {
            return configured;
        }

        if (environment.IsProduction() || configured.Length > 0)
        {
            throw new InvalidOperationException($"Identity:Jwt:SigningKey kamida {MinSigningKeyBytes} bayt bo‘lishi shart.");
        }

        // Lokal ishlab chiqish: kalit har ishga tushganda yangilanadi va repozitoriyga tushmaydi
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(MinSigningKeyBytes));
    }
}
