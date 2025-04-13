using Ghanavats.Domain.Primitives;

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
