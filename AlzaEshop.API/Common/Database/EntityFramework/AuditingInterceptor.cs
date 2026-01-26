using AlzaEshop.API.Common.Database.Contract;
using AlzaEshop.API.Common.Services.EntityIdProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AlzaEshop.API.Common.Database.EntityFramework;

/// <summary>
/// Interceptor implementation to ensure that properties of created and modified entities
/// are correctly filled in.
/// </summary>
public class AuditingInterceptor : SaveChangesInterceptor
{
    private readonly IEntityIdProvider _idProvider;
    private readonly TimeProvider _timeProvider;

    public AuditingInterceptor(IEntityIdProvider provider, TimeProvider timeProvider)
    {
        _idProvider = provider;
        _timeProvider = timeProvider;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            UpdateAuditableEntities(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditableEntities(DbContext context)
    {
        DateTimeOffset utcNow = _timeProvider.GetUtcNow();
        var entities = context.ChangeTracker.Entries<IEntity>()
            .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified)
            .ToList();

        foreach (var entry in entities)
        {
            if (entry.State == EntityState.Added)
            {
                // ensure that id is created
                var idProperty = entry.Property(nameof(IEntity.Id));
                if (idProperty.CurrentValue is null || (Guid)idProperty.CurrentValue == Guid.Empty)
                {
                    idProperty.CurrentValue = _idProvider.CreateNewId();
                }

                SetCurrentPropertyValue(entry, nameof(IEntity.CreatedOnUtc), utcNow);
            }

            if (entry.State == EntityState.Modified)
            {
                SetCurrentPropertyValue(entry, nameof(IEntity.ModifiedOnUtc), utcNow);
            }

        }

        static void SetCurrentPropertyValue(
            EntityEntry entry,
            string propertyName,
            DateTimeOffset utcNow) =>
                entry.Property(propertyName).CurrentValue = utcNow;
    }
}
