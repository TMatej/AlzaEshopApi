namespace AlzaEshop.API.Common.Database.Contract;

/// <summary>
/// Generic repository abstraction with common CRUD async methods.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IRepository<TEntity>
    where TEntity : IEntity
{
    Task<List<TEntity>> GetAllAsync(CancellationToken ct);
    Task<TEntity?> GetSingleAsync(Guid id, CancellationToken ct);
    Task<TEntity> CreateSingleAsync(TEntity entity, CancellationToken ct);
    Task UpdateSingleAsync(TEntity entity, CancellationToken ct);
    Task DeleteSingleAsync(TEntity entity, CancellationToken ct);
}
