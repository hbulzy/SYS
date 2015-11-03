using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
namespace Tunynet.Caching
{
	[System.Serializable]
	public class RealTimeCacheHelper
	{
		private int globalVersion;
		private ConcurrentDictionary<object, int> entityVersionDictionary = new ConcurrentDictionary<object, int>();
		private ConcurrentDictionary<string, ConcurrentDictionary<int, int>> areaVersionDictionary = new ConcurrentDictionary<string, ConcurrentDictionary<int, int>>();
		public bool EnableCache
		{
			get;
			private set;
		}
		public CachingExpirationType CachingExpirationType
		{
			get;
			set;
		}
		public System.Collections.Generic.IEnumerable<System.Reflection.PropertyInfo> PropertiesOfArea
		{
			get;
			set;
		}
		public System.Reflection.PropertyInfo PropertyNameOfBody
		{
			get;
			set;
		}
		public string TypeHashID
		{
			get;
			private set;
		}
		public RealTimeCacheHelper(bool enableCache, string typeHashID)
		{
			this.EnableCache = enableCache;
			this.TypeHashID = typeHashID;
		}
		public int GetGlobalVersion()
		{
			return this.globalVersion;
		}
		public int GetEntityVersion(object primaryKey)
		{
			int result = 0;
			this.entityVersionDictionary.TryGetValue(primaryKey, out result);
			return result;
		}
		public int GetAreaVersion(string propertyName, object propertyValue)
		{
			int result = 0;
			if (string.IsNullOrEmpty(propertyName))
			{
				return result;
			}
			propertyName = propertyName.ToLower();
			ConcurrentDictionary<int, int> concurrentDictionary;
			if (this.areaVersionDictionary.TryGetValue(propertyName, out concurrentDictionary))
			{
				concurrentDictionary.TryGetValue(propertyValue.GetHashCode(), out result);
			}
			return result;
		}
		public void IncreaseGlobalVersion()
		{
			this.globalVersion++;
		}
		public void IncreaseEntityCacheVersion(object entityId)
		{
			ICacheService cacheService = DIContainer.Resolve<ICacheService>();
			if (cacheService.EnableDistributedCache)
			{
				int num;
				if (this.entityVersionDictionary.TryGetValue(entityId, out num))
				{
					num++;
				}
				else
				{
					num = 1;
				}
				this.entityVersionDictionary[entityId] = num;
				this.OnChanged();
			}
		}
		public void IncreaseListCacheVersion(IEntity entity)
		{
			if (this.PropertiesOfArea != null)
			{
				foreach (System.Reflection.PropertyInfo current in this.PropertiesOfArea)
				{
					object value = current.GetValue(entity, null);
					if (value != null)
					{
						this.IncreaseAreaVersion(current.Name.ToLower(), new object[]
						{
							value
						}, false);
					}
				}
			}
			this.IncreaseGlobalVersion();
			this.OnChanged();
		}
		public void IncreaseAreaVersion(string propertyName, object propertyValue)
		{
			if (propertyValue != null)
			{
				this.IncreaseAreaVersion(propertyName, new object[]
				{
					propertyValue
				}, true);
			}
		}
		public void IncreaseAreaVersion(string propertyName, System.Collections.Generic.IEnumerable<object> propertyValues)
		{
			this.IncreaseAreaVersion(propertyName, propertyValues, true);
		}
		private void IncreaseAreaVersion(string propertyName, System.Collections.Generic.IEnumerable<object> propertyValues, bool raiseChangeEvent)
		{
			if (string.IsNullOrEmpty(propertyName))
			{
				return;
			}
			propertyName = propertyName.ToLower();
			int num = 0;
			ConcurrentDictionary<int, int> concurrentDictionary;
			if (!this.areaVersionDictionary.TryGetValue(propertyName, out concurrentDictionary))
			{
				this.areaVersionDictionary[propertyName] = new ConcurrentDictionary<int, int>();
				concurrentDictionary = this.areaVersionDictionary[propertyName];
			}
			foreach (object current in propertyValues)
			{
				int hashCode = current.GetHashCode();
				if (concurrentDictionary.TryGetValue(hashCode, out num))
				{
					num++;
				}
				else
				{
					num = 1;
				}
				concurrentDictionary[hashCode] = num;
			}
			if (raiseChangeEvent)
			{
				this.OnChanged();
			}
		}
		public void MarkDeletion(IEntity entity)
		{
			ICacheService cacheService = DIContainer.Resolve<ICacheService>();
			if (this.EnableCache)
			{
				cacheService.MarkDeletion(this.GetCacheKeyOfEntity(entity.EntityId), entity, CachingExpirationType.SingleObject);
			}
		}
		public string GetCacheKeyOfEntity(object primaryKey)
		{
			ICacheService cacheService = DIContainer.Resolve<ICacheService>();
			if (cacheService.EnableDistributedCache)
			{
				return string.Concat(new object[]
				{
					this.TypeHashID,
					":",
					primaryKey,
					":",
					this.GetEntityVersion(primaryKey)
				});
			}
			return this.TypeHashID + ":" + primaryKey;
		}
		public string GetCacheKeyOfEntityBody(object primaryKey)
		{
			ICacheService cacheService = DIContainer.Resolve<ICacheService>();
			if (cacheService.EnableDistributedCache)
			{
				return string.Concat(new object[]
				{
					this.TypeHashID,
					":B-",
					primaryKey,
					":",
					this.GetEntityVersion(primaryKey)
				});
			}
			return this.TypeHashID + ":B-" + primaryKey;
		}
		public string GetListCacheKeyPrefix(IListCacheSetting cacheVersionSetting)
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(this.TypeHashID);
			stringBuilder.Append("-L:");
			switch (cacheVersionSetting.CacheVersionType)
			{
			case CacheVersionType.GlobalVersion:
				stringBuilder.AppendFormat("{0}:", this.GetGlobalVersion());
				break;
			case CacheVersionType.AreaVersion:
				stringBuilder.AppendFormat("{0}-{1}-{2}:", cacheVersionSetting.AreaCachePropertyName, cacheVersionSetting.AreaCachePropertyValue.ToString(), this.GetAreaVersion(cacheVersionSetting.AreaCachePropertyName, cacheVersionSetting.AreaCachePropertyValue));
				break;
			}
			return stringBuilder.ToString();
		}
		public string GetListCacheKeyPrefix(CacheVersionType cacheVersionType)
		{
			return this.GetListCacheKeyPrefix(cacheVersionType, null, null);
		}
		public string GetListCacheKeyPrefix(CacheVersionType cacheVersionType, string areaCachePropertyName, object areaCachePropertyValue)
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(this.TypeHashID);
			stringBuilder.Append("-L:");
			switch (cacheVersionType)
			{
			case CacheVersionType.GlobalVersion:
				stringBuilder.AppendFormat("{0}:", this.GetGlobalVersion());
				break;
			case CacheVersionType.AreaVersion:
				stringBuilder.AppendFormat("{0}-{1}-{2}:", areaCachePropertyName, areaCachePropertyValue, this.GetAreaVersion(areaCachePropertyName, areaCachePropertyValue));
				break;
			}
			return stringBuilder.ToString();
		}
		internal static string GetCacheKeyOfTimelinessHelper(string typeHashID)
		{
			return "CacheTimelinessHelper:" + typeHashID;
		}
		private void OnChanged()
		{
			ICacheService cacheService = DIContainer.Resolve<ICacheService>();
			if (cacheService.EnableDistributedCache)
			{
				cacheService.Set(RealTimeCacheHelper.GetCacheKeyOfTimelinessHelper(this.TypeHashID), this, CachingExpirationType.Invariable);
			}
		}
	}
}
