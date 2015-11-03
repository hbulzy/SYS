using System;
using System.Security.Cryptography;
using System.Text;
namespace Tunynet.Utilities
{
	public static class EncryptionUtility
	{
		public static string SymmetricEncrypt(SymmetricEncryptType encryptType, string str, string ivString, string keyString)
		{
			if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(ivString) || string.IsNullOrEmpty(keyString))
			{
				return str;
			}
			return new SymmetricEncrypt(encryptType)
			{
				IVString = ivString,
				KeyString = keyString
			}.Encrypt(str);
		}
		public static string SymmetricDncrypt(SymmetricEncryptType encryptType, string str, string ivString, string keyString)
		{
			if (string.IsNullOrEmpty(str))
			{
				return str;
			}
			return new SymmetricEncrypt(encryptType)
			{
				IVString = ivString,
				KeyString = keyString
			}.Decrypt(str);
		}
		public static string MD5(string str)
		{
			byte[] array = System.Text.Encoding.UTF8.GetBytes(str);
			array = new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(array);
			string text = "";
			for (int i = 0; i < array.Length; i++)
			{
				text += array[i].ToString("x").PadLeft(2, '0');
			}
			return text;
		}
		public static string MD5_16(string str)
		{
			return EncryptionUtility.MD5(str).Substring(8, 16);
		}
		public static string Base64_Encode(string str)
		{
			byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
			return System.Convert.ToBase64String(bytes);
		}
		public static string Base64_Decode(string str)
		{
			byte[] bytes = System.Convert.FromBase64String(str);
			return System.Text.Encoding.UTF8.GetString(bytes);
		}
	}
}
