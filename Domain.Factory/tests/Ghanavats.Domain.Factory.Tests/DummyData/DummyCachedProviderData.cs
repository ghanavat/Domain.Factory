using System.Reflection;
using Ghanavats.Domain.Factory.Attributes;
using Ghanavats.Domain.Primitives.Attributes;

namespace Ghanavats.Domain.Factory.Tests.DummyData;

internal static class DummyCachedProviderData
{
    internal static MethodInfo GetValidMethodInfoThatWasCached()
    {
        return null!;
    }
}

[AggregateRoot]
internal class DummyEntityOne
{
    public int DummyProperty { get; set; }

    public DummyEntityOne()
    {
    }

    private DummyEntityOne(int dummyProperty)
    {
        DummyProperty = dummyProperty;
    }

    [FactoryMethod]
    internal static DummyEntityOne DummyFactoryMethod(int dummyProperty)
    {
        return new DummyEntityOne(dummyProperty);
    }
}
