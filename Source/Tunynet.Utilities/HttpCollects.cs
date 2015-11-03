using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
namespace Tunynet.Utilities
{
	public class HttpCollects
	{
		public static string GetHTMLContent(string url)
		{
			return HttpCollects.GetHTMLContent(url, null, null);
		}
		public static string GetHTMLContent(string url, string endRegexString)
		{
			return HttpCollects.GetHTMLContent(url, null, endRegexString);
		}
		public static string GetHTMLContent(string url, System.Text.Encoding encoding, string endRegexString)
		{
			HttpWebResponse httpWebResponse = null;
			System.IO.Stream stream = null;
			System.IO.StreamReader streamReader = null;
			string result;
			try
			{
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
				httpWebRequest.Timeout = 30000;
				httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
				if (httpWebResponse.StatusCode != HttpStatusCode.OK)
				{
					result = null;
				}
				else
				{
					stream = httpWebResponse.GetResponseStream();
					if (encoding == null)
					{
						try
						{
							if (string.IsNullOrEmpty(httpWebResponse.CharacterSet) || httpWebResponse.CharacterSet == "ISO-8859-1")
							{
								encoding = HttpCollects.getEncoding(url);
							}
							else
							{
								encoding = System.Text.Encoding.GetEncoding(httpWebResponse.CharacterSet);
							}
						}
						catch
						{
							encoding = System.Text.Encoding.UTF8;
						}
						if (encoding == null)
						{
							encoding = System.Text.Encoding.UTF8;
						}
					}
					httpWebRequest.Timeout = 8000;
					httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
					httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
					stream = httpWebResponse.GetResponseStream();
					streamReader = new System.IO.StreamReader(stream, encoding);
					string text;
					if (string.IsNullOrEmpty(endRegexString))
					{
						text = streamReader.ReadToEnd();
					}
					else
					{
						Regex regex = new Regex(endRegexString, RegexOptions.IgnoreCase);
						System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
						string text2 = string.Empty;
						while ((text2 = streamReader.ReadLine()) != null)
						{
							stringBuilder.Append(text2);
							text2 = stringBuilder.ToString();
							if (regex.IsMatch(text2))
							{
								break;
							}
						}
						text = stringBuilder.ToString();
					}
					streamReader.Close();
					stream.Close();
					httpWebResponse.Close();
					result = text;
				}
			}
			catch (WebException)
			{
				result = null;
			}
			catch (System.IO.IOException)
			{
				result = null;
			}
			finally
			{
				if (streamReader != null)
				{
					streamReader.Close();
				}
				if (stream != null)
				{
					stream.Close();
				}
				if (httpWebResponse != null)
				{
					httpWebResponse.Close();
				}
			}
			return result;
		}
		private static System.Text.Encoding getEncoding(string url)
		{
			string hTMLContent = HttpCollects.GetHTMLContent(url, System.Text.Encoding.UTF8, "charset\\b\\s*=\\s*(?<charset>[a-zA-Z\\d|-]*)");
			Regex regex = new Regex("charset\\b\\s*=\\s*(?<charset>[a-zA-Z\\d|-]*)");
			System.Text.Encoding encoding = System.Text.Encoding.UTF8;
			if (regex.IsMatch(hTMLContent))
			{
				foreach (Match match in regex.Matches(hTMLContent))
				{
					try
					{
						if (!string.IsNullOrEmpty(match.Groups["charset"].Value))
						{
							encoding = System.Text.Encoding.GetEncoding(match.Groups["charset"].Value);
							if (encoding != null)
							{
								break;
							}
						}
					}
					catch
					{
					}
				}
			}
			return encoding;
		}
		public static string GetMetaString(string html, string regStart, string regEnd, bool ignoreCase)
		{
			string pattern = string.Format("{0}(?<getcontent>[\\s|\\S]*?){1}", regStart, regEnd);
			Regex regex;
			if (ignoreCase)
			{
				regex = new Regex(pattern, RegexOptions.IgnoreCase);
			}
			else
			{
				regex = new Regex(pattern);
			}
			return regex.Match(html).Groups["getcontent"].Value;
		}
		public static string GetTitle(string html, bool ignoreCas)
		{
			string metaString = HttpCollects.GetMetaString(html, "<meta name=\"title\"([\\s]*)content=\"", "\"([\\s]*)/?>", ignoreCas);
			if (string.IsNullOrEmpty(metaString))
			{
				string pattern = "(?<=<title.*>)([\\s\\S]*)(?=</title>)";
				Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
				return regex.Match(html).Value.Trim();
			}
			return metaString;
		}
		public static string GetDescription(string html, bool ignoreCas)
		{
			string metaString = HttpCollects.GetMetaString(html, "<meta([\\s]*)name=\"description\"([\\s]*)content=\"", "\"([\\s]*)/?>", ignoreCas);
			if (string.IsNullOrEmpty(metaString))
			{
				metaString = HttpCollects.GetMetaString(html, "<meta([\\s]*)content=\"", "\"([\\s]*)name=\"description\"([\\s]*)/?>", ignoreCas);
			}
			return metaString;
		}
		public static string GetHtmlByUrl(string url, string sMethod, string Param, bool bAutoRedirect, System.Text.Encoding ecode)
		{
			sMethod = sMethod.ToUpper();
			sMethod = ((sMethod != "POST") ? "GET" : sMethod);
			string result = "";
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
			httpWebRequest.Method = sMethod;
			httpWebRequest.AllowAutoRedirect = bAutoRedirect;
			httpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; MyIE2; .NET CLR 1.1.4322)";
			httpWebRequest.Timeout = 10000;
			if (sMethod == "POST")
			{
				httpWebRequest.ContentType = "application/x-www-form-urlencoded";
				byte[] bytes = System.Text.Encoding.UTF8.GetBytes(Param);
				httpWebRequest.ContentLength = (long)bytes.Length;
				try
				{
					System.IO.Stream requestStream = httpWebRequest.GetRequestStream();
					requestStream.Write(bytes, 0, bytes.Length);
					requestStream.Close();
				}
				catch (System.Exception)
				{
					httpWebRequest = null;
					return "-1";
				}
			}
			HttpWebResponse httpWebResponse = null;
			System.IO.Stream stream = null;
			System.IO.StreamReader streamReader = null;
			try
			{
				httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
				stream = httpWebResponse.GetResponseStream();
				streamReader = new System.IO.StreamReader(stream, ecode);
				result = streamReader.ReadToEnd();
			}
			catch (WebException ex)
			{
				result = ex.ToString();
			}
			if (httpWebResponse != null)
			{
				httpWebResponse.Close();
				httpWebResponse = null;
			}
			if (stream != null)
			{
				stream.Close();
				stream = null;
			}
			if (streamReader != null)
			{
				streamReader.Close();
				streamReader = null;
			}
			httpWebRequest = null;
			return result;
		}
		public static string GetHTMLByUrlCookie(string url, ref CookieContainer cookie, string sMethod, string Param, bool bAutoRedirect, System.Text.Encoding ecode)
		{
			sMethod = sMethod.ToUpper();
			sMethod = ((sMethod != "POST") ? "GET" : sMethod);
			string result = "";
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
			httpWebRequest.CookieContainer = cookie;
			httpWebRequest.Method = sMethod;
			httpWebRequest.AllowAutoRedirect = bAutoRedirect;
			httpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; MyIE2; .NET CLR 1.1.4322)";
			httpWebRequest.Timeout = 2000;
			if (sMethod == "POST")
			{
				httpWebRequest.ContentType = "application/x-www-form-urlencoded";
				byte[] bytes = System.Text.Encoding.UTF8.GetBytes(Param);
				httpWebRequest.ContentLength = (long)bytes.Length;
				try
				{
					System.IO.Stream requestStream = httpWebRequest.GetRequestStream();
					requestStream.Write(bytes, 0, bytes.Length);
					requestStream.Close();
				}
				catch (System.Exception)
				{
					httpWebRequest = null;
					return "-1";
				}
			}
			HttpWebResponse httpWebResponse = null;
			System.IO.Stream stream = null;
			System.IO.StreamReader streamReader = null;
			try
			{
				httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
				stream = httpWebResponse.GetResponseStream();
				streamReader = new System.IO.StreamReader(stream, ecode);
				result = streamReader.ReadToEnd();
			}
			catch (WebException ex)
			{
				result = ex.ToString();
			}
			if (httpWebResponse != null)
			{
				httpWebResponse.Close();
				httpWebResponse = null;
			}
			if (stream != null)
			{
				stream.Close();
				stream = null;
			}
			if (streamReader != null)
			{
				streamReader.Close();
				streamReader = null;
			}
			httpWebRequest = null;
			return result;
		}
		private static string GetHtml(string sUrl, System.Text.Encoding sEncode, int iMaxRetry, int iCurrentRetry)
		{
			string result = string.Empty;
			try
			{
				Uri requestUri = new Uri(sUrl);
				WebRequest webRequest = WebRequest.Create(requestUri);
				WebResponse response = webRequest.GetResponse();
				System.IO.Stream responseStream = response.GetResponseStream();
				System.IO.StreamReader streamReader = new System.IO.StreamReader(responseStream, sEncode);
				result = streamReader.ReadToEnd();
				streamReader.Close();
				response.Close();
			}
			catch
			{
				iCurrentRetry++;
				if (iCurrentRetry <= iMaxRetry)
				{
					HttpCollects.GetHtml(sUrl, sEncode, iMaxRetry, iCurrentRetry);
				}
			}
			return result;
		}
		public static string GetHtmlWithTried(string sUrl, System.Text.Encoding sEncode, int iMaxRetry)
		{
			string empty = string.Empty;
			return HttpCollects.GetHtml(sUrl, sEncode, iMaxRetry, 0);
		}
	}
}
