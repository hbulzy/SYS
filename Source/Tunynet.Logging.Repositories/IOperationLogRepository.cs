using System;
using Tunynet.Repositories;
namespace Tunynet.Logging.Repositories
{
	public interface IOperationLogRepository : IRepository<OperationLogEntry>
	{
		int Clean(System.DateTime? startDate, System.DateTime? endDate);
		PagingDataSet<OperationLogEntry> GetLogs(OperationLogQuery query, int pageSize, int pageIndex);
	}
}
