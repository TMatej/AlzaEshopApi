using AlzaEshop.API.Common.Database.EntityFramework;
using AlzaEshop.API.Features.Products.Common.Model;

namespace AlzaEshop.API.Features.Products.Common.Database;

/// <summary>
/// Implementation of products repository utilizing entity framework as underlying ORM.
/// </summary>
public class EFProductsRepository : EFRepository<Product>, IProductsRepository
{
    public EFProductsRepository(ProductsDbContext context)
        : base(context)
    { }

    /* TODO add other (more complex) explicit queries logic */
}
