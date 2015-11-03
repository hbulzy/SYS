using System;
using System.IO;
namespace Tunynet.FileStore
{
	public interface IStoreFile
	{
		string Name
		{
			get;
		}
		string Extension
		{
			get;
		}
		long Size
		{
			get;
		}
		string RelativePath
		{
			get;
		}
		System.DateTime LastModified
		{
			get;
		}
		System.IO.Stream OpenReadStream();
	}
}
