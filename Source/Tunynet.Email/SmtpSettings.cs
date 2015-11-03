using PetaPoco;
using System;
using Tunynet.Caching;
namespace Tunynet.Email
{
	[PrimaryKey("Id", autoIncrement = true), TableName("tn_SmtpSettings"), CacheSetting(false)]
	[System.Serializable]
	public class SmtpSettings : IEntity
	{
		private bool forceSmtpUserAsFromAddress;
		public long Id
		{
			get;
			protected set;
		}
		public virtual string Host
		{
			get;
			set;
		}
		public virtual int Port
		{
			get;
			set;
		}
		public virtual bool EnableSsl
		{
			get;
			set;
		}
		public virtual bool RequireCredentials
		{
			get;
			set;
		}
		public virtual string UserName
		{
			get;
			set;
		}
		public virtual string UserEmailAddress
		{
			get;
			set;
		}
		public virtual string Password
		{
			get;
			set;
		}
		public bool ForceSmtpUserAsFromAddress
		{
			get
			{
				return this.forceSmtpUserAsFromAddress;
			}
			set
			{
				this.forceSmtpUserAsFromAddress = value;
			}
		}
		public int DailyLimit
		{
			get;
			set;
		}
		[Ignore]
		public virtual int TodaySendCount
		{
			get;
			set;
		}
		[Ignore]
		public object EntityId
		{
			get
			{
				return this.Id;
			}
		}
		[Ignore]
		public bool IsDeletedInDatabase
		{
			get;
			set;
		}
	}
}
