using System.Diagnostics.CodeAnalysis;
using Ghanavats.Domain.Factory.Abstractions;
using Ghanavats.Domain.Factory.Attributes;
using Ghanavats.Domain.Factory.Handlers;
using Ghanavats.Domain.Factory.Tests.DummyData;
using Moq;
using Shouldly;
using Xunit;

namespace Ghanavats.Domain.Factory.Tests;

[ExcludeFromCodeCoverage]
public class FactoryMethodHandlerTests
{
    private readonly Mock<ICacheProvider> _mockCacheProvider;
    private readonly FactoryMethodHandler _sut;
    public FactoryMethodHandlerTests()
    {
        _mockCacheProvider = new Mock<ICacheProvider>();
        _sut = new FactoryMethodHandler(_mockCacheProvider.Object);
    }

    [Fact]
    public void GetFactoryMethod_ShouldReturnNull_WhenFactoryMethodNotDecoratedWithCorrectAttribute()
    {
        //Arrange
        var expectedType = DummyFactoryMethodHandlerData.GetValidType();

        _mockCacheProvider.Setup(x => x.Get(It.IsAny<object>()))
            .Returns(string.Empty);
        
        //Act
        var result = _sut.GetFactoryMethod(expectedType);

        //Assert
        result.ShouldBeNull();
    }
    
    [Fact]
    public void GetFactoryMethod_ShouldReturnCorrectMethodInfo_WhenFactoryMethodDecoratedWithCorrectAttributeWithoutOptionalNameParameter()
    {
        //Arrange
        var expectedType = DummyFactoryMethodHandlerData.GetValidTypeForTestingCorrectReturnTypeWithoutOptionalAttributeParameter();

        _mockCacheProvider.Setup(x => x.Get(It.IsAny<object>()))
            .Returns(string.Empty);
        
        //Act
        var result = _sut.GetFactoryMethod(expectedType);

        //Assert
        result.ShouldNotBeNull();
        result.CustomAttributes.ShouldNotBeEmpty();
        result.CustomAttributes.ShouldContain(data => data.AttributeType.IsEquivalentTo(typeof(FactoryMethodAttribute)));
        
        _mockCacheProvider.Verify(x => x.Insert(It.IsAny<object>(), It.IsAny<object>()), Times.Once);
    }
    
    [Fact]
    public void GetFactoryMethod_ShouldReturnNull_WhenFactoryMethodDecoratedWithCorrectAttributeButWithOptionalNameParameterWithIncorrectValue()
    {
        //Arrange
        var expectedType = DummyFactoryMethodHandlerData.GetValidTypeWithIncorrectlyConfiguredFactoryMethod();

        _mockCacheProvider.Setup(x => x.Get(It.IsAny<object>()))
            .Returns(string.Empty);
        
        //Act
        var result = _sut.GetFactoryMethod(expectedType);

        //Assert
        result.ShouldBeNull();
    }
}
