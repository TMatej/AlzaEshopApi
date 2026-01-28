using AlzaEshop.API.Common;
using AlzaEshop.API.Common.Database.EntityFramework;
using AlzaEshop.API.Features.Products.Common.Model;
using Microsoft.EntityFrameworkCore;

namespace AlzaEshop.API.Features.Products.Common.Database;

/// <summary>
/// Implementation of products repository utilizing entity framework as underlying ORM.
/// </summary>
public class EFProductsRepository : EFRepository<Product>, IProductsRepository
{
    public EFProductsRepository(ProductsDbContext context)
        : base(context)
    { }

    public Task<List<Product>> GetAllAsync(int pageNumber, int pageSize, SortOrder sortOrder, CancellationToken ct)
    {
        IOrderedQueryable<Product> ordered = sortOrder switch
        {
            SortOrder.Descending => Set.OrderByDescending(x => x.CreatedOnUtc),
            SortOrder.Ascending => Set.OrderByDescending(x => x.CreatedOnUtc),
            _ => Set.OrderByDescending(x => x.CreatedOnUtc)
        };

        // db offset based pagination
        return ordered.AsNoTracking()
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        throw new NotImplementedException();
    }

    /* TODO add other (more complex) explicit queries logic */
}
