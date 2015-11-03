using System;
using System.Collections.Generic;
namespace Tunynet.Repositories
{
	public interface IRepository<TEntity> where TEntity : class, IEntity
	{
		object Insert(TEntity entity);
		void Update(TEntity entity);
		int DeleteByEntityId(object primaryKey);
		int Delete(TEntity entity);
		bool Exists(object primaryKey);
		TEntity Get(object primaryKey);
		System.Collections.Generic.IEnumerable<TEntity> GetAll();
		System.Collections.Generic.IEnumerable<TEntity> GetAll(string orderBy);
		System.Collections.Generic.IEnumerable<TEntity> PopulateEntitiesByEntityIds<T>(System.Collections.Generic.IEnumerable<T> entityIds);
	}
}
