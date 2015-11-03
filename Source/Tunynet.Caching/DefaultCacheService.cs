using System;
using System.Collections.Generic;
namespace Tunynet.Caching
{
	[System.Serializable]
	public class DefaultCacheService : ICacheService
	{
		private ICache localCache;
		private ICache cache;
		private readonly System.Collections.Generic.Dictionary<CachingExpirationType, System.TimeSpan> cachingExpirationDictionary;
		private bool enableDistributedCache;
		public bool EnableDistributedCache
		{
			get
			{
				return this.enableDistributedCache;
			}
		}
		public DefaultCacheService(ICache cache, float cacheExpirationFactor) : this(cache, cache, cacheExpirationFactor, false)
		{
		}
		public DefaultCacheService(ICache cache, ICache localCache, float cacheExpirationFactor, bool enableDistributedCache)
		{
			this.cache = cache;
			this.localCache = localCache;
			this.enableDistributedCache = enableDistributedCache;
			this.cachingExpirationDictionary = new System.Collections.Generic.Dictionary<CachingExpirationType, System.TimeSpan>();
			this.cachingExpirationDictionary.Add(CachingExpirationType.Invariable, new System.TimeSpan(0, 0, (int)(86400f * cacheExpirationFactor)));
			this.cachingExpirationDictionary.Add(CachingExpirationType.Stable, new System.TimeSpan(0, 0, (int)(28800f * cacheExpirationFactor)));
			this.cachingExpirationDictionary.Add(CachingExpirationType.RelativelyStable, new System.TimeSpan(0, 0, (int)(7200f * cacheExpirationFactor)));
			this.cachingExpirationDictionary.Add(CachingExpirationType.UsualSingleObject, new System.TimeSpan(0, 0, (int)(600f * cacheExpirationFactor)));
			this.cachingExpirationDictionary.Add(CachingExpirationType.UsualObjectCollection, new System.TimeSpan(0, 0, (int)(300f * cacheExpirationFactor)));
			this.cachingExpirationDictionary.Add(CachingExpirationType.SingleObject, new System.TimeSpan(0, 0, (int)(180f * cacheExpirationFactor)));
			this.cachingExpirationDictionary.Add(CachingExpirationType.ObjectCollection, new System.TimeSpan(0, 0, (int)(180f * cacheExpirationFactor)));
		}
		public void Add(string cacheKey, object value, CachingExpirationType cachingExpirationType)
		{
			this.Add(cacheKey, value, this.cachingExpirationDictionary[cachingExpirationType]);
		}
		public void Add(string cacheKey, object value, System.TimeSpan timeSpan)
		{
			this.cache.Add(cacheKey, value, timeSpan);
		}
		public void Set(string cacheKey, object value, CachingExpirationType cachingExpirationType)
		{
			this.Set(cacheKey, value, this.cachingExpirationDictionary[cachingExpirationType]);
		}
		public void Set(string cacheKey, object value, System.TimeSpan timeSpan)
		{
			this.cache.Set(cacheKey, value, timeSpan);
		}
		public void Remove(string cacheKey)
		{
			this.cache.Remove(cacheKey);
		}
		public void MarkDeletion(string cacheKey, IEntity entity, CachingExpirationType cachingExpirationType)
		{
			entity.IsDeletedInDatabase = true;
			this.cache.MarkDeletion(cacheKey, entity, this.cachingExpirationDictionary[cachingExpirationType]);
		}
		public void Clear()
		{
			this.cache.Clear();
		}
		public object Get(string cacheKey)
		{
			object obj = null;
			if (this.enableDistributedCache)
			{
				obj = this.localCache.Get(cacheKey);
			}
			if (obj == null)
			{
				obj = this.cache.Get(cacheKey);
				if (this.enableDistributedCache)
				{
					this.localCache.Add(cacheKey, obj, this.cachingExpirationDictionary[CachingExpirationType.SingleObject]);
				}
			}
			return obj;
		}
		public T Get<T>(string cacheKey) where T : class
		{
			object obj = this.Get(cacheKey);
			if (obj != null)
			{
				return obj as T;
			}
			return default(T);
		}
		public object GetFromFirstLevel(string cacheKey)
		{
			return this.cache.Get(cacheKey);
		}
		public T GetFromFirstLevel<T>(string cacheKey) where T : class
		{
			object fromFirstLevel = this.GetFromFirstLevel(cacheKey);
			if (fromFirstLevel != null)
			{
				return fromFirstLevel as T;
			}
			return default(T);
		}
	}
}
