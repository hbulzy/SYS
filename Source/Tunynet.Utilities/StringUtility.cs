using System;
using System.Text;
using System.Text.RegularExpressions;
namespace Tunynet.Utilities
{
	public static class StringUtility
	{
		public static string Trim(string rawString, int charLimit)
		{
			return StringUtility.Trim(rawString, charLimit, "...");
		}
		public static string Trim(string rawString, int charLimit, string appendString)
		{
			if (string.IsNullOrEmpty(rawString) || rawString.Length <= charLimit)
			{
				return rawString;
			}
			int num = System.Text.Encoding.UTF8.GetBytes(rawString).Length;
			if (num <= charLimit * 2)
			{
				return rawString;
			}
			charLimit = charLimit * 2 - System.Text.Encoding.UTF8.GetBytes(appendString).Length;
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			int num2 = 0;
			for (int i = 0; i < rawString.Length; i++)
			{
				char c = rawString[i];
				stringBuilder.Append(c);
				num2 += ((c > '\u0080') ? 2 : 1);
				if (num2 >= charLimit)
				{
					break;
				}
			}
			return stringBuilder.Append(appendString).ToString();
		}
		public static string UnicodeEncode(string rawString)
		{
			if (rawString == null || rawString == string.Empty)
			{
				return rawString;
			}
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			for (int i = 0; i < rawString.Length; i++)
			{
				int num = (int)rawString[i];
				string text;
				if (num > 126)
				{
					stringBuilder.Append("\\u");
					text = num.ToString("x");
					for (int j = 0; j < 4 - text.Length; j++)
					{
						stringBuilder.Append("0");
					}
				}
				else
				{
					text = ((char)num).ToString();
				}
				stringBuilder.Append(text);
			}
			return stringBuilder.ToString();
		}
		public static string CleanInvalidCharsForXML(string rawXml)
		{
			if (string.IsNullOrEmpty(rawXml))
			{
				return rawXml;
			}
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			char[] array = rawXml.ToCharArray();
			for (int i = 0; i < array.Length; i++)
			{
				int num = System.Convert.ToInt32(array[i]);
				if ((num < 0 || num > 8) && (num < 11 || num > 12) && (num < 14 || num > 31))
				{
					stringBuilder.Append(array[i]);
				}
			}
			return stringBuilder.ToString();
		}
		public static string StripSQLInjection(string sql)
		{
			if (!string.IsNullOrEmpty(sql))
			{
				string pattern = "((\\%27)|(\\'))\\s*((\\%6F)|o|(\\%4F))((\\%72)|r|(\\%52))";
				string pattern2 = "(\\%27)|(\\')|(\\-\\-)";
				string pattern3 = "\\s+exec(\\s|\\+)+(s|x)p\\w+";
				sql = Regex.Replace(sql, pattern, string.Empty, RegexOptions.IgnoreCase);
				sql = Regex.Replace(sql, pattern2, string.Empty, RegexOptions.IgnoreCase);
				sql = Regex.Replace(sql, pattern3, string.Empty, RegexOptions.IgnoreCase);
				sql = sql.Replace("%", "[%]");
			}
			return sql;
		}
	}
}
