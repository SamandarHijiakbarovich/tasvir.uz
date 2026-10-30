using Microsoft.Extensions.DependencyInjection;
using Tasvir.Modules.Content.Application.Landing;

namespace Tasvir.Modules.Content.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddContentApplication(this IServiceCollection services)
    {
        services.AddScoped<GetLandingContentHandler>();
        return services;
    }
}
