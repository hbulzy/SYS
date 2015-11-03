using System;
using Tunynet.Globalization;
namespace Tunynet
{
	public class ExceptionMessageDescriptor
	{
		public string MessageFormat
		{
			get;
			set;
		}
		public string MessageFormatResourceKey
		{
			get;
			set;
		}
		public int ApplicationId
		{
			get;
			set;
		}
		public object[] Arguments
		{
			get;
			set;
		}
		public ExceptionMessageDescriptor()
		{
		}
		public ExceptionMessageDescriptor(string messageFormatResourceKey, params object[] args) : this(messageFormatResourceKey, 0, args)
		{
		}
		public ExceptionMessageDescriptor(string messageFormatResourceKey, int applicationId = 0, params object[] args)
		{
			this.MessageFormatResourceKey = messageFormatResourceKey;
			if (applicationId > 0)
			{
				this.ApplicationId = applicationId;
			}
			this.Arguments = args;
		}
		internal string GetExceptionMeassage()
		{
			string text = null;
			if (!string.IsNullOrEmpty(this.MessageFormatResourceKey))
			{
				if (this.ApplicationId > 0)
				{
					text = ResourceAccessor.GetString(this.MessageFormatResourceKey, this.ApplicationId);
				}
				else
				{
					text = ResourceAccessor.GetString(this.MessageFormatResourceKey);
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(this.MessageFormat))
				{
					text = this.MessageFormat;
				}
			}
			if (text != null)
			{
				return string.Format(text, this.Arguments);
			}
			return string.Empty;
		}
	}
}
