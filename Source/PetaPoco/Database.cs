using PetaPoco.Internal;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Tunynet;
namespace PetaPoco
{
	public class Database : System.IDisposable
	{
		private object syncObj = new object();
		private static Regex rxParamsPrefix = new Regex("(?<!@)@\\w+", RegexOptions.Compiled);
		internal DatabaseType _dbType;
		private string _connectionString;
		private string _providerName;
		private DbProviderFactory _factory;
		private IDbConnection _sharedConnection;
		private IDbTransaction _transaction;
		private int _sharedConnectionDepth;
		private int _transactionDepth;
		private bool _transactionCancelled;
		private string _lastSql;
		private object[] _lastArgs;
		private string _paramPrefix;
		public bool KeepConnectionAlive
		{
			get;
			set;
		}
		public IDbConnection Connection
		{
			get
			{
				return this._sharedConnection;
			}
		}
		public string LastSQL
		{
			get
			{
				return this._lastSql;
			}
		}
		public object[] LastArgs
		{
			get
			{
				return this._lastArgs;
			}
		}
		public string LastCommand
		{
			get
			{
				return this.FormatCommand(this._lastSql, this._lastArgs);
			}
		}
		public bool EnableAutoSelect
		{
			get;
			set;
		}
		public bool EnableNamedParams
		{
			get;
			set;
		}
		public int CommandTimeout
		{
			get;
			set;
		}
		public int OneTimeCommandTimeout
		{
			get;
			set;
		}
		public static Database CreateInstance(string connectionStringName = null)
		{
			return new Database(connectionStringName)
			{
				EnableAutoSelect = true,
				EnableNamedParams = true
			};
		}
		public int Execute(System.Collections.Generic.IEnumerable<Sql> sqls)
		{
			int result;
			try
			{
				this.OpenSharedConnection();
				try
				{
					lock (this.syncObj)
					{
						int num = 0;
						foreach (Sql current in sqls)
						{
							using (IDbCommand dbCommand = this.CreateCommand(this._sharedConnection, current.SQL, current.Arguments))
							{
								num += dbCommand.ExecuteNonQuery();
								this.OnExecutedCommand(dbCommand);
							}
						}
						result = num;
					}
				}
				finally
				{
					this.CloseSharedConnection();
				}
			}
			catch (System.Exception x)
			{
				this.OnException(x);
				throw;
			}
			return result;
		}
		public System.Collections.Generic.IEnumerable<object> FetchFirstColumn(Sql sql)
		{
			return this.FetchFirstColumn(sql.SQL, sql.Arguments);
		}
		public System.Collections.Generic.IEnumerable<object> FetchFirstColumn(string sql, params object[] args)
		{
			this.OpenSharedConnection();
			System.Collections.Generic.List<object> list = new System.Collections.Generic.List<object>();
			try
			{
				lock (this.syncObj)
				{
					using (IDbCommand dbCommand = this.CreateCommand(this._sharedConnection, sql, args))
					{
						using (IDataReader dataReader = dbCommand.ExecuteReader())
						{
							this.OnExecutedCommand(dbCommand);
							while (dataReader.Read())
							{
								list.Add(dataReader[0]);
							}
							dataReader.Close();
						}
					}
				}
			}
			finally
			{
				this.CloseSharedConnection();
			}
			return list;
		}
		public PagingEntityIdCollection FetchPagingPrimaryKeys<TEntity>(long maxRecords, int pageSize, int pageIndex, Sql sql) where TEntity : IEntity
		{
			string sQL = sql.SQL;
			object[] arguments = sql.Arguments;
			string sql2;
			string sql3;
			this.BuildPagingPrimaryKeyQueries<TEntity>(maxRecords, (long)((pageIndex - 1) * pageSize), (long)pageSize, sQL, ref arguments, out sql2, out sql3);
			long totalRecords = this.ExecuteScalar<long>(sql2, arguments);
			System.Collections.Generic.List<object> entityIds = this.FetchFirstColumn(sql3, arguments).ToList<object>();
			return new PagingEntityIdCollection(entityIds, totalRecords);
		}
		public PagingEntityIdCollection FetchPagingPrimaryKeys(long maxRecords, int pageSize, int pageIndex, string primaryKey, Sql sql)
		{
			string sQL = sql.SQL;
			object[] arguments = sql.Arguments;
			string sql2;
			string sql3;
			this.BuildPagingPrimaryKeyQueries(maxRecords, (long)((pageIndex - 1) * pageSize), (long)pageSize, primaryKey, sQL, ref arguments, out sql2, out sql3);
			long totalRecords = this.ExecuteScalar<long>(sql2, arguments);
			System.Collections.Generic.List<object> entityIds = this.FetchFirstColumn(sql3, arguments).ToList<object>();
			return new PagingEntityIdCollection(entityIds, totalRecords);
		}
		public System.Collections.Generic.IEnumerable<object> FetchTopPrimaryKeys<TEntity>(int topNumber, Sql sql) where TEntity : IEntity
		{
			string sQL = sql.SQL;
			object[] arguments = sql.Arguments;
			string sql2 = this.BuildTopSql<TEntity>(topNumber, sQL);
			return this.FetchFirstColumn(sql2, arguments);
		}
		public System.Collections.Generic.IEnumerable<T> FetchTop<T>(int topNumber, Sql sql)
		{
			string sQL = sql.SQL;
			object[] arguments = sql.Arguments;
			string sql2 = this.BuildTopSql(topNumber, sQL);
			return this.Fetch<T>(sql2, arguments);
		}
		public System.Collections.Generic.IEnumerable<T> FetchByPrimaryKeys<T>(System.Collections.Generic.IEnumerable<object> primaryKeys)
		{
			if (primaryKeys == null || primaryKeys.Count<object>() == 0)
			{
				return new System.Collections.Generic.List<T>();
			}
			string arg = this._dbType.EscapeSqlIdentifier(PocoData.ForType(typeof(T)).TableInfo.PrimaryKey);
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder("WHERE ");
			int num = 0;
			foreach (object arg_52_0 in primaryKeys)
			{
				stringBuilder.AppendFormat("{0} = @{1} or ", arg, num);
				num++;
			}
			stringBuilder.Remove(stringBuilder.Length - 4, 3);
			return this.Fetch<T>(stringBuilder.ToString(), primaryKeys.ToArray<object>());
		}
		protected void BuildPagingPrimaryKeyQueries<T>(long maxRecords, long skip, long take, string sql, ref object[] args, out string sqlCount, out string sqlPage)
		{
			PocoData pocoData = PocoData.ForType(typeof(T));
			string primaryKey = string.Empty;
			if (sql.Contains(pocoData.TableInfo.TableName))
			{
				primaryKey = pocoData.TableInfo.TableName + "." + pocoData.TableInfo.PrimaryKey;
			}
			else
			{
				primaryKey = pocoData.TableInfo.PrimaryKey;
			}
			if (this.EnableAutoSelect)
			{
				sql = AutoSelectHelper.AddSelectClause<T>(this._dbType, sql, primaryKey);
			}
			this.BuildPagingPrimaryKeyQueries(maxRecords, skip, take, primaryKey, sql, ref args, out sqlCount, out sqlPage);
		}
		protected void BuildPagingPrimaryKeyQueries(long maxRecords, long skip, long take, string primaryKey, string sql, ref object[] args, out string sqlCount, out string sqlPage)
		{
			string sqlSelectRemoved;
			string sqlOrderBy;
			if (!this.SplitSqlForPagingOptimized(maxRecords, sql, primaryKey, out sqlCount, out sqlSelectRemoved, out sqlOrderBy))
			{
				throw new System.Exception("Unable to parse SQL statement for paged query");
			}
			sqlPage = this._dbType.BuildPageQuery(skip, take, new PagingHelper.SQLParts
			{
				sql = sql,
				sqlCount = sqlCount,
				sqlSelectRemoved = sqlSelectRemoved,
				sqlOrderBy = sqlOrderBy
			}, ref args, primaryKey);
		}
		protected bool SplitSqlForPagingOptimized(long maxRecords, string sql, string primaryKey, out string sqlCount, out string sqlSelectRemoved, out string sqlOrderBy)
		{
			sqlSelectRemoved = null;
			sqlCount = null;
			sqlOrderBy = null;
			Match match = PagingHelper.rxColumns.Match(sql);
			if (!match.Success)
			{
				return false;
			}
			Group group = match.Groups[1];
			sqlSelectRemoved = sql.Substring(group.Index);
			if (PagingHelper.rxDistinct.IsMatch(sqlSelectRemoved))
			{
				sqlCount = string.Concat(new string[]
				{
					sql.Substring(0, group.Index),
					"COUNT(",
					match.Groups[1].ToString().Trim(),
					") ",
					sql.Substring(group.Index + group.Length)
				});
			}
			else
			{
				if (maxRecords > 0L)
				{
					if (this._providerName.StartsWith("MySql"))
					{
						sqlCount = string.Concat(new object[]
						{
							"select count(*) from (",
							sql,
							" limit ",
							maxRecords,
							" ) as TempCountTable"
						});
					}
					else
					{
						sqlCount = string.Concat(new object[]
						{
							"select count(*) from (",
							sql.Substring(0, group.Index),
							" top ",
							maxRecords,
							" ",
							primaryKey,
							" ",
							sql.Substring(group.Index + group.Length),
							" ) as TempCountTable"
						});
					}
				}
				else
				{
					sqlCount = sql.Substring(0, group.Index) + "COUNT(*) " + sql.Substring(group.Index + group.Length);
				}
			}
			match = PagingHelper.rxOrderBy.Match(sqlCount);
			if (!match.Success)
			{
				sqlOrderBy = null;
			}
			else
			{
				group = match.Groups[0];
				sqlOrderBy = group.ToString();
				sqlCount = sqlCount.Substring(0, group.Index) + sqlCount.Substring(group.Index + group.Length);
			}
			return true;
		}
		protected string BuildTopSql<T>(int topNumber, string sql)
		{
			PocoData pocoData = PocoData.ForType(typeof(T));
			string primaryKey = pocoData.TableInfo.TableName + "." + pocoData.TableInfo.PrimaryKey;
			if (this.EnableAutoSelect)
			{
				sql = AutoSelectHelper.AddSelectClause<T>(this._dbType, sql, primaryKey);
			}
			return this.BuildTopSql(topNumber, sql);
		}
		protected string BuildTopSql(int topNumber, string sql)
		{
			Match match = PagingHelper.rxColumns.Match(sql);
			if (!match.Success)
			{
				return null;
			}
			Group group = match.Groups[1];
			string result;
			if (this._providerName.StartsWith("MySql"))
			{
				result = sql + " limit " + topNumber;
			}
			else
			{
				result = string.Concat(new object[]
				{
					sql.Substring(0, group.Index),
					" top ",
					topNumber,
					" ",
					group.Value,
					" ",
					sql.Substring(group.Index + group.Length)
				});
			}
			return result;
		}
		public Database(IDbConnection connection)
		{
			this._sharedConnection = connection;
			this._connectionString = connection.ConnectionString;
			this._sharedConnectionDepth = 2;
			this.CommonConstruct();
		}
		public Database(string connectionString, string providerName)
		{
			this._connectionString = connectionString;
			this._providerName = providerName;
			this.CommonConstruct();
		}
		public Database(string connectionString, DbProviderFactory provider)
		{
			this._connectionString = connectionString;
			this._factory = provider;
			this.CommonConstruct();
		}
		public Database(string connectionStringName)
		{
			if (string.IsNullOrEmpty(connectionStringName))
			{
				int count = ConfigurationManager.ConnectionStrings.Count;
				if (count <= 0)
				{
					throw new System.InvalidOperationException("Can't find a connection string '");
				}
				connectionStringName = ConfigurationManager.ConnectionStrings[count - 1].Name;
			}
			string providerName = "System.Data.SqlClient";
			if (ConfigurationManager.ConnectionStrings[connectionStringName] != null)
			{
				if (!string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings[connectionStringName].ProviderName))
				{
					providerName = ConfigurationManager.ConnectionStrings[connectionStringName].ProviderName;
				}
				this._connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
				this._providerName = providerName;
				this.CommonConstruct();
				return;
			}
			throw new System.InvalidOperationException("Can't find a connection string with the name '" + connectionStringName + "'");
		}
		private void CommonConstruct()
		{
			this._transactionDepth = 0;
			this.EnableAutoSelect = true;
			this.EnableNamedParams = true;
			if (this._providerName != null)
			{
				this._factory = DbProviderFactories.GetFactory(this._providerName);
			}
			string name = ((this._factory == null) ? this._sharedConnection.GetType() : this._factory.GetType()).Name;
			this._dbType = DatabaseType.Resolve(name, this._providerName);
			this._paramPrefix = this._dbType.GetParameterPrefix(this._connectionString);
		}
		public void Dispose()
		{
			this.CloseSharedConnection();
		}
		public void OpenSharedConnection()
		{
			lock (this.syncObj)
			{
				if (this._sharedConnectionDepth == 0)
				{
					this._sharedConnection = this._factory.CreateConnection();
					this._sharedConnection.ConnectionString = this._connectionString;
					if (this._sharedConnection.State == ConnectionState.Broken)
					{
						this._sharedConnection.Close();
					}
					if (this._sharedConnection.State == ConnectionState.Closed)
					{
						this._sharedConnection.Open();
					}
					this._sharedConnection = this.OnConnectionOpened(this._sharedConnection);
					if (this.KeepConnectionAlive)
					{
						this._sharedConnectionDepth++;
					}
				}
				this._sharedConnectionDepth++;
			}
		}
		public void CloseSharedConnection()
		{
			lock (this.syncObj)
			{
				if (this._sharedConnectionDepth > 0)
				{
					this._sharedConnectionDepth--;
					if (this._sharedConnectionDepth == 0)
					{
						this.OnConnectionClosing(this._sharedConnection);
						this._sharedConnection.Dispose();
						this._sharedConnection = null;
					}
				}
			}
		}
		public ITransaction GetTransaction()
		{
			return new Transaction(this);
		}
		public virtual void OnBeginTransaction()
		{
		}
		public virtual void OnEndTransaction()
		{
		}
		public void BeginTransaction()
		{
			this._transactionDepth++;
			if (this._transactionDepth == 1)
			{
				this.OpenSharedConnection();
				this._transaction = this._sharedConnection.BeginTransaction();
				this._transactionCancelled = false;
				this.OnBeginTransaction();
			}
		}
		private void CleanupTransaction()
		{
			this.OnEndTransaction();
			if (this._transactionCancelled)
			{
				this._transaction.Rollback();
			}
			else
			{
				this._transaction.Commit();
			}
			this._transaction.Dispose();
			this._transaction = null;
			this.CloseSharedConnection();
		}
		public void AbortTransaction()
		{
			this._transactionCancelled = true;
			if (--this._transactionDepth == 0)
			{
				this.CleanupTransaction();
			}
		}
		public void CompleteTransaction()
		{
			if (--this._transactionDepth == 0)
			{
				this.CleanupTransaction();
			}
		}
		private void AddParam(IDbCommand cmd, object value, System.Reflection.PropertyInfo pi)
		{
			if (pi != null)
			{
				IMapper mapper = Mappers.GetMapper(pi.DeclaringType);
				Func<object, object> toDbConverter = mapper.GetToDbConverter(pi);
				if (toDbConverter != null)
				{
					value = toDbConverter(value);
				}
			}
			IDbDataParameter dbDataParameter = value as IDbDataParameter;
			if (dbDataParameter != null)
			{
				dbDataParameter.ParameterName = string.Format("{0}{1}", this._paramPrefix, cmd.Parameters.Count);
				cmd.Parameters.Add(dbDataParameter);
				return;
			}
			IDbDataParameter dbDataParameter2 = cmd.CreateParameter();
			dbDataParameter2.ParameterName = string.Format("{0}{1}", this._paramPrefix, cmd.Parameters.Count);
			if (value == null)
			{
				dbDataParameter2.Value = System.DBNull.Value;
			}
			else
			{
				value = this._dbType.MapParameterValue(value);
				System.Type type = value.GetType();
				if (type.IsEnum)
				{
					dbDataParameter2.Value = (int)value;
				}
				else
				{
					if (type == typeof(System.Guid))
					{
						dbDataParameter2.Value = value.ToString();
						dbDataParameter2.DbType = DbType.String;
						dbDataParameter2.Size = 40;
					}
					else
					{
						if (type == typeof(string))
						{
							if ((value as string).Length + 1 > 4000 && dbDataParameter2.GetType().Name == "SqlCeParameter")
							{
								dbDataParameter2.GetType().GetProperty("SqlDbType").SetValue(dbDataParameter2, SqlDbType.NText, null);
							}
							dbDataParameter2.Size = System.Math.Max((value as string).Length + 1, 4000);
							dbDataParameter2.Value = value;
						}
						else
						{
							if (type == typeof(AnsiString))
							{
								dbDataParameter2.Size = System.Math.Max((value as AnsiString).Value.Length + 1, 4000);
								dbDataParameter2.Value = (value as AnsiString).Value;
								dbDataParameter2.DbType = DbType.AnsiString;
							}
							else
							{
								if (value.GetType().Name == "SqlGeography")
								{
									dbDataParameter2.GetType().GetProperty("UdtTypeName").SetValue(dbDataParameter2, "geography", null);
									dbDataParameter2.Value = value;
								}
								else
								{
									if (value.GetType().Name == "SqlGeometry")
									{
										dbDataParameter2.GetType().GetProperty("UdtTypeName").SetValue(dbDataParameter2, "geometry", null);
										dbDataParameter2.Value = value;
									}
									else
									{
										dbDataParameter2.Value = value;
									}
								}
							}
						}
					}
				}
			}
			cmd.Parameters.Add(dbDataParameter2);
		}
		public IDbCommand CreateCommand(IDbConnection connection, string sql, params object[] args)
		{
			if (this.EnableNamedParams)
			{
				System.Collections.Generic.List<object> list = new System.Collections.Generic.List<object>();
				sql = ParametersHelper.ProcessParams(sql, args, list);
				args = list.ToArray();
			}
			if (this._paramPrefix != "@")
			{
				sql = Database.rxParamsPrefix.Replace(sql, (Match m) => this._paramPrefix + m.Value.Substring(1));
			}
			sql = sql.Replace("@@", "@");
			IDbCommand dbCommand = connection.CreateCommand();
			dbCommand.Connection = connection;
			dbCommand.CommandText = sql;
			dbCommand.Transaction = this._transaction;
			object[] array = args;
			for (int i = 0; i < array.Length; i++)
			{
				object value = array[i];
				this.AddParam(dbCommand, value, null);
			}
			this._dbType.PreExecute(dbCommand);
			if (!string.IsNullOrEmpty(sql))
			{
				this.DoPreExecute(dbCommand);
			}
			return dbCommand;
		}
		public virtual bool OnException(System.Exception x)
		{
			return true;
		}
		public virtual IDbConnection OnConnectionOpened(IDbConnection conn)
		{
			return conn;
		}
		public virtual void OnConnectionClosing(IDbConnection conn)
		{
		}
		public virtual void OnExecutingCommand(IDbCommand cmd)
		{
		}
		public virtual void OnExecutedCommand(IDbCommand cmd)
		{
		}
		public int Execute(string sql, params object[] args)
		{
			int result;
			try
			{
				this.OpenSharedConnection();
				try
				{
					lock (this.syncObj)
					{
						using (IDbCommand dbCommand = this.CreateCommand(this._sharedConnection, sql, args))
						{
							int num = dbCommand.ExecuteNonQuery();
							this.OnExecutedCommand(dbCommand);
							result = num;
						}
					}
				}
				finally
				{
					this.CloseSharedConnection();
				}
			}
			catch (System.Exception x)
			{
				if (this.OnException(x))
				{
					throw;
				}
				result = -1;
			}
			return result;
		}
		public int Execute(Sql sql)
		{
			return this.Execute(sql.SQL, sql.Arguments);
		}
		public T ExecuteScalar<T>(string sql, params object[] args)
		{
			T result;
			try
			{
				this.OpenSharedConnection();
				try
				{
					lock (this.syncObj)
					{
						using (IDbCommand dbCommand = this.CreateCommand(this._sharedConnection, sql, args))
						{
							object obj2 = dbCommand.ExecuteScalar();
							this.OnExecutedCommand(dbCommand);
							System.Type underlyingType = System.Nullable.GetUnderlyingType(typeof(T));
							if (underlyingType != null && obj2 == null)
							{
								result = default(T);
							}
							else
							{
								result = (T)((object)System.Convert.ChangeType(obj2, (underlyingType == null) ? typeof(T) : underlyingType));
							}
						}
					}
				}
				finally
				{
					this.CloseSharedConnection();
				}
			}
			catch (System.Exception x)
			{
				if (this.OnException(x))
				{
					throw;
				}
				result = default(T);
			}
			return result;
		}
		public T ExecuteScalar<T>(Sql sql)
		{
			return this.ExecuteScalar<T>(sql.SQL, sql.Arguments);
		}
		public System.Collections.Generic.List<T> Fetch<T>(string sql, params object[] args)
		{
			return this.Query<T>(sql, args).ToList<T>();
		}
		public System.Collections.Generic.List<T> Fetch<T>(Sql sql)
		{
			return this.Fetch<T>(sql.SQL, sql.Arguments);
		}
		private void BuildPageQueries<T>(long skip, long take, string sql, ref object[] args, out string sqlCount, out string sqlPage)
		{
			if (this.EnableAutoSelect)
			{
				sql = AutoSelectHelper.AddSelectClause<T>(this._dbType, sql, null);
			}
			PagingHelper.SQLParts parts;
			if (!PagingHelper.SplitSQL(sql, out parts))
			{
				throw new System.Exception("Unable to parse SQL statement for paged query");
			}
			sqlPage = this._dbType.BuildPageQuery(skip, take, parts, ref args, null);
			sqlCount = parts.sqlCount;
		}
		public Page<T> Page<T>(long page, long itemsPerPage, string sqlCount, object[] countArgs, string sqlPage, object[] pageArgs)
		{
			int oneTimeCommandTimeout = this.OneTimeCommandTimeout;
			Page<T> page2 = new Page<T>
			{
				CurrentPage = page,
				ItemsPerPage = itemsPerPage,
				TotalItems = this.ExecuteScalar<long>(sqlCount, countArgs)
			};
			page2.TotalPages = page2.TotalItems / itemsPerPage;
			if (page2.TotalItems % itemsPerPage != 0L)
			{
				page2.TotalPages += 1L;
			}
			this.OneTimeCommandTimeout = oneTimeCommandTimeout;
			page2.Items = this.Fetch<T>(sqlPage, pageArgs);
			return page2;
		}
		public Page<T> Page<T>(long page, long itemsPerPage, string sql, params object[] args)
		{
			string sqlCount;
			string sqlPage;
			this.BuildPageQueries<T>((page - 1L) * itemsPerPage, itemsPerPage, sql, ref args, out sqlCount, out sqlPage);
			return this.Page<T>(page, itemsPerPage, sqlCount, args, sqlPage, args);
		}
		public Page<T> Page<T>(long page, long itemsPerPage, Sql sql)
		{
			return this.Page<T>(page, itemsPerPage, sql.SQL, sql.Arguments);
		}
		public Page<T> Page<T>(long page, long itemsPerPage, Sql sqlCount, Sql sqlPage)
		{
			return this.Page<T>(page, itemsPerPage, sqlCount.SQL, sqlCount.Arguments, sqlPage.SQL, sqlPage.Arguments);
		}
		public System.Collections.Generic.List<T> Fetch<T>(long page, long itemsPerPage, string sql, params object[] args)
		{
			return this.SkipTake<T>((page - 1L) * itemsPerPage, itemsPerPage, sql, args);
		}
		public System.Collections.Generic.List<T> Fetch<T>(long page, long itemsPerPage, Sql sql)
		{
			return this.SkipTake<T>((page - 1L) * itemsPerPage, itemsPerPage, sql.SQL, sql.Arguments);
		}
		public System.Collections.Generic.List<T> SkipTake<T>(long skip, long take, string sql, params object[] args)
		{
			string text;
			string sql2;
			this.BuildPageQueries<T>(skip, take, sql, ref args, out text, out sql2);
			return this.Fetch<T>(sql2, args);
		}
		public System.Collections.Generic.List<T> SkipTake<T>(long skip, long take, Sql sql)
		{
			return this.SkipTake<T>(skip, take, sql.SQL, sql.Arguments);
		}
		public System.Collections.Generic.IEnumerable<T> Query<T>(string sql, params object[] args)
		{
			if (this.EnableAutoSelect)
			{
				sql = AutoSelectHelper.AddSelectClause<T>(this._dbType, sql, null);
			}
			System.Collections.Generic.List<T> list = new System.Collections.Generic.List<T>();
			PocoData pocoData = PocoData.ForType(typeof(T));
			this.OpenSharedConnection();
			try
			{
				lock (this.syncObj)
				{
					using (IDbCommand dbCommand = this.CreateCommand(this._sharedConnection, sql, args))
					{
						using (IDataReader dataReader = dbCommand.ExecuteReader())
						{
							this.OnExecutedCommand(dbCommand);
							Func<IDataReader, T> func = pocoData.GetFactory(dbCommand.CommandText, this._sharedConnection.ConnectionString, 0, dataReader.FieldCount, dataReader) as Func<IDataReader, T>;
							while (dataReader.Read())
							{
								T item = func(dataReader);
								list.Add(item);
							}
							dataReader.Close();
						}
					}
				}
			}
			finally
			{
				this.CloseSharedConnection();
			}
			return list;
		}
		public System.Collections.Generic.IEnumerable<T> Query<T>(Sql sql)
		{
			return this.Query<T>(sql.SQL, sql.Arguments);
		}
		public bool Exists<T>(string sqlCondition, params object[] args)
		{
			TableInfo tableInfo = PocoData.ForType(typeof(T)).TableInfo;
			return this.ExecuteScalar<int>(string.Format(this._dbType.GetExistsSql(), tableInfo.TableName, sqlCondition), args) != 0;
		}
		public bool Exists<T>(object primaryKey)
		{
			return this.Exists<T>(string.Format("{0}=@0", this._dbType.EscapeSqlIdentifier(PocoData.ForType(typeof(T)).TableInfo.PrimaryKey)), new object[]
			{
				primaryKey
			});
		}
		public T Single<T>(object primaryKey)
		{
			return this.Single<T>(string.Format("WHERE {0}=@0", this._dbType.EscapeSqlIdentifier(PocoData.ForType(typeof(T)).TableInfo.PrimaryKey)), new object[]
			{
				primaryKey
			});
		}
		public T SingleOrDefault<T>(object primaryKey)
		{
			return this.SingleOrDefault<T>(string.Format("WHERE {0}=@0", this._dbType.EscapeSqlIdentifier(PocoData.ForType(typeof(T)).TableInfo.PrimaryKey)), new object[]
			{
				primaryKey
			});
		}
		public T Single<T>(string sql, params object[] args)
		{
			return this.Query<T>(sql, args).Single<T>();
		}
		public T SingleOrDefault<T>(string sql, params object[] args)
		{
			return this.Query<T>(sql, args).SingleOrDefault<T>();
		}
		public T First<T>(string sql, params object[] args)
		{
			return this.Query<T>(sql, args).First<T>();
		}
		public T FirstOrDefault<T>(string sql, params object[] args)
		{
			return this.Query<T>(sql, args).FirstOrDefault<T>();
		}
		public T Single<T>(Sql sql)
		{
			return this.Query<T>(sql).Single<T>();
		}
		public T SingleOrDefault<T>(Sql sql)
		{
			return this.Query<T>(sql).SingleOrDefault<T>();
		}
		public T First<T>(Sql sql)
		{
			return this.Query<T>(sql).First<T>();
		}
		public T FirstOrDefault<T>(Sql sql)
		{
			return this.Query<T>(sql).FirstOrDefault<T>();
		}
		public object Insert(string tableName, string primaryKeyName, object poco)
		{
			return this.Insert(tableName, primaryKeyName, true, poco);
		}
		public object Insert(string tableName, string primaryKeyName, bool autoIncrement, object poco)
		{
			object result;
			try
			{
				this.OpenSharedConnection();
				try
				{
					lock (this.syncObj)
					{
						using (IDbCommand dbCommand = this.CreateCommand(this._sharedConnection, "", new object[0]))
						{
							PocoData pocoData = PocoData.ForObject(poco, primaryKeyName);
							System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
							System.Collections.Generic.List<string> list2 = new System.Collections.Generic.List<string>();
							int num = 0;
							foreach (System.Collections.Generic.KeyValuePair<string, PocoColumn> current in pocoData.Columns)
							{
								if (!current.Value.ResultColumn && (SqlBehaviorFlags.Insert & current.Value.SqlBehavior) != (SqlBehaviorFlags)0)
								{
									if (autoIncrement && primaryKeyName != null && string.Compare(current.Key, primaryKeyName, true) == 0)
									{
										string autoIncrementExpression = this._dbType.GetAutoIncrementExpression(pocoData.TableInfo);
										if (autoIncrementExpression != null)
										{
											list.Add(current.Key);
											list2.Add(autoIncrementExpression);
										}
									}
									else
									{
										list.Add(this._dbType.EscapeSqlIdentifier(current.Key));
										list2.Add(string.Format("{0}{1}", this._paramPrefix, num++));
										this.AddParam(dbCommand, current.Value.GetValue(poco), current.Value.PropertyInfo);
									}
								}
							}
							string text = string.Empty;
							if (autoIncrement)
							{
								text = this._dbType.GetInsertOutputClause(primaryKeyName);
							}
							dbCommand.CommandText = string.Format("INSERT INTO {0} ({1}){2} VALUES ({3})", new object[]
							{
								this._dbType.EscapeTableName(tableName),
								string.Join(",", list.ToArray()),
								text,
								string.Join(",", list2.ToArray())
							});
							if (!autoIncrement)
							{
								this.DoPreExecute(dbCommand);
								dbCommand.ExecuteNonQuery();
								this.OnExecutedCommand(dbCommand);
								PocoColumn pocoColumn;
								if (primaryKeyName != null && pocoData.Columns.TryGetValue(primaryKeyName, out pocoColumn))
								{
									result = pocoColumn.GetValue(poco);
								}
								else
								{
									result = null;
								}
							}
							else
							{
								object obj2 = this._dbType.ExecuteInsert(this, dbCommand, primaryKeyName);
								PocoColumn pocoColumn2;
								if (primaryKeyName != null && pocoData.Columns.TryGetValue(primaryKeyName, out pocoColumn2))
								{
									pocoColumn2.SetValue(poco, pocoColumn2.ChangeType(obj2));
								}
								result = obj2;
							}
						}
					}
				}
				finally
				{
					this.CloseSharedConnection();
				}
			}
			catch (System.Exception x)
			{
				if (this.OnException(x))
				{
					throw;
				}
				result = null;
			}
			return result;
		}
		public object Insert(object poco)
		{
			PocoData pocoData = PocoData.ForType(poco.GetType());
			return this.Insert(pocoData.TableInfo.TableName, pocoData.TableInfo.PrimaryKey, pocoData.TableInfo.AutoIncrement, poco);
		}
		public int Update(string tableName, string primaryKeyName, object poco, object primaryKeyValue)
		{
			return this.Update(tableName, primaryKeyName, poco, primaryKeyValue, null);
		}
		public int Update(string tableName, string primaryKeyName, object poco, object primaryKeyValue, System.Collections.Generic.IEnumerable<string> columns)
		{
			int result;
			try
			{
				this.OpenSharedConnection();
				try
				{
					lock (this.syncObj)
					{
						using (IDbCommand dbCommand = this.CreateCommand(this._sharedConnection, "", new object[0]))
						{
							System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
							int num = 0;
							PocoData pocoData = PocoData.ForObject(poco, primaryKeyName);
							if (columns == null)
							{
								using (System.Collections.Generic.Dictionary<string, PocoColumn>.Enumerator enumerator = pocoData.Columns.GetEnumerator())
								{
									while (enumerator.MoveNext())
									{
										System.Collections.Generic.KeyValuePair<string, PocoColumn> current = enumerator.Current;
										if (string.Compare(current.Key, primaryKeyName, true) == 0)
										{
											if (primaryKeyValue == null)
											{
												primaryKeyValue = current.Value.GetValue(poco);
											}
										}
										else
										{
											if (!current.Value.ResultColumn && (SqlBehaviorFlags.Update & current.Value.SqlBehavior) != (SqlBehaviorFlags)0)
											{
												if (num > 0)
												{
													stringBuilder.Append(", ");
												}
												stringBuilder.AppendFormat("{0} = {1}{2}", this._dbType.EscapeSqlIdentifier(current.Key), this._paramPrefix, num++);
												this.AddParam(dbCommand, current.Value.GetValue(poco), current.Value.PropertyInfo);
											}
										}
									}
									goto IL_1CB;
								}
							}
							foreach (string current2 in columns)
							{
								PocoColumn pocoColumn = pocoData.Columns[current2];
								if (num > 0)
								{
									stringBuilder.Append(", ");
								}
								stringBuilder.AppendFormat("{0} = {1}{2}", this._dbType.EscapeSqlIdentifier(current2), this._paramPrefix, num++);
								this.AddParam(dbCommand, pocoColumn.GetValue(poco), pocoColumn.PropertyInfo);
							}
							if (primaryKeyValue == null)
							{
								PocoColumn pocoColumn2 = pocoData.Columns[primaryKeyName];
								primaryKeyValue = pocoColumn2.GetValue(poco);
							}
							IL_1CB:
							System.Reflection.PropertyInfo pi = null;
							if (primaryKeyName != null)
							{
								pi = pocoData.Columns[primaryKeyName].PropertyInfo;
							}
							dbCommand.CommandText = string.Format("UPDATE {0} SET {1} WHERE {2} = {3}{4}", new object[]
							{
								this._dbType.EscapeTableName(tableName),
								stringBuilder.ToString(),
								this._dbType.EscapeSqlIdentifier(primaryKeyName),
								this._paramPrefix,
								num++
							});
							this.AddParam(dbCommand, primaryKeyValue, pi);
							this.DoPreExecute(dbCommand);
							int num2 = dbCommand.ExecuteNonQuery();
							this.OnExecutedCommand(dbCommand);
							result = num2;
						}
					}
				}
				finally
				{
					this.CloseSharedConnection();
				}
			}
			catch (System.Exception x)
			{
				if (this.OnException(x))
				{
					throw;
				}
				result = -1;
			}
			return result;
		}
		public int Update(string tableName, string primaryKeyName, object poco)
		{
			return this.Update(tableName, primaryKeyName, poco, null);
		}
		public int Update(string tableName, string primaryKeyName, object poco, System.Collections.Generic.IEnumerable<string> columns)
		{
			return this.Update(tableName, primaryKeyName, poco, null, columns);
		}
		public int Update(object poco, System.Collections.Generic.IEnumerable<string> columns)
		{
			return this.Update(poco, null, columns);
		}
		public int Update(object poco)
		{
			return this.Update(poco, null, null);
		}
		public int Update(object poco, object primaryKeyValue)
		{
			return this.Update(poco, primaryKeyValue, null);
		}
		public int Update(object poco, object primaryKeyValue, System.Collections.Generic.IEnumerable<string> columns)
		{
			PocoData pocoData = PocoData.ForType(poco.GetType());
			return this.Update(pocoData.TableInfo.TableName, pocoData.TableInfo.PrimaryKey, poco, primaryKeyValue, columns);
		}
		public int Update<T>(string sql, params object[] args)
		{
			PocoData pocoData = PocoData.ForType(typeof(T));
			return this.Execute(string.Format("UPDATE {0} {1}", this._dbType.EscapeTableName(pocoData.TableInfo.TableName), sql), args);
		}
		public int Update<T>(Sql sql)
		{
			PocoData pocoData = PocoData.ForType(typeof(T));
			return this.Execute(new Sql(string.Format("UPDATE {0}", this._dbType.EscapeTableName(pocoData.TableInfo.TableName)), new object[0]).Append(sql));
		}
		public int Delete(string tableName, string primaryKeyName, object poco)
		{
			return this.Delete(tableName, primaryKeyName, poco, null);
		}
		public int Delete(string tableName, string primaryKeyName, object poco, object primaryKeyValue)
		{
			if (primaryKeyValue == null)
			{
				PocoData pocoData = PocoData.ForObject(poco, primaryKeyName);
				PocoColumn pocoColumn;
				if (pocoData.Columns.TryGetValue(primaryKeyName, out pocoColumn))
				{
					primaryKeyValue = pocoColumn.GetValue(poco);
				}
			}
			string sql = string.Format("DELETE FROM {0} WHERE {1}=@0", this._dbType.EscapeTableName(tableName), this._dbType.EscapeSqlIdentifier(primaryKeyName));
			return this.Execute(sql, new object[]
			{
				primaryKeyValue
			});
		}
		public int Delete(object poco)
		{
			PocoData pocoData = PocoData.ForType(poco.GetType());
			return this.Delete(pocoData.TableInfo.TableName, pocoData.TableInfo.PrimaryKey, poco);
		}
		public int Delete<T>(object pocoOrPrimaryKey)
		{
			if (pocoOrPrimaryKey.GetType() == typeof(T))
			{
				return this.Delete(pocoOrPrimaryKey);
			}
			PocoData pocoData = PocoData.ForType(typeof(T));
			return this.Delete(pocoData.TableInfo.TableName, pocoData.TableInfo.PrimaryKey, null, pocoOrPrimaryKey);
		}
		public int Delete<T>(string sql, params object[] args)
		{
			PocoData pocoData = PocoData.ForType(typeof(T));
			return this.Execute(string.Format("DELETE FROM {0} {1}", this._dbType.EscapeTableName(pocoData.TableInfo.TableName), sql), args);
		}
		public int Delete<T>(Sql sql)
		{
			PocoData pocoData = PocoData.ForType(typeof(T));
			return this.Execute(new Sql(string.Format("DELETE FROM {0}", this._dbType.EscapeTableName(pocoData.TableInfo.TableName)), new object[0]).Append(sql));
		}
		public bool IsNew(string primaryKeyName, object poco)
		{
			PocoData pocoData = PocoData.ForObject(poco, primaryKeyName);
			PocoColumn pocoColumn;
			object value;
			if (pocoData.Columns.TryGetValue(primaryKeyName, out pocoColumn))
			{
				value = pocoColumn.GetValue(poco);
			}
			else
			{
				if (poco.GetType() == typeof(ExpandoObject))
				{
					return true;
				}
				System.Reflection.PropertyInfo property = poco.GetType().GetProperty(primaryKeyName);
				if (property == null)
				{
					throw new System.ArgumentException(string.Format("The object doesn't have a property matching the primary key column name '{0}'", primaryKeyName));
				}
				value = property.GetValue(poco, null);
			}
			if (value == null)
			{
				return true;
			}
			System.Type type = value.GetType();
			if (!type.IsValueType)
			{
				return value == null;
			}
			if (type == typeof(long))
			{
				return (long)value == 0L;
			}
			if (type == typeof(ulong))
			{
				return (ulong)value == 0uL;
			}
			if (type == typeof(int))
			{
				return (int)value == 0;
			}
			if (type == typeof(uint))
			{
				return (uint)value == 0u;
			}
			if (type == typeof(System.Guid))
			{
				return (System.Guid)value == default(System.Guid);
			}
			return value == System.Activator.CreateInstance(value.GetType());
		}
		public bool IsNew(object poco)
		{
			PocoData pocoData = PocoData.ForType(poco.GetType());
			if (!pocoData.TableInfo.AutoIncrement)
			{
				throw new System.InvalidOperationException("IsNew() and Save() are only supported on tables with auto-increment/identity primary key columns");
			}
			return this.IsNew(pocoData.TableInfo.PrimaryKey, poco);
		}
		public void Save(string tableName, string primaryKeyName, object poco)
		{
			if (this.IsNew(primaryKeyName, poco))
			{
				this.Insert(tableName, primaryKeyName, true, poco);
				return;
			}
			this.Update(tableName, primaryKeyName, poco);
		}
		public void Save(object poco)
		{
			PocoData pocoData = PocoData.ForType(poco.GetType());
			this.Save(pocoData.TableInfo.TableName, pocoData.TableInfo.PrimaryKey, poco);
		}
		public System.Collections.Generic.List<TRet> Fetch<T1, T2, TRet>(Func<T1, T2, TRet> cb, string sql, params object[] args)
		{
			return this.Query<T1, T2, TRet>(cb, sql, args).ToList<TRet>();
		}
		public System.Collections.Generic.List<TRet> Fetch<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, string sql, params object[] args)
		{
			return this.Query<T1, T2, T3, TRet>(cb, sql, args).ToList<TRet>();
		}
		public System.Collections.Generic.List<TRet> Fetch<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, string sql, params object[] args)
		{
			return this.Query<T1, T2, T3, T4, TRet>(cb, sql, args).ToList<TRet>();
		}
		public System.Collections.Generic.IEnumerable<TRet> Query<T1, T2, TRet>(Func<T1, T2, TRet> cb, string sql, params object[] args)
		{
			return this.Query<TRet>(new System.Type[]
			{
				typeof(T1),
				typeof(T2)
			}, cb, sql, args);
		}
		public System.Collections.Generic.IEnumerable<TRet> Query<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, string sql, params object[] args)
		{
			return this.Query<TRet>(new System.Type[]
			{
				typeof(T1),
				typeof(T2),
				typeof(T3)
			}, cb, sql, args);
		}
		public System.Collections.Generic.IEnumerable<TRet> Query<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, string sql, params object[] args)
		{
			return this.Query<TRet>(new System.Type[]
			{
				typeof(T1),
				typeof(T2),
				typeof(T3),
				typeof(T4)
			}, cb, sql, args);
		}
		public System.Collections.Generic.List<TRet> Fetch<T1, T2, TRet>(Func<T1, T2, TRet> cb, Sql sql)
		{
			return this.Query<T1, T2, TRet>(cb, sql.SQL, sql.Arguments).ToList<TRet>();
		}
		public System.Collections.Generic.List<TRet> Fetch<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, Sql sql)
		{
			return this.Query<T1, T2, T3, TRet>(cb, sql.SQL, sql.Arguments).ToList<TRet>();
		}
		public System.Collections.Generic.List<TRet> Fetch<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, Sql sql)
		{
			return this.Query<T1, T2, T3, T4, TRet>(cb, sql.SQL, sql.Arguments).ToList<TRet>();
		}
		public System.Collections.Generic.IEnumerable<TRet> Query<T1, T2, TRet>(Func<T1, T2, TRet> cb, Sql sql)
		{
			return this.Query<TRet>(new System.Type[]
			{
				typeof(T1),
				typeof(T2)
			}, cb, sql.SQL, sql.Arguments);
		}
		public System.Collections.Generic.IEnumerable<TRet> Query<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, Sql sql)
		{
			return this.Query<TRet>(new System.Type[]
			{
				typeof(T1),
				typeof(T2),
				typeof(T3)
			}, cb, sql.SQL, sql.Arguments);
		}
		public System.Collections.Generic.IEnumerable<TRet> Query<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, Sql sql)
		{
			return this.Query<TRet>(new System.Type[]
			{
				typeof(T1),
				typeof(T2),
				typeof(T3),
				typeof(T4)
			}, cb, sql.SQL, sql.Arguments);
		}
		public System.Collections.Generic.List<T1> Fetch<T1, T2>(string sql, params object[] args)
		{
			return this.Query<T1, T2>(sql, args).ToList<T1>();
		}
		public System.Collections.Generic.List<T1> Fetch<T1, T2, T3>(string sql, params object[] args)
		{
			return this.Query<T1, T2, T3>(sql, args).ToList<T1>();
		}
		public System.Collections.Generic.List<T1> Fetch<T1, T2, T3, T4>(string sql, params object[] args)
		{
			return this.Query<T1, T2, T3, T4>(sql, args).ToList<T1>();
		}
		public System.Collections.Generic.IEnumerable<T1> Query<T1, T2>(string sql, params object[] args)
		{
			return this.Query<T1>(new System.Type[]
			{
				typeof(T1),
				typeof(T2)
			}, null, sql, args);
		}
		public System.Collections.Generic.IEnumerable<T1> Query<T1, T2, T3>(string sql, params object[] args)
		{
			return this.Query<T1>(new System.Type[]
			{
				typeof(T1),
				typeof(T2),
				typeof(T3)
			}, null, sql, args);
		}
		public System.Collections.Generic.IEnumerable<T1> Query<T1, T2, T3, T4>(string sql, params object[] args)
		{
			return this.Query<T1>(new System.Type[]
			{
				typeof(T1),
				typeof(T2),
				typeof(T3),
				typeof(T4)
			}, null, sql, args);
		}
		public System.Collections.Generic.List<T1> Fetch<T1, T2>(Sql sql)
		{
			return this.Query<T1, T2>(sql.SQL, sql.Arguments).ToList<T1>();
		}
		public System.Collections.Generic.List<T1> Fetch<T1, T2, T3>(Sql sql)
		{
			return this.Query<T1, T2, T3>(sql.SQL, sql.Arguments).ToList<T1>();
		}
		public System.Collections.Generic.List<T1> Fetch<T1, T2, T3, T4>(Sql sql)
		{
			return this.Query<T1, T2, T3, T4>(sql.SQL, sql.Arguments).ToList<T1>();
		}
		public System.Collections.Generic.IEnumerable<T1> Query<T1, T2>(Sql sql)
		{
			return this.Query<T1>(new System.Type[]
			{
				typeof(T1),
				typeof(T2)
			}, null, sql.SQL, sql.Arguments);
		}
		public System.Collections.Generic.IEnumerable<T1> Query<T1, T2, T3>(Sql sql)
		{
			return this.Query<T1>(new System.Type[]
			{
				typeof(T1),
				typeof(T2),
				typeof(T3)
			}, null, sql.SQL, sql.Arguments);
		}
		public System.Collections.Generic.IEnumerable<T1> Query<T1, T2, T3, T4>(Sql sql)
		{
			return this.Query<T1>(new System.Type[]
			{
				typeof(T1),
				typeof(T2),
				typeof(T3),
				typeof(T4)
			}, null, sql.SQL, sql.Arguments);
		}
		public System.Collections.Generic.IEnumerable<TRet> Query<TRet>(System.Type[] types, object cb, string sql, params object[] args)
		{
			this.OpenSharedConnection();
			try
			{
				using (IDbCommand dbCommand = this.CreateCommand(this._sharedConnection, sql, args))
				{
					IDataReader dataReader;
					try
					{
						dataReader = dbCommand.ExecuteReader();
						this.OnExecutedCommand(dbCommand);
					}
					catch (System.Exception x)
					{
						if (this.OnException(x))
						{
							throw;
						}
						base.System.IDisposable.Dispose();
						goto IL_20A;
					}
					Func<IDataReader, object, TRet> factory = MultiPocoFactory.GetFactory<TRet>(types, this._sharedConnection.ConnectionString, sql, dataReader);
					if (cb == null)
					{
						cb = MultiPocoFactory.GetAutoMapper(types.ToArray<System.Type>());
					}
					bool flag = false;
					using (dataReader)
					{
						while (true)
						{
							TRet tRet;
							try
							{
								if (!dataReader.Read())
								{
									break;
								}
								tRet = factory(dataReader, cb);
							}
							catch (System.Exception x2)
							{
								if (this.OnException(x2))
								{
									throw;
								}
								base.System.IDisposable.Dispose();
								goto IL_20A;
							}
							if (tRet != null)
							{
								yield return tRet;
							}
							else
							{
								flag = true;
							}
						}
						if (flag)
						{
							TRet tRet2 = (TRet)((object)(cb as System.Delegate).DynamicInvoke(new object[types.Length]));
							if (tRet2 == null)
							{
								yield break;
							}
							yield return tRet2;
						}
					}
				}
			}
			finally
			{
				this.CloseSharedConnection();
			}
			IL_20A:
			yield break;
		}
		public string FormatCommand(IDbCommand cmd)
		{
			return this.FormatCommand(cmd.CommandText, (
				from IDataParameter parameter in cmd.Parameters
				select parameter.Value).ToArray<object>());
		}
		public string FormatCommand(string sql, object[] args)
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			if (sql == null)
			{
				return "";
			}
			stringBuilder.Append(sql);
			if (args != null && args.Length > 0)
			{
				stringBuilder.Append("\n");
				for (int i = 0; i < args.Length; i++)
				{
					stringBuilder.AppendFormat("\t -> {0}{1} [{2}] = \"{3}\"\n", new object[]
					{
						this._paramPrefix,
						i,
						args[i].GetType().Name,
						args[i]
					});
				}
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			return stringBuilder.ToString();
		}
		internal void ExecuteNonQueryHelper(IDbCommand cmd)
		{
			this.DoPreExecute(cmd);
			cmd.ExecuteNonQuery();
			this.OnExecutedCommand(cmd);
		}
		internal object ExecuteScalarHelper(IDbCommand cmd)
		{
			this.DoPreExecute(cmd);
			object result = cmd.ExecuteScalar();
			this.OnExecutedCommand(cmd);
			return result;
		}
		internal void DoPreExecute(IDbCommand cmd)
		{
			if (this.OneTimeCommandTimeout != 0)
			{
				cmd.CommandTimeout = this.OneTimeCommandTimeout;
				this.OneTimeCommandTimeout = 0;
			}
			else
			{
				if (this.CommandTimeout != 0)
				{
					cmd.CommandTimeout = this.CommandTimeout;
				}
			}
			this.OnExecutingCommand(cmd);
			this._lastSql = cmd.CommandText;
			this._lastArgs = (
				from IDataParameter parameter in cmd.Parameters
				select parameter.Value).ToArray<object>();
		}
	}
}
