using AlzaEshop.API.Common.Database.InMemory;
using AlzaEshop.API.Features.Products.Common.Model;

namespace AlzaEshop.API.Features.Products.Common.Database;

/// <summary>
/// Implementation of products repository utilizing in memory solution.
/// </summary>
public class InMemoryProductsRepository : InMemoryRepository<Product>, IProductsRepository
{
    /* TODO add other (more complex) explicit queries logic */
}
