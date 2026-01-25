using AlzaEshop.API.Common.Database.EntityFramework;
using AlzaEshop.API.Common.Database.InMemory;
using Microsoft.EntityFrameworkCore;

namespace AlzaEshop.API.Common;

public static class ExtensionMethods
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration, bool useInMemory = false)
    {
        if (useInMemory)
        {
            services.AddInMemoryDatabase();
        }
        else
        {
            services.AddDbContext<ProductsDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("ProductsDatabase"));
            });
        }

        return services;
    }
}
