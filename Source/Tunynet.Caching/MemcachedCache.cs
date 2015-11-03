using Enyim.Caching;
using System;
using System.Collections.Generic;
using System.Web;
namespace Tunynet.Caching
{
	public class MemcachedCache : ICache
	{
		private MemcachedClient cache = new MemcachedClient();
		public void Add(string key, object value, System.TimeSpan timeSpan)
		{
			key = key.ToLower();
			this.cache.Store(3, key, value, System.DateTime.Now.Add(timeSpan));
		}
		public void AddWithFileDependency(string key, object value, string fullFileNameOfFileDependency)
		{
			this.Add(key, value, new System.TimeSpan(30, 0, 0, 0));
		}
		public void Set(string key, object value, System.TimeSpan timeSpan)
		{
			this.Add(key, value, timeSpan);
		}
		public void MarkDeletion(string key, object value, System.TimeSpan timeSpan)
		{
			this.Set(key, value, timeSpan);
		}
		public object Get(string cacheKey)
		{
			cacheKey = cacheKey.ToLower();
			HttpContext current = HttpContext.Current;
			if (current != null && current.Items.Contains(cacheKey))
			{
				return current.Items[cacheKey];
			}
			object obj = this.cache.Get(cacheKey);
			if (current != null && obj != null)
			{
				current.Items[cacheKey] = obj;
			}
			return obj;
		}
		public void Remove(string cacheKey)
		{
			cacheKey = cacheKey.ToLower();
			this.cache.Remove(cacheKey);
		}
		public void Clear()
		{
			this.cache.FlushAll();
		}
		public System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>> GetStatistics()
		{
			throw new System.NotImplementedException();
		}
	}
}
