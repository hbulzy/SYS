using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Text;
namespace Tunynet
{
	[System.Serializable]
	public class PropertySerializer
	{
		private NameValueCollection extendedAttributes = new NameValueCollection();
		public PropertySerializer(string propertyNames, string propertyValues)
		{
			if (!string.IsNullOrEmpty(propertyNames) && !string.IsNullOrEmpty(propertyValues))
			{
				this.extendedAttributes = PropertySerializer.ConvertToNameValueCollection(propertyNames, propertyValues);
				return;
			}
			this.extendedAttributes = new NameValueCollection();
		}
		public void Serialize(ref string propertyNames, ref string propertyValues)
		{
			PropertySerializer.ConvertFromNameValueCollection(this.extendedAttributes, ref propertyNames, ref propertyValues);
		}
		public T GetExtendedProperty<T>(string propertyName)
		{
			if (typeof(T) == typeof(string))
			{
				TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
				return this.GetExtendedProperty<T>(propertyName, (T)((object)converter.ConvertFrom(string.Empty)));
			}
			return this.GetExtendedProperty<T>(propertyName, default(T));
		}
		public T GetExtendedProperty<T>(string propertyName, T defaultValue)
		{
			string text = this.extendedAttributes[propertyName];
			if (text == null)
			{
				return defaultValue;
			}
			return (T)((object)System.Convert.ChangeType(text, typeof(T)));
		}
		public void SetExtendedProperty(string propertyName, object propertyValue)
		{
			if (propertyValue == null)
			{
				this.extendedAttributes.Remove(propertyName);
			}
			string value = propertyValue.ToString().Trim();
			if (string.IsNullOrEmpty(value))
			{
				this.extendedAttributes.Remove(propertyName);
				return;
			}
			this.extendedAttributes[propertyName] = value;
		}
		private static NameValueCollection ConvertToNameValueCollection(string propertyNames, string propertyValues)
		{
			NameValueCollection nameValueCollection = new NameValueCollection();
			if (propertyNames != null && propertyValues != null && propertyNames.Length > 0 && propertyValues.Length > 0)
			{
				char[] separator = new char[]
				{
					':'
				};
				string[] array = propertyNames.Split(separator);
				for (int i = 0; i < array.Length / 4; i++)
				{
					int num = int.Parse(array[i * 4 + 2], System.Globalization.CultureInfo.InvariantCulture);
					int num2 = int.Parse(array[i * 4 + 3], System.Globalization.CultureInfo.InvariantCulture);
					string name = array[i * 4];
					if (array[i * 4 + 1] == "S" && num >= 0 && num2 > 0 && propertyValues.Length >= num + num2)
					{
						nameValueCollection[name] = propertyValues.Substring(num, num2);
					}
				}
			}
			return nameValueCollection;
		}
		private static void ConvertFromNameValueCollection(NameValueCollection nvc, ref string propertyNames, ref string propertyValues)
		{
			if (nvc == null || nvc.Count == 0)
			{
				return;
			}
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			System.Text.StringBuilder stringBuilder2 = new System.Text.StringBuilder();
			int num = 0;
			string[] allKeys = nvc.AllKeys;
			for (int i = 0; i < allKeys.Length; i++)
			{
				string text = allKeys[i];
				if (text.IndexOf(':') != -1)
				{
					throw new System.ArgumentException("SerializableProperties Name can not contain the character \":\"");
				}
				string text2 = nvc[text];
				if (!string.IsNullOrEmpty(text2))
				{
					stringBuilder.AppendFormat("{0}:S:{1}:{2}:", text, num, text2.Length);
					stringBuilder2.Append(text2);
					num += text2.Length;
				}
			}
			propertyNames = stringBuilder.ToString();
			propertyValues = stringBuilder2.ToString();
		}
	}
}
