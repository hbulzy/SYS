using System;
namespace Tunynet.Logging
{
	public static class LoggerExtension
	{
		public static void Debug(this ILogger logger, object message)
		{
			logger.Log(LogLevel.Debug, message);
		}
		public static void Info(this ILogger logger, object message)
		{
			logger.Log(LogLevel.Debug, message);
		}
		public static void Warn(this ILogger logger, object message)
		{
			logger.Log(LogLevel.Warning, message);
		}
		public static void Error(this ILogger logger, object message)
		{
			logger.Log(LogLevel.Error, message);
		}
		public static void Fatal(this ILogger logger, object message)
		{
			logger.Log(LogLevel.Fatal, message);
		}
		public static void Debug(this ILogger logger, System.Exception exception, object message)
		{
			logger.Log(LogLevel.Debug, exception, message);
		}
		public static void Info(this ILogger logger, System.Exception exception, object message)
		{
			logger.Log(LogLevel.Information, exception, message);
		}
		public static void Warn(this ILogger logger, System.Exception exception, object message)
		{
			logger.Log(LogLevel.Warning, exception, message);
		}
		public static void Error(this ILogger logger, System.Exception exception, object message)
		{
			logger.Log(LogLevel.Error, exception, message);
		}
		public static void Fatal(this ILogger logger, System.Exception exception, object message)
		{
			logger.Log(LogLevel.Fatal, exception, message);
		}
		public static void DebugFormat(this ILogger logger, string format, params object[] args)
		{
			logger.Log(LogLevel.Debug, format, args);
		}
		public static void InfoFormat(this ILogger logger, string format, params object[] args)
		{
			logger.Log(LogLevel.Information, format, args);
		}
		public static void WarnFormat(this ILogger logger, string format, params object[] args)
		{
			logger.Log(LogLevel.Warning, format, args);
		}
		public static void ErrorFormat(this ILogger logger, string format, params object[] args)
		{
			logger.Log(LogLevel.Error, format, args);
		}
		public static void FatalFormat(this ILogger logger, string format, params object[] args)
		{
			logger.Log(LogLevel.Fatal, format, args);
		}
	}
}
