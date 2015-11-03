using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
namespace Tunynet.Imaging
{
	public class EXIFMetaDataService
	{
		private int[] EnabledEXIFIds = new int[]
		{
			271,
			272,
			36867,
			40962,
			40963,
			37381,
			37386,
			33434,
			34852,
			37385
		};
		public System.Collections.Generic.Dictionary<int, string> Read(System.IO.Stream imageStream)
		{
			if (imageStream == null || !imageStream.CanRead)
			{
				throw new System.ArgumentException("imageStream isn't validate", "imageStream");
			}
			Image image = Image.FromStream(imageStream);
			System.Collections.Generic.Dictionary<int, string> dictionary = null;
			if (image.PropertyItems != null)
			{
				dictionary = new System.Collections.Generic.Dictionary<int, string>();
				int[] enabledEXIFIds = this.EnabledEXIFIds;
				for (int i = 0; i < enabledEXIFIds.Length; i++)
				{
					int num = enabledEXIFIds[i];
					if (image.PropertyIdList.Contains(num))
					{
						dictionary[num] = EXIFMetaDataService.GetValueOfType(image.GetPropertyItem(num));
					}
				}
			}
			return dictionary;
		}
		public string Read(System.IO.Stream imageStream, int propId)
		{
			if (imageStream == null || !imageStream.CanRead)
			{
				throw new System.ArgumentException("imageStream isn't validate", "imageStream");
			}
			Image image = Image.FromStream(imageStream);
			string result = string.Empty;
			if (image.PropertyItems != null)
			{
				result = EXIFMetaDataService.GetValueOfType(image.GetPropertyItem(propId));
			}
			return result;
		}
		private static string GetValueOfType(PropertyItem propItem)
		{
			switch (propItem.Type)
			{
			case 1:
				return EXIFMetaDataService.GetValueOfType1(propItem.Value);
			case 2:
				return EXIFMetaDataService.GetValueOfType2(propItem.Value);
			case 3:
				return EXIFMetaDataService.GetValueOfType3(propItem.Value);
			case 4:
				return EXIFMetaDataService.GetValueOfType4(propItem.Value);
			case 5:
				return EXIFMetaDataService.GetValueOfType5(propItem.Value);
			case 7:
				return EXIFMetaDataService.GetValueOfType7(propItem.Value, propItem.Id);
			}
			return string.Empty;
		}
		private static string GetValueOfType1(byte[] value)
		{
			return System.Text.Encoding.ASCII.GetString(value);
		}
		private static string GetValueOfType2(byte[] value)
		{
			return System.Text.Encoding.ASCII.GetString(value);
		}
		private static string GetValueOfType3(byte[] value)
		{
			if (value.Length != 2)
			{
				return string.Empty;
			}
			return System.Convert.ToUInt16((int)value[1] << 8 | (int)value[0]).ToString();
		}
		private static string GetValueOfType4(byte[] value)
		{
			if (value.Length != 4)
			{
				return string.Empty;
			}
			return System.Convert.ToUInt32((int)value[3] << 24 | (int)value[2] << 16 | (int)value[1] << 8 | (int)value[0]).ToString();
		}
		private static string GetValueOfType5(byte[] value)
		{
			if (value.Length != 8)
			{
				return string.Empty;
			}
			uint num = System.Convert.ToUInt32((int)value[7] << 24 | (int)value[6] << 16 | (int)value[5] << 8 | (int)value[4]);
			return System.Convert.ToUInt32((int)value[3] << 24 | (int)value[2] << 16 | (int)value[1] << 8 | (int)value[0]).ToString() + "/" + num.ToString();
		}
		private static string GetValueOfType7(byte[] value, int propId)
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			for (int i = 0; i < value.Length; i++)
			{
				if (propId == 36864 || propId == 40960)
				{
					System.Text.StringBuilder arg_26_0 = stringBuilder;
					char c = (char)value[i];
					arg_26_0.Append(c.ToString());
				}
				else
				{
					stringBuilder.Append(value[i].ToString());
				}
			}
			return stringBuilder.ToString();
		}
	}
}
