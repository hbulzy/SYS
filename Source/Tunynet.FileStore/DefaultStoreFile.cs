using System;
using System.IO;
namespace Tunynet.FileStore
{
	public class DefaultStoreFile : IStoreFile
	{
		private readonly System.IO.FileInfo fileInfo;
		private string relativePath;
		private string fullLocalPath;
		public string Name
		{
			get
			{
				return this.fileInfo.Name;
			}
		}
		public string Extension
		{
			get
			{
				return this.fileInfo.Extension;
			}
		}
		public long Size
		{
			get
			{
				return this.fileInfo.Length;
			}
		}
		public System.DateTime LastModified
		{
			get
			{
				return this.fileInfo.LastWriteTime;
			}
		}
		public string RelativePath
		{
			get
			{
				return this.relativePath;
			}
		}
		public string FullLocalPath
		{
			get
			{
				return this.fullLocalPath;
			}
		}
		public DefaultStoreFile(string relativePath, System.IO.FileInfo fileInfo)
		{
			this.relativePath = relativePath;
			this.fileInfo = fileInfo;
			this.fullLocalPath = fileInfo.FullName;
		}
		public System.IO.Stream OpenReadStream()
		{
			return new System.IO.FileStream(this.fileInfo.FullName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
		}
	}
}
