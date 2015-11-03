using System;
using Tunynet.Logging.Repositories;
namespace Tunynet.Logging
{
	public class OperationLogService
	{
		private IOperationLogRepository repository;
		public OperationLogService() : this(new OperationLogRepository())
		{
		}
		public OperationLogService(IOperationLogRepository repository)
		{
			this.repository = repository;
		}
		public long Create(OperationLogEntry entry)
		{
			this.repository.Insert(entry);
			return entry.Id;
		}
		public OperationLogEntry Create<TEntity>(TEntity entity, string operationType) where TEntity : class
		{
			return this.Create<TEntity>(entity, operationType, default(TEntity));
		}
		public OperationLogEntry Create<TEntity>(TEntity entity, string operationType, TEntity historyData) where TEntity : class
		{
			IOperatorInfoGetter operatorInfoGetter = DIContainer.Resolve<IOperatorInfoGetter>();
			if (operatorInfoGetter == null)
			{
				throw new System.ApplicationException("IOperatorInfoGetter not registered to DIContainer");
			}
			OperatorInfo operatorInfo = operatorInfoGetter.GetOperatorInfo();
			OperationLogEntry operationLogEntry = new OperationLogEntry(operatorInfo);
			IOperationLogSpecificPartProcesser<TEntity> operationLogSpecificPartProcesser = DIContainer.Resolve<IOperationLogSpecificPartProcesser<TEntity>>();
			if (operationLogSpecificPartProcesser == null)
			{
				throw new System.ApplicationException(string.Format("IOperationLogSpecificPartProcesser<{0}> not registered to DIContainer", typeof(TEntity).Name));
			}
			if (historyData == null)
			{
				operationLogSpecificPartProcesser.Process(entity, operationType, operationLogEntry);
			}
			else
			{
				operationLogSpecificPartProcesser.Process(entity, operationType, historyData, operationLogEntry);
			}
			this.repository.Insert(operationLogEntry);
			return operationLogEntry;
		}
		public void Delete(long entryId)
		{
			this.repository.DeleteByEntityId(entryId);
		}
		public int Clean(System.DateTime? startDate, System.DateTime? endDate)
		{
			return this.repository.Clean(startDate, endDate);
		}
		public PagingDataSet<OperationLogEntry> GetLogs(OperationLogQuery query, int pageSize, int pageIndex)
		{
			return this.repository.GetLogs(query, pageSize, pageIndex);
		}
	}
}
