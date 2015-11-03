using System;
using Tunynet.Logging;
namespace Tunynet
{
	public class ResourceExceptionDescriptor : ExceptionDescriptor
	{
		public ResourceExceptionDescriptor() : this(null)
		{
		}
		public ResourceExceptionDescriptor(string message)
		{
			base.IsLogEnabled = true;
			base.LogLevel = LogLevel.Warning;
			base.Message = message;
		}
		public ResourceExceptionDescriptor(string messageFormat, params object[] args) : this(string.Format(messageFormat, args))
		{
		}
		public override string GetLoggingMessage()
		{
			return this.GetFriendlyMessage() + "--" + this.GetOperationContextMessage();
		}
		public ResourceExceptionDescriptor WithContentNotFound(string title, object contentId = null)
		{
			string messageFormatResourceKey = "Exception_ContentNotFound";
			contentId = ((contentId == null) ? string.Empty : contentId.ToString());
			base.MessageDescriptor = new ExceptionMessageDescriptor(messageFormatResourceKey, new object[]
			{
				new
				{
					title,
					contentId
				}
			});
			return this;
		}
		public ResourceExceptionDescriptor WithUserNotFound(string userName, int userId)
		{
			string messageFormatResourceKey = "Exception_UserNotFound";
			base.MessageDescriptor = new ExceptionMessageDescriptor(messageFormatResourceKey, new object[]
			{
				new
				{
					userName,
					userId
				}
			});
			return this;
		}
	}
}
