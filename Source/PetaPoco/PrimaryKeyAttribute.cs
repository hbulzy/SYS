using System;
namespace PetaPoco
{
	[System.AttributeUsage(System.AttributeTargets.Class)]
	public class PrimaryKeyAttribute : System.Attribute
	{
		public string Value
		{
			get;
			private set;
		}
		public string sequenceName
		{
			get;
			set;
		}
		public bool autoIncrement
		{
			get;
			set;
		}
		public PrimaryKeyAttribute(string primaryKey)
		{
			this.Value = primaryKey;
			this.autoIncrement = true;
		}
	}
}
