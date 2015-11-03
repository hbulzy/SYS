using System;
using System.Reflection;
namespace PetaPoco
{
	public class StandardMapper : IMapper
	{
		public TableInfo GetTableInfo(System.Type pocoType)
		{
			return TableInfo.FromPoco(pocoType);
		}
		public ColumnInfo GetColumnInfo(System.Reflection.PropertyInfo pocoProperty)
		{
			return ColumnInfo.FromProperty(pocoProperty);
		}
		public Func<object, object> GetFromDbConverter(System.Reflection.PropertyInfo TargetProperty, System.Type SourceType)
		{
			return null;
		}
		public Func<object, object> GetToDbConverter(System.Reflection.PropertyInfo SourceProperty)
		{
			return null;
		}
	}
}
