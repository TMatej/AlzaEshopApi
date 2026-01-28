using AlzaEshop.API.Common;
using AlzaEshop.API.Common.Endpoints;
using AlzaEshop.API.Common.Responses;
using AlzaEshop.API.Features.Products.Common.Database;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace AlzaEshop.API.Features.Products.v2;

public sealed record PagingRequest
{
    public int PageNumber { get; set; } = 0;
    public int PageSize { get; set; } = 10;
}

public sealed class PagingRequestValidator : AbstractValidator<PagingRequest>
{
    public PagingRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(150);
    }
}

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
            .WithName("Get all products v2")
            .WithDescription("This endpoint allows retrieval of all products.")
            .WithTags("products")
            .Produces<GetProductsResponse>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")
            .MapToApiVersion(2);
    }

    private static async Task<IResult> Handle(
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize,
        IValidator<PagingRequest> validator,
        IProductsRepository productsRepository,
        ILogger<GetProductsEndpoint> logger,
        CancellationToken cancellationToken)
    {
        if (pageNumber is null)
        {
            pageNumber = 0;
        }

        if (pageSize is null)
        {
            pageSize = 10;
        }

        var request = new PagingRequest
        {
            PageNumber = pageNumber.Value,
            PageSize = pageSize.Value
        };

        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var validationRepresentation = validationResult.ToDictionary();
            logger.LogWarning("Request validation did not succeed: {@Request}. Validation result: {@ValidationResult}", request, validationRepresentation);
            return Results.ValidationProblem(validationRepresentation);
        }

        var products = await productsRepository.GetAllAsync(request.PageNumber, request.PageSize, SortOrder.Descending, cancellationToken);

        var productResponses = new GetProductsResponse
        {
            Items = products
            .Select(x => new ProductModel
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
