using System;
using Tunynet.Logging;
namespace Tunynet
{
	public class PermissionExceptionDescriptor : ExceptionDescriptor
	{
		public PermissionExceptionDescriptor() : this(null)
		{
		}
		public PermissionExceptionDescriptor(string message)
		{
			base.IsLogEnabled = true;
			base.LogLevel = LogLevel.Warning;
			base.Message = message;
		}
		public PermissionExceptionDescriptor(string messageFormat, params object[] args) : this(string.Format(messageFormat, args))
		{
		}
		public override string GetLoggingMessage()
		{
			return this.GetFriendlyMessage() + "--" + this.GetOperationContextMessage();
		}
		public PermissionExceptionDescriptor WithBasicManagementAccessDenied(string content)
		{
			string messageFormatResourceKey = "Exception_BasicManagementAccessDenied";
			base.MessageDescriptor = new ExceptionMessageDescriptor(messageFormatResourceKey, new object[]
			{
				new
				{
					content
				}
			});
			return this;
		}
		public PermissionExceptionDescriptor WithAccessDenied()
		{
			string messageFormatResourceKey = "Exception_AccessDenied";
			base.MessageDescriptor = new ExceptionMessageDescriptor(messageFormatResourceKey, new object[]
			{
				new
				{

				}
			});
			return this;
		}
		public PermissionExceptionDescriptor WithPostAccessDenied(string content)
		{
			string messageFormatResourceKey = "Exception_AccessDenied";
			base.MessageDescriptor = new ExceptionMessageDescriptor(messageFormatResourceKey, new object[]
			{
				new
				{
					content
				}
			});
			return this;
		}
		public PermissionExceptionDescriptor WithPostReplyAccessDenied()
		{
			string messageFormatResourceKey = "Exception_PostReplyAccessDenied";
			base.MessageDescriptor = new ExceptionMessageDescriptor(messageFormatResourceKey, new object[]
			{
				new
				{

				}
			});
			return this;
		}
		public PermissionExceptionDescriptor WithLicenseAuthorizeDenied()
		{
			string messageFormatResourceKey = "Exception_LicenseAuthorizeDenied";
			base.MessageDescriptor = new ExceptionMessageDescriptor(messageFormatResourceKey, new object[]
			{
				new
				{

				}
			});
			return this;
		}
	}
}
