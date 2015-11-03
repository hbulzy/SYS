using System;
namespace Tunynet.Logging
{
	public interface ILogger
	{
		bool IsEnabled(LogLevel level);
		void Log(LogLevel level, object message);
		void Log(LogLevel level, System.Exception exception, object message);
		void Log(LogLevel level, string format, params object[] args);
	}
}
