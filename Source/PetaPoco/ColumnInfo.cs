using System;
using System.Reflection;
namespace PetaPoco
{
	public class ColumnInfo
	{
		public string ColumnName
		{
			get;
			set;
		}
		public bool ResultColumn
		{
			get;
			set;
		}
		public bool ForceToUtc
		{
			get;
			set;
		}
		public static ColumnInfo FromProperty(System.Reflection.PropertyInfo pi)
		{
			bool flag = pi.DeclaringType.GetCustomAttributes(typeof(ExplicitColumnsAttribute), true).Length > 0;
			object[] customAttributes = pi.GetCustomAttributes(typeof(ColumnAttribute), true);
			if (flag)
			{
				if (customAttributes.Length == 0)
				{
					return null;
				}
			}
			else
			{
				if (pi.GetCustomAttributes(typeof(IgnoreAttribute), true).Length != 0)
				{
					return null;
				}
			}
			ColumnInfo columnInfo = new ColumnInfo();
			if (customAttributes.Length > 0)
			{
				ColumnAttribute columnAttribute = (ColumnAttribute)customAttributes[0];
				columnInfo.ColumnName = ((columnAttribute.Name == null) ? pi.Name : columnAttribute.Name);
				columnInfo.ForceToUtc = columnAttribute.ForceToUtc;
				if (columnAttribute is ResultColumnAttribute)
				{
					columnInfo.ResultColumn = true;
				}
			}
			else
			{
				columnInfo.ColumnName = pi.Name;
				columnInfo.ForceToUtc = false;
				columnInfo.ResultColumn = false;
			}
			return columnInfo;
		}
	}
}
