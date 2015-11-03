using System;
using System.Collections.Generic;
using Tunynet.Repositories;
namespace Tunynet.Email
{
	public interface IEmailQueueRepository : IRepository<EmailQueueEntry>
	{
		System.Collections.Generic.IEnumerable<EmailQueueEntry> Dequeue(int maxNumber);
	}
}
