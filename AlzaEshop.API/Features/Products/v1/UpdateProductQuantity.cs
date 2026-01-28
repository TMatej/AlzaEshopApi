using AlzaEshop.API.Common.Endpoints;
using AlzaEshop.API.Features.Products.Common.Database;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace AlzaEshop.API.Features.Products.v1;

public sealed record UpdateProductQuantityRequest
{
    public required int Quantity { get; set; }
}

public sealed record UpdateProductQuantityCommand
{
    public Guid Id { get; set; }
    public int Quantity { get; set; }
}

public sealed class UpdateProductQuantityCommandValidator : AbstractValidator<UpdateProductQuantityCommand>
{
    public UpdateProductQuantityCommandValidator()
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
        app.MapPut("/products/{productId:guid}/quantity", Handle)
            .WithName("Update Product Quantity")
            .WithDescription("This endpoint allows update of the product quantity.")
            .WithTags("products")
            .Accepts<UpdateProductQuantityRequest>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid productId,
        [FromBody] UpdateProductQuantityRequest request,
        IValidator<UpdateProductQuantityCommand> validator,
        IProductsRepository productsRepository,
        ILogger<UpdateProductQuantityEndpoint> logger,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductQuantityCommand
        {
            Id = productId,
            Quantity = request.Quantity
        };

        var validationResult = await validator.ValidateAsync(command, cancellationToken);
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

        product.Quantity = request.Quantity;
        await productsRepository.UpdateSingleAsync(product, cancellationToken);

        logger.LogInformation("Product quantity was updated from {OriginalProductQuantity} to {NewProductQuantity}", originalQuantity, product.Quantity);

        return Results.Ok();
    }
}
