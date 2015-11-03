using PetaPoco;
using System;
using Tunynet.Caching;
namespace Tunynet.Tasks
{
	[PrimaryKey("Id", autoIncrement = true), TableName("tn_TaskDetails"), CacheSetting(false)]
	[System.Serializable]
	public class TaskDetail : IEntity
	{
		public int Id
		{
			get;
			set;
		}
		public string Name
		{
			get;
			set;
		}
		public bool Enabled
		{
			get;
			set;
		}
		public string TaskRule
		{
			get;
			set;
		}
		public bool RunAtRestart
		{
			get;
			set;
		}
		public RunAtServer RunAtServer
		{
			get;
			set;
		}
		public string ClassType
		{
			get;
			set;
		}
		public System.DateTime? LastStart
		{
			get;
			set;
		}
		public System.DateTime? LastEnd
		{
			get;
			set;
		}
		public bool? LastIsSuccess
		{
			get;
			set;
		}
		public System.DateTime? NextStart
		{
			get;
			set;
		}
		public System.DateTime StartDate
		{
			get;
			set;
		}
		public System.DateTime? EndDate
		{
			get;
			set;
		}
		public bool IsRunning
		{
			get;
			set;
		}
		object IEntity.EntityId
		{
			get
			{
				return this.Id;
			}
		}
		bool IEntity.IsDeletedInDatabase
		{
			get;
			set;
		}
		public static TaskDetail New()
		{
			return new TaskDetail();
		}
		public string GetRulePart(RulePart rulePart)
		{
			if (string.IsNullOrEmpty(this.TaskRule))
			{
				if (RulePart.dayofweek != rulePart)
				{
					return "1";
				}
				return null;
			}
			else
			{
				string text = this.TaskRule.Split(new char[]
				{
					' '
				}).GetValue((int)rulePart).ToString();
				if (text == "*" || text == "?")
				{
					if (RulePart.dayofweek != rulePart)
					{
						return "1";
					}
					return null;
				}
				else
				{
					if (text.Contains("/"))
					{
						return text.Substring(text.IndexOf("/") + 1);
					}
					return text;
				}
			}
		}
	}
}
