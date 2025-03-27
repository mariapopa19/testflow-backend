using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestFlow.Application.Intefaces;
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

        services.AddScoped<IEndpointRepository, EndpointRepository>();

        // You can register logging, caching, messaging, etc. here too
        return services;
    }
}
