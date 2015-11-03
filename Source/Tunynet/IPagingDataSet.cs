using System;
namespace Tunynet
{
	public interface IPagingDataSet
	{
		int PageIndex
		{
			get;
			set;
		}
		int PageSize
		{
			get;
			set;
		}
		long TotalRecords
		{
			get;
			set;
		}
	}
}
