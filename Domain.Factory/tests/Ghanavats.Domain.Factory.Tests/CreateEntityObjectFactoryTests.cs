using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Ghanavats.Domain.Factory.Abstractions;
using Ghanavats.Domain.Factory.Abstractions.ActionOptions;
using Ghanavats.Domain.Factory.Handlers;
using Ghanavats.Domain.Factory.Tests.DummyData;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Shouldly;
using Xunit;

namespace Ghanavats.Domain.Factory.Tests;

[ExcludeFromCodeCoverage]
public class CreateEntityObjectFactoryTests
{
    private readonly Mock<IFactoryMethodHandler> _mockFactoryMethodHandler;
    private readonly Mock<ICacheProvider> _mockCacheProvider;
    private readonly Mock<IReadCache> _mockReadCache;
    private readonly Mock<ICacheEntry> _mockCacheEntry;
    private readonly Mock<IMemoryCache> _mockMemoryCache;
    
    public CreateEntityObjectFactoryTests()
    {
        _mockFactoryMethodHandler = new Mock<IFactoryMethodHandler>();
        _mockCacheProvider = new Mock<ICacheProvider>();
        _mockReadCache = new Mock<IReadCache>();
        _mockCacheEntry = new Mock<ICacheEntry>();
        _mockMemoryCache = new Mock<IMemoryCache>();
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

    [Fact]
    public void CreateEntityObject_ShouldUseTheCachedMethodInfo()
    {
        //Arrange
        var expectedCacheKey = "DummyEntityForCachingTest.FactoryMethod";
        
        var sut = new CreateEntityObjectFactory<DummyCreateEntityObjectFactoryData.DummyCreateCommand, 
            DummyEntityForCachingTest>(_mockFactoryMethodHandler.Object, _mockReadCache.Object);
        
        var request = DummyCreateEntityObjectFactoryData.GetValidDummyRequest();
        
        var expectedMethodInfo = typeof(DummyEntityForCachingTest)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .FirstOrDefault(x => x.Name == nameof(DummyEntityForCachingTest.DummyCachedFactoryMethod));
        
        _mockMemoryCache.Setup(x => x.CreateEntry(expectedCacheKey))
            .Returns(_mockCacheEntry.Object.SetValue(expectedMethodInfo));

        _mockReadCache.Setup(x => x.Get(expectedCacheKey)).Returns(expectedMethodInfo!);
        
        _mockFactoryMethodHandler.Setup(x => x.GetFactoryMethod(It.IsAny<Type>()))
            .Returns(expectedMethodInfo);
        
        //Act
        var actual = sut.CreateEntityObject(request);
        
        //Arrange
        actual.ShouldNotBeNull();
        actual.Cache.ShouldBeEquivalentTo(expectedMethodInfo);
        _mockFactoryMethodHandler.Verify(x => x.GetFactoryMethod(It.IsAny<Type>()), Times.Once);
        
        //Doing this to ensure the cached Factory MethodInfo is used by verifying a new cache entry was NOT created.
        _mockCacheProvider.Verify(x => x.Insert(It.IsAny<object>(), It.IsAny<object>()), Times.Never());
        
        _mockReadCache.Verify(x => x.Get(It.IsAny<object>()), Times.Once());
    }

    [Fact]
    public void CreateEntityObject_ShouldReturnError_WhenNumberOfPropertiesInRequestIsZero()
    {
        //Arrange
        var sut = new CreateEntityObjectFactory<DummyCreateEntityObjectFactoryData.DummyCreateCommandWithNoProperties, 
            DummyEntity>(_mockFactoryMethodHandler.Object, _mockReadCache.Object);
        
        var request = DummyCreateEntityObjectFactoryData.GetInvalidDummyRequest();
        
        var expectedMethodInfo = typeof(DummyEntity)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .FirstOrDefault(x => x.Name == nameof(DummyEntity.TestDummyEntityFactoryMethod));
        
        _mockFactoryMethodHandler.Setup(x => x.GetFactoryMethod(It.IsAny<Type>()))
            .Returns(expectedMethodInfo);
        
        //Act
        var actual = sut.CreateEntityObject(request);
        
        //Assert
        actual.ShouldNotBeNull();
        actual.ErrorMessage.ShouldBe("No parameters found in the request type DummyCreateCommandWithNoProperties.");
    }

    [Fact]
    public void CreateEntityObject_ShouldCorrectlyReturnInstantiatedEntity_WhenRemoveIgnoredPropertyListHasItems()
    {
        //Arrange
        var sut = new CreateEntityObjectFactory<DummyCreateEntityObjectFactoryData.DummyCreateCommandWithPropertyToBeIgnored, 
            DummyEntity>(_mockFactoryMethodHandler.Object, _mockReadCache.Object);
        
        var request = DummyCreateEntityObjectFactoryData.GetValidDummyRequestWithIgnoreProperty();
        
        var expectedMethodInfo = typeof(DummyEntity)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .FirstOrDefault(x => x.Name == nameof(DummyEntity.TestDummyEntityFactoryMethod));
        
        _mockFactoryMethodHandler.Setup(x => x.GetFactoryMethod(It.IsAny<Type>()))
            .Returns(expectedMethodInfo);
        
        //Act
        var actual = sut.CreateEntityObject(request, options =>
        {
            options.IgnoreProperties([
                "IgnoreA"
            ]);
        });
        
        //Assert
        actual.ShouldNotBeNull();
        actual.Value.ShouldNotBeNull();
        actual.Value.GetType().ShouldBe(typeof(DummyEntity));
        actual.ErrorMessage.ShouldBeNullOrEmpty();
    }

    [Fact]
    public void CreateEntityObject_ShouldCorrectlyHandleAdditionalProperties_WhenTheActionListIsPopulated()
    {
        //Arrange
        var sut = new CreateEntityObjectFactory<DummyCreateEntityObjectFactoryData.DummyCreateCommandWithLessPropertiesThanFactoryMethod, 
            DummyEntityWithMorePropertiesThanCommandRecord>(_mockFactoryMethodHandler.Object, _mockReadCache.Object);
        
        var request = DummyCreateEntityObjectFactoryData.GetValidDummyRequestWithLessPropertiesThanFactory();
        
        var expectedMethodInfo = typeof(DummyEntityWithMorePropertiesThanCommandRecord)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .FirstOrDefault(x => x.Name == nameof(DummyEntityWithMorePropertiesThanCommandRecord.DummyFactoryMethod));
        
        _mockFactoryMethodHandler.Setup(x => x.GetFactoryMethod(It.IsAny<Type>()))
            .Returns(expectedMethodInfo);
        
        //Act
        var actual = sut.CreateEntityObject(request, options =>
        {
            options.AddProperties(new Dictionary<string, object>
            {
                {"AdditionalProperty", "SomeTestValue"}
            }.ToImmutableDictionary());
        });
        
        //Assert
        actual.ShouldNotBeNull();
        actual.Value.ShouldNotBeNull();
        actual.Value.GetType().ShouldBe(typeof(DummyEntityWithMorePropertiesThanCommandRecord));
        actual.ErrorMessage.ShouldBeNullOrEmpty();
    }

    [Fact]
    public void CreateEntityObject_ShouldNotThrowException_WhenNullableValueTypePropertyIdentifiedWithNullValue()
    {
        //Arrange
        var sut = new CreateEntityObjectFactory<DummyCreateEntityObjectFactoryData.DummyCreateCommandWithNullableValueTypeProperty, 
            DummyEntityWithNullableValueTypeProperty>(_mockFactoryMethodHandler.Object, _mockReadCache.Object);
        
        var request = DummyCreateEntityObjectFactoryData.GetValidDummyRequestWithNullableValueTypeProperty();
        
        var expectedMethodInfo = typeof(DummyEntityWithNullableValueTypeProperty)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .FirstOrDefault(x => x.Name == nameof(DummyEntityWithNullableValueTypeProperty.DummyFactoryMethodForNullableValueTypeScenario));
        
        _mockFactoryMethodHandler.Setup(x => x.GetFactoryMethod(It.IsAny<Type>()))
            .Returns(expectedMethodInfo);
        
        //Act
        var actual = sut.CreateEntityObject(request);

        //Arrange
        actual.ShouldNotBeNull();
        actual.Value.ShouldNotBeNull();
        actual.Value.GetType().ShouldBe(typeof(DummyEntityWithNullableValueTypeProperty));
        actual.ErrorMessage.ShouldBeNullOrEmpty();
    }
    
    [Fact]
    public void CreateEntityObject_ShouldNotThrowException_WhenNullableRefTypePropertyIdentifiedWithNullValue()
    {
        //Arrange
        var sut = new CreateEntityObjectFactory<DummyCreateEntityObjectFactoryData.DummyCreateCommandWithNullableReferenceTypeProperty, 
            DummyEntityWithNullableReferenceTypeProperty>(_mockFactoryMethodHandler.Object, _mockReadCache.Object);
        
        var request = DummyCreateEntityObjectFactoryData.GetValidDummyRequestWithNullableReferenceTypeProperty();
        
        var expectedMethodInfo = typeof(DummyEntityWithNullableReferenceTypeProperty)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .FirstOrDefault(x => x.Name == nameof(DummyEntityWithNullableReferenceTypeProperty.DummyFactoryMethodForNullableReferenceTypeScenario));
        
        _mockFactoryMethodHandler.Setup(x => x.GetFactoryMethod(It.IsAny<Type>()))
            .Returns(expectedMethodInfo);
        
        //Act
        var actual = sut.CreateEntityObject(request);

        //Arrange
        actual.ShouldNotBeNull();
        actual.Value.ShouldNotBeNull();
        actual.Value.GetType().ShouldBe(typeof(DummyEntityWithNullableReferenceTypeProperty));
        actual.ErrorMessage.ShouldBeNullOrEmpty();
    }

    [Fact]
    public void CreateEntityObject_ShouldThrowException_WhenFactoryMethodFailedToInvoke()
    {
        //Arrange
        var sut = new CreateEntityObjectFactory<DummyCreateEntityObjectFactoryData.DummyCreateCommandWithMismatchNumberProperties, 
            DummyEntityWithMismatchProperties>(_mockFactoryMethodHandler.Object, _mockReadCache.Object);
        
        var request = DummyCreateEntityObjectFactoryData.GetValidDummyRequestWithMismatchProperties();
        
        var expectedMethodInfo = typeof(DummyEntityWithMismatchProperties)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .FirstOrDefault(x => x.Name == nameof(DummyEntityWithMismatchProperties.DummyFactoryMethodForMismatchNumberOfProperties));
        
        _mockFactoryMethodHandler.Setup(x => x.GetFactoryMethod(It.IsAny<Type>()))
            .Returns(expectedMethodInfo);
        
        //Act/Assert
        var action = new Action(() => sut.CreateEntityObject(request));
        action.ShouldThrow<TargetParameterCountException>();
    }

    [Fact]
    public void CreateEntityObject_ShouldReturnCorrectError_WhenInvokeMethodInfoReturnsNull()
    {
        //Arrange
        var sut = new CreateEntityObjectFactory<DummyCreateEntityObjectFactoryData.DummyCreateCommand, 
            DummyEntity2>(_mockFactoryMethodHandler.Object, _mockReadCache.Object);
        
        var request = DummyCreateEntityObjectFactoryData.GetValidDummyRequest();
        
        var expectedMethodInfo = typeof(DummyEntity2)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .FirstOrDefault(x => x.Name == "TestDummyEntityFactoryMethod");
        
        _mockFactoryMethodHandler.Setup(x => x.GetFactoryMethod(It.IsAny<Type>()))
            .Returns(expectedMethodInfo);
        
        //Act
        var actual = sut.CreateEntityObject(request);

        //Assert
        actual.ShouldNotBeNull();
        actual.Value.ShouldBeNull();
        actual.ErrorMessage.ShouldBe("Could not invoke the factory method with the given parameters.");
    }
}
