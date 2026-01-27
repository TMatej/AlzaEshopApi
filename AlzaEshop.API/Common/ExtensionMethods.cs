using AlzaEshop.API.Common.Database.EntityFramework;
using AlzaEshop.API.Common.Database.InMemory;
using AlzaEshop.API.Common.Services.EntityIdProvider;
using Microsoft.EntityFrameworkCore;

namespace AlzaEshop.API.Common;

public static class ExtensionMethods
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration, bool useInMemory = false)
    {
        if (useInMemory)
        {
            // in memory databse services
            services.AddInMemoryDatabase();
        }
        else
        {
            // interceptors
            services.AddSingleton<AuditingInterceptor>();

            // db context
            services.AddDbContext<ProductsDbContext>((sp, options) =>
            {
                options.UseSqlServer(configuration.GetConnectionString("ProductsConnection"))
                    .AddInterceptors(
                        sp.GetRequiredService<AuditingInterceptor>());
            });

            // ef database services
            services.AddEfDatabase();
        }

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IEntityIdProvider, DefaultEntityIdProvider>();

        return services;
    }
}
