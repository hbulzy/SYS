using log4net;
using System;
namespace Tunynet.Logging.Log4Net
{
	public class Log4NetLogger : ILogger
	{
		private ILog log;
		internal Log4NetLogger(ILog log)
		{
			this.log = log;
		}
		public bool IsEnabled(LogLevel level)
		{
			switch (level)
			{
			case LogLevel.Debug:
				return this.log.get_IsDebugEnabled();
			case LogLevel.Information:
				return this.log.get_IsInfoEnabled();
			case LogLevel.Warning:
				return this.log.get_IsWarnEnabled();
			case LogLevel.Error:
				return this.log.get_IsErrorEnabled();
			case LogLevel.Fatal:
				return this.log.get_IsFatalEnabled();
			default:
				return false;
			}
		}
		public void Log(LogLevel level, object message)
		{
			if (!this.IsEnabled(level))
			{
				return;
			}
			switch (level)
			{
			case LogLevel.Debug:
				this.log.Debug(message);
				return;
			case LogLevel.Information:
				this.log.Info(message);
				return;
			case LogLevel.Warning:
				this.log.Warn(message);
				return;
			case LogLevel.Error:
				this.log.Error(message);
				return;
			case LogLevel.Fatal:
				this.log.Fatal(message);
				return;
			default:
				return;
			}
		}
		public void Log(LogLevel level, System.Exception exception, object message)
		{
			if (!this.IsEnabled(level))
			{
				return;
			}
			switch (level)
			{
			case LogLevel.Debug:
				this.log.Debug(message, exception);
				return;
			case LogLevel.Information:
				this.log.Info(message, exception);
				return;
			case LogLevel.Warning:
				this.log.Warn(message, exception);
				return;
			case LogLevel.Error:
				this.log.Error(message, exception);
				return;
			case LogLevel.Fatal:
				this.log.Fatal(message, exception);
				return;
			default:
				return;
			}
		}
		public void Log(LogLevel level, string format, params object[] args)
		{
			if (!this.IsEnabled(level))
			{
				return;
			}
			switch (level)
			{
			case LogLevel.Debug:
				this.log.DebugFormat(format, args);
				return;
			case LogLevel.Information:
				this.log.InfoFormat(format, args);
				return;
			case LogLevel.Warning:
				this.log.WarnFormat(format, args);
				return;
			case LogLevel.Error:
				this.log.ErrorFormat(format, args);
				return;
			case LogLevel.Fatal:
				this.log.FatalFormat(format, args);
				return;
			default:
				return;
			}
		}
	}
}
