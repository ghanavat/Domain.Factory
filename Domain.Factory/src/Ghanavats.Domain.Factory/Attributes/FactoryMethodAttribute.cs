using System.Runtime.InteropServices;

namespace Ghanavats.Domain.Factory.Attributes;

/// <summary>
/// To mark a factory method.
/// </summary>
/// <remarks>
/// This is used by the base factory to look up the factory method with some validation
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public class FactoryMethodAttribute : Attribute
{
    /// <summary>
    /// String parameter that is used to mark the factory method with its parent class name
    /// </summary>
    public string? FactoryMethodFor { get; private set; }

    /// <summary>
    /// The custom attribute constructor
    /// </summary>
    /// <param name="factoryMethodFor">An optional parameter to indicate what aggregate root this attribute belongs to. 
    /// It can be used to look up the factory method in the aggregate root class</param>
    public FactoryMethodAttribute([Optional] string factoryMethodFor)
    {
        if (!string.IsNullOrWhiteSpace(factoryMethodFor))
        {
            FactoryMethodFor = factoryMethodFor;
        }
    }
}
