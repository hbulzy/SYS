using PetaPoco;
using System;
using System.Collections.Generic;
using Tunynet.Repositories;
namespace Tunynet.Logging.Repositories
{
	public class OperationLogRepository : Repository<OperationLogEntry>, IOperationLogRepository, IRepository<OperationLogEntry>
	{
		public int Clean(System.DateTime? startDate, System.DateTime? endDate)
		{
			Sql builder = Sql.Builder;
			builder.Append("delete from tn_OperationLogs", new object[0]);
			if (startDate.HasValue)
			{
				builder.Where("DateCreated >= @0", new object[]
				{
					startDate.Value
				});
			}
			if (endDate.HasValue)
			{
				builder.Where("DateCreated <= @0", new object[]
				{
					endDate.Value
				});
			}
			return this.CreateDAO().Execute(builder);
		}
		public PagingDataSet<OperationLogEntry> GetLogs(OperationLogQuery query, int pageSize, int pageIndex)
		{
			Sql builder = Sql.Builder;
			if (query.ApplicationId.HasValue)
			{
				builder.Where("ApplicationId = @0", new object[]
				{
					query.ApplicationId
				});
			}
			if (!string.IsNullOrEmpty(query.Keyword))
			{
				builder.Where("OperationObjectName like @0 or Description like @0", new object[]
				{
					'%' + query.Keyword + '%'
				});
			}
			if (!string.IsNullOrEmpty(query.OperationType))
			{
				builder.Where("OperationType = @0", new object[]
				{
					query.OperationType
				});
			}
			if (!string.IsNullOrEmpty(query.Operator))
			{
				builder.Where("Operator like @0", new object[]
				{
					"%" + query.Operator + "%"
				});
			}
			if (query.StartDateTime.HasValue)
			{
				builder.Where("DateCreated >= @0", new object[]
				{
					query.StartDateTime.Value
				});
			}
			if (query.EndDateTime.HasValue)
			{
				builder.Where("DateCreated <= @0", new object[]
				{
					query.EndDateTime.Value
				});
			}
			if (query.OperatorUserId.HasValue)
			{
				builder.Where("OperatorUserId = @0", new object[]
				{
					query.OperatorUserId.Value
				});
			}
			if (!string.IsNullOrEmpty(query.Source))
			{
				builder.Where("Source like @0", new object[]
				{
					"%" + query.Source + "%"
				});
			}
			builder.OrderBy(new object[]
			{
				"Id desc"
			});
			PagingEntityIdCollection pagingEntityIdCollection = this.CreateDAO().FetchPagingPrimaryKeys<OperationLogEntry>((long)this.PrimaryMaxRecords, pageSize, pageIndex, builder);
			System.Collections.Generic.IEnumerable<OperationLogEntry> entities = this.PopulateEntitiesByEntityIds<object>(pagingEntityIdCollection.GetPagingEntityIds(pageSize, pageIndex));
			return new PagingDataSet<OperationLogEntry>(entities)
			{
				PageIndex = pageIndex,
				PageSize = pageSize,
				TotalRecords = pagingEntityIdCollection.TotalRecords
			};
		}
	}
}
