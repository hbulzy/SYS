using System;
namespace PetaPoco
{
	public interface ITransaction : System.IDisposable
	{
		void Complete();
	}
}
