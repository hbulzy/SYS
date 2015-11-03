using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
namespace PetaPoco.Internal
{
	internal static class ParametersHelper
	{
		private static Regex rxParams = new Regex("(?<!@)@\\w+", RegexOptions.Compiled);
		public static string ProcessParams(string sql, object[] args_src, System.Collections.Generic.List<object> args_dest)
		{
			return ParametersHelper.rxParams.Replace(sql, delegate(Match m)
			{
				string text = m.Value.Substring(1);
				int num;
				object obj;
				if (int.TryParse(text, out num))
				{
					if (num < 0 || num >= args_src.Length)
					{
						throw new System.ArgumentOutOfRangeException(string.Format("Parameter '@{0}' specified but only {1} parameters supplied (in `{2}`)", num, args_src.Length, sql));
					}
					obj = args_src[num];
				}
				else
				{
					bool flag = false;
					obj = null;
					object[] args_src2 = args_src;
					for (int i = 0; i < args_src2.Length; i++)
					{
						object obj2 = args_src2[i];
						System.Reflection.PropertyInfo property = obj2.GetType().GetProperty(text);
						if (property != null)
						{
							obj = property.GetValue(obj2, null);
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						throw new System.ArgumentException(string.Format("Parameter '@{0}' specified but none of the passed arguments have a property with this name (in '{1}')", text, sql));
					}
				}
				if (obj is System.Collections.IEnumerable && !(obj is string) && !(obj is byte[]))
				{
					System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
					foreach (object current in obj as System.Collections.IEnumerable)
					{
						stringBuilder.Append(((stringBuilder.Length == 0) ? "@" : ",@") + args_dest.Count.ToString());
						args_dest.Add(current);
					}
					return stringBuilder.ToString();
				}
				args_dest.Add(obj);
				return "@" + (args_dest.Count - 1).ToString();
			});
		}
	}
}
