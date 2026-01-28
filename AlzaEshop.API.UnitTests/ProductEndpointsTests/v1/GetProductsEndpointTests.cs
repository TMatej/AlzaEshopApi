using System.Net;
using System.Net.Http.Json;
using AlzaEshop.API.Common.Services.EntityIdProvider;
using AlzaEshop.API.Features.Products.Common.Database;
using AlzaEshop.API.Features.Products.Common.Model;
using AlzaEshop.API.Features.Products.v1;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;

public class GetProductsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    // use WebApplicationFactory as the tested logic is part of the endpoint handler
    // instead of separte services (due to simple logic)
    public GetProductsEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetProducts_WithMultipleProducts_ReturnsAllProducts()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);

        var product1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var product2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var product3Id = Guid.Parse("33333333-3333-3333-3333-333333333333");

        var products = new List<Product>
        {
            new Product("Product 1", "https://example.com/image1.jpg", price: 99.99m, quantity: 10)
            {
                Id = product1Id,
                Description = "Description 1",
                CreatedOnUtc = timeProvider.GetUtcNow().AddDays(-5)
            },
            new Product("Product 2", "https://example.com/image2.jpg", price: 49.99m, quantity: 20)
            {
                Id = product2Id,
                Description = "Description 2",
                CreatedOnUtc = timeProvider.GetUtcNow().AddDays(-3)
            },
            new Product("Product 3", "https://example.com/image3.jpg", price: 149.99m, quantity: 5)
            {
                Id = product3Id,
                Description = "Description 3",
                CreatedOnUtc = timeProvider.GetUtcNow().AddDays(-1)
            }
        };

        var mockRepository = new Mock<IProductsRepository>();
        mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var mockLogger = new Mock<ILogger<GetProductsEndpoint>>();

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IProductsRepository>();
                services.RemoveAll<ILogger<GetProductsEndpoint>>();
                services.RemoveAll<TimeProvider>();

                services.AddSingleton(mockRepository.Object);
                services.AddSingleton(mockLogger.Object);
                services.AddSingleton<TimeProvider>(timeProvider);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("api/v1/products");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GetProductsResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Equal(3, result.Items.Count);

        // Verify first product mapping
        var firstProduct = result.Items[0];
        Assert.Equal(products[0].Id, firstProduct.Id);
        Assert.Equal(products[0].Title, firstProduct.Title);
        Assert.Equal(products[0].ImageUrl, firstProduct.ImageUrl);
        Assert.Equal(products[0].Price, firstProduct.Price);
        Assert.Equal(products[0].Description, firstProduct.Description);
        Assert.Equal(products[0].Quantity, firstProduct.Quantity);
        Assert.Equal(products[0].CreatedOnUtc, firstProduct.CreatedOnUtc);

        // Verify repository was called once
        mockRepository.Verify(
            r => r.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetProducts_WithEmptyDatabase_ReturnsEmptyCollection()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);

        var mockRepository = new Mock<IProductsRepository>();
        mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        var mockLogger = new Mock<ILogger<GetProductsEndpoint>>();

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IProductsRepository>();
                services.RemoveAll<ILogger<GetProductsEndpoint>>();
                services.RemoveAll<TimeProvider>();

                services.AddSingleton(mockRepository.Object);
                services.AddSingleton(mockLogger.Object);
                services.AddSingleton<TimeProvider>(timeProvider);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("api/v1/products");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GetProductsResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Empty(result.Items);

        mockRepository.Verify(
            r => r.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetProducts_WithSingleProduct_ReturnsSingleProduct()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero));
        var mockIdProvider = new Mock<IEntityIdProvider>();
        var productId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        mockIdProvider.Setup(p => p.CreateNewId()).Returns(productId);

        var createdDate = timeProvider.GetUtcNow().AddDays(-2);
        var products = new List<Product>
        {
            new Product("Single Product", "https://example.com/single.jpg", price: 79.99m, quantity: 15)
            {
                Id = productId,
                Description = "Single Product Description",
                CreatedOnUtc = createdDate
            }
        };

        var mockRepository = new Mock<IProductsRepository>();
        mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var mockLogger = new Mock<ILogger<GetProductsEndpoint>>();

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IProductsRepository>();
                services.RemoveAll<ILogger<GetProductsEndpoint>>();
                services.RemoveAll<IEntityIdProvider>();
                services.RemoveAll<TimeProvider>();

                services.AddSingleton(mockRepository.Object);
                services.AddSingleton(mockLogger.Object);
                services.AddSingleton(mockIdProvider.Object);
                services.AddSingleton<TimeProvider>(timeProvider);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("api/v1/products");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GetProductsResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Single(result.Items);

        var product = result.Items[0];
        Assert.Equal(productId, product.Id);
        Assert.Equal("Single Product", product.Title);
        Assert.Equal("https://example.com/single.jpg", product.ImageUrl);
        Assert.Equal(79.99m, product.Price);
        Assert.Equal("Single Product Description", product.Description);
        Assert.Equal(15, product.Quantity);
        Assert.Equal(createdDate, product.CreatedOnUtc);
    }

    [Fact]
    public async Task GetProducts_MapsAllProductPropertiesCorrectly()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2024, 2, 20, 14, 45, 30, TimeSpan.Zero));
        var mockIdProvider = new Mock<IEntityIdProvider>();
        var productId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        mockIdProvider.Setup(p => p.CreateNewId()).Returns(productId);

        var createdDate = timeProvider.GetUtcNow().AddHours(-3);
        var products = new List<Product>
        {
            new Product("Test Product", "https://example.com/test.jpg", price: 123.45m, quantity: 42)
            {
                Id = productId,
                Description = "Test Description",
                CreatedOnUtc = createdDate
            }
        };

        var mockRepository = new Mock<IProductsRepository>();
        mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var mockLogger = new Mock<ILogger<GetProductsEndpoint>>();

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IProductsRepository>();
                services.RemoveAll<ILogger<GetProductsEndpoint>>();
                services.RemoveAll<IEntityIdProvider>();
                services.RemoveAll<TimeProvider>();

                services.AddSingleton(mockRepository.Object);
                services.AddSingleton(mockLogger.Object);
                services.AddSingleton(mockIdProvider.Object);
                services.AddSingleton<TimeProvider>(timeProvider);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("api/v1/products");

        // Assert
        var result = await response.Content.ReadFromJsonAsync<GetProductsResponse>();
        Assert.NotNull(result);

        var product = result.Items![0];

        // Verify exact mapping of all properties
        Assert.Equal(productId, product.Id);
        Assert.Equal("Test Product", product.Title);
        Assert.Equal("https://example.com/test.jpg", product.ImageUrl);
        Assert.Equal(123.45m, product.Price);
        Assert.Equal("Test Description", product.Description);
        Assert.Equal(42, product.Quantity);
        Assert.Equal(createdDate, product.CreatedOnUtc);
    }

    [Fact]
    public async Task GetProducts_WithNullDescription_ReturnsNullDescription()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var mockIdProvider = new Mock<IEntityIdProvider>();
        var productId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

        mockIdProvider.Setup(p => p.CreateNewId()).Returns(productId);

        var products = new List<Product>
        {
            new Product("Product Without Description", "https://example.com/no-desc.jpg", price: 25.00m, quantity: 100)
            {
                Id = productId,
                Description = null, // Explicitly null
                CreatedOnUtc = timeProvider.GetUtcNow()
            }
        };

        var mockRepository = new Mock<IProductsRepository>();
        mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var mockLogger = new Mock<ILogger<GetProductsEndpoint>>();

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IProductsRepository>();
                services.RemoveAll<ILogger<GetProductsEndpoint>>();
                services.RemoveAll<IEntityIdProvider>();
                services.RemoveAll<TimeProvider>();

                services.AddSingleton(mockRepository.Object);
                services.AddSingleton(mockLogger.Object);
                services.AddSingleton(mockIdProvider.Object);
                services.AddSingleton<TimeProvider>(timeProvider);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("api/v1/products");

        // Assert
        var result = await response.Content.ReadFromJsonAsync<GetProductsResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Single(result.Items);
        Assert.Null(result.Items[0].Description);
    }

    [Fact]
    public async Task GetProducts_WithZeroQuantityAndPrice_ReturnsCorrectValues()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var mockIdProvider = new Mock<IEntityIdProvider>();
        var productId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

        mockIdProvider.Setup(p => p.CreateNewId()).Returns(productId);

        var products = new List<Product>
        {
            new Product("Free Product", "https://example.com/free.jpg", price: 0m, quantity: 0)
            {
                Id = productId,
                Description = "Out of stock and free",
                CreatedOnUtc = timeProvider.GetUtcNow()
            }
        };

        var mockRepository = new Mock<IProductsRepository>();
        mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var mockLogger = new Mock<ILogger<GetProductsEndpoint>>();

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IProductsRepository>();
                services.RemoveAll<ILogger<GetProductsEndpoint>>();
                services.RemoveAll<IEntityIdProvider>();
                services.RemoveAll<TimeProvider>();

                services.AddSingleton(mockRepository.Object);
                services.AddSingleton(mockLogger.Object);
                services.AddSingleton(mockIdProvider.Object);
                services.AddSingleton<TimeProvider>(timeProvider);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("api/v1/products");

        // Assert
        var result = await response.Content.ReadFromJsonAsync<GetProductsResponse>();
        Assert.NotNull(result);

        var product = result.Items![0];
        Assert.Equal(0m, product.Price);
        Assert.Equal(0, product.Quantity);
    }

    [Fact]
    public async Task GetProducts_WithLargeDataset_ReturnsAllProducts()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2024, 3, 10, 8, 0, 0, TimeSpan.Zero));
        var mockIdProvider = new Mock<IEntityIdProvider>();

        var productIds = Enumerable.Range(1, 100)
            .Select(i => Guid.Parse($"{i:00000000}-0000-0000-0000-000000000000"))
            .ToList();

        var idQueue = new Queue<Guid>(productIds);
        mockIdProvider.Setup(p => p.CreateNewId()).Returns(() => idQueue.Dequeue());

        var products = Enumerable.Range(1, 100)
            .Select(i => new Product(
                $"Product {i}",
                $"https://example.com/image{i}.jpg",
                price: i * 10m,
                quantity: i * 5)
            {
                Id = productIds[i - 1],
                Description = $"Description {i}",
                CreatedOnUtc = timeProvider.GetUtcNow().AddDays(-i)
            })
            .ToList();

        var mockRepository = new Mock<IProductsRepository>();
        mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var mockLogger = new Mock<ILogger<GetProductsEndpoint>>();

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IProductsRepository>();
                services.RemoveAll<ILogger<GetProductsEndpoint>>();
                services.RemoveAll<IEntityIdProvider>();
                services.RemoveAll<TimeProvider>();

                services.AddSingleton(mockRepository.Object);
                services.AddSingleton(mockLogger.Object);
                services.AddSingleton(mockIdProvider.Object);
                services.AddSingleton<TimeProvider>(timeProvider);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("api/v1/products");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GetProductsResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Equal(100, result.Items.Count);

        // Verify first and last products
        Assert.Equal("Product 1", result.Items[0].Title);
        Assert.Equal("Product 100", result.Items[99].Title);
    }

    [Fact]
    public async Task GetProducts_PassesCancellationTokenToRepository()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);

        CancellationToken capturedToken = default;
        var mockRepository = new Mock<IProductsRepository>();
        mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(ct => capturedToken = ct)
            .ReturnsAsync(new List<Product>());

        var mockLogger = new Mock<ILogger<GetProductsEndpoint>>();

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IProductsRepository>();
                services.RemoveAll<ILogger<GetProductsEndpoint>>();
                services.RemoveAll<TimeProvider>();

                services.AddSingleton(mockRepository.Object);
                services.AddSingleton(mockLogger.Object);
                services.AddSingleton<TimeProvider>(timeProvider);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("api/v1/products");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEqual(default, capturedToken);

        mockRepository.Verify(
            r => r.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetProducts_PreservesProductOrder()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2024, 4, 5, 12, 0, 0, TimeSpan.Zero));
        var mockIdProvider = new Mock<IEntityIdProvider>();

        var product1Id = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var product2Id = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var product3Id = Guid.Parse("00000000-0000-0000-0000-000000000003");

        var idQueue = new Queue<Guid>(new[] { product1Id, product2Id, product3Id });
        mockIdProvider.Setup(p => p.CreateNewId()).Returns(() => idQueue.Dequeue());

        var products = new List<Product>
        {
            new Product("First", "https://example.com/1.jpg", quantity: 1)
            {
                Id = product1Id,
                CreatedOnUtc = timeProvider.GetUtcNow().AddDays(-3)
            },
            new Product("Second", "https://example.com/2.jpg", quantity: 2)
            {
                Id = product2Id,
                CreatedOnUtc = timeProvider.GetUtcNow().AddDays(-2)
            },
            new Product("Third", "https://example.com/3.jpg", quantity: 3)
            {
                Id = product3Id,
                CreatedOnUtc = timeProvider.GetUtcNow().AddDays(-1)
            }
        };

        var mockRepository = new Mock<IProductsRepository>();
        mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var mockLogger = new Mock<ILogger<GetProductsEndpoint>>();

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IProductsRepository>();
                services.RemoveAll<ILogger<GetProductsEndpoint>>();
                services.RemoveAll<IEntityIdProvider>();
                services.RemoveAll<TimeProvider>();

                services.AddSingleton(mockRepository.Object);
                services.AddSingleton(mockLogger.Object);
                services.AddSingleton(mockIdProvider.Object);
                services.AddSingleton<TimeProvider>(timeProvider);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("api/v1/products");

        // Assert
        var result = await response.Content.ReadFromJsonAsync<GetProductsResponse>();
        Assert.NotNull(result);
        Assert.Equal(3, result.Items!.Count);

        // Verify order is preserved
        Assert.Equal(product1Id, result.Items[0].Id);
        Assert.Equal("First", result.Items[0].Title);

        Assert.Equal(product2Id, result.Items[1].Id);
        Assert.Equal("Second", result.Items[1].Title);

        Assert.Equal(product3Id, result.Items[2].Id);
        Assert.Equal("Third", result.Items[2].Title);
    }
}
