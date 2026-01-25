using AlzaEshop.API.Common.Database.Contract;
using Microsoft.EntityFrameworkCore;

namespace AlzaEshop.API.Common.Database.EntityFramework;

public class ProductsRepository : IRepository<Product>
{
    private ProductsDbContext _context;

    public ProductsRepository(ProductsDbContext context)
    {
        _context = context;
    }

    public async Task<Product> CreateSingleAsync(Product entity, CancellationToken ct)
    {
        _context.Set<Product>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task DeleteSingleAsync(Product entity, CancellationToken ct)
    {
        _context.Set<Product>().Add(entity);
        await _context.SaveChangesAsync(ct);
    }

    public Task<List<Product>> GetAllAsync(CancellationToken ct)
    {
        return _context.Set<Product>().AsNoTracking().ToListAsync(ct);
    }

    public async Task<Product?> GetSingleAsync(Guid id, CancellationToken ct)
    {
        return await _context.Set<Product>().FindAsync([id], cancellationToken: ct);
    }

    public async Task UpdateSingleAsync(Product entity, CancellationToken ct)
    {
        _context.Set<Product>().Update(entity);
        await _context.SaveChangesAsync(ct);
    }
}
