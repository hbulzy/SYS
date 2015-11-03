using System;
using System.Net.Mail;
using System.Xml;
namespace Tunynet.Email
{
	public sealed class EmailTemplate
	{
		private string templateName;
		private string from;
		private string subject = "";
		private MailPriority priority;
		public string TemplateName
		{
			get
			{
				return this.templateName;
			}
			set
			{
				this.templateName = value;
			}
		}
		public string From
		{
			get
			{
				return this.from;
			}
			set
			{
				this.from = value;
			}
		}
		public string Subject
		{
			get
			{
				return this.subject;
			}
			set
			{
				this.subject = value;
			}
		}
		public string Body
		{
			get;
			set;
		}
		public string BodyUrl
		{
			get;
			set;
		}
		public MailPriority Priority
		{
			get
			{
				return this.priority;
			}
			set
			{
				this.priority = value;
			}
		}
		public EmailTemplate(XmlNode rootNode)
		{
			XmlNode namedItem = rootNode.Attributes.GetNamedItem("priority");
			MailPriority mailPriority;
			if (namedItem != null && System.Enum.TryParse<MailPriority>(namedItem.InnerText, out mailPriority))
			{
				this.priority = mailPriority;
			}
			namedItem = rootNode.Attributes.GetNamedItem("templateName");
			if (namedItem != null)
			{
				this.templateName = namedItem.InnerText;
			}
			XmlNode xmlNode = rootNode.SelectSingleNode("subject");
			if (xmlNode != null)
			{
				this.subject = xmlNode.InnerText;
			}
			XmlNode xmlNode2 = rootNode.SelectSingleNode("from");
			if (xmlNode2 != null)
			{
				this.From = xmlNode2.InnerText;
			}
			XmlNode xmlNode3 = rootNode.SelectSingleNode("body");
			if (xmlNode3 != null)
			{
				namedItem = xmlNode3.Attributes.GetNamedItem("url");
				if (namedItem != null)
				{
					this.BodyUrl = namedItem.InnerText;
					return;
				}
				this.Body = xmlNode3.InnerXml;
			}
		}
	}
}
