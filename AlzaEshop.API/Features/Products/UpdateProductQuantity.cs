using AlzaEshop.API.Common.Endpoints;
using AlzaEshop.API.Features.Products.Common.Database;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace AlzaEshop.API.Features.Products;

public sealed record UpdateProductQuantityRequest
{
    public int? Quantity { get; set; }
}

public sealed record UpdateProductQuantityQuery
{
    public Guid Id { get; set; }
    public int? Quantity { get; set; }
}

public sealed class UpdateProductQuantityQueryValidator : AbstractValidator<UpdateProductQuantityQuery>
{
    public UpdateProductQuantityQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .NotNull()
            .GreaterThanOrEqualTo(0);
    }
}

public class UpdateProductQuantityEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/products/{productId:guid}/quantity", Handle);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid productId,
        [FromBody] UpdateProductQuantityRequest request,
        IValidator<UpdateProductQuantityQuery> validator,
        IProductsRepository productsRepository,
        ILogger<UpdateProductQuantityEndpoint> logger,
        CancellationToken cancellationToken)
    {
        var query = new UpdateProductQuantityQuery
        {
            Id = productId,
            Quantity = request.Quantity
        };

        var validationResult = await validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            var validationRepresentation = validationResult.ToDictionary();
            logger.LogWarning("Request validation did not succeed: {@Request}. Validation result: {@ValidationResult}", request, validationRepresentation);
            return Results.ValidationProblem(validationRepresentation);
        }

        var product = await productsRepository.GetSingleAsync(productId, cancellationToken);
        if (product is null)
        {
            return Results.NotFound(
                new ProblemDetails
                {
                    Title = "Product not found",
                    Detail = $"Product with ID {productId} was not found",
                    Status = StatusCodes.Status404NotFound
                });
        }

        var originalQuantity = product.Quantity;

        product.Quantity = request.Quantity!.Value;
        await productsRepository.UpdateSingleAsync(product, cancellationToken);

        logger.LogInformation("Product quantity was updated from {OriginalProductQuantity} to {NewProductQuantity}", originalQuantity, product.Quantity);

        return Results.Ok();
    }
}
