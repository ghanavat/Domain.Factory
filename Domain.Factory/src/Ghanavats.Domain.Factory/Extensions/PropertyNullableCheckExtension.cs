using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Ghanavats.Domain.Factory.Extensions;

[ExcludeFromCodeCoverage]
public static class PropertyNullableCheckExtension
{
    public static bool IsNullablePropertyType(this PropertyInfo property)
    {
        if (property.PropertyType.IsValueType)
        {
            return Nullable.GetUnderlyingType(property.PropertyType) != null;
        }
        
        var nullabilityInfo = new NullabilityInfoContext().Create(property);
        return nullabilityInfo.WriteState == NullabilityState.Nullable;
    }
}
