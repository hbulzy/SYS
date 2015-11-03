using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
namespace Tunynet.Utilities
{
	public static class ValueUtility
	{
		public static System.DateTime GetSafeSqlDateTime(System.DateTime date)
		{
			if (date < (System.DateTime)SqlDateTime.MinValue)
			{
				return SqlDateTime.MinValue.Value.AddYears(1);
			}
			if (date > (System.DateTime)SqlDateTime.MaxValue)
			{
				return (System.DateTime)SqlDateTime.MaxValue;
			}
			return date;
		}
		public static int GetSafeSqlInt(int i)
		{
			if (i <= (int)SqlInt32.MinValue)
			{
				return (int)SqlInt32.MinValue + 1;
			}
			if (i >= (int)SqlInt32.MaxValue)
			{
				return (int)SqlInt32.MaxValue - 1;
			}
			return i;
		}
		public static int GetSqlMaxInt()
		{
			return (int)SqlInt32.MaxValue - 1;
		}
		public static System.Collections.Generic.List<int> ParseInt(string[] strArray)
		{
			System.Collections.Generic.List<int> list = new System.Collections.Generic.List<int>();
			if (strArray == null || strArray.Length == 0)
			{
				return list;
			}
			for (int i = 0; i < strArray.Length; i++)
			{
				string s = strArray[i];
				int item = 0;
				if (int.TryParse(s, out item))
				{
					list.Add(item);
				}
			}
			return list;
		}
		public static T ChangeType<T>(object value)
		{
			return ValueUtility.ChangeType<T>(value, default(T));
		}
		public static T ChangeType<T>(object value, T defalutValue)
		{
			if (value != null)
			{
				System.Type typeFromHandle = typeof(T);
				if (typeFromHandle.IsInterface || (typeFromHandle.IsClass && typeFromHandle != typeof(string)))
				{
					if (value is T)
					{
						return (T)((object)value);
					}
				}
				else
				{
					if (typeFromHandle.IsGenericType && typeFromHandle.GetGenericTypeDefinition() == typeof(System.Nullable<>))
					{
						return (T)((object)System.Convert.ChangeType(value, System.Nullable.GetUnderlyingType(typeFromHandle)));
					}
					if (typeFromHandle.IsEnum)
					{
						return (T)((object)System.Enum.Parse(typeFromHandle, value.ToString()));
					}
					return (T)((object)System.Convert.ChangeType(value, typeFromHandle));
				}
			}
			return defalutValue;
		}
	}
}
