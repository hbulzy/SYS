using PetaPoco.Internal;
using System;
using System.Data;
using System.Linq;
namespace PetaPoco.DatabaseTypes
{
	internal class SqlServerDatabaseType : DatabaseType
	{
		public override string BuildPageQuery(long skip, long take, PagingHelper.SQLParts parts, ref object[] args, string primaryKey = null)
		{
			parts.sqlSelectRemoved = PagingHelper.rxOrderBy.Replace(parts.sqlSelectRemoved, "", 1);
			if (PagingHelper.rxDistinct.IsMatch(parts.sqlSelectRemoved))
			{
				parts.sqlSelectRemoved = "peta_inner.* FROM (SELECT " + parts.sqlSelectRemoved + ") peta_inner";
			}
			string text = primaryKey ?? string.Empty;
			if (primaryKey.Contains('.') && !primaryKey.EndsWith("."))
			{
				text = primaryKey.Substring(primaryKey.LastIndexOf(".") + 1);
			}
			string result = string.Format("SELECT peta_paged.{4} FROM (SELECT ROW_NUMBER() OVER ({0}) peta_rn, {1}) peta_paged WHERE peta_rn>@{2} AND peta_rn<=@{3}", new object[]
			{
				(parts.sqlOrderBy == null) ? "ORDER BY (SELECT NULL)" : parts.sqlOrderBy,
				parts.sqlSelectRemoved,
				args.Length,
				args.Length + 1,
				text
			});
			args = args.Concat(new object[]
			{
				skip,
				skip + take
			}).ToArray<object>();
			return result;
		}
		public override object ExecuteInsert(Database db, IDbCommand cmd, string PrimaryKeyName)
		{
			return db.ExecuteScalarHelper(cmd);
		}
		public override string GetExistsSql()
		{
			return "IF EXISTS (SELECT 1 FROM {0} WHERE {1}) SELECT 1 ELSE SELECT 0";
		}
		public override string GetInsertOutputClause(string primaryKeyName)
		{
			return string.Format(" OUTPUT INSERTED.[{0}]", primaryKeyName);
		}
	}
}
