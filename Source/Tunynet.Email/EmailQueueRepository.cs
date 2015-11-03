using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using Tunynet.Repositories;
namespace Tunynet.Email
{
	public class EmailQueueRepository : Repository<EmailQueueEntry>, IEmailQueueRepository, IRepository<EmailQueueEntry>
	{
		public System.Collections.Generic.IEnumerable<EmailQueueEntry> Dequeue(int maxNumber)
		{
			if (maxNumber < 1)
			{
				return null;
			}
			Sql sql = Sql.Builder.Select(new object[]
			{
				"Id"
			}).From(new object[]
			{
				"tn_EmailQueue"
			}).Where("NextTryTime < @0 ", new object[]
			{
				System.DateTime.UtcNow
			}).Where("IsFailed = 0", new object[0]).OrderBy(new object[]
			{
				"Priority desc"
			});
			System.Collections.Generic.IEnumerable<object> source = this.CreateDAO().FetchTopPrimaryKeys<EmailQueueEntry>(maxNumber, sql);
			return this.PopulateEntitiesByEntityIds<int>(source.Cast<int>());
		}
	}
}
