using System;
using System.Data.Common;
using System.Reflection;
namespace PetaPoco
{
	public class OracleProvider : DbProviderFactory
	{
		private const string _assemblyName = "Oracle.DataAccess";
		private const string _connectionTypeName = "Oracle.DataAccess.Client.OracleConnection";
		private const string _commandTypeName = "Oracle.DataAccess.Client.OracleCommand";
		private static System.Type _connectionType;
		private static System.Type _commandType;
		public static OracleProvider Instance = new OracleProvider();
		public OracleProvider()
		{
			OracleProvider._connectionType = OracleProvider.TypeFromAssembly("Oracle.DataAccess.Client.OracleConnection", "Oracle.DataAccess");
			OracleProvider._commandType = OracleProvider.TypeFromAssembly("Oracle.DataAccess.Client.OracleCommand", "Oracle.DataAccess");
			if (OracleProvider._connectionType == null)
			{
				throw new System.InvalidOperationException("Can't find Connection type: Oracle.DataAccess.Client.OracleConnection");
			}
		}
		public override DbConnection CreateConnection()
		{
			return (DbConnection)System.Activator.CreateInstance(OracleProvider._connectionType);
		}
		public override DbCommand CreateCommand()
		{
			DbCommand dbCommand = (DbCommand)System.Activator.CreateInstance(OracleProvider._commandType);
			System.Reflection.PropertyInfo property = OracleProvider._commandType.GetProperty("BindByName");
			property.SetValue(dbCommand, true, null);
			return dbCommand;
		}
		public static System.Type TypeFromAssembly(string typeName, string assemblyName)
		{
			System.Type result;
			try
			{
				System.Type type = System.Type.GetType(typeName);
				if (type != null)
				{
					result = type;
				}
				else
				{
					if (assemblyName == null)
					{
						string message = "Could not load type " + typeName + ". Possible cause: no assembly name specified.";
						throw new System.TypeLoadException(message);
					}
					System.Reflection.Assembly assembly = System.Reflection.Assembly.Load(assemblyName);
					if (assembly == null)
					{
						throw new System.InvalidOperationException("Can't find assembly: " + assemblyName);
					}
					type = assembly.GetType(typeName);
					if (type == null)
					{
						result = null;
					}
					else
					{
						result = type;
					}
				}
			}
			catch (System.Exception)
			{
				result = null;
			}
			return result;
		}
	}
}
