using Ghanavats.Domain.Factory.Attributes;
using Ghanavats.Domain.Primitives.Attributes;

namespace Ghanavats.Domain.Factory.Tests.DummyData;

internal static class DummyFactoryMethodHandlerData
{
    internal static Type GetValidType()
    {
        return typeof(DummyFactoryHandlerEntity);
    }

    internal static Type GetValidTypeForTestingCorrectReturnTypeWithoutOptionalAttributeParameter()
    {
        return typeof(DummyEntityTwo);
    }

    internal static Type GetValidTypeWithIncorrectlyConfiguredFactoryMethod()
    {
        return typeof(DummyEntityThree);
    }
}

[AggregateRoot]
internal class DummyFactoryHandlerEntity
{
    public string DummyProperty { get; set; } = string.Empty;

    public DummyFactoryHandlerEntity()
    {
    }

    private DummyFactoryHandlerEntity(string dummyProperty)
    {
        DummyProperty = dummyProperty;
    }

    internal static DummyFactoryHandlerEntity DummyFactoryMethod(string dummyProperty)
    {
        return new DummyFactoryHandlerEntity(dummyProperty);
    }
}

[AggregateRoot]
internal class DummyEntityTwo
{
    public int DummyProperty { get; set; }

    public DummyEntityTwo()
    {
    }

    private DummyEntityTwo(int dummyProperty)
    {
        DummyProperty = dummyProperty;
    }

    [FactoryMethod]
    internal static DummyEntityTwo DummyFactoryMethod(int dummyProperty)
    {
        return new DummyEntityTwo(dummyProperty);
    }
}

[AggregateRoot]
internal class DummyEntityThree
{
    public decimal DummyProperty { get; set; }

    public DummyEntityThree()
    {
    }

    private DummyEntityThree(decimal dummyProperty)
    {
        DummyProperty = dummyProperty;
    }

    [FactoryMethod("DummyEntityIncorrectName")]
    internal static DummyEntityThree DummyFactoryMethod(decimal dummyProperty)
    {
        return new DummyEntityThree(dummyProperty);
    }
}
