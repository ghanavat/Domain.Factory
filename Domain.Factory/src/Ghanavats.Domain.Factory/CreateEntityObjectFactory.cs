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
    : DomainFactoryBase, IDomainFactory<TRequest, TResponse>
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
        
        // var cachedMethodInfo = _cacheProvider.Get(CacheKey);
        //
        // var method = cachedMethodInfo.ToString() != string.Empty ? cachedMethodInfo : GetMethod();
        
        //var method = CachedMethodInfoCollection.TryGetValue(CacheKey, out var result) ? result : GetMethod();
        // if (method is null 
        //     || method.GetType() != typeof(MethodInfo))
        // {
        //     return DomainFactoryResponseModel<TResponse>
        //         .Failure($"Could not get/find the factory method for the type {typeof(TResponse).Name}.");
        // }
        //
        // _cacheProvider.Insert(CacheKey, method);
        
        //CachedMethodInfoCollection[cacheKey] = method;
        
        var constructorInfo = typeof(TResponse).GetConstructor(Type.EmptyTypes);
        if (constructorInfo is null)
        {
            return DomainFactoryResponseModel<TResponse>
                .Failure("Could not find a public constructor.");
        }

        var parameters = PopulateParameterValues(request,
            option.PropertyInfoItems,
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

    // private static MethodInfo? GetMethod()
    // {
    //     var method = typeof(TResponse)
    //         .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
    //         .FirstOrDefault(x =>
    //         {
    //             if (x.GetCustomAttribute<FactoryMethodAttribute>() is null) return false;
    //             
    //             var factoryMethodName = x.GetCustomAttribute<FactoryMethodAttribute>()?.FactoryMethodName?.ToString();
    //             if (!string.IsNullOrWhiteSpace(factoryMethodName))
    //             {
    //                 return factoryMethodName == typeof(TResponse).Name;
    //             }
    //
    //             return true;
    //         });
    //     
    //     return method ?? null;
    // }

    // private static ConstructorInfo? GetConstructor()
    // {
    //     return typeof(TResponse).GetConstructor(Type.EmptyTypes);
    // }

    // private static string[] PopulateResponseCache()
    // {
    //     var cacheItemsArray = new string[CachedMethodInfoCollection.Count];
    //
    //     foreach (var cachedMethodInfoItem in CachedMethodInfoCollection.Values)
    //     {
    //         cacheItemsArray = [cachedMethodInfoItem.Name];
    //     }
    //
    //     return cacheItemsArray;
    // }

    /// <summary>
    /// Populates an argument list for the constructor to be invoked.
    /// </summary>
    /// <param name="request">The request type from which the parameter types and values are retrieved from</param>
    /// <param name="ignoredProperties">List of the properties to be ignored from the iteration</param>
    /// <param name="additionalProperties">List of the properties to be added to the iteration</param>
    /// <returns>Object array of parameter values in the order they were defined in the request type</returns>
    private static object[] PopulateParameterValues(TRequest request,
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
        
        var objValues = new object[properties.Count];
        var propertyList = properties.ToList();
        
        for (var i = 0; i < objValues.Length; i++)
        {
            objValues[i] = propertyList[i].GetValue(request).CheckForNull();

            if (propertyList[i].IsValueTypeNullable()
                && propertyList[i].IsReferenceTypeNullable())
            {
                continue;
            }

            var propertyName = propertyList[i].Name;

            objValues[i].CheckForNull(() =>
                new NullReferenceException($"Value of property {propertyName} cannot be null or empty."));
        }

        return additionalProperties.Count <= 0
            ? objValues
            : additionalProperties.Aggregate(objValues, (current, item) => current.Append(item.Value).ToArray());

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
