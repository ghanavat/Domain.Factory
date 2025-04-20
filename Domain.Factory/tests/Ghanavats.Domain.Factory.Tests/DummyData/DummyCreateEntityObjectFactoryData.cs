using Ghanavats.Domain.Factory.Attributes;
using Ghanavats.Domain.Primitives;
using Ghanavats.Domain.Primitives.Attributes;

namespace Ghanavats.Domain.Factory.Tests.DummyData;

internal static class DummyCreateEntityObjectFactoryData
{
    internal record DummyCreateCommand(string DummyField1, int DummyField2);
    internal record DummyCreateCommandWithNoProperties;
    internal record DummyCreateCommandWithPropertyToBeIgnored(string DummyField2, int DummyField3, string IgnoreA);
    internal record DummyCreateCommandWithNullableReferenceTypeProperty(string? DummyField2, int DummyField3);
    internal record DummyCreateCommandWithNullableValueTypeProperty(string DummyField6, int? DummyField7);
    internal record DummyCreateCommandWithMismatchNumberProperties(string DummyField67, int? DummyField99, string ExtraMismatchField);

    internal record DummyCreateCommandWithLessPropertiesThanFactoryMethod(string DummyField5678, int DummyField9876);
    
    internal static DummyCreateCommand GetValidDummyRequest()
    {
        return new DummyCreateCommand("DummyField1", Random.Shared.Next());
    }
    
    internal static DummyCreateCommandWithNoProperties GetInvalidDummyRequest()
    {
        return new DummyCreateCommandWithNoProperties();
    }
    
    internal static DummyCreateCommandWithPropertyToBeIgnored GetValidDummyRequestWithIgnoreProperty()
    {
        return new DummyCreateCommandWithPropertyToBeIgnored("DummyValue1", Random.Shared.Next(), "SomeTestValue");
    }
    
    internal static DummyCreateCommandWithNullableReferenceTypeProperty GetValidDummyRequestWithNullableReferenceTypeProperty()
    {
        return new DummyCreateCommandWithNullableReferenceTypeProperty(null, Random.Shared.Next());
    }
    
    internal static DummyCreateCommandWithNullableValueTypeProperty GetValidDummyRequestWithNullableValueTypeProperty()
    {
        return new DummyCreateCommandWithNullableValueTypeProperty("TestValue1", null);
    }
    
    internal static DummyCreateCommandWithMismatchNumberProperties GetValidDummyRequestWithMismatchProperties()
    {
        return new DummyCreateCommandWithMismatchNumberProperties("TestValue1", Random.Shared.Next(), "TestValue123");
    }
    
    internal static DummyCreateCommandWithLessPropertiesThanFactoryMethod GetValidDummyRequestWithLessPropertiesThanFactory()
    {
        return new DummyCreateCommandWithLessPropertiesThanFactoryMethod("TestValue1", Random.Shared.Next());
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
internal class DummyEntity2 : EntityBase
{
    public string DummyField1 { get; set; } = string.Empty;
    public int DummyField2 { get; set; }
    
    public DummyEntity2()
    {
    }

    private DummyEntity2(string dummyField1, int dummyField2)
    {
        DummyField1 = dummyField1;
        DummyField2 = dummyField2;
    }

    [FactoryMethod("DummyEntity")]
    protected static void TestDummyEntityFactoryMethod(string dummyField1, int dummyField2)
    {
        var returnType = new DummyEntity2(dummyField1, dummyField2);
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

[AggregateRoot]
internal class DummyEntityForCachingTest : EntityBase
{
    public string DummyField5 { get; set; } = string.Empty;
    public int DummyField6 { get; set; }

    public DummyEntityForCachingTest()
    {
    }
    
    private DummyEntityForCachingTest(string dummyField5, int dummyField6)
    {
        DummyField5 = dummyField5;
        DummyField6 = dummyField6;
    }
    
    [FactoryMethod]
    internal static DummyEntityForCachingTest DummyCachedFactoryMethod(string dummyField5, int dummyField6)
    {
        return new DummyEntityForCachingTest(dummyField5, dummyField6);
    }
}

[AggregateRoot]
internal class DummyEntityWithNullableValueTypeProperty : EntityBase
{
    public string DummyField5 { get; set; } = string.Empty; 
    public int? DummyField6 { get; set; }

    public DummyEntityWithNullableValueTypeProperty()
    {
    }
    
    private DummyEntityWithNullableValueTypeProperty(string dummyField5, int? dummyField6)
    {
        DummyField5 = dummyField5;
        DummyField6 = dummyField6;
    }
    
    [FactoryMethod]
    internal static DummyEntityWithNullableValueTypeProperty DummyFactoryMethodForNullableValueTypeScenario(string dummyField5, int? dummyField6)
    {
        return new DummyEntityWithNullableValueTypeProperty(dummyField5, dummyField6);
    }
}

[AggregateRoot]
internal class DummyEntityWithNullableReferenceTypeProperty : EntityBase
{
    public string DummyField5 { get; set; } = string.Empty; 
    public int? DummyField6 { get; set; }

    public DummyEntityWithNullableReferenceTypeProperty()
    {
    }
    
    private DummyEntityWithNullableReferenceTypeProperty(string dummyField5, int? dummyField6)
    {
        DummyField5 = dummyField5;
        DummyField6 = dummyField6;
    }
    
    [FactoryMethod]
    internal static DummyEntityWithNullableReferenceTypeProperty DummyFactoryMethodForNullableReferenceTypeScenario(string dummyField5, int? dummyField6)
    {
        return new DummyEntityWithNullableReferenceTypeProperty(dummyField5, dummyField6);
    }
}

[AggregateRoot]
internal class DummyEntityWithMismatchProperties : EntityBase
{
    public string DummyField5 { get; set; } = string.Empty; 
    public int? DummyField6 { get; set; }

    public DummyEntityWithMismatchProperties()
    {
    }
    
    private DummyEntityWithMismatchProperties(string dummyField5, int? dummyField6)
    {
        DummyField5 = dummyField5;
        DummyField6 = dummyField6;
    }
    
    [FactoryMethod]
    internal static DummyEntityWithMismatchProperties DummyFactoryMethodForMismatchNumberOfProperties(string dummyField5, int? dummyField6)
    {
        return new DummyEntityWithMismatchProperties(dummyField5, dummyField6);
    }
}

[AggregateRoot]
internal class DummyEntityWithMorePropertiesThanCommandRecord : EntityBase
{
    public string DummyField5678 { get; set; } = string.Empty; 
    public int? DummyField9876 { get; set; }        
    public string AdditionalProperty { get; set; } = string.Empty;

    public DummyEntityWithMorePropertiesThanCommandRecord()
    {
    }
    
    private DummyEntityWithMorePropertiesThanCommandRecord(string dummyField5678, int? dummyField9876, string additionalProperty)
    {
        DummyField5678 = dummyField5678;
        DummyField9876 = dummyField9876;
        AdditionalProperty = additionalProperty;
    }
    
    [FactoryMethod]
    internal static DummyEntityWithMorePropertiesThanCommandRecord DummyFactoryMethod(string dummyField5678, int? dummyField9876, string additionalProperty)
    {
        return new DummyEntityWithMorePropertiesThanCommandRecord(dummyField5678, dummyField9876, additionalProperty);
    }
}
