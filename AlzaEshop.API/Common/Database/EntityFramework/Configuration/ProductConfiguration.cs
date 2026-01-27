using AlzaEshop.API.Features.Products.Common.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlzaEshop.API.Common.Database.EntityFramework.Configuration;

/// <summary>
/// Entity configuration usable for code first approach.
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Primary Key
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()") // Optimized for SQL Server indexing
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(Constraints.Products.TitleLength)
            .IsRequired();

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(Constraints.Products.ImageUrlLength)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(Constraints.Products.DescriptionLength)
            .IsRequired(false);

        builder.Property(x => x.Quantity)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.CreatedOnUtc)
            .HasColumnType("datetimeoffset")
            .HasDefaultValueSql("SYSDATETIMEOFFSET()")
            .IsRequired();

        builder.Property(x => x.ModifiedOnUtc)
            .HasColumnType("datetimeoffset")
            .IsRequired(false);

        builder.ToTable("Products");
    }
}
