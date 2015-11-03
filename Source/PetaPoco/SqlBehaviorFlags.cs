using System;
namespace PetaPoco
{
	[System.Flags]
	public enum SqlBehaviorFlags
	{
		Insert = 1,
		Update = 2,
		All = 3
	}
}
