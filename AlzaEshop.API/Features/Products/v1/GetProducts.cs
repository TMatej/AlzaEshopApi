using AlzaEshop.API.Common.Endpoints;
using AlzaEshop.API.Common.Responses;
using AlzaEshop.API.Features.Products.Common.Database;
using Microsoft.AspNetCore.Mvc;

namespace AlzaEshop.API.Features.Products.v1;

public sealed record ProductModel()
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
}

public sealed record GetProductsResponse : CollectionResponse<ProductModel>
{ };

public class GetProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", Handle)
            .WithName("Get all products")
            .WithDescription("This endpoint allows retrieval of all products.")
            .WithTags("products")
            .Produces<GetProductsResponse>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        IProductsRepository productsRepository,
        ILogger<GetProductsEndpoint> logger,
        CancellationToken cancellationToken)
    {
        var products = await productsRepository.GetAllAsync(cancellationToken);

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
                CreatedOnUtc = x.CreatedOnUtc
            }).ToList()
        };

        return Results.Ok(productResponses);
    }
}
