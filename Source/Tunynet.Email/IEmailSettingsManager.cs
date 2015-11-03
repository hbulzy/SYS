using System;
namespace Tunynet.Email
{
	public interface IEmailSettingsManager
	{
		EmailSettings Get();
		void Save(EmailSettings emailSettings);
	}
}
