using PetaPoco;
using System;
using Tunynet.Caching;
namespace Tunynet.Logging
{
	[PrimaryKey("Id", autoIncrement = true), TableName("tn_OperationLogs"), CacheSetting(false)]
	[System.Serializable]
	public class OperationLogEntry : IEntity, IOperationLogSpecificPart
	{
		public long Id
		{
			get;
			protected set;
		}
		public int ApplicationId
		{
			get;
			set;
		}
		public string Source
		{
			get;
			set;
		}
		public string OperationType
		{
			get;
			set;
		}
		public string OperationObjectName
		{
			get;
			set;
		}
		public long OperationObjectId
		{
			get;
			set;
		}
		public string Description
		{
			get;
			set;
		}
		public long OperatorUserId
		{
			get;
			set;
		}
		public string Operator
		{
			get;
			set;
		}
		public string OperatorIP
		{
			get;
			set;
		}
		public string AccessUrl
		{
			get;
			set;
		}
		public System.DateTime DateCreated
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
		public OperationLogEntry()
		{
		}
		public OperationLogEntry(OperatorInfo operatorInfo)
		{
			this.OperatorUserId = operatorInfo.OperatorUserId;
			this.OperatorIP = operatorInfo.OperatorIP;
			this.Operator = operatorInfo.Operator;
			this.AccessUrl = operatorInfo.AccessUrl;
			this.DateCreated = System.DateTime.UtcNow;
		}
	}
}
