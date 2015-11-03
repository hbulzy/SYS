using PetaPoco.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace PetaPoco
{
	public class Sql
	{
		public class SqlJoinClause
		{
			private readonly Sql _sql;
			public SqlJoinClause(Sql sql)
			{
				this._sql = sql;
			}
			public Sql On(string onClause, params object[] args)
			{
				return this._sql.Append("ON " + onClause, args);
			}
		}
		private string _sql;
		private object[] _args;
		private Sql _rhs;
		private string _sqlFinal;
		private object[] _argsFinal;
		public static Sql Builder
		{
			get
			{
				return new Sql();
			}
		}
		public string SQL
		{
			get
			{
				this.Build();
				return this._sqlFinal;
			}
		}
		public object[] Arguments
		{
			get
			{
				this.Build();
				return this._argsFinal;
			}
		}
		public Sql()
		{
		}
		public Sql(string sql, params object[] args)
		{
			this._sql = sql;
			this._args = args;
		}
		private void Build()
		{
			if (this._sqlFinal != null)
			{
				return;
			}
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			System.Collections.Generic.List<object> list = new System.Collections.Generic.List<object>();
			this.Build(stringBuilder, list, null);
			this._sqlFinal = stringBuilder.ToString();
			this._argsFinal = list.ToArray();
		}
		public Sql Append(Sql sql)
		{
			if (this._rhs != null)
			{
				this._rhs.Append(sql);
			}
			else
			{
				this._rhs = sql;
			}
			return this;
		}
		public Sql Append(string sql, params object[] args)
		{
			return this.Append(new Sql(sql, args));
		}
		private static bool Is(Sql sql, string sqltype)
		{
			return sql != null && sql._sql != null && sql._sql.StartsWith(sqltype, System.StringComparison.InvariantCultureIgnoreCase);
		}
		private void Build(System.Text.StringBuilder sb, System.Collections.Generic.List<object> args, Sql lhs)
		{
			if (!string.IsNullOrEmpty(this._sql))
			{
				if (sb.Length > 0)
				{
					sb.Append("\n");
				}
				string text = ParametersHelper.ProcessParams(this._sql, this._args, args);
				if (Sql.Is(lhs, "WHERE ") && Sql.Is(this, "WHERE "))
				{
					text = "AND " + text.Substring(6);
				}
				if (Sql.Is(lhs, "ORDER BY ") && Sql.Is(this, "ORDER BY "))
				{
					text = ", " + text.Substring(9);
				}
				sb.Append(text);
			}
			if (this._rhs != null)
			{
				this._rhs.Build(sb, args, this);
			}
		}
		public Sql Where(string sql, params object[] args)
		{
			return this.Append(new Sql("WHERE (" + sql + ")", args));
		}
		public Sql OrderBy(params object[] columns)
		{
			return this.Append(new Sql("ORDER BY " + string.Join(", ", (
				from x in columns
				select x.ToString()).ToArray<string>()), new object[0]));
		}
		public Sql Select(params object[] columns)
		{
			return this.Append(new Sql("SELECT " + string.Join(", ", (
				from x in columns
				select x.ToString()).ToArray<string>()), new object[0]));
		}
		public Sql From(params object[] tables)
		{
			return this.Append(new Sql("FROM " + string.Join(", ", (
				from x in tables
				select x.ToString()).ToArray<string>()), new object[0]));
		}
		public Sql GroupBy(params object[] columns)
		{
			return this.Append(new Sql("GROUP BY " + string.Join(", ", (
				from x in columns
				select x.ToString()).ToArray<string>()), new object[0]));
		}
		private Sql.SqlJoinClause Join(string JoinType, string table)
		{
			return new Sql.SqlJoinClause(this.Append(new Sql(JoinType + table, new object[0])));
		}
		public Sql.SqlJoinClause InnerJoin(string table)
		{
			return this.Join("INNER JOIN ", table);
		}
		public Sql.SqlJoinClause LeftJoin(string table)
		{
			return this.Join("LEFT JOIN ", table);
		}
	}
}
