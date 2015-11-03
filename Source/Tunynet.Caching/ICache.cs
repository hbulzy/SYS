using System;
namespace Tunynet.Caching
{
	public interface ICache
	{
		void Add(string key, object value, System.TimeSpan timeSpan);
		void AddWithFileDependency(string key, object value, string fullFileNameOfFileDependency);
		void Set(string key, object value, System.TimeSpan timeSpan);
		void MarkDeletion(string key, object value, System.TimeSpan timeSpan);
		object Get(string cacheKey);
		void Remove(string cacheKey);
		void Clear();
	}
}
