using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
namespace Tunynet.Utilities
{
	public static class ReadOnlyCollectionExtension
	{
		public static System.Collections.Generic.IList<T> ToReadOnly<T>(this System.Collections.Generic.IEnumerable<T> enumerable)
		{
			return new System.Collections.ObjectModel.ReadOnlyCollection<T>(enumerable.ToList<T>());
		}
	}
}
