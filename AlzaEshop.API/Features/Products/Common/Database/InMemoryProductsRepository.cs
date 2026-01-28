using AlzaEshop.API.Common;
using AlzaEshop.API.Common.Database.InMemory;
using AlzaEshop.API.Common.Services.EntityIdProvider;
using AlzaEshop.API.Features.Products.Common.Model;

namespace AlzaEshop.API.Features.Products.Common.Database;

/// <summary>
/// Implementation of products repository utilizing in memory solution.
/// </summary>
public class InMemoryProductsRepository : InMemoryRepository<Product>, IProductsRepository
{
    public InMemoryProductsRepository(IEntityIdProvider idProvider, TimeProvider timeProvider)
        : base(idProvider, timeProvider)
    { }

    public Task<List<Product>> GetAllAsync(int pageNumber, int pageSize, SortOrder sortOrder, CancellationToken ct)
    {
        var result = Data.Values
            .OrderBy(x => x.CreatedOnUtc)
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .ToList();
        return Task.FromResult(result);
    }

    /* TODO add other (more complex) explicit queries logic */
}
