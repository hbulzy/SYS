using System;
namespace Tunynet.Logging
{
	public interface IOperationLogSpecificPart
	{
		int ApplicationId
		{
			get;
			set;
		}
		string Source
		{
			get;
			set;
		}
		string OperationType
		{
			get;
			set;
		}
		string OperationObjectName
		{
			get;
			set;
		}
		long OperationObjectId
		{
			get;
			set;
		}
		string Description
		{
			get;
			set;
		}
	}
}
