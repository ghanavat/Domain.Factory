namespace Ghanavats.Domain.Factory.Abstractions;

public interface IReadCache
{
    /// <summary>
    /// Retrieves cache value from memory for the given key
    /// </summary>
    /// <param name="key">Cache Key</param>
    /// <returns>Cache Value stored in cache for the given key</returns>
    object Get(object key);
}
