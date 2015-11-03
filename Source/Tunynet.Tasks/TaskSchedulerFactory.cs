using System;
namespace Tunynet.Tasks
{
	public static class TaskSchedulerFactory
	{
		private static ITaskScheduler _scheduler;
		public static ITaskScheduler GetScheduler()
		{
			if (TaskSchedulerFactory._scheduler == null)
			{
				TaskSchedulerFactory._scheduler = DIContainer.Resolve<ITaskScheduler>();
			}
			return TaskSchedulerFactory._scheduler;
		}
	}
}
