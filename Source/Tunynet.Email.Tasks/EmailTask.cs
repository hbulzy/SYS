using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading;
using Tunynet.Tasks;
namespace Tunynet.Email.Tasks
{
	public class EmailTask : ITask
	{
		private static ReaderWriterLockSlim RWLock = new ReaderWriterLockSlim();
		public void Execute(TaskDetail taskDetail = null)
		{
			EmailService emailService = new EmailService();
			System.Collections.Generic.List<int> list = new System.Collections.Generic.List<int>();
			System.Collections.Generic.List<int> list2 = new System.Collections.Generic.List<int>();
			EmailTask.RWLock.EnterWriteLock();
			IEmailSettingsManager emailSettingsManager = DIContainer.Resolve<IEmailSettingsManager>();
			EmailSettings emailSettings = emailSettingsManager.Get();
			System.Collections.Generic.Dictionary<int, MailMessage> dictionary = emailService.Dequeue(emailSettings.BatchSendLimit);
			foreach (System.Collections.Generic.KeyValuePair<int, MailMessage> current in dictionary)
			{
				if (emailService.Send(current.Value))
				{
					list.Add(current.Key);
				}
				else
				{
					list2.Add(current.Key);
				}
			}
			emailService.SendFailed(list2, emailSettings.SendTimeInterval, emailSettings.NumberOfTries);
			emailService.Delete(list);
			EmailTask.RWLock.ExitWriteLock();
		}
	}
}
