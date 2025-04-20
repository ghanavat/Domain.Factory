using System.Collections.Immutable;

namespace Ghanavats.Domain.Factory.Abstractions.ActionOptions;

public class DomainFactoryOption
{
    private readonly List<string> _ignorePropertyItems = [];
    private readonly Dictionary<string, object> _additionalPropertyItems = [];
    
    public IReadOnlyCollection<string> IgnorePropertiesCollection => _ignorePropertyItems.AsReadOnly();
    public IReadOnlyDictionary<string, object> AdditionalProperties => _additionalPropertyItems.AsReadOnly();
    
    /// <summary>
    /// Properties that are not supposed to be passed to the factory for the creation of the domain entity.
    /// <para>
    /// Often, commands are sent with all properties to satisfy the business logic.
    /// However, not all of them are needed to create a new domain entity object.
    /// </para>
    /// <para>
    /// The property will not be ignored if it is expected by the entity constructor. 
    /// If you do this, an error message will be returned.
    /// </para>
    /// </summary>
    /// <param name="propertyNames">A list of property names to be ignored by the factory.</param>
    public DomainFactoryOption IgnoreProperties(IImmutableList<string> propertyNames)
    {
        foreach (var item in propertyNames)
        {
            _ignorePropertyItems.Add(item);
        }

        return this;
    }
    
    /// <summary>
    /// <para>
    /// Properties that are not sent via the request command and are calculated after the request was sent by clients.
    /// </para>
    /// <para>
    /// To keep the API requests clean from unnecessary constraints to the business logics,
    /// often some of the domain entity arguments might be calculated in the business logic code,
    /// and then passed to the entity constructor.
    /// </para>
    /// </summary>
    /// <param name="propertyDetails"></param>
    public DomainFactoryOption AddProperties(IImmutableDictionary<string, object> propertyDetails)
    {
        foreach (var item in propertyDetails)
        {
            _additionalPropertyItems.Add(item.Key, item.Value);
        }

        return this;
    }
}
