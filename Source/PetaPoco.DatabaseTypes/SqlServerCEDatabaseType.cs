using PetaPoco.Internal;
using System;
using System.Data;
using System.Linq;
namespace PetaPoco.DatabaseTypes
{
	internal class SqlServerCEDatabaseType : DatabaseType
	{
		public override string BuildPageQuery(long skip, long take, PagingHelper.SQLParts parts, ref object[] args, string primaryKey = null)
		{
			string result = string.Format("{0}\nOFFSET @{1} ROWS FETCH NEXT @{2} ROWS ONLY", parts.sql, args.Length, args.Length + 1);
			args = args.Concat(new object[]
			{
				skip,
				take
			}).ToArray<object>();
			return result;
		}
		public override object ExecuteInsert(Database db, IDbCommand cmd, string PrimaryKeyName)
		{
			db.ExecuteNonQueryHelper(cmd);
			return db.ExecuteScalar<object>("SELECT @@@IDENTITY AS NewID;", new object[0]);
		}
	}
}
