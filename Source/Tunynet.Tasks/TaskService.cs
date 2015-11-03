using System;
using System.Collections.Generic;
namespace Tunynet.Tasks
{
	public class TaskService
	{
		private ITaskDetailRepository taskDetailRepository;
		public TaskService() : this(new TaskDetailRepository())
		{
		}
		public TaskService(ITaskDetailRepository taskDetailRepository)
		{
			this.taskDetailRepository = taskDetailRepository;
		}
		public TaskDetail Get(int Id)
		{
			return this.taskDetailRepository.Get(Id);
		}
		public System.Collections.Generic.IEnumerable<TaskDetail> GetAll()
		{
			return this.taskDetailRepository.GetAll();
		}
		public void Update(TaskDetail entity)
		{
			this.taskDetailRepository.Update(entity);
			TaskSchedulerFactory.GetScheduler().Update(entity);
		}
		public void SaveTaskStatus(TaskDetail entity)
		{
			this.taskDetailRepository.SaveTaskStatus(entity);
		}
	}
}
