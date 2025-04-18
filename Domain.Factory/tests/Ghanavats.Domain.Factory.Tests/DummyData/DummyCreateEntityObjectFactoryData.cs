using Ghanavats.Domain.Factory.Attributes;
using Ghanavats.Domain.Primitives;
using Ghanavats.Domain.Primitives.Attributes;

namespace Ghanavats.Domain.Factory.Tests.DummyData;

internal static class DummyCreateEntityObjectFactoryData
{
    internal record DummyCreateCommand(string DummyField1, int DummyField2);
    
    internal static DummyCreateCommand GetValidDummyRequest()
    {
        return new DummyCreateCommand("DummyField1", Random.Shared.Next());
    }
}

internal class DummyResponseEntity : EntityBase
{
    public string PropertyA { get; set; } = string.Empty;
}

[AggregateRoot]
internal class DummyEntity : EntityBase
{
    public string DummyField1 { get; set; } = string.Empty;
    public int DummyField2 { get; set; }
    
    public DummyEntity()
    {
    }

    private DummyEntity(string dummyField1, int dummyField2)
    {
        DummyField1 = dummyField1;
        DummyField2 = dummyField2;
    }

    [FactoryMethod("DummyEntity")]
    internal static DummyEntity TestDummyEntityFactoryMethod(string dummyField1, int dummyField2)
    {
         return new DummyEntity(dummyField1, dummyField2);
    }
}

[AggregateRoot]
internal class DummyWithNoFactoryMethodAttrEntity : EntityBase
{
    private DummyWithNoFactoryMethodAttrEntity(string dummyField1, int dummyField2)
    {
    }
    
    internal static DummyWithNoFactoryMethodAttrEntity DummyFactoryMethod()
    {
        return new DummyWithNoFactoryMethodAttrEntity(string.Empty, 0);
    }
}

[AggregateRoot]
internal class DummyWithFactoryMethodAttrEntityViolatedName : EntityBase
{
    private DummyWithFactoryMethodAttrEntityViolatedName(string dummyField1, int dummyField2)
    {
    }
    
    [FactoryMethod("IncorrectEntityName")]
    internal static DummyWithFactoryMethodAttrEntityViolatedName DummyFactoryMethod()
    {
        return new DummyWithFactoryMethodAttrEntityViolatedName(string.Empty, 0);
    }
}

[AggregateRoot]
internal class DummyWithFactoryMethodAttrEntityWithoutOptionalName : EntityBase
{
    public string DummyField2 { get; set; } = string.Empty;
    public int DummyField3 { get; set; }

    public DummyWithFactoryMethodAttrEntityWithoutOptionalName()
    {
    }
    
    private DummyWithFactoryMethodAttrEntityWithoutOptionalName(string dummyField2, int dummyField3)
    {
        DummyField2 = dummyField2;
        DummyField3 = dummyField3;
    }
    
    [FactoryMethod]
    internal static DummyWithFactoryMethodAttrEntityWithoutOptionalName DummyFactoryMethod(string dummyField2, int dummyField3)
    {
        return new DummyWithFactoryMethodAttrEntityWithoutOptionalName(dummyField2, dummyField3);
    }
}

[AggregateRoot]
internal class DummyWithNoPublicConstructorEntity : EntityBase
{
    public string DummyField4 { get; set; }
    public int DummyField5 { get; set; }

    private DummyWithNoPublicConstructorEntity(string dummyField4, int dummyField5)
    {
        DummyField4 = dummyField4;
        DummyField5 = dummyField5;
    }
    
    [FactoryMethod]
    internal static DummyWithNoPublicConstructorEntity DummyFactoryMethod(string dummyField4, int dummyField5)
    {
        return new DummyWithNoPublicConstructorEntity(dummyField4, dummyField5);
    }
}
