using Microsoft.Extensions.DependencyInjection;

namespace Tasvir.Modules.Identity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        return services;
    }
}
