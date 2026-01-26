using AlzaEshop.API.Features.Products.Common.Database;

namespace AlzaEshop.API.Common.Database.EntityFramework;

public static class EFDatabaseRegistration
{
    public static void AddEfDatabase(this IServiceCollection services)
    {
        services.AddScoped<IProductsRepository, EFProductsRepository>();
    }
}
