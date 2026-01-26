using AlzaEshop.API.Features.Products.Common.Database;

namespace AlzaEshop.API.Common.Database.InMemory;

public static class InMemoryDependencyRegistration
{
    public static void AddInMemoryDatabase(this IServiceCollection services)
    {
        services.AddSingleton<IDatabaseContext, InMemoryDatabaseContext>();
        services.AddScoped<IProductsRepository, InMemoryProductsRepository>();
    }
}
