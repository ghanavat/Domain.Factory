using System.Reflection;
using System.Runtime.InteropServices;
using Ghanavats.Domain.Factory.Abstractions;
using Ghanavats.Domain.Factory.Abstractions.ActionOptions;
using Ghanavats.Domain.Factory.Abstractions.Responses;
using Ghanavats.Domain.Factory.Extensions;
using Ghanavats.Domain.Factory.Handlers;
using Ghanavats.Domain.Primitives;
using Ghanavats.Domain.Primitives.Attributes;

namespace Ghanavats.Domain.Factory;

public class CreateEntityObjectFactory<TRequest, TResponse>
    : IDomainFactory<TRequest, TResponse>
    where TRequest : class
    where TResponse : EntityBase
{
    private readonly IFactoryMethodHandler _factoryMethodHandler;
    private readonly IReadCache _readCache;

    public CreateEntityObjectFactory(IFactoryMethodHandler factoryMethodHandler, IReadCache readCache)
    {
        _factoryMethodHandler = factoryMethodHandler.CheckForNull();
        _readCache = readCache.CheckForNull();
    }

    /// <inheritdoc/>
    public DomainFactoryResponseModel<TResponse> CreateEntityObject(TRequest request, [Optional] Action<DomainFactoryOption>? action)
    {
        if (!IsResponseTypeAggregateRoot())
        {
            return DomainFactoryResponseModel<TResponse>
                .Failure($"Operation is not allowed. The response type {typeof(TResponse).Name} is not an Aggregate Root.");
        }
        
        var option = new DomainFactoryOption();
        action?.Invoke(option);
        
        var factoryMethod = _factoryMethodHandler.GetFactoryMethod(typeof(TResponse));
        if (factoryMethod is null)
        {
            return DomainFactoryResponseModel<TResponse>
                .Failure($"Could not get/find the factory method for type {typeof(TResponse).Name}.");
        }
        
        var constructorInfo = typeof(TResponse).GetConstructor(Type.EmptyTypes);
        if (constructorInfo is null)
        {
            return DomainFactoryResponseModel<TResponse>
                .Failure("Could not find a public constructor.");
        }

        var parameters = PopulateParameterValues(request,
            option.IgnorePropertiesCollection,
            option.AdditionalProperties);

        if (parameters.Length == 0)
        {
            return DomainFactoryResponseModel<TResponse>
                .Failure($"No parameters found in the request type {typeof(TRequest).Name}.");
        }
        
        var responseValue = (TResponse?)factoryMethod.Invoke(constructorInfo.Invoke(null), parameters);
        if (responseValue is null)
        {
            return DomainFactoryResponseModel<TResponse>
                .Failure("Could not invoke the factory method with the given parameters.");
        }

        return DomainFactoryResponseModel<TResponse>
            .Success(responseValue, _readCache.Get($"{typeof(TResponse).Name}.FactoryMethod"));
    }
    
    /// <summary>
    /// Populates an argument list for the constructor to be invoked.
    /// </summary>
    /// <param name="request">The request type from which the parameter types and values are retrieved from</param>
    /// <param name="ignoredProperties">List of the properties to be ignored from the iteration</param>
    /// <param name="additionalProperties">List of the properties to be added to the iteration</param>
    /// <returns>Object array of parameter values in the order they were defined in the request type</returns>
    private static object?[] PopulateParameterValues(TRequest request,
        IReadOnlyCollection<string> ignoredProperties,
        IReadOnlyDictionary<string, object> additionalProperties)
    {
        ICollection<PropertyInfo> properties = request.GetType().GetProperties();

        if (properties.Count == 0)
        {
            return [];
        }

        var ignoredPropertyList = ignoredProperties.ToList();
        if (ignoredPropertyList.Count > 0)
        {
            RemoveIgnoredProperty(ignoredPropertyList, ref properties);
        }
        
        var propertyValues = new object?[properties.Count];
        var propertyList = properties.ToList();
        
        for (var i = 0; i < propertyList.Count; i++)
        {
            if (propertyList[i].GetValue(request) is null 
                && propertyList[i].IsNullablePropertyType())
            {
                propertyValues[i] = null;
                continue;
            }
            
            var propertyName = propertyList[i].Name;
            propertyValues[i] = propertyList[i].GetValue(request).CheckForNull(() =>
                new NullReferenceException($"Value of property {propertyName} cannot be null or empty."));
        }

        return additionalProperties.Count <= 0
            ? propertyValues
            : additionalProperties.Aggregate(propertyValues, (current, item) => current.Append(item.Value).ToArray());

        void RemoveIgnoredProperty(IEnumerable<string> ignoredPropertyCollection, ref ICollection<PropertyInfo> propertySourceCollection)
        {
            var propertySourceList = propertySourceCollection.ToList();
            foreach (var ignoredPropertyItem in ignoredPropertyCollection)
            {
                propertySourceList.RemoveAll(x => x.Name == ignoredPropertyItem);
            }

            propertySourceCollection = propertySourceList;
        }
    }

    private static bool IsResponseTypeAggregateRoot()
    {
        return typeof(TResponse).GetCustomAttributes<AggregateRootAttribute>().Any();
    }
}
