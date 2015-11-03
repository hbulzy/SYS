using System;
using System.Reflection;
namespace PetaPoco.Internal
{
	internal class PocoColumn
	{
		public string ColumnName;
		public System.Reflection.PropertyInfo PropertyInfo;
		public bool ResultColumn;
		public bool ForceToUtc = true;
		private SqlBehaviorFlags sqlBehavior = SqlBehaviorFlags.All;
		public SqlBehaviorFlags SqlBehavior
		{
			get
			{
				return this.sqlBehavior;
			}
			set
			{
				this.sqlBehavior = value;
			}
		}
		public virtual void SetValue(object target, object val)
		{
			this.PropertyInfo.SetValue(target, val, null);
		}
		public virtual object GetValue(object target)
		{
			return this.PropertyInfo.GetValue(target, null);
		}
		public virtual object ChangeType(object val)
		{
			return System.Convert.ChangeType(val, this.PropertyInfo.PropertyType);
		}
	}
}
