using PetaPoco.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
namespace PetaPoco
{
	public static class Mappers
	{
		private static System.Collections.Generic.Dictionary<object, IMapper> _mappers = new System.Collections.Generic.Dictionary<object, IMapper>();
		private static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
		public static void Register(System.Reflection.Assembly assembly, IMapper mapper)
		{
			Mappers.RegisterInternal(assembly, mapper);
		}
		public static void Register(System.Type type, IMapper mapper)
		{
			Mappers.RegisterInternal(type, mapper);
		}
		public static void Revoke(System.Reflection.Assembly assembly)
		{
			Mappers.RevokeInternal(assembly);
		}
		public static void Revoke(System.Type type)
		{
			Mappers.RevokeInternal(type);
		}
		public static void Revoke(IMapper mapper)
		{
			Mappers._lock.EnterWriteLock();
			try
			{
				foreach (System.Collections.Generic.KeyValuePair<object, IMapper> current in (
					from kvp in Mappers._mappers
					where kvp.Value == mapper
					select kvp).ToList<System.Collections.Generic.KeyValuePair<object, IMapper>>())
				{
					Mappers._mappers.Remove(current.Key);
				}
			}
			finally
			{
				Mappers._lock.ExitWriteLock();
				Mappers.FlushCaches();
			}
		}
		public static IMapper GetMapper(System.Type t)
		{
			Mappers._lock.EnterReadLock();
			IMapper result;
			try
			{
				IMapper mapper;
				if (Mappers._mappers.TryGetValue(t, out mapper))
				{
					result = mapper;
				}
				else
				{
					if (Mappers._mappers.TryGetValue(t.Assembly, out mapper))
					{
						result = mapper;
					}
					else
					{
						result = Singleton<StandardMapper>.Instance;
					}
				}
			}
			finally
			{
				Mappers._lock.ExitReadLock();
			}
			return result;
		}
		private static void RegisterInternal(object typeOrAssembly, IMapper mapper)
		{
			Mappers._lock.EnterWriteLock();
			try
			{
				Mappers._mappers.Add(typeOrAssembly, mapper);
			}
			finally
			{
				Mappers._lock.ExitWriteLock();
				Mappers.FlushCaches();
			}
		}
		private static void RevokeInternal(object typeOrAssembly)
		{
			Mappers._lock.EnterWriteLock();
			try
			{
				Mappers._mappers.Remove(typeOrAssembly);
			}
			finally
			{
				Mappers._lock.ExitWriteLock();
				Mappers.FlushCaches();
			}
		}
		private static void FlushCaches()
		{
			MultiPocoFactory.FlushCaches();
			PocoData.FlushCaches();
		}
	}
}
