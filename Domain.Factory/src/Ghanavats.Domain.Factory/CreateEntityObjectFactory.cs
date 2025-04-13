using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.InteropServices;
using Ghanavats.Domain.Factory.Abstractions;
using Ghanavats.Domain.Factory.Abstractions.ActionOptions;
using Ghanavats.Domain.Factory.Abstractions.Responses;
using Ghanavats.Domain.Factory.Attributes;
using Ghanavats.Domain.Factory.Extensions;
using Ghanavats.Domain.Factory.StaticMembers;
using Ghanavats.Domain.Primitives;
using Ghanavats.Domain.Primitives.Attributes;

namespace Ghanavats.Domain.Factory;

public class CreateEntityObjectFactory<TRequest, TResponse>
    : MethodInfoTypeCache, IDomainFactory<TRequest, TResponse>
    where TRequest : class
    where TResponse : EntityBase
{
    /// <inheritdoc/>
    public DomainFactoryResponseModel<TResponse> CreateEntityObject(TRequest request, [Optional] Action<DomainFactoryOption>? action)
    {
        var response = new DomainFactoryResponseModel<TResponse>();
        
        if (!IsResponseAggregateRoot())
        {
            response.ErrorMessage = $"Operation is not allowed. The response type of {typeof(TResponse)} is not an Aggregate Root object.";
            
            return response;
        }
        
        var cacheKey = $"{typeof(TResponse).FullName}.FactoryMethod";

        var domainFactoryOption = new DomainFactoryOption();
        if (action is not null)
        {
            domainFactoryOption = new DomainFactoryOption();
            action(domainFactoryOption);
        }
        
        var method = CachedMethodInfoCollection.TryGetValue(cacheKey, out var result) ? result : GetMethod();
        if (method is null)
        {
            response.ErrorMessage = $"Could not get/find the factory method for the type of {typeof(TResponse)}.";
            
            return response;
        }
        
        CachedMethodInfoCollection[cacheKey] = method;
        
        var constructorInfo = GetConstructor();
        if (constructorInfo is null)
        {
            response.ErrorMessage = "Could not find a public constructor.";
            
            return response;
        }

        var parameters = PopulateParameterValues(request,
            domainFactoryOption.PropertyInfoItems,
            domainFactoryOption.AdditionalProperties);

        if (parameters.Length == 0)
        {
            response.ErrorMessage = $"No parameters found in the request type of {typeof(TRequest)}.";
            
            return response;
        }
        
        var responseValue = (TResponse?)method.Invoke(constructorInfo.Invoke(null), parameters);
        if (responseValue is null)
        {
            response.ErrorMessage = "Could not invoke the factory method with the given parameters.";
            
            return response;
        }
        
        response.Value = responseValue;
        return response;
    }

    private static MethodInfo? GetMethod()
    {
        var method = typeof(TResponse)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .FirstOrDefault(x => x.GetCustomAttribute<FactoryMethodAttribute>() is not null
                                 && x.GetCustomAttribute<FactoryMethodAttribute>()?.FactoryMethodName?.ToString() ==
                                 typeof(TResponse).Name);
        
        return method ?? null;
    }

    private static ConstructorInfo? GetConstructor()
    {
        return typeof(TResponse).GetConstructor(Type.EmptyTypes);
    }

    /// <summary>
    /// Populates an argument list for the constructor to be invoked.
    /// </summary>
    /// <param name="request">The request type from which the parameter types and values are retrieved from</param>
    /// <param name="ignoredProperties">List of the properties to be ignored from the iteration</param>
    /// <param name="additionalProperties">List of the properties to be added to the iteration</param>
    /// <returns>Object array of parameter values in the order they were defined in the request type</returns>
    private static object[] PopulateParameterValues(TRequest request,
        IImmutableList<string> ignoredProperties,
        IImmutableDictionary<string, object> additionalProperties)
    {
        var properties = typeof(TRequest).GetProperties().ToList();

        if (properties.Count == 0)
        {
            return [];
        }

        RemoveIgnoredProperty(ignoredProperties, properties);

        var objValues = new object[properties.Count];
        for (var i = 0; i < objValues.Length; i++)
        {
            objValues[i] = properties[i].GetValue(request).CheckForNull();

            if (properties[i].IsValueTypeNullable()
                && properties[i].IsReferenceTypeNullable())
            {
                continue;
            }

            var propertyName = properties[i].Name;

            objValues[i].CheckForNull(() =>
                new NullReferenceException($"Value of property {propertyName} cannot be null or empty."));
        }

        return additionalProperties.Count <= 0
            ? objValues
            : additionalProperties.Aggregate(objValues, (current, item) => current.Append(item.Value).ToArray());

        void RemoveIgnoredProperty(IImmutableList<string> ignoredPropertyList, List<PropertyInfo> propertySourceList)
        {
            foreach (var ignoredPropertyItem in ignoredPropertyList)
            {
                var index = propertySourceList.FindIndex(x => x.Name == ignoredPropertyItem);
                propertySourceList.RemoveAt(index);
            }
        }
    }

    private static bool IsResponseAggregateRoot()
    {
        return typeof(TResponse).GetCustomAttributes<AggregateRootAttribute>().Any();
    }
}
