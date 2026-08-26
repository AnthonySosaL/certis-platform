using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NutriBoost.Client.Application.Products;
using NutriBoost.Client.Infrastructure.Persistence;

namespace NutriBoost.Client.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("NutriBoostDb")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:NutriBoostDb.");

        services.AddDbContext<NutriBoostDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}
