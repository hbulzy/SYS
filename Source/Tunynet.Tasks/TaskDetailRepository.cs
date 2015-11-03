using PetaPoco;
using System;
using Tunynet.Repositories;
namespace Tunynet.Tasks
{
	public class TaskDetailRepository : Repository<TaskDetail>, ITaskDetailRepository, IRepository<TaskDetail>
	{
		public void SaveTaskStatus(TaskDetail taskDetail)
		{
			Sql builder = Sql.Builder;
			builder.Append("update tn_TaskDetails \r\n                       set LastStart = @0, LastEnd = @1,NextStart = @2,IsRunning = @3\r\n                       where Id = @4", new object[]
			{
				taskDetail.LastStart,
				taskDetail.LastEnd,
				taskDetail.NextStart,
				taskDetail.IsRunning,
				taskDetail.Id
			});
			this.CreateDAO().Execute(builder);
		}
	}
}
