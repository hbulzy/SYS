using System;
namespace Tunynet
{
	public abstract class IdGenerator
	{
		private static volatile IdGenerator _defaultInstance = null;
		private static readonly object lockObject = new object();
		private static IdGenerator Instance()
		{
			if (IdGenerator._defaultInstance == null)
			{
				lock (IdGenerator.lockObject)
				{
					if (IdGenerator._defaultInstance == null)
					{
						IdGenerator._defaultInstance = DIContainer.Resolve<IdGenerator>();
						if (IdGenerator._defaultInstance == null)
						{
							throw new ExceptionFacade("未在DIContainer注册IdGenerator的具体实现类", null);
						}
					}
				}
			}
			return IdGenerator._defaultInstance;
		}
		public static long Next()
		{
			long result;
			lock (IdGenerator.lockObject)
			{
				result = IdGenerator.Instance().NextLong();
			}
			return result;
		}
		protected abstract long NextLong();
	}
}
