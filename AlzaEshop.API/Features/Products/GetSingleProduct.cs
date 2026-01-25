using AlzaEshop.API.Common.Database.Contract;
using AlzaEshop.API.Common.Endpoints;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace AlzaEshop.API.Features.Products;

public sealed record GetSingleProductQuery(Guid Id);

public class GetSingleProductQueryValidator : AbstractValidator<GetSingleProductQuery>
{
    public GetSingleProductQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}

public sealed record GetProductResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
}

public class GetSingleProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{productId:guid}", Handle);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid productId,
        IValidator<GetSingleProductQuery> validator,
        IDatabaseContext dbContext,
        ILogger<GetSingleProductQuery> logger,
        CancellationToken cancellationToken)
    {
        var query = new GetSingleProductQuery(productId);

        var validationResult = await validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            var validationRepresentation = validationResult.ToDictionary();
            logger.LogWarning("Request validation did not succeed: {@Request}. Validation result: {@ValidationResult}", query, validationRepresentation);
            return Results.ValidationProblem(validationRepresentation);
        }

        var product = await dbContext.Products.GetSingleAsync(query.Id, cancellationToken);
        if (product is null)
        {
            return Results.NotFound();
        }

        var productResponse = new GetProductResponse
        {
            Id = product.Id,
            Title = product.Title,
            ImageUrl = product.ImageUrl,
            Price = product.Price,
            Description = product.Description,
            Quantity = product.Quantity,
            CreatedAt = product.CreatedAt
        };

        return Results.Ok(productResponse);
    }
}
