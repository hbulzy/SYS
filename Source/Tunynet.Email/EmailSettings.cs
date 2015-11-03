using System;
using Tunynet.Caching;
namespace Tunynet.Email
{
	[CacheSetting(true)]
	[System.Serializable]
	public class EmailSettings : IEntity
	{
		private int batchSendLimit = 100;
		private string adminEmailAddress = "admin@yourdomain.com";
		private string noReplyAddress = "noreply@yourdomain.com";
		private int numberOfTries = 6;
		private int sendTimeInterval = 10;
		public int BatchSendLimit
		{
			get
			{
				return this.batchSendLimit;
			}
			set
			{
				this.batchSendLimit = value;
			}
		}
		public string AdminEmailAddress
		{
			get
			{
				return this.adminEmailAddress;
			}
			set
			{
				this.adminEmailAddress = value;
			}
		}
		public string NoReplyAddress
		{
			get
			{
				return this.noReplyAddress;
			}
			set
			{
				this.noReplyAddress = value;
			}
		}
		public int NumberOfTries
		{
			get
			{
				return this.numberOfTries;
			}
			set
			{
				this.numberOfTries = value;
			}
		}
		public int SendTimeInterval
		{
			get
			{
				return this.sendTimeInterval;
			}
			set
			{
				this.sendTimeInterval = value;
			}
		}
		public SmtpSettings SmtpSettings
		{
			get;
			set;
		}
		object IEntity.EntityId
		{
			get
			{
				return typeof(EmailSettings).FullName;
			}
		}
		bool IEntity.IsDeletedInDatabase
		{
			get;
			set;
		}
	}
}
