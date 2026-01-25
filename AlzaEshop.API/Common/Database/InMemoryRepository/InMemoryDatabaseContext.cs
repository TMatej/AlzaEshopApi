using AlzaEshop.API.Common.Database.Contract;

namespace AlzaEshop.API.Common.Database.InMemoryRepository;

public class InMemoryDatabaseContext : IDatabaseContext
{
    public InMemoryDatabaseContext(IRepository<Product> products)
    {
        Products = products;
    }

    public IRepository<Product> Products { get; set; }
}
