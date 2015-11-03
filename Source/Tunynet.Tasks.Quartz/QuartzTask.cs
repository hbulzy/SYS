using Quartz;
using System;
using Tunynet.Logging;
namespace Tunynet.Tasks.Quartz
{
	public class QuartzTask : IJob
	{
		public void Execute(IJobExecutionContext context)
		{
			int @int = context.get_JobDetail().get_JobDataMap().GetInt("Id");
			TaskDetail task = TaskSchedulerFactory.GetScheduler().GetTask(@int);
			if (task == null)
			{
				throw new System.ArgumentException("Not found task ：" + task.Name);
			}
			TaskService taskService = new TaskService();
			task.IsRunning = true;
			System.DateTime utcNow = System.DateTime.UtcNow;
			try
			{
				ITask task2 = (ITask)System.Activator.CreateInstance(System.Type.GetType(task.ClassType));
				task2.Execute(task);
				task.LastIsSuccess = new bool?(true);
			}
			catch (System.Exception exception)
			{
				LoggerFactory.GetLogger().Error(exception, string.Format("Exception while running job {0} of type {1}", context.get_JobDetail().get_Key(), context.get_JobDetail().get_JobType().ToString()));
				task.LastIsSuccess = new bool?(false);
			}
			task.IsRunning = false;
			task.LastStart = new System.DateTime?(utcNow);
			if (context.get_NextFireTimeUtc().HasValue)
			{
				task.NextStart = new System.DateTime?(context.get_NextFireTimeUtc().Value.UtcDateTime);
			}
			else
			{
				task.NextStart = null;
			}
			task.LastEnd = new System.DateTime?(System.DateTime.UtcNow);
			taskService.SaveTaskStatus(task);
		}
	}
}
