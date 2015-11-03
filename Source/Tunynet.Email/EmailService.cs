using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using Tunynet.Events;
namespace Tunynet.Email
{
	public class EmailService
	{
		private object _lock = new object();
		private IEmailQueueRepository emailQueueRepository;
		private static int _index;
		public static System.Collections.Generic.List<SmtpSettings> AllSmtpSettings
		{
			get;
			set;
		}
		public EmailService() : this(new EmailQueueRepository())
		{
		}
		public EmailService(IEmailQueueRepository emailQueueRepository)
		{
			this.emailQueueRepository = emailQueueRepository;
		}
		public void SendFailed(System.Collections.Generic.IEnumerable<int> ids, int retryInterval, int maxNumberOfTries)
		{
			if (ids == null)
			{
				return;
			}
			foreach (int current in ids)
			{
				EmailQueueEntry emailQueueEntry = this.emailQueueRepository.Get(current);
				if (emailQueueEntry != null)
				{
					if (emailQueueEntry.NumberOfTries + 1 >= maxNumberOfTries)
					{
						emailQueueEntry.IsFailed = true;
					}
					else
					{
						emailQueueEntry.NumberOfTries++;
						emailQueueEntry.NextTryTime = System.DateTime.UtcNow.AddMinutes((double)retryInterval);
					}
					this.emailQueueRepository.Update(emailQueueEntry);
				}
			}
		}
		public System.Collections.Generic.Dictionary<int, MailMessage> Dequeue(int maxNumber)
		{
			System.Collections.Generic.Dictionary<int, MailMessage> dictionary = new System.Collections.Generic.Dictionary<int, MailMessage>();
			System.Collections.Generic.IEnumerable<EmailQueueEntry> enumerable = this.emailQueueRepository.Dequeue(maxNumber);
			if (enumerable != null)
			{
				foreach (EmailQueueEntry current in enumerable)
				{
					if (current != null)
					{
						dictionary.Add(current.Id, current.AsMailMessage(current));
					}
				}
			}
			return dictionary;
		}
		public int? Enqueue(MailMessage email)
		{
			if (email == null || email.To == null || email.To.Count < 1)
			{
				return null;
			}
			int value = 0;
			int.TryParse(this.emailQueueRepository.Insert(new EmailQueueEntry(email)).ToString(), out value);
			return new int?(value);
		}
		public void Delete(int id)
		{
			EmailQueueEntry emailQueueEntry = this.emailQueueRepository.Get(id);
			if (emailQueueEntry != null)
			{
				EventBus<EmailQueueEntry>.Instance().OnBefore(emailQueueEntry, new CommonEventArgs(EventOperationType.Instance().Delete()));
				this.emailQueueRepository.Delete(emailQueueEntry);
				EventBus<EmailQueueEntry>.Instance().OnAfter(emailQueueEntry, new CommonEventArgs(EventOperationType.Instance().Delete()));
			}
		}
		public void Delete(System.Collections.Generic.IEnumerable<int> ids)
		{
			if (ids == null)
			{
				return;
			}
			foreach (int current in ids)
			{
				this.Delete(current);
			}
		}
		public bool SendAsyn(MailMessage mail, bool isRetry = true)
		{
			string text;
			return this.SendAsyn(mail, out text, isRetry);
		}
		public bool SendAsyn(MailMessage mail, out string errorMessage, bool isRetry = true)
		{
			SmtpClient smtpClient = null;
			SmtpSettings smtpSettings = null;
			bool flag = true;
			errorMessage = string.Empty;
			try
			{
				smtpSettings = this.NextSmtpSettings();
				smtpClient = this.GetSmtpClient(smtpSettings);
			}
			catch (System.Exception ex)
			{
				errorMessage = ex.Message;
				flag = false;
			}
			if (flag && smtpClient != null)
			{
				try
				{
					string displayName = mail.From.DisplayName;
					if (smtpSettings.ForceSmtpUserAsFromAddress)
					{
						mail.From = new MailAddress(smtpSettings.UserEmailAddress, displayName);
						mail.Sender = new MailAddress(smtpSettings.UserEmailAddress, displayName);
					}
					smtpClient.SendAsync(mail, string.Empty);
				}
				catch (System.Exception ex2)
				{
					errorMessage = ex2.Message;
					flag = false;
				}
			}
			if (!flag && isRetry)
			{
				this.Enqueue(mail);
			}
			return flag;
		}
		public bool Send(MailMessage mail)
		{
			string text;
			return this.Send(mail, out text);
		}
		public bool Send(MailMessage mail, out string errorMessage)
		{
			SmtpClient smtpClient = null;
			SmtpSettings smtpSettings = null;
			try
			{
				smtpSettings = this.NextSmtpSettings();
				smtpClient = this.GetSmtpClient(smtpSettings);
			}
			catch (System.Exception ex)
			{
				errorMessage = ex.Message;
				bool result = false;
				return result;
			}
			if (smtpClient != null)
			{
				try
				{
					string displayName = mail.From.DisplayName;
					if (smtpSettings.ForceSmtpUserAsFromAddress)
					{
						mail.From = new MailAddress(smtpSettings.UserEmailAddress, displayName);
						mail.Sender = new MailAddress(smtpSettings.UserEmailAddress, displayName);
					}
					smtpClient.Send(mail);
				}
				catch (System.Exception ex2)
				{
					errorMessage = ex2.Message;
					bool result = false;
					return result;
				}
			}
			errorMessage = string.Empty;
			return true;
		}
		private SmtpClient GetSmtpClient(SmtpSettings smtpSettings = null)
		{
			if (smtpSettings == null)
			{
				IEmailSettingsManager emailSettingsManager = DIContainer.Resolve<IEmailSettingsManager>();
				EmailSettings emailSettings = emailSettingsManager.Get();
				smtpSettings = emailSettings.SmtpSettings;
			}
			SmtpClient smtpClient = new SmtpClient(smtpSettings.Host, smtpSettings.Port);
			smtpClient.EnableSsl = smtpSettings.EnableSsl;
			if (smtpSettings.RequireCredentials)
			{
				smtpClient.UseDefaultCredentials = false;
				smtpClient.Credentials = new NetworkCredential(smtpSettings.UserName, smtpSettings.Password);
				smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
			}
			return smtpClient;
		}
		private SmtpSettings NextSmtpSettings()
		{
			if (EmailService.AllSmtpSettings == null)
			{
				IEmailSettingsManager emailSettingsManager = DIContainer.Resolve<IEmailSettingsManager>();
				EmailSettings emailSettings = emailSettingsManager.Get();
				return emailSettings.SmtpSettings;
			}
			int num = 0;
			lock (this._lock)
			{
				EmailService._index = (EmailService._index + 1) % EmailService.AllSmtpSettings.Count<SmtpSettings>();
				num = EmailService._index;
			}
			for (int i = num; i < EmailService.AllSmtpSettings.Count<SmtpSettings>(); i++)
			{
				SmtpSettings smtpSettings = EmailService.AllSmtpSettings.ElementAt(i);
				if (smtpSettings.DailyLimit > smtpSettings.TodaySendCount)
				{
					smtpSettings.TodaySendCount++;
					return smtpSettings;
				}
			}
			for (int j = 0; j < num; j++)
			{
				SmtpSettings smtpSettings2 = EmailService.AllSmtpSettings.ElementAt(j);
				if (smtpSettings2.DailyLimit > smtpSettings2.TodaySendCount)
				{
					smtpSettings2.TodaySendCount++;
					return smtpSettings2;
				}
			}
			throw new System.Exception("所有的Smtp设置都超出了使用限制，请尝试添加更多的Smtp设置");
		}
	}
}
