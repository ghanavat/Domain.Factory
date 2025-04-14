using Ghanavats.Domain.Factory.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Ghanavats.Domain.Factory.DependencyInjection;

public static class DomainFactoryExtensions
{
    public static IServiceCollection AddDomainFactory(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped(typeof(IDomainFactory<,>), typeof(CreateEntityObjectFactory<,>));

        return serviceCollection;
    }
}
