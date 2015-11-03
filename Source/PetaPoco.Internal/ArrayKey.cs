using System;
namespace PetaPoco.Internal
{
	internal class ArrayKey<T>
	{
		private T[] _keys;
		private int _hashCode;
		public ArrayKey(T[] keys)
		{
			this._keys = keys;
			this._hashCode = 17;
			for (int i = 0; i < keys.Length; i++)
			{
				T t = keys[i];
				this._hashCode = this._hashCode * 23 + ((t == null) ? 0 : t.GetHashCode());
			}
		}
		private bool Equals(ArrayKey<T> other)
		{
			if (other == null)
			{
				return false;
			}
			if (other._hashCode != this._hashCode)
			{
				return false;
			}
			if (other._keys.Length != this._keys.Length)
			{
				return false;
			}
			for (int i = 0; i < this._keys.Length; i++)
			{
				if (!object.Equals(this._keys[i], other._keys[i]))
				{
					return false;
				}
			}
			return true;
		}
		public override bool Equals(object obj)
		{
			return this.Equals(obj as ArrayKey<T>);
		}
		public override int GetHashCode()
		{
			return this._hashCode;
		}
	}
}
