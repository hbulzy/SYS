using System;
namespace PetaPoco
{
	[System.AttributeUsage(System.AttributeTargets.Property)]
	public class ResultColumnAttribute : ColumnAttribute
	{
		public ResultColumnAttribute()
		{
		}
		public ResultColumnAttribute(string name) : base(name)
		{
		}
	}
}
