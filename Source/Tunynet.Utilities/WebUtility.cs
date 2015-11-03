using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
namespace Tunynet.Utilities
{
	public static class WebUtility
	{
		public static readonly string HtmlNewLine = "<br />";
		public static string ResolveUrl(string relativeUrl)
		{
			if (string.IsNullOrEmpty(relativeUrl))
			{
				return relativeUrl;
			}
			if (relativeUrl.StartsWith("~/"))
			{
				string[] array = relativeUrl.Split(new char[]
				{
					'?'
				});
				string text = VirtualPathUtility.ToAbsolute(array[0]);
				if (array.Length > 1)
				{
					text = text + "?" + array[1];
				}
				return text;
			}
			return relativeUrl;
		}
		public static string HostPath(Uri uri)
		{
			if (uri == null)
			{
				return string.Empty;
			}
			string str = uri.IsDefaultPort ? string.Empty : (":" + System.Convert.ToString(uri.Port, System.Globalization.CultureInfo.InvariantCulture));
			return uri.Scheme + Uri.SchemeDelimiter + uri.Host + str;
		}
		public static string GetPhysicalFilePath(string filePath)
		{
			string result;
			if (filePath.IndexOf(":\\") != -1 || filePath.IndexOf("\\\\") != -1)
			{
				result = filePath;
			}
			else
			{
				if (HostingEnvironment.IsHosted)
				{
					result = HostingEnvironment.MapPath(filePath);
				}
				else
				{
					filePath = filePath.Replace('/', System.IO.Path.DirectorySeparatorChar).Replace("~", "");
					result = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, filePath);
				}
			}
			return result;
		}
		public static string FormatCompleteUrl(string content)
		{
			string pattern = "src=[\"']\\s*(/[^\"']*)\\s*[\"']";
			string pattern2 = "href=[\"']\\s*(/[^\"']*)\\s*[\"']";
			string str = WebUtility.HostPath(HttpContext.Current.Request.Url);
			content = Regex.Replace(content, pattern, "src=\"" + str + "$1\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			content = Regex.Replace(content, pattern2, "href=\"" + str + "$1\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			return content;
		}
		public static string GetServerDomain(Uri uri, string[] domainRules)
		{
			if (uri == null)
			{
				return string.Empty;
			}
			string text = uri.Host.ToString().ToLower();
			if (text.IndexOf('.') <= 0)
			{
				return text;
			}
			string[] array = text.Split(new char[]
			{
				'.'
			});
			string s = array.GetValue(array.Length - 1).ToString();
			int num = -1;
			if (int.TryParse(s, out num))
			{
				return text;
			}
			string text2 = string.Empty;
			string text3 = string.Empty;
			string result = string.Empty;
			int i = 0;
			while (i < domainRules.Length)
			{
				if (text.EndsWith(domainRules[i].ToLower()))
				{
					text2 = domainRules[i].ToLower();
					text3 = text.Replace(text2, "");
					if (text3.IndexOf('.') > 0)
					{
						string[] array2 = text3.Split(new char[]
						{
							'.'
						});
						return array2.GetValue(array2.Length - 1).ToString() + text2;
					}
					return text3 + text2;
				}
				else
				{
					result = text;
					i++;
				}
			}
			return result;
		}
		public static string HtmlEncode(string rawContent)
		{
			if (string.IsNullOrEmpty(rawContent))
			{
				return rawContent;
			}
			return HttpUtility.HtmlEncode(rawContent);
		}
		public static string HtmlDecode(string rawContent)
		{
			if (string.IsNullOrEmpty(rawContent))
			{
				return rawContent;
			}
			return HttpUtility.HtmlDecode(rawContent);
		}
		public static string UrlEncode(string urlToEncode)
		{
			if (string.IsNullOrEmpty(urlToEncode))
			{
				return urlToEncode;
			}
			return HttpUtility.UrlEncode(urlToEncode).Replace("'", "%27");
		}
		public static string UrlDecode(string urlToDecode)
		{
			if (string.IsNullOrEmpty(urlToDecode))
			{
				return urlToDecode;
			}
			return HttpUtility.UrlDecode(urlToDecode);
		}
		public static string GetIP()
		{
			return WebUtility.GetIP(HttpContext.Current);
		}
		public static string GetIP(HttpContext httpContext)
		{
			string text = string.Empty;
			if (httpContext == null)
			{
				return text;
			}
			text = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
			if (string.IsNullOrEmpty(text))
			{
				text = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
			}
			if (string.IsNullOrEmpty(text))
			{
				text = HttpContext.Current.Request.UserHostAddress;
			}
			return text;
		}
		public static void Return404(HttpContext httpContext)
		{
			WebUtility.ReturnStatusCode(httpContext, 404, null, false);
			if (httpContext != null)
			{
				httpContext.Response.SuppressContent = true;
				httpContext.Response.End();
			}
		}
		public static void Return403(HttpContext httpContext)
		{
			WebUtility.ReturnStatusCode(httpContext, 403, null, false);
			if (httpContext != null)
			{
				httpContext.Response.SuppressContent = true;
				httpContext.Response.End();
			}
		}
		public static void Return304(HttpContext httpContext, bool endResponse = true)
		{
			WebUtility.ReturnStatusCode(httpContext, 304, "304 Not Modified", endResponse);
		}
		private static void ReturnStatusCode(HttpContext httpContext, int statusCode, string status, bool endResponse)
		{
			if (httpContext == null)
			{
				return;
			}
			httpContext.Response.Clear();
			httpContext.Response.StatusCode = statusCode;
			if (!string.IsNullOrEmpty(status))
			{
				httpContext.Response.Status = status;
			}
			if (endResponse)
			{
				httpContext.Response.End();
			}
		}
		public static void SetStatusCodeForError(HttpResponseBase response)
		{
			response.StatusCode = 300;
		}
	}
}
