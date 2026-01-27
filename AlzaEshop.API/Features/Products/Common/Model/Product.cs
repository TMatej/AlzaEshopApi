using AlzaEshop.API.Common.Database.Contract;

namespace AlzaEshop.API.Features.Products.Common.Model;


/// <summary>
/// Initial representation of a product.
/// </summary>
/// <param name="title"></param>
/// <param name="imageUrl"></param>
public class Product(string title, string imageUrl, decimal? price = null, int? quantity = null) : IEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = title;
    public string ImageUrl { get; set; } = imageUrl;
    public decimal Price { get; set; } = price ?? 0;
    public string? Description { get; set; }
    public int Quantity { get; set; } = quantity ?? 0; // we do not expect the number of products to exceed the int size
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}
