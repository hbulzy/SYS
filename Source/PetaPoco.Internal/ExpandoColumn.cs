using System;
using System.Collections.Generic;
namespace PetaPoco.Internal
{
	internal class ExpandoColumn : PocoColumn
	{
		public override void SetValue(object target, object val)
		{
			(target as System.Collections.Generic.IDictionary<string, object>)[this.ColumnName] = val;
		}
		public override object GetValue(object target)
		{
			object result = null;
			(target as System.Collections.Generic.IDictionary<string, object>).TryGetValue(this.ColumnName, out result);
			return result;
		}
		public override object ChangeType(object val)
		{
			return val;
		}
	}
}
