using System;
namespace Tunynet.Caching
{
	public enum CachingExpirationType
	{
		Invariable,
		Stable,
		RelativelyStable,
		UsualSingleObject,
		UsualObjectCollection,
		SingleObject,
		ObjectCollection
	}
}
