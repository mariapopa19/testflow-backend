using Microsoft.Extensions.DependencyInjection;
using TestFlow.Application.Interfaces;
using TestFlow.Application.Services;

namespace TestFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddInApplication(this IServiceCollection services)
    {
        services.AddScoped<IEndpointIService, EndpointService>();

        // You can register logging, caching, messaging, etc. here too
        return services;
    }
 
}
