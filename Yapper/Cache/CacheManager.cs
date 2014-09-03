using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using Augment;

namespace Yapper.Cache
{
    /// <summary>
    /// 
    /// </summary>
    public class CacheManager : ICacheManager
    {
        #region ICacheManager Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(CacheKey key)
        {
            return MemoryCache.Default.Contains(key.Key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="policy"></param>
        /// <returns></returns>
        public bool Add(CacheKey key, object value, CachePolicy policy)
        {
            CacheItem item = new CacheItem(key.Key, value);

            CacheItemPolicy cachePolicy = CreatePolicy(key, policy);

            CacheItem existing = MemoryCache.Default.AddOrGetExisting(item, cachePolicy);

            return existing.Value == null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="policy"></param>
        public void Set(CacheKey key, object value, CachePolicy policy)
        {
            CacheItem item = new CacheItem(key.Key, value);

            CacheItemPolicy cachePolicy = CreatePolicy(key, policy);

            MemoryCache.Default.Set(item, cachePolicy);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(CacheKey key)
        {
            return MemoryCache.Default.Get(key.Key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Remove(CacheKey key)
        {
            return MemoryCache.Default.Remove(key.Key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        public void Expire(string tag)
        {
            string key = GetFormattedTag(tag);

            CacheItem item = new CacheItem(key, DateTimeOffset.UtcNow.Ticks);
            
            CacheItemPolicy cachePolicy = new CacheItemPolicy { AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration };

            MemoryCache.Default.Set(item, cachePolicy);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        protected string GetFormattedTag(string tag)
        {
            return "global::tag::{0}".FormatArgs(tag);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cachePolicy"></param>
        /// <returns></returns>
        protected CacheItemPolicy CreatePolicy(CacheKey key, CachePolicy cachePolicy)
        {
            var policy = new CacheItemPolicy();

            switch (cachePolicy.Mode)
            {
                case CacheExpirationMode.Sliding:
                    policy.SlidingExpiration = cachePolicy.SlidingExpiration;
                    break;

                case CacheExpirationMode.Absolute:
                    policy.AbsoluteExpiration = cachePolicy.AbsoluteExpiration;
                    break;

                case CacheExpirationMode.Duration:
                    policy.AbsoluteExpiration = DateTimeOffset.Now.Add(cachePolicy.Duration);
                    break;

                default:
                    policy.AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration;
                    break;
            }

            CacheEntryChangeMonitor changeMonitor = CreateChangeMonitor(key);

            if (changeMonitor != null)
            {
                policy.ChangeMonitors.Add(changeMonitor);
            }

            return policy;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected CacheEntryChangeMonitor CreateChangeMonitor(CacheKey key)
        {
            if (key.Tags == null || key.Tags.Count == 0)
            {
                return null;
            }

            MemoryCache cache = MemoryCache.Default;

            IList<string> tags = key.Tags.Select(GetFormattedTag).ToList();

            foreach (string tag in tags)
            {
                cache.AddOrGetExisting(tag, DateTimeOffset.UtcNow.Ticks, ObjectCache.InfiniteAbsoluteExpiration);
            }

            return cache.CreateCacheEntryChangeMonitor(tags);
        }

        #endregion
    }
}