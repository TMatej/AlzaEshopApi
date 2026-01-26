using System.Reflection;
using AlzaEshop.API.Features.Products.Common.Model;
using Microsoft.EntityFrameworkCore;

namespace AlzaEshop.API.Common.Database.EntityFramework;

public class ProductsDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public ProductsDbContext(DbContextOptions options) : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
