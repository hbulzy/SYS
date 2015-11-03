using System;
namespace PetaPoco.Internal
{
	internal static class Singleton<T> where T : new()
	{
		public static T Instance = (default(T) == null) ? System.Activator.CreateInstance<T>() : default(T);
	}
}
