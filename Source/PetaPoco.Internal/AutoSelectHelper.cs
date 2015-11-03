using System;
using System.Linq;
using System.Text.RegularExpressions;
namespace PetaPoco.Internal
{
	internal static class AutoSelectHelper
	{
		private static Regex rxSelect = new Regex("\\A\\s*(SELECT|EXECUTE|CALL)\\s", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.Singleline);
		private static Regex rxFrom = new Regex("\\A\\s*FROM\\s", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.Singleline);
		public static string AddSelectClause<T>(DatabaseType DatabaseType, string sql, string primaryKey = null)
		{
			if (sql.StartsWith(";"))
			{
				return sql.Substring(1);
			}
			if (!AutoSelectHelper.rxSelect.IsMatch(sql))
			{
				PocoData pocoData = PocoData.ForType(typeof(T));
				string tableName = DatabaseType.EscapeTableName(pocoData.TableInfo.TableName);
				string arg = string.Empty;
				if (string.IsNullOrEmpty(primaryKey))
				{
					arg = ((pocoData.Columns.Count != 0) ? string.Join(", ", (
						from c in pocoData.QueryColumns
						select tableName + "." + DatabaseType.EscapeSqlIdentifier(c)).ToArray<string>()) : "NULL");
				}
				else
				{
					arg = primaryKey;
				}
				if (!AutoSelectHelper.rxFrom.IsMatch(sql))
				{
					sql = string.Format("SELECT {0} FROM {1} {2}", arg, tableName, sql);
				}
				else
				{
					sql = string.Format("SELECT {0} {1}", arg, sql);
				}
			}
			return sql;
		}
	}
}
