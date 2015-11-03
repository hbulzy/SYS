using System;
using System.Collections.Generic;
namespace Tunynet.Utilities
{
	public class TrustedHtml
	{
		private HashSet<string> _tagNames;
		private HashSet<string> _globalAttributes;
		private System.Collections.Generic.Dictionary<string, HashSet<string>> _attributes;
		private System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>> _enforcedAttributes;
		private System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, HashSet<string>>> _protocols;
		protected static System.Collections.Generic.Dictionary<TrustedHtmlLevel, TrustedHtml> addedRules = new System.Collections.Generic.Dictionary<TrustedHtmlLevel, TrustedHtml>();
		private bool _encodeHtml;
		public bool EncodeHtml
		{
			get
			{
				return this._encodeHtml;
			}
		}
		public TrustedHtml() : this(false)
		{
		}
		public TrustedHtml(bool encodeHtml)
		{
			this._encodeHtml = encodeHtml;
			this._tagNames = new HashSet<string>();
			this._globalAttributes = new HashSet<string>();
			this._attributes = new System.Collections.Generic.Dictionary<string, HashSet<string>>();
			this._enforcedAttributes = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>>();
			this._protocols = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, HashSet<string>>>();
		}
		public virtual TrustedHtml Basic()
		{
			if (!TrustedHtml.addedRules.ContainsKey(TrustedHtmlLevel.Basic))
			{
				TrustedHtml.addedRules[TrustedHtmlLevel.Basic] = new TrustedHtml(this._encodeHtml).AddTags(new string[]
				{
					"strong",
					"em",
					"u",
					"b",
					"i",
					"font",
					"ul",
					"ol",
					"li",
					"p",
					"address",
					"div",
					"hr",
					"br",
					"a",
					"span",
					"img"
				}).AddGlobalAttributes(new string[]
				{
					"align",
					"style"
				}).AddAttributes("font", new string[]
				{
					"size",
					"color",
					"face"
				}).AddAttributes("em", new string[]
				{
					"rel"
				}).AddAttributes("p", new string[]
				{
					"dir"
				}).AddAttributes("a", new string[]
				{
					"href",
					"title",
					"name",
					"target",
					"rel"
				}).AddAttributes("img", new string[]
				{
					"src",
					"alt",
					"title",
					"border",
					"width",
					"height"
				}).AddProtocols("a", "href", new string[]
				{
					"ftp",
					"http",
					"https",
					"mailto"
				});
			}
			return TrustedHtml.addedRules[TrustedHtmlLevel.Basic];
		}
		public virtual TrustedHtml HtmlEditor()
		{
			if (!TrustedHtml.addedRules.ContainsKey(TrustedHtmlLevel.HtmlEditor))
			{
				TrustedHtml.addedRules[TrustedHtmlLevel.HtmlEditor] = new TrustedHtml(this._encodeHtml).AddTags(new string[]
				{
					"h1",
					"h2",
					"h3",
					"h4",
					"h5",
					"h6",
					"h7",
					"strong",
					"em",
					"u",
					"b",
					"i",
					"strike",
					"sub",
					"sup",
					"font",
					"blockquote",
					"ul",
					"ol",
					"li",
					"p",
					"address",
					"div",
					"hr",
					"br",
					"a",
					"span",
					"img",
					"table",
					"tbody",
					"th",
					"td",
					"tr",
					"pre",
					"code",
					"xmp",
					"object",
					"param",
					"embed"
				}).AddGlobalAttributes(new string[]
				{
					"align",
					"id",
					"style"
				}).AddAttributes("font", new string[]
				{
					"size",
					"color",
					"face"
				}).AddAttributes("blockquote", new string[]
				{
					"dir"
				}).AddAttributes("p", new string[]
				{
					"dir"
				}).AddAttributes("em", new string[]
				{
					"rel"
				}).AddAttributes("a", new string[]
				{
					"href",
					"title",
					"name",
					"target",
					"rel"
				}).AddAttributes("img", new string[]
				{
					"src",
					"alt",
					"title",
					"border",
					"width",
					"height"
				}).AddAttributes("table", new string[]
				{
					"border",
					"cellpadding",
					"cellspacing",
					"bgcorlor",
					"width"
				}).AddAttributes("th", new string[]
				{
					"bgcolor",
					"width"
				}).AddAttributes("td", new string[]
				{
					"rowspan",
					"colspan",
					"bgcolor",
					"width"
				}).AddAttributes("pre", new string[]
				{
					"name",
					"class"
				}).AddAttributes("object", new string[]
				{
					"classid",
					"codebase",
					"width",
					"height",
					"data",
					"type"
				}).AddAttributes("param", new string[]
				{
					"name",
					"value"
				}).AddAttributes("embed", new string[]
				{
					"type",
					"src",
					"width",
					"height",
					"quality",
					"scale",
					"bgcolor",
					"vspace",
					"hspace",
					"base",
					"flashvars",
					"swliveconnect"
				}).AddProtocols("a", "href", new string[]
				{
					"ftp",
					"http",
					"https",
					"mailto"
				}).AddProtocols("img", "src", new string[]
				{
					"http",
					"https"
				}).AddProtocols("blockquote", "cite", new string[]
				{
					"http",
					"https"
				}).AddProtocols("cite", "cite", new string[]
				{
					"http",
					"https"
				});
			}
			return TrustedHtml.addedRules[TrustedHtmlLevel.HtmlEditor];
		}
		public TrustedHtml AddTags(params string[] tags)
		{
			if (tags == null)
			{
				throw new System.ArgumentNullException("tags");
			}
			for (int i = 0; i < tags.Length; i++)
			{
				string text = tags[i];
				if (string.IsNullOrEmpty(text))
				{
					throw new System.Exception("An empty tag was found.");
				}
				this._tagNames.Add(text);
			}
			return this;
		}
		public TrustedHtml AddAttributes(string tag, params string[] keys)
		{
			if (string.IsNullOrEmpty(tag))
			{
				throw new System.ArgumentNullException("tag");
			}
			if (keys == null)
			{
				throw new System.ArgumentNullException("keys");
			}
			HashSet<string> hashSet = new HashSet<string>();
			for (int i = 0; i < keys.Length; i++)
			{
				string text = keys[i];
				if (string.IsNullOrEmpty(text))
				{
					throw new System.Exception("key");
				}
				hashSet.Add(text);
			}
			if (this._attributes.ContainsKey(tag))
			{
				using (HashSet<string>.Enumerator enumerator = hashSet.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						string current = enumerator.Current;
						this._attributes[tag].Add(current);
					}
					return this;
				}
			}
			this._attributes.Add(tag, hashSet);
			return this;
		}
		public TrustedHtml AddGlobalAttributes(params string[] attrs)
		{
			if (attrs == null)
			{
				throw new System.ArgumentNullException("attributes");
			}
			for (int i = 0; i < attrs.Length; i++)
			{
				string text = attrs[i];
				if (string.IsNullOrEmpty(text))
				{
					throw new System.Exception("An empty attribute was found.");
				}
				this._globalAttributes.Add(text);
			}
			return this;
		}
		public TrustedHtml AddEnforcedAttribute(string tag, string key, string value)
		{
			if (string.IsNullOrEmpty(tag))
			{
				throw new System.ArgumentNullException("tag");
			}
			if (string.IsNullOrEmpty(key))
			{
				throw new System.ArgumentNullException("key");
			}
			if (string.IsNullOrEmpty(value))
			{
				throw new System.ArgumentNullException("value");
			}
			if (this._enforcedAttributes.ContainsKey(tag))
			{
				this._enforcedAttributes[tag].Add(key, value);
			}
			else
			{
				System.Collections.Generic.Dictionary<string, string> dictionary = new System.Collections.Generic.Dictionary<string, string>();
				dictionary.Add(key, value);
				this._enforcedAttributes.Add(tag, dictionary);
			}
			return this;
		}
		public TrustedHtml AddProtocols(string tag, string key, params string[] protocols)
		{
			if (string.IsNullOrEmpty(tag))
			{
				throw new System.ArgumentNullException("tag");
			}
			if (string.IsNullOrEmpty(key))
			{
				throw new System.ArgumentNullException("key");
			}
			if (protocols == null)
			{
				throw new System.ArgumentNullException("protocols");
			}
			System.Collections.Generic.Dictionary<string, HashSet<string>> dictionary;
			if (this._protocols.ContainsKey(tag))
			{
				dictionary = this._protocols[tag];
			}
			else
			{
				dictionary = new System.Collections.Generic.Dictionary<string, HashSet<string>>();
				this._protocols.Add(tag, dictionary);
			}
			HashSet<string> hashSet;
			if (dictionary.ContainsKey(key))
			{
				hashSet = dictionary[key];
			}
			else
			{
				hashSet = new HashSet<string>();
				dictionary.Add(key, hashSet);
			}
			for (int i = 0; i < protocols.Length; i++)
			{
				string text = protocols[i];
				if (string.IsNullOrEmpty(text))
				{
					throw new System.Exception("protocol is empty.");
				}
				hashSet.Add(text);
			}
			return this;
		}
		public virtual System.Collections.Generic.Dictionary<string, string> GetEnforcedAttributes(string tag)
		{
			if (this._enforcedAttributes.ContainsKey(tag))
			{
				return this._enforcedAttributes[tag];
			}
			return null;
		}
		public virtual bool IsSafeTag(string tag)
		{
			return this._tagNames.Contains(tag);
		}
		public virtual bool IsSafeAttribute(string tag, string attr, string attrVal)
		{
			return (this._globalAttributes.Contains(attr) || (this._attributes.ContainsKey(tag) && this._attributes[tag].Contains(attr))) && (!this._protocols.ContainsKey(tag) || !this._protocols[tag].ContainsKey(attr) || this.ValidProtocol(tag, attr, attrVal));
		}
		private bool ValidProtocol(string tag, string attr, string attVal)
		{
			if (attVal.ToLowerInvariant().Contains("javascript:"))
			{
				return false;
			}
			if (!attVal.Contains("://"))
			{
				return true;
			}
			foreach (string current in this._protocols[tag][attr])
			{
				string value = current + ":";
				if (attVal.ToLowerInvariant().StartsWith(value))
				{
					return true;
				}
			}
			return false;
		}
	}
}
