namespace AlzaEshop.API.Common.Database.Contract;

/// <summary>
/// Generic repository abstraction with common CRUD async methods.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IRepository<TEntity>
    where TEntity : IEntity
{
    Task<IList<TEntity>> GetAllAsync();
    Task<TEntity?> GetSingleAsync(Guid id);
    Task<TEntity> CreateSingleAsync(TEntity entity);
    Task UpdateSingleAsync(TEntity entity);
    Task DeleteSingleAsync(TEntity entity);
}
