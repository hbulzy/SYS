using System;
namespace Tunynet.Logging
{
	public interface ILoggerFactoryAdapter
	{
		ILogger GetLogger(string loggerName);
	}
}
