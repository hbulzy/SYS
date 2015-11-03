using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Resources;
namespace Tunynet.Globalization
{
	public static class ResourceAccessor
	{
		private static System.Resources.ResourceManager _commonResourceManager;
		private static ConcurrentDictionary<int, System.Resources.ResourceManager> _applicationResourceManagers = new ConcurrentDictionary<int, System.Resources.ResourceManager>();
		public static string GetString(string resourcesKey)
		{
			try
			{
				string @string = ResourceAccessor._commonResourceManager.GetString(resourcesKey);
				if (@string != null)
				{
					return @string;
				}
			}
			catch
			{
			}
			return ResourceAccessor.GetMissingResourcePrompt(resourcesKey);
		}
		public static string GetString(string resourcesKey, int applicationId)
		{
			System.Resources.ResourceManager resourceManager;
			if (ResourceAccessor._applicationResourceManagers.TryGetValue(applicationId, out resourceManager))
			{
				try
				{
					string @string = resourceManager.GetString(resourcesKey);
					if (@string != null)
					{
						string result = @string;
						return result;
					}
				}
				catch
				{
				}
			}
			try
			{
				string @string = ResourceAccessor._commonResourceManager.GetString(resourcesKey);
				if (@string != null)
				{
					string result = @string;
					return result;
				}
			}
			catch
			{
			}
			return ResourceAccessor.GetMissingResourcePrompt(resourcesKey);
		}
		private static string GetMissingResourcePrompt(string resourcesKey)
		{
			return string.Format("<span style=\"color:#ff0000; font-weight:bold\">missing resource: {0}</span>", resourcesKey);
		}
		public static void Initialize(string commonResourceFileBaseName, System.Reflection.Assembly commonResourceAssembly)
		{
			ResourceAccessor._commonResourceManager = new System.Resources.ResourceManager(commonResourceFileBaseName, commonResourceAssembly);
			ResourceAccessor._commonResourceManager.IgnoreCase = true;
		}
		public static void RegisterApplicationResourceManager(int applicationId, string resourceFileBaseName, System.Reflection.Assembly assembly)
		{
			System.Resources.ResourceManager resourceManager = null;
			try
			{
				resourceManager = new System.Resources.ResourceManager(resourceFileBaseName, assembly);
			}
			catch
			{
			}
			if (resourceManager != null)
			{
				resourceManager.IgnoreCase = true;
				ResourceAccessor._applicationResourceManagers[applicationId] = resourceManager;
			}
		}
	}
}
