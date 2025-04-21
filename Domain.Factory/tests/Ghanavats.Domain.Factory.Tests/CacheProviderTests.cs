using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Ghanavats.Domain.Factory.CacheService;
using Ghanavats.Domain.Factory.Tests.DummyData;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Shouldly;
using Xunit;

namespace Ghanavats.Domain.Factory.Tests;

[ExcludeFromCodeCoverage]
public class CacheProviderTests
{
    private readonly Mock<IMemoryCache> _mockMemoryCache;
    private readonly Mock<ICacheEntry> _mockCacheEntry;
    
    public CacheProviderTests()
    {
        _mockMemoryCache = new Mock<IMemoryCache>();
        _mockCacheEntry = new Mock<ICacheEntry>();
    }

    [Fact]
    public void CacheProviderGet_ShouldReturnStringEmpty_WhenKeyIsStringEmpty()
    {
        //Arrange
        var sut = new CacheProvider(_mockMemoryCache.Object);

        //Act
        var actual = sut.Get(string.Empty);

        //Assert
        actual.ShouldBe(string.Empty);
    }
    
    [Fact]
    public void CacheProviderGet_ShouldReturnStringEmpty_WhenKeyNotFoundInMemoryCache()
    {
        //Arrange
        var sut = new CacheProvider(_mockMemoryCache.Object);
        
        _mockMemoryCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object?>.IsAny))
            .Returns(false);

        //Act
        var actual = sut.Get("DummyKey.FactoryMethod");

        //Assert
        actual.ShouldBe(string.Empty);
    }
    
    [Fact]
    public void CacheProviderGet_ShouldReturnCachedValue_WhenKeyIsFoundInMemoryCache()
    {
        //Arrange
        var expectedCacheKey = "DummyEntityOne.FactoryMethod";
        
        var sut = new CacheProvider(_mockMemoryCache.Object);
        
        object? expectedMethodInfo = typeof(DummyEntityOne)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .FirstOrDefault(x => x.Name == nameof(DummyEntityOne.DummyFactoryMethod));
        
        _mockMemoryCache.Setup(x => x.CreateEntry(expectedCacheKey))
            .Returns(_mockCacheEntry.Object.SetValue(expectedMethodInfo));

        _mockMemoryCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out expectedMethodInfo))
            .Returns(true).Verifiable();

        //Act
        var actual = sut.Get("DummyKey.FactoryMethod");

        //Assert
        actual.ShouldBeEquivalentTo(expectedMethodInfo);
        _mockMemoryCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out expectedMethodInfo), Times.Once);
    }

    [Fact]
    public void CacheProviderInsert_ShouldReturnStringEmpty_WhenKeyIsEmpty()
    {
        //Arrange
        var sut = new CacheProvider(_mockMemoryCache.Object);
        
        //Act
        var result = sut.Insert(string.Empty, "SomeValue");

        //Assert
        result.ShouldBe(string.Empty);
    }
    
    [Fact]
    public void CacheProviderInsert_ShouldReturnStringEmpty_WhenValueIsEmpty()
    {
        //Arrange
        var sut = new CacheProvider(_mockMemoryCache.Object);
        
        //Act
        var result = sut.Insert("SomeKey", string.Empty);
        
        //Assert
        result.ShouldBe(string.Empty);
    }
    
    [Fact]
    public void CacheProviderInsert_ShouldReturnCachedValue_WhenKeyAndValueCorrectlySupplied()
    {
        //Arrange
        var sut = new CacheProvider(_mockMemoryCache.Object);
        
        object? expectedMethodInfo = typeof(DummyEntityOne)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .FirstOrDefault(x => x.Name == nameof(DummyEntityOne.DummyFactoryMethod));
        
        _mockMemoryCache.Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns(_mockCacheEntry.Object).Verifiable();
        
        //Act
        var result = sut.Insert("SomeKey", expectedMethodInfo!);
        
        //Assert
        result.ShouldNotBe(string.Empty);
        result.ShouldBeEquivalentTo(expectedMethodInfo);
        
        _mockMemoryCache.Verify(x => x.CreateEntry(It.IsAny<object>()), Times.Once);
    }
}
