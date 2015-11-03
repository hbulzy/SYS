using System;
using Tunynet.Repositories;
namespace Tunynet.Tasks
{
	public interface ITaskDetailRepository : IRepository<TaskDetail>
	{
		void SaveTaskStatus(TaskDetail taskDetail);
	}
}
