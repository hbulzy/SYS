using System;
using System.Collections.Generic;
namespace PetaPoco.Internal
{
	internal static class EnumMapper
	{
		private static Cache<System.Type, System.Collections.Generic.Dictionary<string, object>> _types = new Cache<System.Type, System.Collections.Generic.Dictionary<string, object>>();
		public static object EnumFromString(System.Type enumType, string value)
		{
			System.Collections.Generic.Dictionary<string, object> dictionary = EnumMapper._types.Get(enumType, delegate
			{
				System.Array values = System.Enum.GetValues(enumType);
				System.Collections.Generic.Dictionary<string, object> dictionary2 = new System.Collections.Generic.Dictionary<string, object>(values.Length, System.StringComparer.InvariantCultureIgnoreCase);
				foreach (object current in values)
				{
					dictionary2.Add(current.ToString(), current);
				}
				return dictionary2;
			});
			return dictionary[value];
		}
	}
}
