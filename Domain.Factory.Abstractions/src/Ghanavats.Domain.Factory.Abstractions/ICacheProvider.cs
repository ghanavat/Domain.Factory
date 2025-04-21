namespace Ghanavats.Domain.Factory.Abstractions;

public interface ICacheProvider : IReadCache
{
    /// <summary>
    /// Sets a new Cache Entry for the given key and the value passed in
    /// </summary>
    /// <param name="key">Cache Key for the new Cache Entry</param>
    /// <param name="value">Cache Value for the new Cache Entry</param>
    /// <returns>Cache Value of the new entry</returns>
    object Insert(object key, object value);

    /// <summary>
    /// Sets a new Cache Entry for the given key and the value passed in.
    /// This method accepts an additional TimeSpan parameter to set how long the cache entry should be living for
    /// </summary>
    /// <param name="key">Cache Key for the new Cache Entry</param>
    /// <param name="value">Cache Value for the new Cache Entry</param>
    /// <param name="timeTillExpires">How long the cache should live for</param>
    /// <returns>Cache Value of the new entry</returns>
    object Insert(object key, object value, TimeSpan timeTillExpires);

    /// <summary>
    /// Sets a new Cache Entry for the given key and the value passed in.
    /// This method accepts an additional Datetime parameter to set an absolute expiration
    /// </summary>
    /// <param name="key">Cache Key for the new Cache Entry</param>
    /// <param name="value">Cache Value for the new Cache Entry</param>
    /// <param name="absoluteExpiration">When the cache entry should be expired</param>
    /// <returns>Cache Value of the new entry</returns>
    object Insert(object key, object value, DateTime absoluteExpiration);

    /// <summary>
    /// Removes the cache entry from the memory cache for the given key.
    /// It tries to find if there is an entry for the given key. If found, Remove will be called, otherwise, false will be returned.
    /// </summary>
    /// <param name="key">Cache Key for the Cache Entry to be removed</param>
    /// <returns>Success flag of the Remove call. True, when Remove was successful, false if it was unsuccessful and when Key was not found.</returns>
    object Remove(object key);
}
