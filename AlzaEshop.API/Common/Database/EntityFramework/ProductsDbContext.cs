using AlzaEshop.API.Common.Database.Contract;
using Microsoft.EntityFrameworkCore;

namespace AlzaEshop.API.Common.Database.EntityFramework;

public class ProductsDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public ProductsDbContext(DbContextOptions options) : base(options)
    { }

    public override int SaveChanges()
    {
        ModifyEnitities();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct)
    {
        ModifyEnitities();
        return base.SaveChangesAsync(ct);
    }

    private void ModifyEnitities()
    {
        var entries = ChangeTracker.Entries()
            .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified)
            .ToList();

        foreach (var entry in entries)
        {
            if (entry is IEntity entity)
            {
                var now = TimeProvider.System.GetUtcNow();

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = now;
                }
                entity.UpdatedAt = now;
            }
        }
    }
}
