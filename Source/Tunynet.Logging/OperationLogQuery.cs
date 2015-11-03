using System;
namespace Tunynet.Logging
{
	public class OperationLogQuery
	{
		public string Operator;
		public string Keyword;
		public int? ApplicationId;
		public string OperationType;
		public System.DateTime? StartDateTime;
		public System.DateTime? EndDateTime;
		public long? OperatorUserId
		{
			get;
			set;
		}
		public string Source
		{
			get;
			set;
		}
	}
}
