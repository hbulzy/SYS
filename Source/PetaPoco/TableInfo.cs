using System;
namespace PetaPoco
{
	public class TableInfo
	{
		public string TableName
		{
			get;
			set;
		}
		public string PrimaryKey
		{
			get;
			set;
		}
		public bool AutoIncrement
		{
			get;
			set;
		}
		public string SequenceName
		{
			get;
			set;
		}
		public static TableInfo FromPoco(System.Type t)
		{
			TableInfo tableInfo = new TableInfo();
			object[] customAttributes = t.GetCustomAttributes(typeof(TableNameAttribute), true);
			tableInfo.TableName = ((customAttributes.Length == 0) ? t.Name : (customAttributes[0] as TableNameAttribute).Value);
			customAttributes = t.GetCustomAttributes(typeof(PrimaryKeyAttribute), true);
			tableInfo.PrimaryKey = ((customAttributes.Length == 0) ? "ID" : (customAttributes[0] as PrimaryKeyAttribute).Value);
			tableInfo.SequenceName = ((customAttributes.Length == 0) ? null : (customAttributes[0] as PrimaryKeyAttribute).sequenceName);
			tableInfo.AutoIncrement = (customAttributes.Length != 0 && (customAttributes[0] as PrimaryKeyAttribute).autoIncrement);
			return tableInfo;
		}
	}
}
