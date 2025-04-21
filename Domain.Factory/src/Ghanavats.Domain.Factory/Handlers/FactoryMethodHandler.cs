using System.Reflection;
using Ghanavats.Domain.Factory.Abstractions;
using Ghanavats.Domain.Factory.Attributes;
using Ghanavats.Domain.Primitives.Extensions;

namespace Ghanavats.Domain.Factory.Handlers;

public class FactoryMethodHandler : IFactoryMethodHandler
{
    private readonly ICacheProvider _cacheProvider;
    
    public FactoryMethodHandler(ICacheProvider cacheProvider)
    {
        _cacheProvider = cacheProvider.CheckForNull();
    }
    
    public MethodInfo? GetFactoryMethod(Type type)
    {
        var cachedMethodInfo = _cacheProvider.Get($"{type.Name}.FactoryMethod");

        var method = cachedMethodInfo.ToString() != string.Empty ? cachedMethodInfo : GetMethod();

        return (MethodInfo?)method;
        
        MethodInfo? GetMethod()
        {
            method = type
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .FirstOrDefault(x =>
                {
                    if (x.GetCustomAttribute<FactoryMethodAttribute>() is null)
                    {
                        return false;
                    }
                
                    var factoryMethodFor = x.GetCustomAttribute<FactoryMethodAttribute>()?.FactoryMethodFor?.ToString();
                    if (!string.IsNullOrWhiteSpace(factoryMethodFor))
                    {
                        return factoryMethodFor == type.Name;
                    }

                    return true;
                });

            if (method is null)
            {
                return null;
            }
        
            _cacheProvider.Insert($"{type.Name}.FactoryMethod", method);
            return (MethodInfo?)method;
        }
    }
}
