using AlzaEshop.API.Common.Database.EntityFramework;
using AlzaEshop.API.Common.Database.InMemory;
using AlzaEshop.API.Common.Services.EntityIdProvider;
using Asp.Versioning;
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
        services.AddSingleton<IEntityIdProvider, DefaultEntityIdProvider>();

        return services;
    }

    public static IServiceCollection AddEndpointsVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1);
            // in case that the api is already in production a QueryStringApiVersionReader or HeaderApiVersionReader
            // would be more appropriate as no breaking change is introduced to uri (in case it was not planned from the start)
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'V";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}
