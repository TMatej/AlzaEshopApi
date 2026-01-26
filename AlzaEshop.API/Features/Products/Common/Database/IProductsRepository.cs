using AlzaEshop.API.Common.Database.Contract;

namespace AlzaEshop.API.Features.Products.Common.Database;

/// <summary>
/// Interface representing products repository with available data layer logic.
/// </summary>
public interface IProductsRepository : IRepository<Product>
{
    /* TODO - specific db queries logic */
}
