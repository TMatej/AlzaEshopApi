using AlzaEshop.API.Common.Database.Contract;

namespace AlzaEshop.API.Features.Products.Common.Model;


/// <summary>
/// Initial representation of a product.
/// </summary>
public class Product : IEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; }
    public string ImageUrl { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; } // we do not expect the number of products to exceed the int size
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }

    /// <param name="title"></param>
    /// <param name="imageUrl"></param>
    public Product(string title, string imageUrl, decimal? price = null, int? quantity = null)
    {
        Title = title;
        ImageUrl = imageUrl;
        Price = price ?? 0;
        Quantity = quantity ?? 0;
    }

    private Product()
    {
        Title = string.Empty;
        ImageUrl = string.Empty;
    }
}