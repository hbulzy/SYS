using System;
namespace Tunynet
{
	public interface IEntity
	{
		object EntityId
		{
			get;
		}
		bool IsDeletedInDatabase
		{
			get;
			set;
		}
	}
}
