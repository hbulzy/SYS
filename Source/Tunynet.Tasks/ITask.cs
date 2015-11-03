using System;
namespace Tunynet.Tasks
{
	public interface ITask
	{
		void Execute(TaskDetail taskDetail = null);
	}
}
