using Microsoft.Extensions.DependencyInjection;
using TestFlow.Application.Interfaces.Services;
using TestFlow.Application.Services;

namespace TestFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddInApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IEndpointIService, EndpointService>();
        services.AddScoped<ITestCaseGeneratorService, TestCaseGeneratorService>();
        services.AddScoped<IAIClientService, AIClientService>();
        services.AddScoped<ITestReportService, TestReportService>();

        // You can register logging, caching, messaging, etc. here too
        return services;
    }
 
}
