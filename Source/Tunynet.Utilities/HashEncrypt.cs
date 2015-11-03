using System;
using System.Security.Cryptography;
using System.Text;
namespace Tunynet.Utilities
{
	public class HashEncrypt
	{
		private HashEncryptType _mbytHashType;
		private string _mstrOriginalString;
		private string _mstrHashString;
		private System.Security.Cryptography.HashAlgorithm _mhash;
		private bool mboolUseSalt;
		private string mstrSaltValue = string.Empty;
		private short msrtSaltLength = 8;
		public HashEncryptType HashType
		{
			get
			{
				return this._mbytHashType;
			}
			set
			{
				if (this._mbytHashType != value)
				{
					this._mbytHashType = value;
					this._mstrOriginalString = string.Empty;
					this._mstrHashString = string.Empty;
					this.SetEncryptor();
				}
			}
		}
		public string SaltValue
		{
			get
			{
				return this.mstrSaltValue;
			}
			set
			{
				this.mstrSaltValue = value;
			}
		}
		public bool UseSalt
		{
			get
			{
				return this.mboolUseSalt;
			}
			set
			{
				this.mboolUseSalt = value;
			}
		}
		public short SaltLength
		{
			get
			{
				return this.msrtSaltLength;
			}
			set
			{
				this.msrtSaltLength = value;
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
		public string HashString
		{
			get
			{
				return this._mstrHashString;
			}
			set
			{
				this._mstrHashString = value;
			}
		}
		public HashEncrypt()
		{
			this._mbytHashType = HashEncryptType.MD5;
		}
		public HashEncrypt(HashEncryptType hashType)
		{
			this._mbytHashType = hashType;
		}
		public HashEncrypt(HashEncryptType hashType, string originalString)
		{
			this._mbytHashType = hashType;
			this._mstrOriginalString = originalString;
		}
		public HashEncrypt(HashEncryptType hashType, string originalString, bool useSalt, string saltValue)
		{
			this._mbytHashType = hashType;
			this._mstrOriginalString = originalString;
			this.mboolUseSalt = useSalt;
			this.mstrSaltValue = saltValue;
		}
		private void SetEncryptor()
		{
			switch (this._mbytHashType)
			{
			case HashEncryptType.MD5:
				this._mhash = new System.Security.Cryptography.MD5CryptoServiceProvider();
				return;
			case HashEncryptType.SHA1:
				this._mhash = new System.Security.Cryptography.SHA1CryptoServiceProvider();
				return;
			case HashEncryptType.SHA256:
				this._mhash = new System.Security.Cryptography.SHA256Managed();
				return;
			case HashEncryptType.SHA384:
				this._mhash = new System.Security.Cryptography.SHA384Managed();
				return;
			case HashEncryptType.SHA512:
				this._mhash = new System.Security.Cryptography.SHA512Managed();
				return;
			default:
				return;
			}
		}
		public string Encrypt()
		{
			this.SetEncryptor();
			if (this.mboolUseSalt && this.mstrSaltValue.Length == 0)
			{
				this.mstrSaltValue = this.CreateSalt();
			}
			byte[] bytes = System.Text.Encoding.UTF8.GetBytes(this.mstrSaltValue + this._mstrOriginalString);
			byte[] inArray = this._mhash.ComputeHash(bytes);
			return System.Convert.ToBase64String(inArray);
		}
		public string Encrypt(string originalString)
		{
			this._mstrOriginalString = originalString;
			return this.Encrypt();
		}
		public string Encrypt(string originalString, HashEncryptType hashType)
		{
			this._mstrOriginalString = originalString;
			this._mbytHashType = hashType;
			return this.Encrypt();
		}
		public string Encrypt(string originalString, bool useSalt)
		{
			this._mstrOriginalString = originalString;
			this.mboolUseSalt = useSalt;
			return this.Encrypt();
		}
		public string Encrypt(string originalString, HashEncryptType hashType, string saltValue)
		{
			this._mstrOriginalString = originalString;
			this._mbytHashType = hashType;
			this.mstrSaltValue = saltValue;
			return this.Encrypt();
		}
		public string Encrypt(string originalString, string saltValue)
		{
			this._mstrOriginalString = originalString;
			this.mstrSaltValue = saltValue;
			return this.Encrypt();
		}
		public void Reset()
		{
			this.mstrSaltValue = string.Empty;
			this._mstrOriginalString = string.Empty;
			this._mstrHashString = string.Empty;
			this.mboolUseSalt = false;
			this._mbytHashType = HashEncryptType.MD5;
			this._mhash = null;
		}
		public string CreateSalt()
		{
			byte[] array = new byte[8];
			System.Security.Cryptography.RNGCryptoServiceProvider rNGCryptoServiceProvider = new System.Security.Cryptography.RNGCryptoServiceProvider();
			rNGCryptoServiceProvider.GetBytes(array);
			return System.Convert.ToBase64String(array);
		}
	}
}
