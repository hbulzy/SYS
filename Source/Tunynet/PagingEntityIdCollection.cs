using System;
using System.Collections.Generic;
using System.Linq;
namespace Tunynet
{
	[System.Serializable]
	public class PagingEntityIdCollection
	{
		private System.Collections.Generic.List<object> entityIds;
		private long totalRecords = -1L;
		private bool isContainsMultiplePages;
		public int Count
		{
			get
			{
				if (this.entityIds == null)
				{
					return 0;
				}
				return this.entityIds.Count;
			}
		}
		public long TotalRecords
		{
			get
			{
				if (this.totalRecords > 0L)
				{
					return this.totalRecords;
				}
				return (long)this.Count;
			}
		}
		public bool IsContainsMultiplePages
		{
			get
			{
				return this.isContainsMultiplePages;
			}
			set
			{
				this.isContainsMultiplePages = value;
			}
		}
		public PagingEntityIdCollection(System.Collections.Generic.IEnumerable<object> entityIds)
		{
			this.entityIds = entityIds.ToList<object>();
		}
		public PagingEntityIdCollection(System.Collections.Generic.IEnumerable<object> entityIds, long totalRecords)
		{
			this.entityIds = entityIds.ToList<object>();
			this.totalRecords = totalRecords;
		}
		public System.Collections.Generic.IEnumerable<object> GetPagingEntityIds(int pageSize, int pageIndex)
		{
			if (this.entityIds == null)
			{
				return new System.Collections.Generic.List<object>();
			}
			if (!this.IsContainsMultiplePages)
			{
				return this.entityIds.GetRange(0, (this.Count > pageSize) ? pageSize : this.Count);
			}
			if (pageIndex < 1)
			{
				pageIndex = 1;
			}
			int num = pageSize * (pageIndex - 1);
			int num2 = pageSize * pageIndex;
			int count = this.entityIds.Count;
			if (num >= count)
			{
				return new System.Collections.Generic.List<object>();
			}
			if (num2 < count)
			{
				return this.entityIds.GetRange(num, pageSize);
			}
			return this.entityIds.GetRange(num, count - num);
		}
		public System.Collections.Generic.IEnumerable<object> GetTopEntityIds(int topNumber)
		{
			if (this.entityIds == null)
			{
				return new System.Collections.Generic.List<object>();
			}
			int arg_19_0 = this.entityIds.Count;
			return this.entityIds.GetRange(0, (this.Count > topNumber) ? topNumber : this.Count);
		}
	}
}
