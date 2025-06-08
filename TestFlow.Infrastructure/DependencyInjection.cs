using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestFlow.Application.Interfaces.Repository;
using TestFlow.Infrastructure.Repositories;

namespace TestFlow.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
        services.AddScoped<IEndpointRepository, EndpointRepository>();
        services.AddScoped<ITestRunRepository, TestRunRepository>();
        services.AddScoped<ITestResultRepository, TestResultRepository>();
        services.AddScoped<ITestCaseRepository, TestCaseRepository>();

        return services;
    }
}
