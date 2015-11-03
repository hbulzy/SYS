using System;
using System.Reflection;
namespace PetaPoco
{
	public interface IMapper
	{
		TableInfo GetTableInfo(System.Type pocoType);
		ColumnInfo GetColumnInfo(System.Reflection.PropertyInfo pocoProperty);
		Func<object, object> GetFromDbConverter(System.Reflection.PropertyInfo TargetProperty, System.Type SourceType);
		Func<object, object> GetToDbConverter(System.Reflection.PropertyInfo SourceProperty);
	}
}
