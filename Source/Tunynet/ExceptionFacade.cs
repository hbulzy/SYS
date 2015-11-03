using System;
using System.Runtime.Serialization;
using Tunynet.Logging;
namespace Tunynet
{
	[System.Serializable]
	public class ExceptionFacade : System.Exception, System.Runtime.Serialization.ISerializable
	{
		private readonly ExceptionDescriptor exceptionDescriptor;
		public override string Message
		{
			get
			{
				if (this.exceptionDescriptor != null)
				{
					return this.exceptionDescriptor.GetFriendlyMessage();
				}
				return base.Message;
			}
		}
		public string OperationContextMessage
		{
			get
			{
				return this.exceptionDescriptor.GetOperationContextMessage();
			}
		}
		public ExceptionFacade(ExceptionDescriptor exceptionDescriptor, System.Exception innerException = null) : base(null, innerException)
		{
			this.exceptionDescriptor = exceptionDescriptor;
		}
		public ExceptionFacade(string message = null, System.Exception innerException = null) : base(message, innerException)
		{
			this.exceptionDescriptor = new CommonExceptionDescriptor(message);
		}
		public void Log()
		{
			if (this.exceptionDescriptor != null && this.exceptionDescriptor.IsLogEnabled)
			{
				if (base.InnerException != null)
				{
					LoggerFactory.GetLogger().Log(this.exceptionDescriptor.LogLevel, this.exceptionDescriptor.GetLoggingMessage(), new object[]
					{
						base.InnerException
					});
					return;
				}
				LoggerFactory.GetLogger().Log(this.exceptionDescriptor.LogLevel, this.exceptionDescriptor.GetLoggingMessage(), new object[0]);
			}
		}
	}
}
