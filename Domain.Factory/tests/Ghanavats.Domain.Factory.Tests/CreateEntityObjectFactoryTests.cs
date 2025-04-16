using Ghanavats.Domain.Factory.Tests.DummyData;
using Xunit;

namespace Ghanavats.Domain.Factory.Tests;

public class CreateEntityObjectFactoryTests
{
    private readonly CreateEntityObjectFactory<object, DummyResponseEntity> _sut;
    
    public CreateEntityObjectFactoryTests()
    {
        _sut = new CreateEntityObjectFactory<object, DummyResponseEntity>();
    }
    
    [Fact]
    public void CreateEntityObject_ShouldReturnCorrectResponseWithError_WhenResponseEntityIsNotAggregateRoot()
    {
        //Arrange
        var request = DummyCreateEntityObjectFactoryData.GetValidDummyRequest();
        
        //Act
        var actual = _sut.CreateEntityObject(request);

        //Assert
        Assert.NotNull(actual);
        Assert.Null(actual.Value);
        Assert.Equal("Operation is not allowed. The response type of Ghanavats.Domain.Factory.Tests.DummyData.DummyResponseEntity is not an Aggregate Root object.",
            actual.ErrorMessage);
    }
}
