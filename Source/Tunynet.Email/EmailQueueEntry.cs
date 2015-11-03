using PetaPoco;
using System;
using System.Net.Mail;
namespace Tunynet.Email
{
	[PrimaryKey("Id", autoIncrement = true), TableName("tn_EmailQueue")]
	[System.Serializable]
	public class EmailQueueEntry : IEntity
	{
		public int Id
		{
			get;
			protected set;
		}
		public int Priority
		{
			get;
			set;
		}
		public bool IsBodyHtml
		{
			get;
			set;
		}
		public string MailTo
		{
			get;
			set;
		}
		public string MailCc
		{
			get;
			set;
		}
		public string MailBcc
		{
			get;
			set;
		}
		public string MailFrom
		{
			get;
			set;
		}
		public string Subject
		{
			get;
			set;
		}
		public string Body
		{
			get;
			set;
		}
		public System.DateTime NextTryTime
		{
			get;
			set;
		}
		public int NumberOfTries
		{
			get;
			set;
		}
		public bool IsFailed
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
		public static EmailQueueEntry New()
		{
			return new EmailQueueEntry
			{
				MailFrom = string.Empty,
				MailTo = string.Empty,
				MailBcc = string.Empty,
				MailCc = string.Empty,
				Body = string.Empty,
				Subject = string.Empty,
				NextTryTime = System.DateTime.UtcNow
			};
		}
		public EmailQueueEntry()
		{
		}
		public EmailQueueEntry(MailMessage mail)
		{
			this.Priority = (int)mail.Priority;
			this.IsBodyHtml = mail.IsBodyHtml;
			this.MailTo = mail.To.ToString();
			this.MailCc = mail.CC.ToString();
			this.MailBcc = mail.Bcc.ToString();
			this.MailFrom = mail.From.ToString();
			this.Subject = mail.Subject;
			this.Body = mail.Body;
			this.NextTryTime = System.DateTime.UtcNow;
			this.NumberOfTries = 0;
			this.IsFailed = false;
		}
		public MailMessage AsMailMessage(EmailQueueEntry emailEntry)
		{
			MailMessage mailMessage = new MailMessage();
			mailMessage.Priority = (MailPriority)emailEntry.Priority;
			mailMessage.IsBodyHtml = emailEntry.IsBodyHtml;
			this.String2MailAddressCollection(mailMessage.To, emailEntry.MailTo);
			this.String2MailAddressCollection(mailMessage.CC, emailEntry.MailCc);
			this.String2MailAddressCollection(mailMessage.Bcc, emailEntry.MailBcc);
			mailMessage.From = new MailAddress(emailEntry.MailFrom);
			mailMessage.Subject = emailEntry.Subject;
			mailMessage.Body = emailEntry.Body;
			return mailMessage;
		}
		private void String2MailAddressCollection(MailAddressCollection collection, string emails)
		{
			string[] array = emails.Split(new char[]
			{
				','
			});
			if (array != null && array.Length > 0)
			{
				string[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					string text = array2[i];
					if (!string.IsNullOrEmpty(text.Trim()))
					{
						collection.Add(new MailAddress(text));
					}
				}
			}
		}
	}
}
