using CodeKicker.BBCode;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
namespace Tunynet.Utilities
{
	public class HtmlUtility
	{
		public static string TrimHtml(string rawHtml, int charLimit)
		{
			if (string.IsNullOrEmpty(rawHtml))
			{
				return string.Empty;
			}
			string text = HtmlUtility.StripHtml(rawHtml, true, false);
			text = HtmlUtility.StripBBTags(text);
			if (charLimit <= 0 || charLimit >= text.Length)
			{
				return text;
			}
			return StringUtility.Trim(text, charLimit);
		}
		public static string StripHtml(string rawString, bool removeHtmlEntities, bool enableMultiLine)
		{
			string text = rawString;
			if (enableMultiLine)
			{
				text = Regex.Replace(text, "</p(?:\\s*)>(?:\\s*)<p(?:\\s*)>", "\n\n", RegexOptions.IgnoreCase | RegexOptions.Compiled);
				text = Regex.Replace(text, "<br(?:\\s*)/>", "\n", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			}
			text = text.Replace("\"", "''");
			if (removeHtmlEntities)
			{
				text = Regex.Replace(text, "&[^;]*;", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Compiled);
			}
			return Regex.Replace(text, "<[^>]+>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Compiled);
		}
		public static string StripForPreview(string rawString)
		{
			string text = rawString.Replace("<br>", "\n");
			text = text.Replace("<br/>", "\n");
			text = text.Replace("<br />", "\n");
			text = text.Replace("<p>", "\n");
			text = text.Replace("'", "&#39;");
			text = HtmlUtility.StripHtml(text, false, false);
			return HtmlUtility.StripBBTags(text);
		}
		public static string StripBBTags(string content)
		{
			return Regex.Replace(content, "\\[[^\\]]*?\\]", string.Empty, RegexOptions.IgnoreCase);
		}
		public static string StripScriptTags(string rawString)
		{
			rawString = Regex.Replace(rawString, "<script((.|\n)*?)</script>", "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
			rawString = rawString.Replace("\"javascript:", "\"");
			return rawString;
		}
		public static string CloseHtmlTags(string html)
		{
			if (string.IsNullOrEmpty(html))
			{
				return html;
			}
			HtmlDocument htmlDocument = new HtmlDocument
			{
				OptionAutoCloseOnEnd = true,
				OptionWriteEmptyNodes = true
			};
			htmlDocument.LoadHtml(html);
			return htmlDocument.get_DocumentNode().WriteTo();
		}
		public static string CleanHtml(string rawHtml, TrustedHtmlLevel level)
		{
			if (string.IsNullOrEmpty(rawHtml))
			{
				return rawHtml;
			}
			HtmlDocument htmlDocument = new HtmlDocument
			{
				OptionAutoCloseOnEnd = true,
				OptionWriteEmptyNodes = true
			};
			TrustedHtml trustedHtml = DIContainer.Resolve<TrustedHtml>();
			switch (level)
			{
			case TrustedHtmlLevel.Basic:
				trustedHtml = trustedHtml.Basic();
				break;
			case TrustedHtmlLevel.HtmlEditor:
				trustedHtml = trustedHtml.HtmlEditor();
				break;
			}
			htmlDocument.LoadHtml(rawHtml);
			HtmlNodeCollection htmlNodeCollection = htmlDocument.get_DocumentNode().SelectNodes("//*");
			if (htmlNodeCollection != null)
			{
				string host = string.Empty;
				if (HttpContext.Current != null)
				{
					host = WebUtility.HostPath(HttpContext.Current.Request.Url);
				}
				System.Collections.Generic.Dictionary<string, string> enforcedAttributes;
				htmlNodeCollection.ToList<HtmlNode>().ForEach(delegate(HtmlNode n)
				{
					if (trustedHtml.IsSafeTag(n.get_Name()))
					{
						n.get_Attributes().ToList<HtmlAttribute>().ForEach(delegate(HtmlAttribute attr)
						{
							if (!trustedHtml.IsSafeAttribute(n.get_Name(), attr.get_Name(), attr.get_Value()))
							{
								attr.Remove();
								return;
							}
							if (attr.get_Value().StartsWith("javascirpt:", System.StringComparison.InvariantCultureIgnoreCase))
							{
								attr.set_Value("javascirpt:;");
							}
						});
						enforcedAttributes = trustedHtml.GetEnforcedAttributes(n.get_Name());
						if (enforcedAttributes != null)
						{
							foreach (System.Collections.Generic.KeyValuePair<string, string> current in enforcedAttributes)
							{
								if (!(
									from a in n.get_Attributes()
									select a.get_Name()).Contains(current.Key))
								{
									n.get_Attributes().Add(current.Key, current.Value);
								}
								else
								{
									n.get_Attributes().get_Item(current.Key).set_Value(current.Value);
								}
							}
						}
						if (n.get_Name() == "a" && n.get_Attributes().Contains("href"))
						{
							string value = n.get_Attributes().get_Item("href").get_Value();
							if (value.StartsWith("http://") && !value.ToLowerInvariant().StartsWith(host.ToLower()))
							{
								if (!(
									from a in n.get_Attributes()
									select a.get_Name()).Contains("rel"))
								{
									n.get_Attributes().Add("rel", "nofollow");
									return;
								}
								if (n.get_Attributes().get_Item("rel").get_Value() != "fancybox")
								{
									n.get_Attributes().get_Item("rel").set_Value("nofollow");
									return;
								}
							}
						}
					}
					else
					{
						if (trustedHtml.EncodeHtml)
						{
							n.set_HtmlEncode(true);
							return;
						}
						n.RemoveTag();
					}
				});
			}
			return htmlDocument.get_DocumentNode().WriteTo();
		}
		public static string GetHtmlNode(string html, string xpath)
		{
			if (string.IsNullOrEmpty(html))
			{
				return html;
			}
			HtmlDocument htmlDocument = new HtmlDocument
			{
				OptionAutoCloseOnEnd = true,
				OptionWriteEmptyNodes = true
			};
			htmlDocument.LoadHtml(html);
			HtmlNode htmlNode = htmlDocument.get_DocumentNode().SelectSingleNode(xpath);
			if (htmlNode == null)
			{
				return string.Empty;
			}
			return htmlNode.get_OuterHtml();
		}
		public static System.Collections.Generic.List<string> GetHtmlNodes(string html, string xpath)
		{
			if (string.IsNullOrEmpty(html))
			{
				return null;
			}
			HtmlDocument htmlDocument = new HtmlDocument
			{
				OptionAutoCloseOnEnd = true,
				OptionWriteEmptyNodes = true
			};
			htmlDocument.LoadHtml(html);
			HtmlNodeCollection htmlNodeCollection = htmlDocument.get_DocumentNode().SelectNodes(xpath);
			if (htmlNodeCollection == null)
			{
				return null;
			}
			return (
				from n in htmlNodeCollection
				select n.get_OuterHtml()).ToList<string>();
		}
		public static string BBCodeToHtml(string rawString, BBTag bbTag, bool htmlEncode = false)
		{
			if (string.IsNullOrEmpty(rawString))
			{
				return rawString;
			}
			return HtmlUtility.BBCodeToHtml(rawString, new System.Collections.Generic.List<BBTag>
			{
				bbTag
			}, htmlEncode);
		}
		public static string BBCodeToHtml(string rawString, System.Collections.Generic.IList<BBTag> bbTags, bool htmlEncode = false)
		{
			if (string.IsNullOrEmpty(rawString) || bbTags == null)
			{
				return rawString;
			}
			BBCodeParser bBCodeParser = new BBCodeParser(bbTags);
			return bBCodeParser.ToHtml(rawString, htmlEncode);
		}
	}
}
