using System;
using System.Collections.Generic;
namespace PetaPoco
{
	public class Page<T>
	{
		public long CurrentPage
		{
			get;
			set;
		}
		public long TotalPages
		{
			get;
			set;
		}
		public long TotalItems
		{
			get;
			set;
		}
		public long ItemsPerPage
		{
			get;
			set;
		}
		public System.Collections.Generic.List<T> Items
		{
			get;
			set;
		}
		public object Context
		{
			get;
			set;
		}
	}
}
