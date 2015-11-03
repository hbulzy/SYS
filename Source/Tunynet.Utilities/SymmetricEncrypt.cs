using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
namespace Tunynet.Utilities
{
	public class SymmetricEncrypt
	{
		private SymmetricEncryptType _mbytEncryptionType;
		private string _mstrOriginalString;
		private string _mstrEncryptedString;
		private System.Security.Cryptography.SymmetricAlgorithm _mCSP;
		public SymmetricEncryptType EncryptionType
		{
			get
			{
				return this._mbytEncryptionType;
			}
			set
			{
				if (this._mbytEncryptionType != value)
				{
					this._mbytEncryptionType = value;
					this._mstrOriginalString = string.Empty;
					this._mstrEncryptedString = string.Empty;
					this.SetEncryptor();
				}
			}
		}
		public System.Security.Cryptography.SymmetricAlgorithm CryptoProvider
		{
			get
			{
				return this._mCSP;
			}
			set
			{
				this._mCSP = value;
			}
		}
		public string OriginalString
		{
			get
			{
				return this._mstrOriginalString;
			}
			set
			{
				this._mstrOriginalString = value;
			}
		}
		public string EncryptedString
		{
			get
			{
				return this._mstrEncryptedString;
			}
			set
			{
				this._mstrEncryptedString = value;
			}
		}
		public byte[] key
		{
			get
			{
				return this._mCSP.Key;
			}
			set
			{
				this._mCSP.Key = value;
			}
		}
		public string KeyString
		{
			get
			{
				return System.Convert.ToBase64String(this._mCSP.Key);
			}
			set
			{
				this._mCSP.Key = System.Convert.FromBase64String(value);
			}
		}
		public byte[] IV
		{
			get
			{
				return this._mCSP.IV;
			}
			set
			{
				this._mCSP.IV = value;
			}
		}
		public string IVString
		{
			get
			{
				return System.Convert.ToBase64String(this._mCSP.IV);
			}
			set
			{
				this._mCSP.IV = System.Convert.FromBase64String(value);
			}
		}
		public SymmetricEncrypt()
		{
			this._mbytEncryptionType = SymmetricEncryptType.DES;
			this.SetEncryptor();
		}
		public SymmetricEncrypt(SymmetricEncryptType encryptionType)
		{
			this._mbytEncryptionType = encryptionType;
			this.SetEncryptor();
		}
		public SymmetricEncrypt(SymmetricEncryptType encryptionType, string originalString)
		{
			this._mbytEncryptionType = encryptionType;
			this._mstrOriginalString = originalString;
			this.SetEncryptor();
		}
		public string Encrypt()
		{
			System.Security.Cryptography.ICryptoTransform transform = this._mCSP.CreateEncryptor(this._mCSP.Key, this._mCSP.IV);
			byte[] bytes = System.Text.Encoding.Unicode.GetBytes(this._mstrOriginalString);
			System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
			System.Security.Cryptography.CryptoStream cryptoStream = new System.Security.Cryptography.CryptoStream(memoryStream, transform, System.Security.Cryptography.CryptoStreamMode.Write);
			cryptoStream.Write(bytes, 0, bytes.Length);
			cryptoStream.FlushFinalBlock();
			cryptoStream.Close();
			this._mstrEncryptedString = System.Convert.ToBase64String(memoryStream.ToArray());
			return this._mstrEncryptedString;
		}
		public string Encrypt(string originalString)
		{
			this._mstrOriginalString = originalString;
			return this.Encrypt();
		}
		public string Encrypt(string originalString, SymmetricEncryptType encryptionType)
		{
			this._mstrOriginalString = originalString;
			this._mbytEncryptionType = encryptionType;
			return this.Encrypt();
		}
		public string Decrypt()
		{
			System.Security.Cryptography.ICryptoTransform transform = this._mCSP.CreateDecryptor(this._mCSP.Key, this._mCSP.IV);
			byte[] array = System.Convert.FromBase64String(this._mstrEncryptedString);
			System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
			System.Security.Cryptography.CryptoStream cryptoStream = new System.Security.Cryptography.CryptoStream(memoryStream, transform, System.Security.Cryptography.CryptoStreamMode.Write);
			cryptoStream.Write(array, 0, array.Length);
			cryptoStream.FlushFinalBlock();
			cryptoStream.Close();
			this._mstrOriginalString = System.Text.Encoding.Unicode.GetString(memoryStream.ToArray());
			return this._mstrOriginalString;
		}
		public string Decrypt(string encryptedString)
		{
			this._mstrEncryptedString = encryptedString;
			return this.Decrypt();
		}
		public string Decrypt(string encryptedString, SymmetricEncryptType encryptionType)
		{
			this._mstrEncryptedString = encryptedString;
			this._mbytEncryptionType = encryptionType;
			return this.Decrypt();
		}
		private void SetEncryptor()
		{
			switch (this._mbytEncryptionType)
			{
			case SymmetricEncryptType.DES:
				this._mCSP = new System.Security.Cryptography.DESCryptoServiceProvider();
				break;
			case SymmetricEncryptType.RC2:
				this._mCSP = new System.Security.Cryptography.RC2CryptoServiceProvider();
				break;
			case SymmetricEncryptType.Rijndael:
				this._mCSP = new System.Security.Cryptography.RijndaelManaged();
				break;
			case SymmetricEncryptType.TripleDES:
				this._mCSP = new System.Security.Cryptography.TripleDESCryptoServiceProvider();
				break;
			}
			this._mCSP.GenerateKey();
			this._mCSP.GenerateIV();
		}
		public string GenerateKey()
		{
			this._mCSP.GenerateKey();
			return System.Convert.ToBase64String(this._mCSP.Key);
		}
		public string GenerateIV()
		{
			this._mCSP.GenerateIV();
			return System.Convert.ToBase64String(this._mCSP.IV);
		}
	}
}
