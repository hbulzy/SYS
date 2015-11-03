using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using Tunynet.Logging;
namespace Tunynet.Tasks.Quartz
{
	public class QuartzTaskScheduler : ITaskScheduler
	{
		private static System.Collections.Generic.List<TaskDetail> _tasks;
		public QuartzTaskScheduler() : this(new TaskDetailRepository())
		{
		}
		public QuartzTaskScheduler(RunAtServer runAtServer) : this(new TaskDetailRepository())
		{
			if (QuartzTaskScheduler._tasks != null)
			{
				QuartzTaskScheduler._tasks = (
					from n in QuartzTaskScheduler._tasks
					where n.RunAtServer == runAtServer
					select n).ToList<TaskDetail>();
			}
		}
		public QuartzTaskScheduler(ITaskDetailRepository taskDetailRepository)
		{
			if (QuartzTaskScheduler._tasks == null)
			{
				QuartzTaskScheduler._tasks = new TaskService(taskDetailRepository).GetAll().ToList<TaskDetail>();
			}
		}
		public void Start()
		{
			if (QuartzTaskScheduler._tasks.Count<TaskDetail>() == 0)
			{
				return;
			}
			new TaskService();
			ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
			IScheduler scheduler = schedulerFactory.GetScheduler();
			foreach (TaskDetail current in QuartzTaskScheduler._tasks)
			{
				if (current.Enabled)
				{
					System.Type type = System.Type.GetType(current.ClassType);
					if (!(type == null))
					{
						string text = type.Name + "_trigger";
						IJobDetail jobDetail = JobBuilder.Create(typeof(QuartzTask)).WithIdentity(type.Name).Build();
						jobDetail.get_JobDataMap().Add(new System.Collections.Generic.KeyValuePair<string, object>("Id", current.Id));
						TriggerBuilder triggerBuilder = CronScheduleTriggerBuilderExtensions.WithCronSchedule(TriggerBuilder.Create().WithIdentity(text), current.TaskRule);
						if (current.StartDate > System.DateTime.MinValue)
						{
							triggerBuilder.StartAt(new System.DateTimeOffset(current.StartDate));
						}
						if (current.EndDate > current.StartDate)
						{
							TriggerBuilder arg_14A_0 = triggerBuilder;
							System.DateTime? endDate = current.EndDate;
							arg_14A_0.EndAt(endDate.HasValue ? new System.DateTimeOffset?(endDate.GetValueOrDefault()) : null);
						}
						ICronTrigger cronTrigger = (ICronTrigger)triggerBuilder.Build();
						System.DateTime dateTime = scheduler.ScheduleJob(jobDetail, cronTrigger).DateTime;
						current.NextStart = new System.DateTime?(TimeZoneInfo.ConvertTime(dateTime, cronTrigger.get_TimeZone()));
					}
				}
			}
			scheduler.Start();
		}
		public void Stop()
		{
			ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
			IScheduler scheduler = schedulerFactory.GetScheduler();
			scheduler.Shutdown(true);
		}
		public void Update(TaskDetail task)
		{
			if (task == null)
			{
				return;
			}
			int index = QuartzTaskScheduler._tasks.FindIndex((TaskDetail n) => n.Id == task.Id);
			if (QuartzTaskScheduler._tasks[index] == null)
			{
				return;
			}
			task.ClassType = QuartzTaskScheduler._tasks[index].ClassType;
			task.LastEnd = QuartzTaskScheduler._tasks[index].LastEnd;
			task.LastStart = QuartzTaskScheduler._tasks[index].LastStart;
			task.LastIsSuccess = QuartzTaskScheduler._tasks[index].LastIsSuccess;
			QuartzTaskScheduler._tasks[index] = task;
			System.Type type = System.Type.GetType(task.ClassType);
			if (type == null)
			{
				return;
			}
			this.Remove(type.Name);
			if (!task.Enabled)
			{
				return;
			}
			string text = type.Name + "_trigger";
			ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
			IScheduler scheduler = schedulerFactory.GetScheduler();
			IJobDetail jobDetail = JobBuilder.Create(typeof(QuartzTask)).WithIdentity(type.Name).Build();
			jobDetail.get_JobDataMap().Add(new System.Collections.Generic.KeyValuePair<string, object>("Id", task.Id));
			TriggerBuilder triggerBuilder = CronScheduleTriggerBuilderExtensions.WithCronSchedule(TriggerBuilder.Create().WithIdentity(text), task.TaskRule);
			if (task.StartDate > System.DateTime.MinValue)
			{
				triggerBuilder.StartAt(new System.DateTimeOffset(task.StartDate));
			}
			if (task.EndDate.HasValue && task.EndDate > task.StartDate)
			{
				TriggerBuilder arg_233_0 = triggerBuilder;
				System.DateTime? endDate = task.EndDate;
				arg_233_0.EndAt(endDate.HasValue ? new System.DateTimeOffset?(endDate.GetValueOrDefault()) : null);
			}
			ICronTrigger cronTrigger = (ICronTrigger)triggerBuilder.Build();
			System.DateTime dateTime = scheduler.ScheduleJob(jobDetail, cronTrigger).DateTime;
			task.NextStart = new System.DateTime?(TimeZoneInfo.ConvertTime(dateTime, cronTrigger.get_TimeZone()));
		}
		public void ResumeAll()
		{
			this.Stop();
			this.Start();
		}
		public TaskDetail GetTask(int Id)
		{
			return QuartzTaskScheduler._tasks.FirstOrDefault((TaskDetail n) => n.Id == Id);
		}
		public void Run(int Id)
		{
			TaskDetail task = this.GetTask(Id);
			this.Run(task);
		}
		public void Run(TaskDetail task)
		{
			if (task == null)
			{
				return;
			}
			System.Type type = System.Type.GetType(task.ClassType);
			if (type == null)
			{
				LoggerFactory.GetLogger().Error(string.Format("任务： {0} 的taskType为空。", task.Name));
				return;
			}
			ITask task2 = (ITask)System.Activator.CreateInstance(type);
			if (task2 != null && !task.IsRunning)
			{
				try
				{
					task2.Execute(null);
				}
				catch (System.Exception exception)
				{
					LoggerFactory.GetLogger().Error(exception, string.Format("执行任务： {0} 出现异常。", task.Name));
				}
			}
		}
		public void SaveTaskStatus()
		{
			foreach (TaskDetail current in QuartzTaskScheduler._tasks)
			{
				TaskService taskService = new TaskService();
				taskService.SaveTaskStatus(current);
			}
		}
		private void Remove(string name)
		{
			ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
			IScheduler scheduler = schedulerFactory.GetScheduler();
			scheduler.DeleteJob(new JobKey(name));
		}
	}
}
