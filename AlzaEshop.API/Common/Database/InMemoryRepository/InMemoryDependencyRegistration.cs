using AlzaEshop.API.Common.Database.Contract;

namespace AlzaEshop.API.Common.Database.InMemoryRepository;

public static class InMemoryDependencyRegistration
{
    public static void AddInMemoryDatabase(this IServiceCollection services)
    {
        services.AddSingleton<IDatabaseContext, InMemoryDatabaseContext>();
        services.AddSingleton<IRepository<Product>, InMemoryRepository<Product>>();
    }
}
