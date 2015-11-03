using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
namespace PetaPoco.Internal
{
	internal class PocoData
	{
		private static Cache<System.Type, PocoData> _pocoDatas = new Cache<System.Type, PocoData>();
		private static System.Collections.Generic.List<Func<object, object>> _converters = new System.Collections.Generic.List<Func<object, object>>();
		private static System.Reflection.MethodInfo fnGetValue = typeof(IDataRecord).GetMethod("GetValue", new System.Type[]
		{
			typeof(int)
		});
		private static System.Reflection.MethodInfo fnIsDBNull = typeof(IDataRecord).GetMethod("IsDBNull");
		private static System.Reflection.FieldInfo fldConverters = typeof(PocoData).GetField("_converters", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField);
		private static System.Reflection.MethodInfo fnListGetItem = typeof(System.Collections.Generic.List<Func<object, object>>).GetProperty("Item").GetGetMethod();
		private static System.Reflection.MethodInfo fnInvoke = typeof(Func<object, object>).GetMethod("Invoke");
		public System.Type type;
		private Cache<Tuple<string, string, int, int>, System.Delegate> PocoFactories = new Cache<Tuple<string, string, int, int>, System.Delegate>();
		public string[] QueryColumns
		{
			get;
			private set;
		}
		public TableInfo TableInfo
		{
			get;
			private set;
		}
		public System.Collections.Generic.Dictionary<string, PocoColumn> Columns
		{
			get;
			private set;
		}
		public static PocoData ForObject(object o, string primaryKeyName)
		{
			System.Type type = o.GetType();
			if (type == typeof(ExpandoObject))
			{
				PocoData pocoData = new PocoData();
				pocoData.TableInfo = new TableInfo();
				pocoData.Columns = new System.Collections.Generic.Dictionary<string, PocoColumn>(System.StringComparer.OrdinalIgnoreCase);
				pocoData.Columns.Add(primaryKeyName, new ExpandoColumn
				{
					ColumnName = primaryKeyName
				});
				pocoData.TableInfo.PrimaryKey = primaryKeyName;
				pocoData.TableInfo.AutoIncrement = true;
				foreach (string current in (o as System.Collections.Generic.IDictionary<string, object>).Keys)
				{
					if (current != primaryKeyName)
					{
						pocoData.Columns.Add(current, new ExpandoColumn
						{
							ColumnName = current
						});
					}
				}
				return pocoData;
			}
			return PocoData.ForType(type);
		}
		public static PocoData ForType(System.Type t)
		{
			if (t == typeof(ExpandoObject))
			{
				throw new System.InvalidOperationException("Can't use dynamic types with this method");
			}
			return PocoData._pocoDatas.Get(t, () => new PocoData(t));
		}
		public PocoData()
		{
		}
		public PocoData(System.Type t)
		{
			this.type = t;
			IMapper mapper = Mappers.GetMapper(t);
			this.TableInfo = mapper.GetTableInfo(t);
			this.Columns = new System.Collections.Generic.Dictionary<string, PocoColumn>(System.StringComparer.OrdinalIgnoreCase);
			System.Reflection.PropertyInfo[] properties = t.GetProperties();
			for (int i = 0; i < properties.Length; i++)
			{
				System.Reflection.PropertyInfo propertyInfo = properties[i];
				if (propertyInfo.CanWrite && propertyInfo.CanRead)
				{
					ColumnInfo columnInfo = mapper.GetColumnInfo(propertyInfo);
					if (columnInfo != null)
					{
						PocoColumn pocoColumn = new PocoColumn();
						pocoColumn.PropertyInfo = propertyInfo;
						pocoColumn.ColumnName = columnInfo.ColumnName;
						pocoColumn.ResultColumn = columnInfo.ResultColumn;
						pocoColumn.ForceToUtc = columnInfo.ForceToUtc;
						object[] customAttributes = propertyInfo.GetCustomAttributes(typeof(SqlBehaviorAttribute), true);
						if (customAttributes.Length > 0)
						{
							SqlBehaviorAttribute sqlBehaviorAttribute = customAttributes[0] as SqlBehaviorAttribute;
							if (sqlBehaviorAttribute != null)
							{
								pocoColumn.SqlBehavior = sqlBehaviorAttribute.Behavior;
							}
						}
						this.Columns.Add(pocoColumn.ColumnName, pocoColumn);
					}
				}
			}
			this.QueryColumns = (
				from c in this.Columns
				where !c.Value.ResultColumn
				select c.Key).ToArray<string>();
		}
		private static bool IsIntegralType(System.Type t)
		{
			System.TypeCode typeCode = System.Type.GetTypeCode(t);
			return typeCode >= System.TypeCode.SByte && typeCode <= System.TypeCode.UInt64;
		}
		public System.Delegate GetFactory(string sql, string connString, int firstColumn, int countColumns, IDataReader r)
		{
			Tuple<string, string, int, int> key = Tuple.Create<string, string, int, int>(sql, connString, firstColumn, countColumns);
			return this.PocoFactories.Get(key, delegate
			{
				System.Reflection.Emit.DynamicMethod dynamicMethod = new System.Reflection.Emit.DynamicMethod("petapoco_factory_" + this.PocoFactories.Count.ToString(), this.type, new System.Type[]
				{
					typeof(IDataReader)
				}, true);
				System.Reflection.Emit.ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
				IMapper mapper = Mappers.GetMapper(this.type);
				if (this.type == typeof(object))
				{
					iLGenerator.Emit(System.Reflection.Emit.OpCodes.Newobj, typeof(ExpandoObject).GetConstructor(System.Type.EmptyTypes));
					System.Reflection.MethodInfo method = typeof(System.Collections.Generic.IDictionary<string, object>).GetMethod("Add");
					for (int i = firstColumn; i < firstColumn + countColumns; i++)
					{
						System.Type fieldType = r.GetFieldType(i);
						iLGenerator.Emit(System.Reflection.Emit.OpCodes.Dup);
						iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldstr, r.GetName(i));
						Func<object, object> fromDbConverter = mapper.GetFromDbConverter(null, fieldType);
						PocoData.AddConverterToStack(iLGenerator, fromDbConverter);
						iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
						iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, i);
						iLGenerator.Emit(System.Reflection.Emit.OpCodes.Callvirt, PocoData.fnGetValue);
						iLGenerator.Emit(System.Reflection.Emit.OpCodes.Dup);
						iLGenerator.Emit(System.Reflection.Emit.OpCodes.Isinst, typeof(System.DBNull));
						System.Reflection.Emit.Label label = iLGenerator.DefineLabel();
						iLGenerator.Emit(System.Reflection.Emit.OpCodes.Brfalse_S, label);
						iLGenerator.Emit(System.Reflection.Emit.OpCodes.Pop);
						if (fromDbConverter != null)
						{
							iLGenerator.Emit(System.Reflection.Emit.OpCodes.Pop);
						}
						iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldnull);
						if (fromDbConverter != null)
						{
							System.Reflection.Emit.Label label2 = iLGenerator.DefineLabel();
							iLGenerator.Emit(System.Reflection.Emit.OpCodes.Br_S, label2);
							iLGenerator.MarkLabel(label);
							iLGenerator.Emit(System.Reflection.Emit.OpCodes.Callvirt, PocoData.fnInvoke);
							iLGenerator.MarkLabel(label2);
						}
						else
						{
							iLGenerator.MarkLabel(label);
						}
						iLGenerator.Emit(System.Reflection.Emit.OpCodes.Callvirt, method);
					}
				}
				else
				{
					if (this.type.IsValueType || this.type == typeof(string) || this.type == typeof(byte[]))
					{
						System.Type fieldType2 = r.GetFieldType(0);
						Func<object, object> converter = PocoData.GetConverter(mapper, null, fieldType2, this.type);
						iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
						iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldc_I4_0);
						iLGenerator.Emit(System.Reflection.Emit.OpCodes.Callvirt, PocoData.fnIsDBNull);
						System.Reflection.Emit.Label label3 = iLGenerator.DefineLabel();
						iLGenerator.Emit(System.Reflection.Emit.OpCodes.Brfalse_S, label3);
						iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldnull);
						System.Reflection.Emit.Label label4 = iLGenerator.DefineLabel();
						iLGenerator.Emit(System.Reflection.Emit.OpCodes.Br_S, label4);
						iLGenerator.MarkLabel(label3);
						PocoData.AddConverterToStack(iLGenerator, converter);
						iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
						iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldc_I4_0);
						iLGenerator.Emit(System.Reflection.Emit.OpCodes.Callvirt, PocoData.fnGetValue);
						if (converter != null)
						{
							iLGenerator.Emit(System.Reflection.Emit.OpCodes.Callvirt, PocoData.fnInvoke);
						}
						iLGenerator.MarkLabel(label4);
						iLGenerator.Emit(System.Reflection.Emit.OpCodes.Unbox_Any, this.type);
					}
					else
					{
						iLGenerator.Emit(System.Reflection.Emit.OpCodes.Newobj, this.type.GetConstructor(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic, null, new System.Type[0], null));
						for (int j = firstColumn; j < firstColumn + countColumns; j++)
						{
							PocoColumn pocoColumn;
							if (this.Columns.TryGetValue(r.GetName(j), out pocoColumn))
							{
								System.Type fieldType3 = r.GetFieldType(j);
								System.Type propertyType = pocoColumn.PropertyInfo.PropertyType;
								iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
								iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, j);
								iLGenerator.Emit(System.Reflection.Emit.OpCodes.Callvirt, PocoData.fnIsDBNull);
								System.Reflection.Emit.Label label5 = iLGenerator.DefineLabel();
								iLGenerator.Emit(System.Reflection.Emit.OpCodes.Brtrue_S, label5);
								iLGenerator.Emit(System.Reflection.Emit.OpCodes.Dup);
								Func<object, object> converter2 = PocoData.GetConverter(mapper, pocoColumn, fieldType3, propertyType);
								bool flag = false;
								if (converter2 == null)
								{
									System.Reflection.MethodInfo method2 = typeof(IDataRecord).GetMethod("Get" + fieldType3.Name, new System.Type[]
									{
										typeof(int)
									});
									if (method2 != null && method2.ReturnType == fieldType3 && (method2.ReturnType == propertyType || method2.ReturnType == System.Nullable.GetUnderlyingType(propertyType)))
									{
										iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
										iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, j);
										iLGenerator.Emit(System.Reflection.Emit.OpCodes.Callvirt, method2);
										if (System.Nullable.GetUnderlyingType(propertyType) != null)
										{
											iLGenerator.Emit(System.Reflection.Emit.OpCodes.Newobj, propertyType.GetConstructor(new System.Type[]
											{
												System.Nullable.GetUnderlyingType(propertyType)
											}));
										}
										iLGenerator.Emit(System.Reflection.Emit.OpCodes.Callvirt, pocoColumn.PropertyInfo.GetSetMethod(true));
										flag = true;
									}
								}
								if (!flag)
								{
									PocoData.AddConverterToStack(iLGenerator, converter2);
									iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
									iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, j);
									iLGenerator.Emit(System.Reflection.Emit.OpCodes.Callvirt, PocoData.fnGetValue);
									if (converter2 != null)
									{
										iLGenerator.Emit(System.Reflection.Emit.OpCodes.Callvirt, PocoData.fnInvoke);
									}
									iLGenerator.Emit(System.Reflection.Emit.OpCodes.Unbox_Any, pocoColumn.PropertyInfo.PropertyType);
									iLGenerator.Emit(System.Reflection.Emit.OpCodes.Callvirt, pocoColumn.PropertyInfo.GetSetMethod(true));
								}
								iLGenerator.MarkLabel(label5);
							}
						}
						System.Reflection.MethodInfo methodInfo = PocoData.RecurseInheritedTypes<System.Reflection.MethodInfo>(this.type, (System.Type x) => x.GetMethod("OnLoaded", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic, null, new System.Type[0], null));
						if (methodInfo != null)
						{
							iLGenerator.Emit(System.Reflection.Emit.OpCodes.Dup);
							iLGenerator.Emit(System.Reflection.Emit.OpCodes.Callvirt, methodInfo);
						}
					}
				}
				iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ret);
				return dynamicMethod.CreateDelegate(Expression.GetFuncType(new System.Type[]
				{
					typeof(IDataReader),
					this.type
				}));
			});
		}
		private static void AddConverterToStack(System.Reflection.Emit.ILGenerator il, Func<object, object> converter)
		{
			if (converter != null)
			{
				int count = PocoData._converters.Count;
				PocoData._converters.Add(converter);
				il.Emit(System.Reflection.Emit.OpCodes.Ldsfld, PocoData.fldConverters);
				il.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, count);
				il.Emit(System.Reflection.Emit.OpCodes.Callvirt, PocoData.fnListGetItem);
			}
		}
		private static Func<object, object> GetConverter(IMapper mapper, PocoColumn pc, System.Type srcType, System.Type dstType)
		{
			if (pc != null)
			{
				Func<object, object> fromDbConverter = mapper.GetFromDbConverter(pc.PropertyInfo, srcType);
				if (fromDbConverter != null)
				{
					return fromDbConverter;
				}
			}
			if (pc != null && pc.ForceToUtc && srcType == typeof(System.DateTime) && (dstType == typeof(System.DateTime) || dstType == typeof(System.DateTime?)))
			{
				return (object src) => new System.DateTime(((System.DateTime)src).Ticks, System.DateTimeKind.Utc);
			}
			if (dstType.IsEnum && PocoData.IsIntegralType(srcType))
			{
				if (srcType != typeof(int))
				{
					return (object src) => System.Convert.ChangeType(src, typeof(int), null);
				}
			}
			else
			{
				if (!dstType.IsAssignableFrom(srcType))
				{
					if (dstType.IsEnum && srcType == typeof(string))
					{
						return (object src) => EnumMapper.EnumFromString(dstType, (string)src);
					}
					if (dstType.Equals(typeof(bool)))
					{
						return delegate(object src)
						{
							if (src.ToString() == "0")
							{
								return false;
							}
							return true;
						};
					}
					if (dstType.Equals(typeof(System.Guid)))
					{
						return (object src) => new System.Guid(src.ToString());
					}
					if (dstType.IsGenericType && dstType.GetGenericTypeDefinition() == typeof(System.Nullable<>))
					{
						return (object src) => System.Convert.ChangeType(src, System.Nullable.GetUnderlyingType(dstType));
					}
					return (object src) => System.Convert.ChangeType(src, dstType, null);
				}
			}
			return null;
		}
		private static T RecurseInheritedTypes<T>(System.Type t, Func<System.Type, T> cb)
		{
			while (t != null)
			{
				T t2 = cb(t);
				if (t2 != null)
				{
					return t2;
				}
				t = t.BaseType;
			}
			return default(T);
		}
		internal static void FlushCaches()
		{
			PocoData._pocoDatas.Flush();
		}
	}
}
