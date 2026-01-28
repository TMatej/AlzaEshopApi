using AlzaEshop.API.Common;
using AlzaEshop.API.Common.Database.Contract;
using AlzaEshop.API.Features.Products.Common.Model;

namespace AlzaEshop.API.Features.Products.Common.Database;

/// <summary>
/// Interface representing products repository with available data layer logic.
/// </summary>
public interface IProductsRepository : IRepository<Product>
{
    /// <summary>
    /// Retrieves products using pagination properties.
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="sortOrder"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<List<Product>> GetAllAsync(int pageNumber, int pageSize, SortOrder sortOrder, CancellationToken ct);

    /* TODO - specific db queries logic */
}
