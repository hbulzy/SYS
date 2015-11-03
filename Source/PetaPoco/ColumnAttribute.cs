using System;
namespace PetaPoco
{
	[System.AttributeUsage(System.AttributeTargets.Property)]
	public class ColumnAttribute : System.Attribute
	{
		public string Name
		{
			get;
			set;
		}
		public bool ForceToUtc
		{
			get;
			set;
		}
		public ColumnAttribute()
		{
			this.ForceToUtc = false;
		}
		public ColumnAttribute(string Name)
		{
			this.Name = Name;
			this.ForceToUtc = false;
		}
	}
}
