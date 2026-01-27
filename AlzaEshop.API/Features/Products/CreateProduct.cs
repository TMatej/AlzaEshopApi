using AlzaEshop.API.Common.Database.Contract;
using AlzaEshop.API.Common.Endpoints;
using AlzaEshop.API.Common.Services.EntityIdProvider;
using AlzaEshop.API.Features.Products.Common.Database;
using AlzaEshop.API.Features.Products.Common.Model;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace AlzaEshop.API.Features.Products;

// for now represents both Request and Command objects
public sealed record CreateProductRequest(
    string Title,
    string ImageUrl,
    decimal? Price,
    string? Description,
    int? Quantity);

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(Constraints.Products.TitleLength);

        RuleFor(x => x.ImageUrl)
            .NotEmpty()
            .MaximumLength(Constraints.Products.ImageUrlLength);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0)
                .When(x => x is not null);

        RuleFor(x => x.Description)
            .MaximumLength(Constraints.Products.DescriptionLength)
                .When(x => x is not null);
    }
}

public sealed record CreateProductResponse(
    Guid Id,
    string Title,
    string ImageUrl,
    decimal Price,
    string? Description,
    int Quantity,
    DateTimeOffset CreatedAt);

public class CreateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/products", Handle);
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateProductRequest request,
        IValidator<CreateProductRequest> validator,
        IProductsRepository productsRepository,
        IEntityIdProvider idProvider,
        ILogger<CreateProductEndpoint> logger,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var validationRepresentation = validationResult.ToDictionary();
            logger.LogWarning("Request validation did not succeed: {@Request}. Validation result: {@ValidationResult}", request, validationRepresentation);
            return Results.ValidationProblem(validationRepresentation);
        }

        var id = idProvider.CreateNewId();
        var product = new Product(request.Title, request.ImageUrl, request.Price, request.Quantity)
        {
            Id = id,
            Description = request.Description,
        };

        product = await productsRepository.CreateSingleAsync(product, cancellationToken);

        logger.LogInformation("Created product: {@Product}", product);

        var productResponse = new CreateProductResponse
        (product.Id, product.Title, product.ImageUrl, product.Price, product.Description, product.Quantity, product.CreatedOnUtc);

        return Results.Ok(productResponse);
    }
}
