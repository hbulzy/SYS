using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Tunynet.Caching;
using Tunynet.Utilities;
namespace Tunynet
{
	[System.Serializable]
	public class EntityData
	{
		private static ConcurrentDictionary<System.Type, EntityData> entityDatas = new ConcurrentDictionary<System.Type, EntityData>();
		private RealTimeCacheHelper realTimeCacheHelper;
		private static readonly object lockObject = new object();
		public System.Type Type
		{
			get;
			private set;
		}
		public string TypeHashID
		{
			get;
			private set;
		}
		public RealTimeCacheHelper RealTimeCacheHelper
		{
			get
			{
				ICacheService cacheService = DIContainer.Resolve<ICacheService>();
				if (!cacheService.EnableDistributedCache)
				{
					return this.realTimeCacheHelper;
				}
				string cacheKeyOfTimelinessHelper = RealTimeCacheHelper.GetCacheKeyOfTimelinessHelper(this.TypeHashID);
				RealTimeCacheHelper realTimeCacheHelper = cacheService.GetFromFirstLevel<RealTimeCacheHelper>(cacheKeyOfTimelinessHelper);
				if (realTimeCacheHelper == null)
				{
					realTimeCacheHelper = this.ParseCacheTimelinessHelper(this.Type);
					cacheService.Set(cacheKeyOfTimelinessHelper, realTimeCacheHelper, CachingExpirationType.Invariable);
				}
				return realTimeCacheHelper;
			}
		}
		public EntityData(System.Type t)
		{
			this.Type = t;
			this.TypeHashID = EncryptionUtility.MD5_16(t.FullName);
			ICacheService cacheService = DIContainer.Resolve<ICacheService>();
			RealTimeCacheHelper value = this.ParseCacheTimelinessHelper(t);
			if (cacheService.EnableDistributedCache)
			{
				cacheService.Set(RealTimeCacheHelper.GetCacheKeyOfTimelinessHelper(this.TypeHashID), value, CachingExpirationType.Invariable);
				return;
			}
			this.realTimeCacheHelper = value;
		}
		private RealTimeCacheHelper ParseCacheTimelinessHelper(System.Type t)
		{
			RealTimeCacheHelper realTimeCacheHelper = null;
			object[] customAttributes = t.GetCustomAttributes(typeof(CacheSettingAttribute), true);
			if (customAttributes.Length > 0)
			{
				CacheSettingAttribute cacheSettingAttribute = customAttributes[0] as CacheSettingAttribute;
				if (cacheSettingAttribute != null)
				{
					realTimeCacheHelper = new RealTimeCacheHelper(cacheSettingAttribute.EnableCache, this.TypeHashID);
					if (cacheSettingAttribute.EnableCache)
					{
						switch (cacheSettingAttribute.ExpirationPolicy)
						{
						case EntityCacheExpirationPolicies.Stable:
							realTimeCacheHelper.CachingExpirationType = CachingExpirationType.Stable;
							goto IL_89;
						case EntityCacheExpirationPolicies.Usual:
							realTimeCacheHelper.CachingExpirationType = CachingExpirationType.UsualSingleObject;
							goto IL_89;
						}
						realTimeCacheHelper.CachingExpirationType = CachingExpirationType.SingleObject;
						IL_89:
						System.Collections.Generic.List<System.Reflection.PropertyInfo> list = new System.Collections.Generic.List<System.Reflection.PropertyInfo>();
						if (!string.IsNullOrEmpty(cacheSettingAttribute.PropertyNamesOfArea))
						{
							string[] array = cacheSettingAttribute.PropertyNamesOfArea.Split(new char[]
							{
								','
							}, System.StringSplitOptions.RemoveEmptyEntries);
							string[] array2 = array;
							for (int i = 0; i < array2.Length; i++)
							{
								string name = array2[i];
								System.Reflection.PropertyInfo property = t.GetProperty(name);
								if (property != null)
								{
									list.Add(property);
								}
							}
						}
						realTimeCacheHelper.PropertiesOfArea = list;
						if (!string.IsNullOrEmpty(cacheSettingAttribute.PropertyNameOfBody))
						{
							System.Reflection.PropertyInfo property2 = t.GetProperty(cacheSettingAttribute.PropertyNameOfBody);
							realTimeCacheHelper.PropertyNameOfBody = property2;
						}
					}
				}
			}
			if (realTimeCacheHelper == null)
			{
				realTimeCacheHelper = new RealTimeCacheHelper(true, this.TypeHashID);
			}
			return realTimeCacheHelper;
		}
		public static EntityData ForType(System.Type t)
		{
			EntityData entityData;
			if (!EntityData.entityDatas.TryGetValue(t, out entityData) && entityData == null)
			{
				entityData = new EntityData(t);
				EntityData.entityDatas[t] = entityData;
			}
			return entityData;
		}
	}
}
