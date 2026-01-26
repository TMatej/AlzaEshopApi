using AlzaEshop.API.Common.Database.Contract;

namespace AlzaEshop.API.Common.Database.InMemory;

/// <summary>
/// In-memory repository implementation using a dictionary.
/// </summary>
/// <typeparam name="TEntity">Type of the entity</typeparam>
public abstract class InMemoryRepository<TEntity> : IRepository<TEntity>
    where TEntity : IEntity
{
    private readonly Dictionary<Guid, TEntity> _data;

    /// <summary>
    /// Initialize a new in-memory repository.
    /// </summary>
    public InMemoryRepository()
    {
        _data = [];
    }

    public Task<List<TEntity>> GetAllAsync(CancellationToken ct)
    {
        var result = _data.Values.ToList();
        return Task.FromResult(result);
    }

    public Task<TEntity?> GetSingleAsync(Guid id, CancellationToken ct)
    {
        var result = _data.TryGetValue(id, out var entity) ? entity : default;
        return Task.FromResult(result);
    }

    public Task<TEntity> CreateSingleAsync(TEntity entity, CancellationToken ct)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        var key = Guid.NewGuid();
        entity.Id = key;
        entity.CreatedAt = TimeProvider.System.GetUtcNow();
        _data[key] = entity;
        return Task.FromResult(entity);
    }

    public Task UpdateSingleAsync(TEntity entity, CancellationToken ct)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        _data[entity.Id] = entity;

        return Task.CompletedTask;
    }

    public Task DeleteSingleAsync(TEntity entity, CancellationToken ct)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        _data.Remove(entity.Id);

        return Task.CompletedTask;
    }
}
