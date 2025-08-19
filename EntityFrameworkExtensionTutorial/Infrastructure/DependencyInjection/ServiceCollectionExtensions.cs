using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EntityFrameworkExtensionTutorial.Application.Interfaces;
using EntityFrameworkExtensionTutorial.Application.Services;
using EntityFrameworkExtensionTutorial.Domain.Interfaces;
using EntityFrameworkExtensionTutorial.Infrastructure.Data;
using EntityFrameworkExtensionTutorial.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Scrutor;

namespace EntityFrameworkExtensionTutorial.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
            ));

        // Add Repositories using Scrutor for automatic registration
        services.Scan(scan => scan
            .FromAssemblyOf<Repository<object>>()
            .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // Add specific repositories
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IBulkRepository, BulkRepository>();
        
        // Add logging for repositories
        services.AddLogging();

        // Add Services using Scrutor for automatic registration
        services.Scan(scan => scan
            .FromAssemblyOf<CustomerService>()
            .AddClasses(classes => classes.AssignableTo(typeof(ICustomerService)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // Add AutoMapper
        services.AddAutoMapper(typeof(CustomerService).Assembly);

        return services;
    }
}
