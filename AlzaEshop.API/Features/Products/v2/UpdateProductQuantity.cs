using System.Threading.Channels;
using AlzaEshop.API.Common.Endpoints;
using AlzaEshop.API.Features.Products.Common.Database;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace AlzaEshop.API.Features.Products.v2;

public sealed record UpdateProductQuantityRequest
{
    public required int Quantity { get; set; }
}

public sealed record UpdateProductQuantityCommand
{
    public Guid Id { get; set; }
    public int Quantity { get; set; }
}

public sealed class UpdateProductQuantityQueryValidator : AbstractValidator<UpdateProductQuantityCommand>
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
        app.MapPut("/products/{productId:guid}/quantity", Handle)
            .WithName("Update Product Quantity v2")
            .WithDescription("This endpoint allows async update of the product quantity.")
            .WithTags("products")
            .Accepts<UpdateProductQuantityRequest>("application/json")
            .Produces(StatusCodes.Status202Accepted)
            .MapToApiVersion(2);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid productId,
        [FromBody] UpdateProductQuantityRequest request,
        ProductUpdateQueue queue,
        ILogger<UpdateProductQuantityEndpoint> logger,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductQuantityCommand
        {
            Id = productId,
            Quantity = request.Quantity
        };

        await queue.EnqueueAsync(command, cancellationToken);

        return Results.Accepted();
    }
}

public class ProductServices
{
    private readonly IValidator<UpdateProductQuantityCommand> _validator;
    private readonly ILogger<ProductServices> _logger;
    private readonly IProductsRepository _productsRepository;

    public ProductServices(IValidator<UpdateProductQuantityCommand> validator, ILogger<ProductServices> logger, IProductsRepository productsRepository)
    {
        _validator = validator;
        _logger = logger;
        _productsRepository = productsRepository;
    }

    public async Task Update(UpdateProductQuantityCommand command, CancellationToken ct)
    {
        var validationResult = await _validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            // report the failure
            var validationRepresentation = validationResult.ToDictionary();
            _logger.LogWarning("Command validation did not succeed: {@Command}. Validation result: {@ValidationResult}", command, validationRepresentation);
            return;
        }

        var product = await _productsRepository.GetSingleAsync(command.Id, ct);
        if (product is null)
        {
            // report the failure
            _logger.LogWarning("Product with ID {command.Id} was not found", command.Id);
            return;
        }

        var originalQuantity = product.Quantity;

        product.Quantity = command.Quantity;
        await _productsRepository.UpdateSingleAsync(product, ct);

        _logger.LogInformation("Product quantity was updated from {OriginalProductQuantity} to {NewProductQuantity}", originalQuantity, product.Quantity);
    }
}

public class ProductUpdateQueue
{
    protected readonly Channel<UpdateProductQuantityCommand> _queue;

    public ProductUpdateQueue(int capacity = 100)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<UpdateProductQuantityCommand>(options);
    }

    public ProductUpdateQueue(BoundedChannelOptions options)
    {
        _queue = Channel.CreateBounded<UpdateProductQuantityCommand>(options);
    }

    public async ValueTask EnqueueAsync(UpdateProductQuantityCommand job, CancellationToken ct = default)
    {
        await _queue.Writer.WriteAsync(job, ct);
    }

    public async ValueTask<UpdateProductQuantityCommand> DequeueAsync(CancellationToken ct = default)
    {
        return await _queue.Reader.ReadAsync(ct);
    }
}

public class ProductUpdateBackgroundService : BackgroundService
{
    private readonly ProductUpdateQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProductUpdateBackgroundService> _logger;

    public ProductUpdateBackgroundService(ProductUpdateQueue queue, IServiceProvider serviceProvider, ILogger<ProductUpdateBackgroundService> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background task {BackgroundServiceName} started.", nameof(ProductUpdateBackgroundService));

        // use app cancellation token to gracefully shut down if requested (CTRL + C) -> no bound to HTTP cancellation
        while (!stoppingToken.IsCancellationRequested)
        {
            UpdateProductQuantityCommand? item = null;
            try
            {
                // get the next job in the queue
                item = await _queue.DequeueAsync(stoppingToken);
                _logger.LogInformation("Processing job of task {BackgroundServiceName} with item {@Item}", nameof(ProductUpdateBackgroundService), item);

                var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<ProductServices>();
                await service.Update(item, stoppingToken);

                _logger.LogInformation("Job of task {BackgroundServiceName} with item {@Item} was processed successfully.", nameof(ProductUpdateBackgroundService), item);
            }
            catch (OperationCanceledException)
            {
                // triggered by stoppingToken
                _logger.LogInformation("Processing job of task {BackgroundServiceName} with item {@Item} was cancelled.", nameof(ProductUpdateBackgroundService), item);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing background task {BackgroundServiceName}.", nameof(ProductUpdateBackgroundService));
            }
        }

        _logger.LogInformation("Background task {BackgroundServiceName} stopped.", nameof(ProductUpdateBackgroundService));
    }
}