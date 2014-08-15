using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsureThat;

namespace Yapper.Cache
{
    /// <summary>
    /// A class representing a unique key for cache items.
    /// </summary>
    public class CacheKey
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheKey"/> class.
        /// </summary>
        /// <param name="key">The key for a cache item.</param>
        public CacheKey(string key)
            : this(key, Enumerable.Empty<string>())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheKey"/> class.
        /// </summary>
        /// <param name="key">The key for a cache item.</param>
        /// <param name="tags">The tags for the cache item.</param>
        public CacheKey(string key, IEnumerable<string> tags)
        {
            Ensure.That(key, "key").IsNotNullOrWhiteSpace();

            Key = key;

            if (tags != null)
            {
                Tags = new HashSet<string>(tags);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the key for a cached item.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Gets the tags for a cached item.
        /// </summary>
        public HashSet<string> Tags { get; private set; }

        #endregion
    }
}
