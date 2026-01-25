using AlzaEshop.API.Common.Database.Contract;

/// <summary>
/// Initial representation of a product.
/// </summary>
/// <param name="title"></param>
/// <param name="imageUrl"></param>
public class Product(string title, string? imageUrl) : IEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = title;
    public string? ImageUrl { get; set; } = imageUrl;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; } // we do not expect the number of products to exceed the int size
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
