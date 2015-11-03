using log4net;
using log4net.Config;
using System;
using System.IO;
using Tunynet.Utilities;
namespace Tunynet.Logging.Log4Net
{
	public class Log4NetLoggerFactoryAdapter : ILoggerFactoryAdapter
	{
		private static bool _isConfigLoaded;
		public Log4NetLoggerFactoryAdapter() : this("~/Config/log4net.config")
		{
		}
		public Log4NetLoggerFactoryAdapter(string configFilename)
		{
			if (!Log4NetLoggerFactoryAdapter._isConfigLoaded)
			{
				IRunningEnvironment runningEnvironment = DIContainer.Resolve<IRunningEnvironment>();
				if (string.IsNullOrEmpty(configFilename))
				{
					configFilename = "~/Config/log4net.config";
				}
				System.IO.FileInfo fileInfo = new System.IO.FileInfo(WebUtility.GetPhysicalFilePath(configFilename));
				if (!fileInfo.Exists)
				{
					throw new System.ApplicationException(string.Format("log4net配置文件 {0} 未找到", fileInfo.FullName));
				}
				if (runningEnvironment.IsFullTrust)
				{
					XmlConfigurator.ConfigureAndWatch(fileInfo);
				}
				else
				{
					XmlConfigurator.Configure(fileInfo);
				}
				Log4NetLoggerFactoryAdapter._isConfigLoaded = true;
			}
		}
		public ILogger GetLogger(string loggerName)
		{
			return new Log4NetLogger(LogManager.GetLogger(loggerName));
		}
	}
}
