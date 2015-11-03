using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
namespace Tunynet.FileStore
{
	public class NetworkShareAccesser
	{
		private string uncName;
		private string username;
		private string password;
		public NetworkShareAccesser(string uncName, string username, string password)
		{
			this.uncName = uncName;
			this.username = username;
			this.password = password;
		}
		public void Connect()
		{
			NetResource netResource = new NetResource
			{
				Scope = ResourceScope.GlobalNetwork,
				ResourceType = ResourceType.Disk,
				DisplayType = ResourceDisplayType.Share,
				RemoteName = this.uncName.TrimEnd(new char[]
				{
					'\\'
				})
			};
			int num = NetworkShareAccesser.WNetAddConnection2(netResource, this.password, this.username, 0);
			if (num != 0)
			{
				throw new Win32Exception(num);
			}
		}
		public void Disconnect()
		{
			NetworkShareAccesser.WNetCancelConnection2(this.uncName, 1, true);
		}
		[System.Runtime.InteropServices.DllImport("mpr.dll")]
		private static extern int WNetAddConnection2(NetResource netResource, string password, string username, int flags);
		[System.Runtime.InteropServices.DllImport("mpr.dll")]
		private static extern int WNetCancelConnection2(string name, int flags, bool force);
	}
}
