using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Tunynet.Imaging;
using Tunynet.Utilities;
namespace Tunynet.FileStore
{
	public class DefaultStoreProvider : IStoreProvider
	{
		private string storeRootPath;
		private string directlyRootUrl;
		private static readonly Regex ValidPathPattern;
		private static readonly Regex ValidFileNamePattern;
		public string StoreRootPath
		{
			get
			{
				return this.storeRootPath;
			}
		}
		public string DirectlyRootUrl
		{
			get
			{
				return this.directlyRootUrl;
			}
		}
		public DefaultStoreProvider(string storeRootPath) : this(storeRootPath, WebUtility.ResolveUrl(storeRootPath))
		{
		}
		public DefaultStoreProvider(string storeRootPath, string directlyRootUrl)
		{
			this.storeRootPath = WebUtility.GetPhysicalFilePath(storeRootPath);
			if (!string.IsNullOrEmpty(this.StoreRootPath))
			{
				this.storeRootPath = this.StoreRootPath.TrimEnd(new char[]
				{
					'/',
					'\\'
				});
			}
			this.directlyRootUrl = directlyRootUrl;
			if (!string.IsNullOrEmpty(this.directlyRootUrl))
			{
				this.directlyRootUrl = WebUtility.ResolveUrl(this.directlyRootUrl.TrimEnd(new char[]
				{
					'/',
					'\\'
				}));
			}
		}
		public DefaultStoreProvider(string storeRootPath, string directlyRootUrl, string username, string password) : this(storeRootPath, directlyRootUrl)
		{
			NetworkShareAccesser networkShareAccesser = new NetworkShareAccesser(storeRootPath, username, password);
			networkShareAccesser.Disconnect();
			networkShareAccesser.Connect();
		}
		static DefaultStoreProvider()
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			stringBuilder.Append("^[^");
			char[] invalidFileNameChars = System.IO.Path.GetInvalidFileNameChars();
			for (int i = 0; i < invalidFileNameChars.Length; i++)
			{
				char c = invalidFileNameChars[i];
				stringBuilder.Append(Regex.Escape(new string(c, 1)));
			}
			stringBuilder.Append("]{1,255}$");
			DefaultStoreProvider.ValidFileNamePattern = new Regex(stringBuilder.ToString(), RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
			stringBuilder = new System.Text.StringBuilder();
			stringBuilder.Append("^[^");
			char[] invalidPathChars = System.IO.Path.GetInvalidPathChars();
			for (int j = 0; j < invalidPathChars.Length; j++)
			{
				char c2 = invalidPathChars[j];
				stringBuilder.Append(Regex.Escape(new string(c2, 1)));
			}
			DefaultStoreProvider.ValidPathPattern = new Regex(stringBuilder.ToString() + "]{0,769}$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
		}
		public IStoreFile GetFile(string relativePath, string fileName)
		{
			string fullLocalPath = this.GetFullLocalPath(relativePath, fileName);
			if (System.IO.File.Exists(fullLocalPath))
			{
				return new DefaultStoreFile(relativePath, new System.IO.FileInfo(fullLocalPath));
			}
			return null;
		}
		public IStoreFile GetFile(string relativeFileName)
		{
			string fullLocalPath = this.GetFullLocalPath(relativeFileName);
			string relativePath = this.GetRelativePath(fullLocalPath, true);
			if (System.IO.File.Exists(fullLocalPath))
			{
				return new DefaultStoreFile(relativePath, new System.IO.FileInfo(fullLocalPath));
			}
			return null;
		}
		public System.Collections.Generic.IEnumerable<IStoreFile> GetFiles(string relativePath, bool isOnlyCurrentFolder)
		{
			if (!DefaultStoreProvider.IsValidPath(relativePath))
			{
				throw new System.ArgumentException("The provided path is invalid", "relativePath");
			}
			System.Collections.Generic.List<IStoreFile> list = new System.Collections.Generic.List<IStoreFile>();
			string fullLocalPath = this.GetFullLocalPath(relativePath, string.Empty);
			if (System.IO.Directory.Exists(fullLocalPath))
			{
				System.IO.SearchOption searchOption = System.IO.SearchOption.TopDirectoryOnly;
				if (!isOnlyCurrentFolder)
				{
					searchOption = System.IO.SearchOption.AllDirectories;
				}
				System.IO.FileInfo[] files = new System.IO.DirectoryInfo(fullLocalPath).GetFiles("*.*", searchOption);
				for (int i = 0; i < files.Length; i++)
				{
					System.IO.FileInfo fileInfo = files[i];
					if ((fileInfo.Attributes & System.IO.FileAttributes.Hidden) != System.IO.FileAttributes.Hidden)
					{
						DefaultStoreFile item;
						if (isOnlyCurrentFolder)
						{
							item = new DefaultStoreFile(relativePath, fileInfo);
						}
						else
						{
							item = new DefaultStoreFile(this.GetRelativePath(fileInfo.FullName, true), fileInfo);
						}
						list.Add(item);
					}
				}
			}
			return list;
		}
		public IStoreFile AddOrUpdateFile(string relativePath, string fileName, System.IO.Stream contentStream)
		{
			if (contentStream == null || !contentStream.CanRead)
			{
				return null;
			}
			if (!DefaultStoreProvider.IsValidPathAndFileName(relativePath, fileName))
			{
				throw new System.InvalidOperationException("The provided path and/or file name is invalid.");
			}
			string fullLocalPath = this.GetFullLocalPath(relativePath, fileName);
			DefaultStoreProvider.EnsurePathExists(fullLocalPath, true);
			contentStream.Position = 0L;
			using (System.IO.FileStream fileStream = System.IO.File.OpenWrite(fullLocalPath))
			{
				byte[] array = new byte[(contentStream.Length > 65536L) ? 65536L : contentStream.Length];
				int count;
				while ((count = contentStream.Read(array, 0, array.Length)) > 0)
				{
					fileStream.Write(array, 0, count);
				}
				fileStream.Flush();
				fileStream.Close();
			}
			return new DefaultStoreFile(relativePath, new System.IO.FileInfo(fullLocalPath));
		}
		public void DeleteFile(string relativePath, string fileName)
		{
			if (!DefaultStoreProvider.IsValidPathAndFileName(relativePath, fileName))
			{
				throw new System.InvalidOperationException("The provided path and/or file name is invalid");
			}
			string fullLocalPath = this.GetFullLocalPath(relativePath, fileName);
			if (System.IO.File.Exists(fullLocalPath))
			{
				System.IO.File.Delete(fullLocalPath);
			}
		}
		public void DeleteFiles(string relativePath, string fileNamePrefix)
		{
			if (!DefaultStoreProvider.IsValidPath(relativePath))
			{
				throw new System.InvalidOperationException("The provided path is invalid");
			}
			string fullLocalPath = this.GetFullLocalPath(relativePath, string.Empty);
			if (System.IO.Directory.Exists(fullLocalPath))
			{
				System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(fullLocalPath);
				System.IO.FileInfo[] files = directoryInfo.GetFiles(fileNamePrefix + "*");
				for (int i = 0; i < files.Length; i++)
				{
					System.IO.FileInfo fileInfo = files[i];
					fileInfo.Delete();
				}
			}
		}
		public void DeleteFolder(string relativePath)
		{
			if (!DefaultStoreProvider.IsValidPath(relativePath))
			{
				return;
			}
			string fullLocalPath = this.GetFullLocalPath(relativePath, string.Empty);
			if (System.IO.Directory.Exists(fullLocalPath))
			{
				System.IO.Directory.Delete(fullLocalPath, true);
			}
		}
		public string GetDirectlyUrl(string relativePath, string fileName)
		{
			return this.GetDirectlyUrl(relativePath, fileName, System.DateTime.MinValue);
		}
		public string GetDirectlyUrl(string relativePath, string fileName, System.DateTime lastModified)
		{
			string arg_05_0 = string.Empty;
			if (string.IsNullOrEmpty(relativePath))
			{
				return this.GetDirectlyUrl(fileName, lastModified);
			}
			if (relativePath.EndsWith("\\") || relativePath.EndsWith("/"))
			{
				return this.GetDirectlyUrl(relativePath + fileName, lastModified);
			}
			return this.GetDirectlyUrl(relativePath + "/" + fileName, lastModified);
		}
		public string GetDirectlyUrl(string relativeFileName)
		{
			return this.GetDirectlyUrl(relativeFileName, System.DateTime.MinValue);
		}
		public string GetDirectlyUrl(string relativeFileName, System.DateTime lastModified)
		{
			if (string.IsNullOrEmpty(this.DirectlyRootUrl))
			{
				return string.Empty;
			}
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(this.DirectlyRootUrl);
			relativeFileName = relativeFileName.Replace('\\', '/');
			if (!relativeFileName.StartsWith("/"))
			{
				stringBuilder.Append("/");
			}
			stringBuilder.Append(relativeFileName);
			if (lastModified > System.DateTime.MinValue)
			{
				stringBuilder.Append("?lm=");
				stringBuilder.Append(lastModified.Ticks);
			}
			return stringBuilder.ToString();
		}
		public string JoinDirectory(params string[] directoryParts)
		{
			return string.Join(new string(System.IO.Path.DirectorySeparatorChar, 1), directoryParts);
		}
		public IStoreFile GetResizedImage(string fileRelativePath, string filename, Size size, ResizeMethod resizeMethod)
		{
			if (filename.ToLower().EndsWith(".gif"))
			{
				return this.GetFile(fileRelativePath, filename);
			}
			string sizeImageName = this.GetSizeImageName(filename, size, resizeMethod);
			IStoreFile storeFile = this.GetFile(fileRelativePath, sizeImageName);
			if (storeFile == null)
			{
				IStoreFile file = this.GetFile(fileRelativePath, filename);
				if (file == null)
				{
					return null;
				}
				using (System.IO.Stream stream = file.OpenReadStream())
				{
					if (stream != null)
					{
						using (System.IO.Stream stream2 = ImageProcessor.Resize(stream, size.Width, size.Height, resizeMethod))
						{
							storeFile = this.AddOrUpdateFile(fileRelativePath, sizeImageName, stream2);
						}
					}
				}
			}
			return storeFile;
		}
		public string GetSizeImageName(string filename, Size size, ResizeMethod resizeMethod)
		{
			return string.Format("{0}-{1}-{2}x{3}{4}", new object[]
			{
				filename,
				(resizeMethod != ResizeMethod.KeepAspectRatio) ? resizeMethod.ToString() : string.Empty,
				size.Width,
				size.Height,
				System.IO.Path.GetExtension(filename)
			});
		}
		private static bool IsValidPath(string path)
		{
			return DefaultStoreProvider.ValidPathPattern.IsMatch(path);
		}
		private static bool IsValidFileName(string fileName)
		{
			return DefaultStoreProvider.ValidFileNamePattern == null || DefaultStoreProvider.ValidFileNamePattern.IsMatch(fileName);
		}
		private static bool IsValidPathAndFileName(string path, string fileName)
		{
			return DefaultStoreProvider.IsValidPath(path) && DefaultStoreProvider.IsValidFileName(fileName) && System.Text.Encoding.UTF8.GetBytes(path + "." + fileName).Length <= 1024;
		}
		public string GetFullLocalPath(string relativePath, string fileName)
		{
			string text = this.StoreRootPath;
			if (text.EndsWith(":"))
			{
				text += "\\";
			}
			if (!string.IsNullOrEmpty(relativePath))
			{
				relativePath = relativePath.TrimStart(new char[]
				{
					System.IO.Path.DirectorySeparatorChar
				});
				text = System.IO.Path.Combine(text, relativePath);
			}
			if (!string.IsNullOrEmpty(fileName))
			{
				text = System.IO.Path.Combine(text, fileName);
			}
			return text;
		}
		public string GetFullLocalPath(string relativeFileName)
		{
			string text = this.StoreRootPath;
			if (text.EndsWith(":"))
			{
				text += "\\";
			}
			if (!string.IsNullOrEmpty(relativeFileName))
			{
				text = System.IO.Path.Combine(text, relativeFileName);
			}
			return text;
		}
		public string GetRelativePath(string fullLocalPath, bool pathIncludesFilename)
		{
			string text = pathIncludesFilename ? fullLocalPath.Substring(0, fullLocalPath.LastIndexOf(System.IO.Path.DirectorySeparatorChar)) : fullLocalPath;
			text = text.Replace(this.StoreRootPath, string.Empty);
			return text.Trim(new char[]
			{
				System.IO.Path.DirectorySeparatorChar
			});
		}
		private static void EnsurePathExists(string fullLocalPath, bool pathIncludesFilename)
		{
			string path = pathIncludesFilename ? fullLocalPath.Substring(0, fullLocalPath.LastIndexOf(System.IO.Path.DirectorySeparatorChar)) : fullLocalPath;
			if (!System.IO.Directory.Exists(path))
			{
				System.IO.Directory.CreateDirectory(path);
			}
		}
	}
}
