using System;
using System.Collections.Generic;
using System.Threading;
namespace PetaPoco.Internal
{
	internal class Cache<TKey, TValue>
	{
		private System.Collections.Generic.Dictionary<TKey, TValue> _map = new System.Collections.Generic.Dictionary<TKey, TValue>();
		private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
		public int Count
		{
			get
			{
				return this._map.Count;
			}
		}
		public TValue Get(TKey key, Func<TValue> factory)
		{
			this._lock.EnterReadLock();
			TValue result;
			try
			{
				TValue tValue;
				if (this._map.TryGetValue(key, out tValue))
				{
					result = tValue;
					return result;
				}
			}
			finally
			{
				this._lock.ExitReadLock();
			}
			this._lock.EnterWriteLock();
			try
			{
				TValue tValue;
				if (this._map.TryGetValue(key, out tValue))
				{
					result = tValue;
				}
				else
				{
					tValue = factory();
					this._map.Add(key, tValue);
					result = tValue;
				}
			}
			finally
			{
				this._lock.ExitWriteLock();
			}
			return result;
		}
		public void Flush()
		{
			this._lock.EnterWriteLock();
			try
			{
				this._map.Clear();
			}
			finally
			{
				this._lock.ExitWriteLock();
			}
		}
	}
}
