using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Tunynet.Imaging;
namespace Tunynet.FileStore
{
	public interface IStoreProvider
	{
		string StoreRootPath
		{
			get;
		}
		string DirectlyRootUrl
		{
			get;
		}
		IStoreFile GetFile(string relativePath, string fileName);
		IStoreFile GetFile(string relativeFileName);
		System.Collections.Generic.IEnumerable<IStoreFile> GetFiles(string relativePath, bool isOnlyCurrentFolder);
		IStoreFile AddOrUpdateFile(string relativePath, string fileName, System.IO.Stream contentStream);
		void DeleteFile(string relativePath, string fileName);
		void DeleteFiles(string relativePath, string fileNamePrefix);
		void DeleteFolder(string relativePath);
		string GetDirectlyUrl(string relativePath, string fileName);
		string GetDirectlyUrl(string relativePath, string fileName, System.DateTime lastModified);
		string GetDirectlyUrl(string relativeFileName);
		string GetDirectlyUrl(string relativeFileName, System.DateTime lastModified);
		string GetRelativePath(string fullLocalPath, bool pathIncludesFilename);
		string GetFullLocalPath(string relativePath, string fileName);
		string GetFullLocalPath(string relativeFileName);
		string JoinDirectory(params string[] directoryParts);
		IStoreFile GetResizedImage(string fileRelativePath, string filename, Size size, ResizeMethod resizeMethod);
		string GetSizeImageName(string filename, Size size, ResizeMethod resizeMethod);
	}
}
