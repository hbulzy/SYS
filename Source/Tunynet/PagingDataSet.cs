using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
namespace Tunynet
{
	[System.Serializable]
	public class PagingDataSet<T> : System.Collections.ObjectModel.ReadOnlyCollection<T>, IPagingDataSet
	{
		private int _pageSize = 20;
		private int _pageIndex = 1;
		private long _totalRecords;
		private double queryDuration;
		public int PageSize
		{
			get
			{
				return this._pageSize;
			}
			set
			{
				this._pageSize = value;
			}
		}
		public int PageIndex
		{
			get
			{
				return this._pageIndex;
			}
			set
			{
				this._pageIndex = value;
			}
		}
		public long TotalRecords
		{
			get
			{
				return this._totalRecords;
			}
			set
			{
				this._totalRecords = value;
			}
		}
		public int PageCount
		{
			get
			{
				long num = this.TotalRecords / (long)this.PageSize;
				if (this.TotalRecords % (long)this.PageSize != 0L)
				{
					num += 1L;
				}
				return System.Convert.ToInt32(num);
			}
		}
		public double QueryDuration
		{
			get
			{
				return this.queryDuration;
			}
			set
			{
				this.queryDuration = value;
			}
		}
		public PagingDataSet(System.Collections.Generic.IEnumerable<T> entities) : base(entities.ToList<T>())
		{
		}
		public PagingDataSet(System.Collections.Generic.IList<T> entities) : base(entities)
		{
		}
	}
}
