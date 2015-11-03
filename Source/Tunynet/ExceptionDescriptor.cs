using System;
using System.Text;
using System.Web;
using Tunynet.Globalization;
using Tunynet.Logging;
namespace Tunynet
{
	public abstract class ExceptionDescriptor
	{
		public bool IsLogEnabled
		{
			get;
			protected set;
		}
		public LogLevel LogLevel
		{
			get;
			protected set;
		}
		public string Message
		{
			get;
			set;
		}
		public ExceptionMessageDescriptor MessageDescriptor
		{
			get;
			set;
		}
		public abstract string GetLoggingMessage();
		public virtual string GetFriendlyMessage()
		{
			if (!string.IsNullOrEmpty(this.Message))
			{
				return this.Message;
			}
			if (this.MessageDescriptor != null)
			{
				return this.MessageDescriptor.GetExceptionMeassage();
			}
			return string.Empty;
		}
		public virtual string GetOperationContextMessage()
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			HttpContext current = HttpContext.Current;
			if (current != null && current.Request != null)
			{
				if (current.Request.Url != null)
				{
					stringBuilder.AppendLine(string.Format(ResourceAccessor.GetString("Common_ExceptionUrl"), current.Request.Url.AbsoluteUri));
				}
				if (current.Request.RequestType != null)
				{
					stringBuilder.AppendLine(string.Format(ResourceAccessor.GetString("Common_HttpMethod"), current.Request.RequestType));
				}
				if (current.Request.UserHostAddress != null)
				{
					stringBuilder.AppendLine(string.Format(ResourceAccessor.GetString("Common_UserIP"), current.Request.UserHostAddress));
				}
				if (current.Request.UserAgent != null)
				{
					stringBuilder.AppendLine(string.Format(ResourceAccessor.GetString("Common_UserAgent"), current.Request.UserAgent));
				}
			}
			return stringBuilder.ToString();
		}
	}
}
