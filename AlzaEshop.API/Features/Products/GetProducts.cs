using AlzaEshop.API.Common.Database.Contract;
using AlzaEshop.API.Common.Endpoints;

namespace AlzaEshop.API.Features.Products;

public record CollectionResponse<TEntity>
{
    public IList<TEntity> Items { get; set; } = new List<TEntity>();
    public int Count => Items.Count;
}

public record ProductModel()
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
}

public record GetProductsResponse : CollectionResponse<ProductModel>
{ };

public class GetProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.Map("/products", Handle);
    }

    private async Task<IResult> Handle(
        IDatabaseContext dbContext,
        ILogger<GetProductsEndpoint> logger,
        CancellationToken cancellationToken)
    {
        var products = await dbContext.Products.GetAllAsync();

        var productResponses = new GetProductsResponse
        {
            Items = products.Select(x => new ProductModel
            {
                Id = x.Id,
                Title = x.Title,
                ImageUrl = x.ImageUrl,
                Price = x.Price,
                Description = x.Description,
                Quantity = x.Quantity,
                CreatedAt = x.CreatedAt
            }).ToList()
        };

        return Results.Ok(productResponses);
    }
}
