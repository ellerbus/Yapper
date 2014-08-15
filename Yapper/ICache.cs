using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yapper.Cache;

namespace Yapper
{
    /// <summary>
    /// The CacheManager with support 
    /// </summary>
    public interface ICacheManager
    {
        #region Members

        /// <summary>
        /// Determins if a cache entry exists
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <returns><c>true</c> if found, otherwise <c>false</c></returns>
        bool Contains(CacheKey key);

        /// <summary>
        /// Inserts a cache entry into the cache without overwriting any existing cache entry.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="policy">Determines expiration policy</param>
        /// <returns><c>true</c> if insertion succeeded, or <c>false</c> if there is an already an entry in the cache that has the same key as key.</returns>
        bool Add(CacheKey key, object value, CachePolicy policy);

        /// <summary>
        /// Inserts a cache entry into the cache overwriting any existing cache entry.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="policy">Determines expiration policy</param>
        /// <returns><c>true</c> if insertion succeeded, or <c>false</c> if there is an already an entry in the cache that has the same key as key.</returns>
        void Set(CacheKey key, object value, CachePolicy policy);

        /// <summary>
        /// Gets the cache value for the specified key
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <returns>The cache value for the specified key, if the entry exists; otherwise, <see langword="null"/>.</returns>
        object Get(CacheKey key);

        /// <summary>
        /// Removes a cache entry from the cache. 
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <returns>If the entry is found in the cache, the removed cache entry; otherwise, <see langword="null"/>.</returns>
        object Remove(CacheKey key);

        /// <summary>
        /// Expires the specified cache tag.
        /// </summary>
        /// <param name="tag">The cache tag.</param>
        void Expire(string tag);

        #endregion
    }


    /// <summary>
    /// Extensions to enhance the simplicity of the API
    /// </summary>
    public static class ICacheManagerExtensions
    {
        #region Absolute Expiration

        /// <summary>
        /// Inserts a cache entry into the cache without overwriting any existing cache entry.
        /// </summary>
        /// <param name="mgr">The interface being extended/enhanced</param>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="absoluteExpiration">Determines expiration CachePolicy.WithAbsoluteExpiration(absoluteExpiration)</param>
        /// <returns><c>true</c> if insertion succeeded, or <c>false</c> if there is an already an entry in the cache that has the same key as key.</returns>
        public static bool Add(this ICacheManager mgr, CacheKey key, object value, DateTimeOffset absoluteExpiration)
        {
            return mgr.Add(key, value, CachePolicy.WithAbsoluteExpiration(absoluteExpiration));
        }

        /// <summary>
        /// Inserts a cache entry into the cache overwriting any existing cache entry.
        /// </summary>
        /// <param name="mgr">The interface being extended/enhanced</param>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="absoluteExpiration">Determines expiration CachePolicy.WithAbsoluteExpiration(absoluteExpiration)</param>
        /// <returns><c>true</c> if insertion succeeded, or <c>false</c> if there is an already an entry in the cache that has the same key as key.</returns>
        public static void Set(this ICacheManager mgr, CacheKey key, object value, DateTimeOffset absoluteExpiration)
        {
            mgr.Set(key, value, CachePolicy.WithAbsoluteExpiration(absoluteExpiration));
        }

        #endregion

        #region Sliding Expiration

        /// <summary>
        /// Inserts a cache entry into the cache without overwriting any existing cache entry.
        /// </summary>
        /// <param name="mgr">The interface being extended/enhanced</param>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="slidingExpiration">Determines expiration CachePolicy.WithSlidingExpiration(slidingExpiration)</param>
        /// <returns><c>true</c> if insertion succeeded, or <c>false</c> if there is an already an entry in the cache that has the same key as key.</returns>
        public static bool Add(this ICacheManager mgr, CacheKey key, object value, TimeSpan slidingExpiration)
        {
            return mgr.Add(key, value, CachePolicy.WithSlidingExpiration(slidingExpiration));
        }

        /// <summary>
        /// Inserts a cache entry into the cache overwriting any existing cache entry.
        /// </summary>
        /// <param name="mgr">The interface being extended/enhanced</param>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="slidingExpiration">Determines expiration CachePolicy.WithSlidingExpiration(slidingExpiration)</param>
        /// <returns><c>true</c> if insertion succeeded, or <c>false</c> if there is an already an entry in the cache that has the same key as key.</returns>
        public static void Set(this ICacheManager mgr, CacheKey key, object value, TimeSpan slidingExpiration)
        {
            mgr.Set(key, value, CachePolicy.WithSlidingExpiration(slidingExpiration));
        }

        #endregion

        #region Cache Key from String

        /// <summary>
        /// Determins if a cache entry exists
        /// </summary>
        /// <param name="mgr">The interface being extended/enhanced</param>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <returns><c>true</c> if found, otherwise <c>false</c></returns>
        public static bool Contains(this ICacheManager mgr, string key)
        {
            return mgr.Contains(key.ToCacheKey());
        }

        /// <summary>
        /// Inserts a cache entry into the cache without overwriting any existing cache entry.
        /// </summary>
        /// <param name="mgr">The interface being extended/enhanced</param>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="policy">Determines expiration policy</param>
        /// <returns><c>true</c> if insertion succeeded, or <c>false</c> if there is an already an entry in the cache that has the same key as key.</returns>
        public static bool Add(this ICacheManager mgr, string key, object value, CachePolicy policy)
        {
            return mgr.Add(key.ToCacheKey(), value, policy);
        }

        /// <summary>
        /// Inserts a cache entry into the cache overwriting any existing cache entry.
        /// </summary>
        /// <param name="mgr">The interface being extended/enhanced</param>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="policy">Determines expiration policy</param>
        /// <returns><c>true</c> if insertion succeeded, or <c>false</c> if there is an already an entry in the cache that has the same key as key.</returns>
        public static void Set(this ICacheManager mgr, string key, object value, CachePolicy policy)
        {
            mgr.Set(key.ToCacheKey(), value, policy);
        }

        /// <summary>
        /// Gets the cache value for the specified key
        /// </summary>
        /// <param name="mgr">The interface being extended/enhanced</param>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <returns>The cache value for the specified key, if the entry exists; otherwise, <see langword="null"/>.</returns>
        public static object Get(this ICacheManager mgr, string key)
        {
            return mgr.Get(key.ToCacheKey());
        }

        /// <summary>
        /// Removes a cache entry from the cache. 
        /// </summary>
        /// <param name="mgr">The interface being extended/enhanced</param>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <returns>If the entry is found in the cache, the removed cache entry; otherwise, <see langword="null"/>.</returns>
        public static object Remove(this ICacheManager mgr, string key)
        {
            return mgr.Remove(key.ToCacheKey());
        }

        #endregion

        #region Cache Key from String : Absolute Expiration

        /// <summary>
        /// Inserts a cache entry into the cache without overwriting any existing cache entry.
        /// </summary>
        /// <param name="mgr">The interface being extended/enhanced</param>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="absoluteExpiration">Determines expiration CachePolicy.WithAbsoluteExpiration(absoluteExpiration)</param>
        /// <returns><c>true</c> if insertion succeeded, or <c>false</c> if there is an already an entry in the cache that has the same key as key.</returns>
        public static bool Add(this ICacheManager mgr, string key, object value, DateTimeOffset absoluteExpiration)
        {
            return mgr.Add(key.ToCacheKey(), value, CachePolicy.WithAbsoluteExpiration(absoluteExpiration));
        }

        /// <summary>
        /// Inserts a cache entry into the cache overwriting any existing cache entry.
        /// </summary>
        /// <param name="mgr">The interface being extended/enhanced</param>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="absoluteExpiration">Determines expiration CachePolicy.WithAbsoluteExpiration(absoluteExpiration)</param>
        /// <returns><c>true</c> if insertion succeeded, or <c>false</c> if there is an already an entry in the cache that has the same key as key.</returns>
        public static void Set(this ICacheManager mgr, string key, object value, DateTimeOffset absoluteExpiration)
        {
            mgr.Set(key.ToCacheKey(), value, CachePolicy.WithAbsoluteExpiration(absoluteExpiration));
        }

        #endregion

        #region Cache Key from String : Sliding Expiration

        /// <summary>
        /// Inserts a cache entry into the cache without overwriting any existing cache entry.
        /// </summary>
        /// <param name="mgr">The interface being extended/enhanced</param>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="slidingExpiration">Determines expiration CachePolicy.WithSlidingExpiration(slidingExpiration)</param>
        /// <returns><c>true</c> if insertion succeeded, or <c>false</c> if there is an already an entry in the cache that has the same key as key.</returns>
        public static bool Add(this ICacheManager mgr, string key, object value, TimeSpan slidingExpiration)
        {
            return mgr.Add(key.ToCacheKey(), value, CachePolicy.WithSlidingExpiration(slidingExpiration));
        }

        /// <summary>
        /// Inserts a cache entry into the cache overwriting any existing cache entry.
        /// </summary>
        /// <param name="mgr">The interface being extended/enhanced</param>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="slidingExpiration">Determines expiration CachePolicy.WithSlidingExpiration(slidingExpiration)</param>
        /// <returns><c>true</c> if insertion succeeded, or <c>false</c> if there is an already an entry in the cache that has the same key as key.</returns>
        public static void Set(this ICacheManager mgr, string key, object value, TimeSpan slidingExpiration)
        {
            mgr.Set(key.ToCacheKey(), value, CachePolicy.WithSlidingExpiration(slidingExpiration));
        }

        #endregion

        #region Helpers

        private static CacheKey ToCacheKey(this string key)
        {
            return new CacheKey(key);
        }

        #endregion
    }
}
