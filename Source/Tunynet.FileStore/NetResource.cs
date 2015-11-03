using System;
using System.Runtime.InteropServices;
namespace Tunynet.FileStore
{
	[System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
	public class NetResource
	{
		public ResourceScope Scope;
		public ResourceType ResourceType;
		public ResourceDisplayType DisplayType;
		public int Usage;
		public string LocalName;
		public string RemoteName;
		public string Comment;
		public string Provider;
	}
}
