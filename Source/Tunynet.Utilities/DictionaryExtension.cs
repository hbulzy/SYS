using System;
using System.Collections.Generic;
namespace Tunynet.Utilities
{
	public static class DictionaryExtension
	{
		public static T Get<T>(this System.Collections.Generic.IDictionary<string, object> dictionary, string key, T defaultValue)
		{
			if (dictionary.ContainsKey(key))
			{
				object value;
				dictionary.TryGetValue(key, out value);
				return ValueUtility.ChangeType<T>(value, defaultValue);
			}
			return defaultValue;
		}
	}
}
