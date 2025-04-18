using System.Reflection;
using Ghanavats.Domain.Factory.Abstractions;
using Ghanavats.Domain.Factory.Abstractions.ActionOptions;
using Ghanavats.Domain.Factory.Handlers;
using Ghanavats.Domain.Factory.Tests.DummyData;
using Moq;
using Shouldly;
using Xunit;

namespace Ghanavats.Domain.Factory.Tests;

public class CreateEntityObjectFactoryTests
{
    private readonly Mock<IFactoryMethodHandler> _mockFactoryMethodHandler;
    private readonly Mock<IReadCache> _mockReadCache;
    
    public CreateEntityObjectFactoryTests()
    {
        _mockFactoryMethodHandler = new Mock<IFactoryMethodHandler>();
        _mockReadCache = new Mock<IReadCache>();
    }
    
    [Fact]
    public void CreateEntityObject_ShouldReturnCorrectResponseWithError_WhenResponseEntityIsNotAggregateRoot()
    {
        //Arrange
        var sut = new CreateEntityObjectFactory<DummyCreateEntityObjectFactoryData.DummyCreateCommand, 
            DummyResponseEntity>(_mockFactoryMethodHandler.Object, _mockReadCache.Object);
        
        var request = DummyCreateEntityObjectFactoryData.GetValidDummyRequest();
        
        //Act
        var actual = sut.CreateEntityObject(request);

        //Assert
        Assert.NotNull(actual);
        Assert.Null(actual.Value);
        Assert.Equal("Operation is not allowed. The response type DummyResponseEntity is not an Aggregate Root.",
            actual.ErrorMessage);
        
        _mockFactoryMethodHandler.Verify(x => x.GetFactoryMethod(It.IsAny<Type>()), Times.Never);
    }
    
    [Fact]
    public void CreateEntityObject_ShouldCorrectlyPopulateOptionalAction_WhenItIsSuppliedByConsumer()
    {
        //Arrange
        var sut = new CreateEntityObjectFactory<DummyCreateEntityObjectFactoryData.DummyCreateCommand, 
            DummyEntity>(_mockFactoryMethodHandler.Object, _mockReadCache.Object);
        
        var request = DummyCreateEntityObjectFactoryData.GetValidDummyRequest();
        var actionWasInvoked = false;
        
        //Act
        var actual = sut.CreateEntityObject(request, Action);
        
        //Assert
        actual.ShouldNotBeNull();
        actionWasInvoked.ShouldBeTrue();

        return;

        void Action(DomainFactoryOption factoryOption) => actionWasInvoked = true;
    }

    [Fact]
    public void CreateEntityObject_ShouldReturnResponseWithFailure_WhenFactoryMethodReturnedNull()
    {
        //Arrange
        var sut = new CreateEntityObjectFactory<DummyCreateEntityObjectFactoryData.DummyCreateCommand, 
            DummyWithNoFactoryMethodAttrEntity>(_mockFactoryMethodHandler.Object, _mockReadCache.Object);
        
        var request = DummyCreateEntityObjectFactoryData.GetValidDummyRequest();

        _mockFactoryMethodHandler.Setup(x => x.GetFactoryMethod(It.IsAny<Type>())).Returns((MethodInfo?)null);
        
        //Act
        var actual = sut.CreateEntityObject(request);
        
        //Assert
        actual.ShouldNotBeNull();
        actual.ErrorMessage.ShouldBe("Could not get/find the factory method for type DummyWithNoFactoryMethodAttrEntity.");
        _mockFactoryMethodHandler.Verify(x => x.GetFactoryMethod(It.IsAny<Type>()), Times.Once);
    }

    [Fact]
    public void CreateEntityObject_ShouldReturnResponseWithError_WhenEntityFactoryMethodWithName_Violated()
    {
        //Arrange
        var sut = new CreateEntityObjectFactory<DummyCreateEntityObjectFactoryData.DummyCreateCommand, 
            DummyWithFactoryMethodAttrEntityViolatedName>(_mockFactoryMethodHandler.Object, _mockReadCache.Object);
        
        var request = DummyCreateEntityObjectFactoryData.GetValidDummyRequest();
        
        //Act
        var actual = sut.CreateEntityObject(request);
        
        //Assert
        actual.ShouldNotBeNull();
        actual.ErrorMessage.ShouldBe("Could not get/find the factory method for type DummyWithFactoryMethodAttrEntityViolatedName.");
        _mockFactoryMethodHandler.Verify(x => x.GetFactoryMethod(It.IsAny<Type>()), Times.Once);
    }
    
    [Fact]
    public void CreateEntityObject_ShouldReturnSuccess_WhenEntityFactoryMethodAttrHasNoNameArgument()
    {
        //Arrange
        var sut = new CreateEntityObjectFactory<DummyCreateEntityObjectFactoryData.DummyCreateCommand, 
            DummyWithFactoryMethodAttrEntityWithoutOptionalName>(_mockFactoryMethodHandler.Object, _mockReadCache.Object);
        
        var request = DummyCreateEntityObjectFactoryData.GetValidDummyRequest();

        var expectedFactoryMethod = typeof(DummyWithFactoryMethodAttrEntityWithoutOptionalName)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .FirstOrDefault(x => x.Name == nameof(DummyWithFactoryMethodAttrEntityWithoutOptionalName.DummyFactoryMethod));
        
        _mockFactoryMethodHandler.Setup(x => x.GetFactoryMethod(typeof(DummyWithFactoryMethodAttrEntityWithoutOptionalName)))
            .Returns(expectedFactoryMethod);
        
        //Act
        var actual = sut.CreateEntityObject(request);
        
        //Assert
        actual.ShouldNotBeNull();
        actual.ErrorMessage.ShouldBeNullOrEmpty();
        _mockFactoryMethodHandler.Verify(x => x.GetFactoryMethod(It.IsAny<Type>()), Times.Once);
    }

    [Fact]
    public void CreateEntityObject_ShouldReturnFailure_WhenNoPublicConstructorFoundInEntityClass()
    {
        //Arrange
        var sut = new CreateEntityObjectFactory<DummyCreateEntityObjectFactoryData.DummyCreateCommand, 
            DummyWithNoPublicConstructorEntity>(_mockFactoryMethodHandler.Object, _mockReadCache.Object);
        
        var request = DummyCreateEntityObjectFactoryData.GetValidDummyRequest();
        
        var expectedFactoryMethod = typeof(DummyWithNoPublicConstructorEntity)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .FirstOrDefault(x => x.Name == nameof(DummyWithNoPublicConstructorEntity.DummyFactoryMethod));
        
        _mockFactoryMethodHandler.Setup(x => x.GetFactoryMethod(typeof(DummyWithNoPublicConstructorEntity)))
            .Returns(expectedFactoryMethod);
        
        //Act
        var actual = sut.CreateEntityObject(request);
        
        //Assert
        actual.ShouldNotBeNull();
        actual.ErrorMessage.ShouldBe("Could not find a public constructor.");
    }
}
