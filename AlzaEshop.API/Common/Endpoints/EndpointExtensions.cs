using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AlzaEshop.API.Common.Endpoints;

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        ServiceDescriptor[] endpointServiceDescriptors = assembly.DefinedTypes
            .Where(x => x is { IsAbstract: false, IsInterface: false }
                && x.IsAssignableTo(typeof(IEndpoint)))
            .Select(x => ServiceDescriptor.Transient(typeof(IEndpoint), x))
            .ToArray();

        services.TryAddEnumerable(endpointServiceDescriptors);

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
