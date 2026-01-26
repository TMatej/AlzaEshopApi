using AlzaEshop.API.Common.Database.Contract;
using Microsoft.EntityFrameworkCore;

namespace AlzaEshop.API.Common.Database.EntityFramework;

/// <summary>
/// Default implementation of repository utilizing entity framework as underlying ORM. 
/// </summary>
/// <typeparam name="TEntity">Type of the entity</typeparam>
public abstract class EFRepository<TEntity>(ProductsDbContext context) : IRepository<TEntity>
    where TEntity : class, IEntity
{
    protected readonly ProductsDbContext _context = context;

    public async Task<TEntity> CreateSingleAsync(TEntity entity, CancellationToken ct)
    {
        _context.Set<TEntity>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task DeleteSingleAsync(TEntity entity, CancellationToken ct)
    {
        _context.Set<TEntity>().Add(entity);
        await _context.SaveChangesAsync(ct);
    }

    public Task<List<TEntity>> GetAllAsync(CancellationToken ct)
    {
        return _context.Set<TEntity>().AsNoTracking().ToListAsync(ct);
    }

    public async Task<TEntity?> GetSingleAsync(Guid id, CancellationToken ct)
    {
        return await _context.Set<TEntity>().FindAsync([id], cancellationToken: ct);
    }

    public async Task UpdateSingleAsync(TEntity entity, CancellationToken ct)
    {
        _context.Set<TEntity>().Update(entity);
        await _context.SaveChangesAsync(ct);
    }
}
