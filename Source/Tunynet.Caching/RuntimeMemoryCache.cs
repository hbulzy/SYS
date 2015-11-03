using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
namespace Tunynet.Caching
{
	public class RuntimeMemoryCache : ICache
	{
		private readonly MemoryCache _cache = MemoryCache.Default;
		public void Add(string key, object value, System.TimeSpan timeSpan)
		{
			if (string.IsNullOrEmpty(key) || value == null)
			{
				return;
			}
			this._cache.Add(key, value, System.DateTimeOffset.Now.Add(timeSpan), null);
		}
		public void AddWithFileDependency(string key, object value, string fullFileNameOfFileDependency)
		{
			if (string.IsNullOrEmpty(key) || value == null)
			{
				return;
			}
			CacheItemPolicy cacheItemPolicy = new CacheItemPolicy();
			cacheItemPolicy.AbsoluteExpiration = System.DateTimeOffset.Now.AddMonths(1);
			cacheItemPolicy.ChangeMonitors.Add(new HostFileChangeMonitor(new System.Collections.Generic.List<string>
			{
				fullFileNameOfFileDependency
			}));
			this._cache.Add(key, value, cacheItemPolicy, null);
		}
		public void Set(string key, object value, System.TimeSpan timeSpan)
		{
			this._cache.Set(key, value, System.DateTimeOffset.Now.Add(timeSpan), null);
		}
		public void MarkDeletion(string key, object value, System.TimeSpan timeSpan)
		{
			this.Remove(key);
		}
		public object Get(string cacheKey)
		{
			return this._cache[cacheKey];
		}
		public void Remove(string cacheKey)
		{
			this._cache.Remove(cacheKey, null);
		}
		public void Clear()
		{
			System.Collections.Generic.Dictionary<string, System.Collections.Generic.KeyValuePair<string, object>> dictionary = this._cache.AsParallel<System.Collections.Generic.KeyValuePair<string, object>>().ToDictionary((System.Collections.Generic.KeyValuePair<string, object> a) => a.Key);
			foreach (string current in dictionary.Keys)
			{
				this._cache.Remove(current, null);
			}
		}
	}
}
