using PetaPoco.Internal;
using System;
using System.Data;
namespace PetaPoco.DatabaseTypes
{
	internal class OracleDatabaseType : DatabaseType
	{
		public override string GetParameterPrefix(string ConnectionString)
		{
			return ":";
		}
		public override void PreExecute(IDbCommand cmd)
		{
			cmd.GetType().GetProperty("BindByName").SetValue(cmd, true, null);
		}
		public override string BuildPageQuery(long skip, long take, PagingHelper.SQLParts parts, ref object[] args, string primaryKey = null)
		{
			if (parts.sqlSelectRemoved.StartsWith("*"))
			{
				throw new System.Exception("Query must alias '*' when performing a paged query.\neg. select t.* from table t order by t.id");
			}
			return Singleton<SqlServerDatabaseType>.Instance.BuildPageQuery(skip, take, parts, ref args, null);
		}
		public override string EscapeSqlIdentifier(string str)
		{
			return string.Format("\"{0}\"", str.ToUpperInvariant());
		}
		public override string GetAutoIncrementExpression(TableInfo ti)
		{
			if (!string.IsNullOrEmpty(ti.SequenceName))
			{
				return string.Format("{0}.nextval", ti.SequenceName);
			}
			return null;
		}
		public override object ExecuteInsert(Database db, IDbCommand cmd, string PrimaryKeyName)
		{
			if (PrimaryKeyName != null)
			{
				cmd.CommandText += string.Format(" returning {0} into :newid", this.EscapeSqlIdentifier(PrimaryKeyName));
				IDbDataParameter dbDataParameter = cmd.CreateParameter();
				dbDataParameter.ParameterName = ":newid";
				dbDataParameter.Value = System.DBNull.Value;
				dbDataParameter.Direction = ParameterDirection.ReturnValue;
				dbDataParameter.DbType = DbType.Int64;
				cmd.Parameters.Add(dbDataParameter);
				db.ExecuteNonQueryHelper(cmd);
				return dbDataParameter.Value;
			}
			db.ExecuteNonQueryHelper(cmd);
			return -1;
		}
	}
}
