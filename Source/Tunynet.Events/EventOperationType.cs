using System;
namespace Tunynet.Events
{
	public class EventOperationType
	{
		private static volatile EventOperationType _instance = null;
		private static readonly object lockObject = new object();
		public static EventOperationType Instance()
		{
			if (EventOperationType._instance == null)
			{
				lock (EventOperationType.lockObject)
				{
					if (EventOperationType._instance == null)
					{
						EventOperationType._instance = new EventOperationType();
					}
				}
			}
			return EventOperationType._instance;
		}
		private EventOperationType()
		{
		}
		public string Create()
		{
			return "Create";
		}
		public string Update()
		{
			return "Update";
		}
		public string Delete()
		{
			return "Delete";
		}
		public string Approved()
		{
			return "Approved";
		}
		public string Disapproved()
		{
			return "Disapproved";
		}
		public string SetEssential()
		{
			return "SetEssential";
		}
		public string CancelEssential()
		{
			return "CancelEssential";
		}
		public string SetSticky()
		{
			return "SetSticky";
		}
		public string CancelSticky()
		{
			return "CancelSticky";
		}
		public string SetCategory()
		{
			return "SetCategory";
		}
		public string ControlledView()
		{
			return "ControlledView";
		}
	}
}
