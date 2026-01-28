using System.Net;
using System.Net.Http.Json;
using AlzaEshop.API.Common.Services.EntityIdProvider;
using AlzaEshop.API.Features.Products.Common.Database;
using AlzaEshop.API.Features.Products.Common.Model;
using AlzaEshop.API.Features.Products.v1;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;

public class UpdateProductQuantityEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    // use WebApplicationFactory as the tested logic is part of the endpoint handler
    // instead of separte services (due to simple logic)
    public UpdateProductQuantityEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UpdateProductQuantity_ValidRequest_ReturnsOk()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero));
        var mockIdProvider = new Mock<IEntityIdProvider>();
        var productId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        mockIdProvider.Setup(p => p.CreateNewId()).Returns(productId);

        var existingProduct = new Product(
            "Test Product",
            "https://example.com/image.jpg",
            price: 99.99m,
            quantity: 10)
        {
            Id = productId,
            CreatedOnUtc = timeProvider.GetUtcNow().AddDays(-1)
        };

        var mockRepository = new Mock<IProductsRepository>();
        mockRepository
            .Setup(r => r.GetSingleAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);
        mockRepository
            .Setup(r => r.UpdateSingleAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<UpdateProductQuantityEndpoint>>();

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IProductsRepository>();
                services.RemoveAll<ILogger<UpdateProductQuantityEndpoint>>();
                services.RemoveAll<IValidator<UpdateProductQuantityQuery>>();
                services.RemoveAll<IEntityIdProvider>();
                services.RemoveAll<TimeProvider>();

                services.AddSingleton(mockRepository.Object);
                services.AddSingleton(mockLogger.Object);
                services.AddSingleton<IValidator<UpdateProductQuantityQuery>>(
                    new UpdateProductQuantityQueryValidator());
                services.AddSingleton(mockIdProvider.Object);
                services.AddSingleton<TimeProvider>(timeProvider);
            });
        }).CreateClient();

        var request = new UpdateProductQuantityRequest { Quantity = 50 };

        // Act
        var response = await client.PutAsJsonAsync($"api/v1/products/{productId}/quantity", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify repository calls
        mockRepository.Verify(
            r => r.GetSingleAsync(productId, It.IsAny<CancellationToken>()),
            Times.Once);
        mockRepository.Verify(
            r => r.UpdateSingleAsync(
                It.Is<Product>(p => p.Id == productId && p.Quantity == 50),
                It.IsAny<CancellationToken>()),
            Times.Once);

        // Verify logging
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Product quantity was updated")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateProductQuantity_ProductNotFound_ReturnsNotFound()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var productId = Guid.NewGuid();

        var mockRepository = new Mock<IProductsRepository>();
        mockRepository
            .Setup(r => r.GetSingleAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var mockLogger = new Mock<ILogger<UpdateProductQuantityEndpoint>>();

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IProductsRepository>();
                services.RemoveAll<ILogger<UpdateProductQuantityEndpoint>>();
                services.RemoveAll<IValidator<UpdateProductQuantityQuery>>();
                services.RemoveAll<TimeProvider>();

                services.AddSingleton(mockRepository.Object);
                services.AddSingleton(mockLogger.Object);
                services.AddSingleton<IValidator<UpdateProductQuantityQuery>>(
                    new UpdateProductQuantityQueryValidator());
                services.AddSingleton<TimeProvider>(timeProvider);
            });
        }).CreateClient();

        var request = new UpdateProductQuantityRequest { Quantity = 50 };

        // Act
        var response = await client.PutAsJsonAsync($"api/v1/products/{productId}/quantity", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal("Product not found", problemDetails.Title);
        Assert.Contains(productId.ToString(), problemDetails.Detail);

        // Verify repository calls
        mockRepository.Verify(
            r => r.GetSingleAsync(productId, It.IsAny<CancellationToken>()),
            Times.Once);
        mockRepository.Verify(
            r => r.UpdateSingleAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(-999)]
    public async Task UpdateProductQuantity_NegativeQuantity_ReturnsBadRequest(int invalidQuantity)
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var productId = Guid.NewGuid();

        var mockRepository = new Mock<IProductsRepository>();
        var mockLogger = new Mock<ILogger<UpdateProductQuantityEndpoint>>();

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IProductsRepository>();
                services.RemoveAll<ILogger<UpdateProductQuantityEndpoint>>();
                services.RemoveAll<IValidator<UpdateProductQuantityQuery>>();
                services.RemoveAll<TimeProvider>();

                services.AddSingleton(mockRepository.Object);
                services.AddSingleton(mockLogger.Object);
                services.AddSingleton<IValidator<UpdateProductQuantityQuery>>(
                    new UpdateProductQuantityQueryValidator());
                services.AddSingleton<TimeProvider>(timeProvider);
            });
        }).CreateClient();

        var request = new UpdateProductQuantityRequest { Quantity = invalidQuantity };

        // Act
        var response = await client.PutAsJsonAsync($"api/v1/products/{productId}/quantity", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Verify repository was never called
        mockRepository.Verify(
            r => r.GetSingleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        mockRepository.Verify(
            r => r.UpdateSingleAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateProductQuantity_EmptyGuid_ReturnsBadRequest()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);

        var mockRepository = new Mock<IProductsRepository>();
        var mockLogger = new Mock<ILogger<UpdateProductQuantityEndpoint>>();

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IProductsRepository>();
                services.RemoveAll<ILogger<UpdateProductQuantityEndpoint>>();
                services.RemoveAll<IValidator<UpdateProductQuantityQuery>>();
                services.RemoveAll<TimeProvider>();

                services.AddSingleton(mockRepository.Object);
                services.AddSingleton(mockLogger.Object);
                services.AddSingleton<IValidator<UpdateProductQuantityQuery>>(
                    new UpdateProductQuantityQueryValidator());
                services.AddSingleton<TimeProvider>(timeProvider);
            });
        }).CreateClient();

        var request = new UpdateProductQuantityRequest { Quantity = 50 };

        // Act
        var response = await client.PutAsJsonAsync($"api/v1/products/{Guid.Empty}/quantity", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Verify repository was never called
        mockRepository.Verify(
            r => r.GetSingleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateProductQuantity_ZeroQuantity_ReturnsOk()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2024, 2, 10, 14, 0, 0, TimeSpan.Zero));
        var mockIdProvider = new Mock<IEntityIdProvider>();
        var productId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        mockIdProvider.Setup(p => p.CreateNewId()).Returns(productId);

        var existingProduct = new Product(
            "Test Product",
            "https://example.com/image.jpg",
            quantity: 10)
        {
            Id = productId,
            CreatedOnUtc = timeProvider.GetUtcNow().AddDays(-1)
        };

        var mockRepository = new Mock<IProductsRepository>();
        mockRepository
            .Setup(r => r.GetSingleAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);
        mockRepository
            .Setup(r => r.UpdateSingleAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<UpdateProductQuantityEndpoint>>();

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IProductsRepository>();
                services.RemoveAll<ILogger<UpdateProductQuantityEndpoint>>();
                services.RemoveAll<IValidator<UpdateProductQuantityQuery>>();
                services.RemoveAll<IEntityIdProvider>();
                services.RemoveAll<TimeProvider>();

                services.AddSingleton(mockRepository.Object);
                services.AddSingleton(mockLogger.Object);
                services.AddSingleton<IValidator<UpdateProductQuantityQuery>>(
                    new UpdateProductQuantityQueryValidator());
                services.AddSingleton(mockIdProvider.Object);
                services.AddSingleton<TimeProvider>(timeProvider);
            });
        }).CreateClient();

        var request = new UpdateProductQuantityRequest { Quantity = 0 };

        // Act
        var response = await client.PutAsJsonAsync($"api/v1/products/{productId}/quantity", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        mockRepository.Verify(
            r => r.UpdateSingleAsync(
                It.Is<Product>(p => p.Quantity == 0),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateProductQuantity_LogsOriginalAndNewQuantity()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2024, 3, 5, 9, 15, 0, TimeSpan.Zero));
        var mockIdProvider = new Mock<IEntityIdProvider>();
        var productId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        const int originalQuantity = 25;
        const int newQuantity = 100;

        mockIdProvider.Setup(p => p.CreateNewId()).Returns(productId);

        var existingProduct = new Product(
            "Test Product",
            "https://example.com/image.jpg",
            quantity: originalQuantity)
        {
            Id = productId,
            CreatedOnUtc = timeProvider.GetUtcNow().AddDays(-1)
        };

        var mockRepository = new Mock<IProductsRepository>();
        mockRepository
            .Setup(r => r.GetSingleAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);
        mockRepository
            .Setup(r => r.UpdateSingleAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<UpdateProductQuantityEndpoint>>();

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IProductsRepository>();
                services.RemoveAll<ILogger<UpdateProductQuantityEndpoint>>();
                services.RemoveAll<IValidator<UpdateProductQuantityQuery>>();
                services.RemoveAll<IEntityIdProvider>();
                services.RemoveAll<TimeProvider>();

                services.AddSingleton(mockRepository.Object);
                services.AddSingleton(mockLogger.Object);
                services.AddSingleton<IValidator<UpdateProductQuantityQuery>>(
                    new UpdateProductQuantityQueryValidator());
                services.AddSingleton(mockIdProvider.Object);
                services.AddSingleton<TimeProvider>(timeProvider);
            });
        }).CreateClient();

        var request = new UpdateProductQuantityRequest { Quantity = newQuantity };

        // Act
        var response = await client.PutAsJsonAsync($"api/v1/products/{productId}/quantity", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify specific log message with original and new quantities
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Product quantity was updated") &&
                    v.ToString()!.Contains(originalQuantity.ToString()) &&
                    v.ToString()!.Contains(newQuantity.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateProductQuantity_PreservesOtherProductProperties()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2024, 4, 20, 16, 45, 0, TimeSpan.Zero));
        var mockIdProvider = new Mock<IEntityIdProvider>();
        var productId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        mockIdProvider.Setup(p => p.CreateNewId()).Returns(productId);

        var originalCreatedOn = timeProvider.GetUtcNow().AddDays(-10);
        var existingProduct = new Product(
            "Original Title",
            "https://example.com/original.jpg",
            price: 199.99m,
            quantity: 15)
        {
            Id = productId,
            Description = "Original Description",
            CreatedOnUtc = originalCreatedOn
        };

        Product? capturedProduct = null;
        var mockRepository = new Mock<IProductsRepository>();
        mockRepository
            .Setup(r => r.GetSingleAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);
        mockRepository
            .Setup(r => r.UpdateSingleAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((p, ct) => capturedProduct = p)
            .Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<UpdateProductQuantityEndpoint>>();

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IProductsRepository>();
                services.RemoveAll<ILogger<UpdateProductQuantityEndpoint>>();
                services.RemoveAll<IValidator<UpdateProductQuantityQuery>>();
                services.RemoveAll<IEntityIdProvider>();
                services.RemoveAll<TimeProvider>();

                services.AddSingleton(mockRepository.Object);
                services.AddSingleton(mockLogger.Object);
                services.AddSingleton<IValidator<UpdateProductQuantityQuery>>(
                    new UpdateProductQuantityQueryValidator());
                services.AddSingleton(mockIdProvider.Object);
                services.AddSingleton<TimeProvider>(timeProvider);
            });
        }).CreateClient();

        var request = new UpdateProductQuantityRequest { Quantity = 75 };

        // Act
        var response = await client.PutAsJsonAsync($"api/v1/products/{productId}/quantity", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(capturedProduct);

        // Verify only quantity changed, other properties preserved
        Assert.Equal(75, capturedProduct.Quantity);
        Assert.Equal("Original Title", capturedProduct.Title);
        Assert.Equal("https://example.com/original.jpg", capturedProduct.ImageUrl);
        Assert.Equal(199.99m, capturedProduct.Price);
        Assert.Equal("Original Description", capturedProduct.Description);
        Assert.Equal(originalCreatedOn, capturedProduct.CreatedOnUtc);
    }

    [Fact]
    public async Task UpdateProductQuantity_SetsModifiedOnUtcToCurrentTimee()
    {
        // Arrange
        var fixedTime = new DateTimeOffset(2024, 5, 15, 12, 30, 45, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(fixedTime);
        var mockIdProvider = new Mock<IEntityIdProvider>();
        var productId = Guid.Parse("55555555-5555-5555-5555-555555555555");

        mockIdProvider.Setup(p => p.CreateNewId()).Returns(productId);

        // Create product using object initializer instead of constructor
        var existingProduct = new Product("Test Product", "https://example.com/test.jpg")
        {
            Id = productId,
            Quantity = 10,
            Price = 99.99m,
            CreatedOnUtc = fixedTime.AddDays(-5),
            ModifiedOnUtc = null
        };

        Product? capturedProduct = null;
        var mockRepository = new Mock<IProductsRepository>();
        mockRepository
            .Setup(r => r.GetSingleAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);
        mockRepository
            .Setup(r => r.UpdateSingleAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((p, ct) =>
            {
                capturedProduct = p;
                // Simulate what the interceptor does
                p.ModifiedOnUtc = timeProvider.GetUtcNow();
            })
            .Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<UpdateProductQuantityEndpoint>>();

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IProductsRepository>();
                services.RemoveAll<ILogger<UpdateProductQuantityEndpoint>>();
                services.RemoveAll<IValidator<UpdateProductQuantityQuery>>();
                services.RemoveAll<IEntityIdProvider>();
                services.RemoveAll<TimeProvider>();

                services.AddSingleton(mockRepository.Object);
                services.AddSingleton(mockLogger.Object);
                services.AddSingleton<IValidator<UpdateProductQuantityQuery>>(
                    new UpdateProductQuantityQueryValidator());
                services.AddSingleton(mockIdProvider.Object);
                services.AddSingleton<TimeProvider>(timeProvider);
            });
        }).CreateClient();

        var request = new UpdateProductQuantityRequest { Quantity = 50 };

        // Act
        var response = await client.PutAsJsonAsync($"api/v1/products/{productId}/quantity", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(capturedProduct);
        Assert.NotNull(capturedProduct.ModifiedOnUtc);

        // Verify ModifiedOnUtc was set to exact fixed time
        Assert.Equal(fixedTime, capturedProduct.ModifiedOnUtc);
        Assert.Equal(50, capturedProduct.Quantity);

        // Verify CreatedOnUtc was not changed
        Assert.Equal(fixedTime.AddDays(-5), capturedProduct.CreatedOnUtc);

        mockRepository.Verify(
            r => r.UpdateSingleAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}