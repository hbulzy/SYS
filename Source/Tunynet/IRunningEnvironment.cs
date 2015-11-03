using System;
namespace Tunynet
{
	public interface IRunningEnvironment
	{
		bool IsFullTrust
		{
			get;
		}
		void RestartAppDomain();
	}
}
