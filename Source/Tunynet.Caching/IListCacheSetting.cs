using System;
namespace Tunynet.Caching
{
	public interface IListCacheSetting
	{
		CacheVersionType CacheVersionType
		{
			get;
		}
		string AreaCachePropertyName
		{
			get;
			set;
		}
		object AreaCachePropertyValue
		{
			get;
			set;
		}
	}
}
