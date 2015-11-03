using System;
using Tunynet.Repositories;
namespace Tunynet.Email
{
	public class SmtpSettingsRepository : Repository<SmtpSettings>, ISmtpSettingsRepository, IRepository<SmtpSettings>
	{
	}
}
