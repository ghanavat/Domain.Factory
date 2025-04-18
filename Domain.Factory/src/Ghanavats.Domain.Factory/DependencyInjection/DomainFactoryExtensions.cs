using System.Diagnostics.CodeAnalysis;
using Ghanavats.Domain.Factory.Abstractions;
using Ghanavats.Domain.Factory.CacheService;
using Ghanavats.Domain.Factory.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Ghanavats.Domain.Factory.DependencyInjection;

[ExcludeFromCodeCoverage]
public static class DomainFactoryExtensions
{
    public static IServiceCollection AddDomainFactory(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped(typeof(IDomainFactory<,>), typeof(CreateEntityObjectFactory<,>));
        serviceCollection.AddScoped<IFactoryMethodHandler, FactoryMethodHandler>();
        serviceCollection.AddScoped<ICacheProvider, CacheProvider>();
            
        return serviceCollection;
    }
}
