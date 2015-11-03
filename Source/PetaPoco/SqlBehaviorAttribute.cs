using System;
namespace PetaPoco
{
	[System.AttributeUsage(System.AttributeTargets.Property)]
	public class SqlBehaviorAttribute : System.Attribute
	{
		public SqlBehaviorFlags Behavior
		{
			get;
			private set;
		}
		public SqlBehaviorAttribute(SqlBehaviorFlags behavior)
		{
			this.Behavior = behavior;
		}
	}
}
