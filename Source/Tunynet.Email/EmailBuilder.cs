using Microsoft.CSharp.RuntimeBinder;
using RazorEngine;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Xml;
using Tunynet.Caching;
using Tunynet.Utilities;
namespace Tunynet.Email
{
	public class EmailBuilder
	{
		[System.Runtime.CompilerServices.CompilerGenerated]
		private static class <Resolve>o__SiteContainer2
		{
			public static CallSite<Func<CallSite, object, bool>> <>p__Site3;
			public static CallSite<Func<CallSite, object, object, object>> <>p__Site4;
			public static CallSite<Func<CallSite, System.Type, string, object, MailAddress>> <>p__Site5;
			public static CallSite<Func<CallSite, object, object>> <>p__Site6;
			public static CallSite<Func<CallSite, object, string>> <>p__Site7;
			public static CallSite<Func<CallSite, System.Type, string, object, string, object>> <>p__Site8;
			public static CallSite<Func<CallSite, object, string>> <>p__Site9;
			public static CallSite<Func<CallSite, System.Type, string, object, string, object>> <>p__Sitea;
		}
		private static volatile EmailBuilder _defaultInstance = null;
		private static readonly object lockObject = new object();
		private static bool isInitialized;
		private static System.Collections.Generic.Dictionary<string, EmailTemplate> emailTemplates = null;
		public System.Collections.Generic.IList<EmailTemplate> EmailTemplates
		{
			get
			{
				return EmailBuilder.emailTemplates.Values.ToReadOnly<EmailTemplate>();
			}
		}
		private EmailBuilder()
		{
		}
		public static EmailBuilder Instance()
		{
			if (EmailBuilder._defaultInstance == null)
			{
				lock (EmailBuilder.lockObject)
				{
					if (EmailBuilder._defaultInstance == null)
					{
						EmailBuilder._defaultInstance = new EmailBuilder();
					}
				}
			}
			EmailBuilder.EnsureLoadTemplates();
			return EmailBuilder._defaultInstance;
		}
		private static void EnsureLoadTemplates()
		{
			if (!EmailBuilder.isInitialized)
			{
				lock (EmailBuilder.lockObject)
				{
					if (!EmailBuilder.isInitialized)
					{
						EmailBuilder.emailTemplates = EmailBuilder.LoadEmailTemplates();
						EmailBuilder.isInitialized = true;
					}
				}
			}
		}
		public MailMessage Resolve(string templateName, ExpandoObject model, string to, string from = null)
		{
			return this.Resolve(templateName, model, new string[]
			{
				to
			}, from, null, null);
		}
		public MailMessage Resolve(string templateName, [Dynamic] dynamic model, System.Collections.Generic.IEnumerable<string> to, string from = null, System.Collections.Generic.IEnumerable<string> cc = null, System.Collections.Generic.IEnumerable<string> bcc = null)
		{
			if (to == null)
			{
				return null;
			}
			if (EmailBuilder.<Resolve>o__SiteContainer2.<>p__Site3 == null)
			{
				EmailBuilder.<Resolve>o__SiteContainer2.<>p__Site3 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(EmailBuilder), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, object, bool> arg_A5_0 = EmailBuilder.<Resolve>o__SiteContainer2.<>p__Site3.Target;
			CallSite arg_A5_1 = EmailBuilder.<Resolve>o__SiteContainer2.<>p__Site3;
			if (EmailBuilder.<Resolve>o__SiteContainer2.<>p__Site4 == null)
			{
				EmailBuilder.<Resolve>o__SiteContainer2.<>p__Site4 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(EmailBuilder), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.Constant, null)
				}));
			}
			if (arg_A5_0(arg_A5_1, EmailBuilder.<Resolve>o__SiteContainer2.<>p__Site4.Target(EmailBuilder.<Resolve>o__SiteContainer2.<>p__Site4, model, null)))
			{
				model = new ExpandoObject();
			}
			IEmailSettingsManager emailSettingsManager = DIContainer.Resolve<IEmailSettingsManager>();
			EmailSettings emailSettings = emailSettingsManager.Get();
			EmailTemplate emailTemplate = EmailBuilder.GetEmailTemplate(templateName);
			if (string.IsNullOrEmpty(from))
			{
				if (string.Equals(emailTemplate.From, "NoReplyAddress", System.StringComparison.CurrentCultureIgnoreCase))
				{
					from = emailSettings.NoReplyAddress;
				}
				else
				{
					if (!string.Equals(emailTemplate.From, "AdminAddress", System.StringComparison.CurrentCultureIgnoreCase))
					{
						throw new ExceptionFacade(new CommonExceptionDescriptor("发件人不能为null"), null);
					}
					from = emailSettings.AdminEmailAddress;
				}
			}
			MailMessage mailMessage = new MailMessage();
			mailMessage.IsBodyHtml = true;
			try
			{
				MailMessage arg_1E6_0 = mailMessage;
				if (EmailBuilder.<Resolve>o__SiteContainer2.<>p__Site5 == null)
				{
					EmailBuilder.<Resolve>o__SiteContainer2.<>p__Site5 = CallSite<Func<CallSite, System.Type, string, object, MailAddress>>.Create(Binder.InvokeConstructor(CSharpBinderFlags.None, typeof(EmailBuilder), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, System.Type, string, object, MailAddress> arg_1E1_0 = EmailBuilder.<Resolve>o__SiteContainer2.<>p__Site5.Target;
				CallSite arg_1E1_1 = EmailBuilder.<Resolve>o__SiteContainer2.<>p__Site5;
				System.Type arg_1E1_2 = typeof(MailAddress);
				string arg_1E1_3 = from;
				if (EmailBuilder.<Resolve>o__SiteContainer2.<>p__Site6 == null)
				{
					EmailBuilder.<Resolve>o__SiteContainer2.<>p__Site6 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "SiteName", typeof(EmailBuilder), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				arg_1E6_0.From = arg_1E1_0(arg_1E1_1, arg_1E1_2, arg_1E1_3, EmailBuilder.<Resolve>o__SiteContainer2.<>p__Site6.Target(EmailBuilder.<Resolve>o__SiteContainer2.<>p__Site6, model));
			}
			catch
			{
			}
			foreach (string current in to)
			{
				try
				{
					mailMessage.To.Add(current);
				}
				catch
				{
				}
			}
			if (cc != null)
			{
				foreach (string current2 in cc)
				{
					try
					{
						mailMessage.CC.Add(current2);
					}
					catch
					{
					}
				}
			}
			if (bcc != null)
			{
				foreach (string current3 in bcc)
				{
					try
					{
						mailMessage.Bcc.Add(current3);
					}
					catch
					{
					}
				}
			}
			try
			{
				MailMessage arg_377_0 = mailMessage;
				if (EmailBuilder.<Resolve>o__SiteContainer2.<>p__Site7 == null)
				{
					EmailBuilder.<Resolve>o__SiteContainer2.<>p__Site7 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(string), typeof(EmailBuilder)));
				}
				Func<CallSite, object, string> arg_372_0 = EmailBuilder.<Resolve>o__SiteContainer2.<>p__Site7.Target;
				CallSite arg_372_1 = EmailBuilder.<Resolve>o__SiteContainer2.<>p__Site7;
				if (EmailBuilder.<Resolve>o__SiteContainer2.<>p__Site8 == null)
				{
					EmailBuilder.<Resolve>o__SiteContainer2.<>p__Site8 = CallSite<Func<CallSite, System.Type, string, object, string, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "Parse", null, typeof(EmailBuilder), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
					}));
				}
				arg_377_0.Subject = arg_372_0(arg_372_1, EmailBuilder.<Resolve>o__SiteContainer2.<>p__Site8.Target(EmailBuilder.<Resolve>o__SiteContainer2.<>p__Site8, typeof(Razor), emailTemplate.Subject, model, emailTemplate.TemplateName));
			}
			catch (System.Exception innerException)
			{
				throw new ExceptionFacade(new CommonExceptionDescriptor("编译邮件模板标题时报错"), innerException);
			}
			mailMessage.Priority = emailTemplate.Priority;
			if (!string.IsNullOrEmpty(emailTemplate.Body))
			{
				try
				{
					MailMessage arg_477_0 = mailMessage;
					if (EmailBuilder.<Resolve>o__SiteContainer2.<>p__Site9 == null)
					{
						EmailBuilder.<Resolve>o__SiteContainer2.<>p__Site9 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(string), typeof(EmailBuilder)));
					}
					Func<CallSite, object, string> arg_472_0 = EmailBuilder.<Resolve>o__SiteContainer2.<>p__Site9.Target;
					CallSite arg_472_1 = EmailBuilder.<Resolve>o__SiteContainer2.<>p__Site9;
					if (EmailBuilder.<Resolve>o__SiteContainer2.<>p__Sitea == null)
					{
						EmailBuilder.<Resolve>o__SiteContainer2.<>p__Sitea = CallSite<Func<CallSite, System.Type, string, object, string, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "Parse", null, typeof(EmailBuilder), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
						}));
					}
					arg_477_0.Body = arg_472_0(arg_472_1, EmailBuilder.<Resolve>o__SiteContainer2.<>p__Sitea.Target(EmailBuilder.<Resolve>o__SiteContainer2.<>p__Sitea, typeof(Razor), emailTemplate.Body, model, emailTemplate.Body));
					return mailMessage;
				}
				catch (System.Exception innerException2)
				{
					throw new ExceptionFacade("编译邮件模板内容时报错", innerException2);
				}
			}
			if (string.IsNullOrEmpty(emailTemplate.BodyUrl))
			{
				throw new ExceptionFacade("邮件模板中Body、BodyUrl必须填一个", null);
			}
			mailMessage.Body = HttpCollects.GetHTMLContent(emailTemplate.BodyUrl);
			return mailMessage;
		}
		private static System.Collections.Generic.Dictionary<string, EmailTemplate> LoadEmailTemplates()
		{
			string str = "zh-CN";
			string cacheKey = "EmailTemplates::" + str;
			ICacheService cacheService = DIContainer.Resolve<ICacheService>();
			System.Collections.Generic.Dictionary<string, EmailTemplate> dictionary = cacheService.Get<System.Collections.Generic.Dictionary<string, EmailTemplate>>(cacheKey);
			if (dictionary == null)
			{
				dictionary = new System.Collections.Generic.Dictionary<string, EmailTemplate>();
				string searchPattern = "*.xml";
				string physicalFilePath = WebUtility.GetPhysicalFilePath(string.Format("~/Languages/" + str + "/emails/", new object[0]));
				string[] array = System.IO.Directory.GetFiles(physicalFilePath, searchPattern);
				string physicalFilePath2 = WebUtility.GetPhysicalFilePath("~/Applications/");
				System.Collections.Generic.IEnumerable<string> enumerable = new System.Collections.Generic.List<string>();
				if (System.IO.Directory.Exists(physicalFilePath2))
				{
					string[] directories = System.IO.Directory.GetDirectories(physicalFilePath2);
					for (int i = 0; i < directories.Length; i++)
					{
						string path = directories[i];
						string path2 = System.IO.Path.Combine(path, "Languages\\" + str + "\\emails\\");
						if (System.IO.Directory.Exists(path2))
						{
							enumerable = enumerable.Union(System.IO.Directory.GetFiles(path2, searchPattern));
						}
					}
				}
				array = array.Union(enumerable).ToArray<string>();
				object obj = new ExpandoObject();
				System.Type type = obj.GetType();
				string[] array2 = array;
				for (int j = 0; j < array2.Length; j++)
				{
					string text = array2[j];
					if (System.IO.File.Exists(text))
					{
						XmlDocument xmlDocument = new XmlDocument();
						xmlDocument.Load(text);
						foreach (XmlNode xmlNode in xmlDocument.GetElementsByTagName("email"))
						{
							XmlNode namedItem = xmlNode.Attributes.GetNamedItem("templateName");
							if (namedItem != null)
							{
								string innerText = namedItem.InnerText;
								EmailTemplate emailTemplate = new EmailTemplate(xmlNode);
								dictionary[innerText] = emailTemplate;
								if (!string.IsNullOrEmpty(emailTemplate.Body))
								{
									Razor.Compile(emailTemplate.Body, type, innerText);
								}
							}
						}
					}
				}
				cacheService.Add(cacheKey, dictionary, CachingExpirationType.Stable);
			}
			return dictionary;
		}
		private static EmailTemplate GetEmailTemplate(string templateName)
		{
			if (EmailBuilder.emailTemplates != null && EmailBuilder.emailTemplates.ContainsKey(templateName))
			{
				return EmailBuilder.emailTemplates[templateName];
			}
			throw new ExceptionFacade(new ResourceExceptionDescriptor().WithContentNotFound("邮件模板", templateName), null);
		}
	}
}
