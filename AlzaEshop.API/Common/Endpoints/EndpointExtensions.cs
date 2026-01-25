using System.Reflection;
using AlzaEshop.API.Features.Products;

namespace AlzaEshop.API.Common.Endpoints;

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        services.AddTransient<IEndpoint, GetProductsEndpoint>();
        services.AddTransient<IEndpoint, GetSingleProductEndpoint>();
        services.AddTransient<IEndpoint, CreateProductEndpoint>();

        return services;
    }

    public static IApplicationBuilder MapEndpoints(this WebApplication app)
    {
        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();
        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(app);
        }

        return app;
    }
}
