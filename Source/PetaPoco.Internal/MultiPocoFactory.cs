using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
namespace PetaPoco.Internal
{
	internal class MultiPocoFactory
	{
		private System.Collections.Generic.List<System.Delegate> _delegates;
		private static Cache<Tuple<System.Type, ArrayKey<System.Type>, string, string>, object> MultiPocoFactories = new Cache<Tuple<System.Type, ArrayKey<System.Type>, string, string>, object>();
		private static Cache<ArrayKey<System.Type>, object> AutoMappers = new Cache<ArrayKey<System.Type>, object>();
		public System.Delegate GetItem(int index)
		{
			return this._delegates[index];
		}
		public static object GetAutoMapper(System.Type[] types)
		{
			ArrayKey<System.Type> key = new ArrayKey<System.Type>(types);
			return MultiPocoFactory.AutoMappers.Get(key, delegate
			{
				System.Reflection.Emit.DynamicMethod dynamicMethod = new System.Reflection.Emit.DynamicMethod("petapoco_automapper", types[0], types, true);
				System.Reflection.Emit.ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
				int i;
				for (i = 1; i < types.Length; i++)
				{
					bool flag = false;
					for (int j = i - 1; j >= 0; j--)
					{
						System.Collections.Generic.IEnumerable<System.Reflection.PropertyInfo> source = 
							from p in types[j].GetProperties()
							where p.PropertyType == types[i]
							select p;
						if (source.Count<System.Reflection.PropertyInfo>() != 0)
						{
							if (source.Count<System.Reflection.PropertyInfo>() > 1)
							{
								throw new System.InvalidOperationException(string.Format("Can't auto join {0} as {1} has more than one property of type {0}", types[i], types[j]));
							}
							iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldarg_S, j);
							iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldarg_S, i);
							iLGenerator.Emit(System.Reflection.Emit.OpCodes.Callvirt, source.First<System.Reflection.PropertyInfo>().GetSetMethod(true));
							flag = true;
						}
					}
					if (!flag)
					{
						throw new System.InvalidOperationException(string.Format("Can't auto join {0}", types[i]));
					}
				}
				iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
				iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ret);
				return dynamicMethod.CreateDelegate(Expression.GetFuncType(types.Concat(types.Take(1)).ToArray<System.Type>()));
			});
		}
		private static System.Delegate FindSplitPoint(System.Type typeThis, System.Type typeNext, string ConnectionString, string sql, IDataReader r, ref int pos)
		{
			if (typeNext == null)
			{
				return PocoData.ForType(typeThis).GetFactory(sql, ConnectionString, pos, r.FieldCount - pos, r);
			}
			PocoData pocoData = PocoData.ForType(typeThis);
			PocoData pocoData2 = PocoData.ForType(typeNext);
			int num = pos;
			System.Collections.Generic.Dictionary<string, bool> dictionary = new System.Collections.Generic.Dictionary<string, bool>();
			while (pos < r.FieldCount)
			{
				string name = r.GetName(pos);
				if (dictionary.ContainsKey(name) || (!pocoData.Columns.ContainsKey(name) && pocoData2.Columns.ContainsKey(name)))
				{
					return pocoData.GetFactory(sql, ConnectionString, num, pos - num, r);
				}
				dictionary.Add(name, true);
				pos++;
			}
			throw new System.InvalidOperationException(string.Format("Couldn't find split point between {0} and {1}", typeThis, typeNext));
		}
		private static Func<IDataReader, object, TRet> CreateMultiPocoFactory<TRet>(System.Type[] types, string ConnectionString, string sql, IDataReader r)
		{
			System.Reflection.Emit.DynamicMethod dynamicMethod = new System.Reflection.Emit.DynamicMethod("petapoco_multipoco_factory", typeof(TRet), new System.Type[]
			{
				typeof(MultiPocoFactory),
				typeof(IDataReader),
				typeof(object)
			}, typeof(MultiPocoFactory));
			System.Reflection.Emit.ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
			iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldarg_2);
			System.Collections.Generic.List<System.Delegate> list = new System.Collections.Generic.List<System.Delegate>();
			int num = 0;
			for (int i = 0; i < types.Length; i++)
			{
				System.Delegate @delegate = MultiPocoFactory.FindSplitPoint(types[i], (i + 1 < types.Length) ? types[i + 1] : null, ConnectionString, sql, r, ref num);
				list.Add(@delegate);
				iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
				iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, i);
				iLGenerator.Emit(System.Reflection.Emit.OpCodes.Callvirt, typeof(MultiPocoFactory).GetMethod("GetItem"));
				iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
				System.Reflection.MethodInfo method = @delegate.GetType().GetMethod("Invoke");
				iLGenerator.Emit(System.Reflection.Emit.OpCodes.Callvirt, method);
			}
			iLGenerator.Emit(System.Reflection.Emit.OpCodes.Callvirt, Expression.GetFuncType(types.Concat(new System.Type[]
			{
				typeof(TRet)
			}).ToArray<System.Type>()).GetMethod("Invoke"));
			iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ret);
			return (Func<IDataReader, object, TRet>)dynamicMethod.CreateDelegate(typeof(Func<IDataReader, object, TRet>), new MultiPocoFactory
			{
				_delegates = list
			});
		}
		internal static void FlushCaches()
		{
			MultiPocoFactory.MultiPocoFactories.Flush();
			MultiPocoFactory.AutoMappers.Flush();
		}
		public static Func<IDataReader, object, TRet> GetFactory<TRet>(System.Type[] types, string ConnectionString, string sql, IDataReader r)
		{
			Tuple<System.Type, ArrayKey<System.Type>, string, string> key = Tuple.Create<System.Type, ArrayKey<System.Type>, string, string>(typeof(TRet), new ArrayKey<System.Type>(types), ConnectionString, sql);
			return (Func<IDataReader, object, TRet>)MultiPocoFactory.MultiPocoFactories.Get(key, () => MultiPocoFactory.CreateMultiPocoFactory<TRet>(types, ConnectionString, sql, r));
		}
	}
}
