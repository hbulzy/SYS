using System;
namespace PetaPoco
{
	[System.AttributeUsage(System.AttributeTargets.Class)]
	public class TableNameAttribute : System.Attribute
	{
		public string Value
		{
			get;
			private set;
		}
		public TableNameAttribute(string tableName)
		{
			this.Value = tableName;
		}
	}
}
