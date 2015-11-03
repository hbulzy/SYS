using System;
namespace Tunynet.Caching
{
	public interface ICacheService
	{
		bool EnableDistributedCache
		{
			get;
		}
		void Add(string cacheKey, object value, CachingExpirationType cachingExpirationType);
		void Add(string cacheKey, object value, System.TimeSpan timeSpan);
		void Set(string cacheKey, object value, CachingExpirationType cachingExpirationType);
		void Set(string cacheKey, object value, System.TimeSpan timeSpan);
		void Remove(string cacheKey);
		void MarkDeletion(string cacheKey, IEntity entity, CachingExpirationType cachingExpirationType);
		void Clear();
		object Get(string cacheKey);
		T Get<T>(string cacheKey) where T : class;
		object GetFromFirstLevel(string cacheKey);
		T GetFromFirstLevel<T>(string cacheKey) where T : class;
	}
}
