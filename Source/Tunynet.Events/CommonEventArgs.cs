using System;
using Tunynet.Logging;
namespace Tunynet.Events
{
	public class CommonEventArgs : System.EventArgs
	{
		private string _eventOperationType;
		private int _applicationId;
		private OperatorInfo operatorInfo;
		public string EventOperationType
		{
			get
			{
				return this._eventOperationType;
			}
		}
		public int ApplicationId
		{
			get
			{
				return this._applicationId;
			}
		}
		public OperatorInfo OperatorInfo
		{
			get
			{
				return this.operatorInfo;
			}
		}
		public CommonEventArgs(string eventOperationType) : this(eventOperationType, 0)
		{
		}
		public CommonEventArgs(string eventOperationType, int applicationId)
		{
			this._eventOperationType = eventOperationType;
			this._applicationId = applicationId;
			IOperatorInfoGetter operatorInfoGetter = DIContainer.Resolve<IOperatorInfoGetter>();
			if (operatorInfoGetter == null)
			{
				throw new System.ApplicationException("IOperatorInfoGetter not registered to DIContainer");
			}
			this.operatorInfo = operatorInfoGetter.GetOperatorInfo();
		}
	}
}
