using System;
namespace Tunynet.Caching
{
	[System.AttributeUsage(System.AttributeTargets.Class)]
	public class CacheSettingAttribute : System.Attribute
	{
		private EntityCacheExpirationPolicies expirationPolicy = EntityCacheExpirationPolicies.Normal;
		public bool EnableCache
		{
			get;
			private set;
		}
		public EntityCacheExpirationPolicies ExpirationPolicy
		{
			get
			{
				return this.expirationPolicy;
			}
			set
			{
				this.expirationPolicy = value;
			}
		}
		public string PropertyNameOfBody
		{
			get;
			set;
		}
		public string PropertyNamesOfArea
		{
			get;
			set;
		}
		public CacheSettingAttribute(bool enableCache)
		{
			this.EnableCache = enableCache;
		}
	}
}
