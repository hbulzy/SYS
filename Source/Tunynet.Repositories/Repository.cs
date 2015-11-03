using PetaPoco;
using PetaPoco.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using Tunynet.Caching;
namespace Tunynet.Repositories
{
	public class Repository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
	{
		public ICacheService cacheService = DIContainer.Resolve<ICacheService>();
		private Database database;
		private int cacheablePageCount = 30;
		private int primaryMaxRecords = 50000;
		private int secondaryMaxRecords = 1000;
		protected static RealTimeCacheHelper RealTimeCacheHelper
		{
			get
			{
				return EntityData.ForType(typeof(TEntity)).RealTimeCacheHelper;
			}
		}
		protected virtual int CacheablePageCount
		{
			get
			{
				return this.cacheablePageCount;
			}
		}
		protected virtual int PrimaryMaxRecords
		{
			get
			{
				return this.primaryMaxRecords;
			}
		}
		protected virtual int SecondaryMaxRecords
		{
			get
			{
				return this.secondaryMaxRecords;
			}
		}
		protected virtual Database CreateDAO()
		{
			if (this.database == null)
			{
				this.database = Database.CreateInstance(null);
			}
			return this.database;
		}
		public virtual object Insert(TEntity entity)
		{
			if (entity is ISerializableProperties)
			{
				ISerializableProperties serializableProperties = entity as ISerializableProperties;
				if (serializableProperties != null)
				{
					serializableProperties.Serialize();
				}
			}
			object result = this.CreateDAO().Insert(entity);
			this.OnInserted(entity);
			return result;
		}
		public virtual void Update(TEntity entity)
		{
			Database database = this.CreateDAO();
			if (entity is ISerializableProperties)
			{
				ISerializableProperties serializableProperties = entity as ISerializableProperties;
				if (serializableProperties != null)
				{
					serializableProperties.Serialize();
				}
			}
			int num;
			if (Repository<TEntity>.RealTimeCacheHelper.PropertyNameOfBody != null && Repository<TEntity>.RealTimeCacheHelper.PropertyNameOfBody.GetValue(entity, null) == null)
			{
				PocoData pocoData = PocoData.ForType(typeof(TEntity));
				System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
				foreach (System.Collections.Generic.KeyValuePair<string, PocoColumn> current in pocoData.Columns)
				{
					if (string.Compare(current.Key, pocoData.TableInfo.PrimaryKey, true) != 0 && (SqlBehaviorFlags.Update & current.Value.SqlBehavior) != (SqlBehaviorFlags)0 && string.Compare(current.Key, Repository<TEntity>.RealTimeCacheHelper.PropertyNameOfBody.Name, true) != 0 && !current.Value.ResultColumn)
					{
						list.Add(current.Key);
					}
				}
				num = database.Update(entity, list);
			}
			else
			{
				num = database.Update(entity);
			}
			if (num > 0)
			{
				this.OnUpdated(entity);
			}
		}
		public virtual int DeleteByEntityId(object entityId)
		{
			TEntity tEntity = this.Get(entityId);
			if (tEntity == null)
			{
				return 0;
			}
			return this.Delete(tEntity);
		}
		public virtual int Delete(TEntity entity)
		{
			if (entity == null)
			{
				return 0;
			}
			int num = this.CreateDAO().Delete(entity);
			if (num > 0)
			{
				this.OnDeleted(entity);
			}
			return num;
		}
		public bool Exists(object entityId)
		{
			return this.CreateDAO().Exists<TEntity>(entityId);
		}
		public virtual TEntity Get(object entityId)
		{
			TEntity tEntity = default(TEntity);
			if (Repository<TEntity>.RealTimeCacheHelper.EnableCache)
			{
				tEntity = this.cacheService.Get<TEntity>(Repository<TEntity>.RealTimeCacheHelper.GetCacheKeyOfEntity(entityId));
			}
			if (tEntity == null)
			{
				tEntity = this.CreateDAO().SingleOrDefault<TEntity>(entityId);
				if (Repository<TEntity>.RealTimeCacheHelper.EnableCache && tEntity != null)
				{
					if (Repository<TEntity>.RealTimeCacheHelper.PropertyNameOfBody != null)
					{
						Repository<TEntity>.RealTimeCacheHelper.PropertyNameOfBody.SetValue(tEntity, null, null);
					}
					this.cacheService.Add(Repository<TEntity>.RealTimeCacheHelper.GetCacheKeyOfEntity(tEntity.EntityId), tEntity, Repository<TEntity>.RealTimeCacheHelper.CachingExpirationType);
				}
			}
			if (tEntity == null || tEntity.IsDeletedInDatabase)
			{
				return default(TEntity);
			}
			return tEntity;
		}
		public System.Collections.Generic.IEnumerable<TEntity> GetAll()
		{
			return this.GetAll(null);
		}
		public System.Collections.Generic.IEnumerable<TEntity> GetAll(string orderBy)
		{
			System.Collections.Generic.IEnumerable<object> enumerable = null;
			string text = null;
			if (Repository<TEntity>.RealTimeCacheHelper.EnableCache)
			{
				text = Repository<TEntity>.RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.GlobalVersion);
				if (!string.IsNullOrEmpty(orderBy))
				{
					text = text + "SB-" + orderBy;
				}
				enumerable = this.cacheService.Get<System.Collections.Generic.IEnumerable<object>>(text);
			}
			if (enumerable == null)
			{
				PocoData pocoData = PocoData.ForType(typeof(TEntity));
				Sql sql = Sql.Builder.Select(new object[]
				{
					pocoData.TableInfo.PrimaryKey
				}).From(new object[]
				{
					pocoData.TableInfo.TableName
				});
				if (!string.IsNullOrEmpty(orderBy))
				{
					sql.OrderBy(new object[]
					{
						orderBy
					});
				}
				enumerable = this.CreateDAO().FetchFirstColumn(sql);
				if (Repository<TEntity>.RealTimeCacheHelper.EnableCache)
				{
					this.cacheService.Add(text, enumerable, Repository<TEntity>.RealTimeCacheHelper.CachingExpirationType);
				}
			}
			return this.PopulateEntitiesByEntityIds<object>(enumerable);
		}
		protected virtual PagingDataSet<TEntity> GetPagingEntities(int pageSize, int pageIndex, Sql sql)
		{
			PagingEntityIdCollection pagingEntityIdCollection = this.CreateDAO().FetchPagingPrimaryKeys<TEntity>((long)this.PrimaryMaxRecords, pageSize, pageIndex, sql);
			System.Collections.Generic.IEnumerable<TEntity> entities = this.PopulateEntitiesByEntityIds<object>(pagingEntityIdCollection.GetPagingEntityIds(pageSize, pageIndex));
			return new PagingDataSet<TEntity>(entities)
			{
				PageIndex = pageIndex,
				PageSize = pageSize,
				TotalRecords = pagingEntityIdCollection.TotalRecords
			};
		}
		protected virtual PagingDataSet<TEntity> GetPagingEntities(int pageSize, int pageIndex, CachingExpirationType cachingExpirationTypes, Func<string> getCacheKey, Func<Sql> generateSql)
		{
			PagingEntityIdCollection pagingEntityIdCollection;
			if (pageIndex < this.CacheablePageCount && pageSize <= this.SecondaryMaxRecords)
			{
				string cacheKey = getCacheKey();
				pagingEntityIdCollection = this.cacheService.Get<PagingEntityIdCollection>(cacheKey);
				if (pagingEntityIdCollection == null)
				{
					pagingEntityIdCollection = this.CreateDAO().FetchPagingPrimaryKeys<TEntity>((long)this.PrimaryMaxRecords, pageSize * this.CacheablePageCount, 1, generateSql());
					pagingEntityIdCollection.IsContainsMultiplePages = true;
					this.cacheService.Add(cacheKey, pagingEntityIdCollection, cachingExpirationTypes);
				}
			}
			else
			{
				pagingEntityIdCollection = this.CreateDAO().FetchPagingPrimaryKeys<TEntity>((long)this.PrimaryMaxRecords, pageSize, pageIndex, generateSql());
			}
			System.Collections.Generic.IEnumerable<TEntity> entities = this.PopulateEntitiesByEntityIds<object>(pagingEntityIdCollection.GetPagingEntityIds(pageSize, pageIndex));
			return new PagingDataSet<TEntity>(entities)
			{
				PageIndex = pageIndex,
				PageSize = pageSize,
				TotalRecords = pagingEntityIdCollection.TotalRecords
			};
		}
		protected virtual System.Collections.Generic.IEnumerable<TEntity> GetTopEntities(int topNumber, CachingExpirationType cachingExpirationTypes, Func<string> getCacheKey, Func<Sql> generateSql)
		{
			string cacheKey = getCacheKey();
			PagingEntityIdCollection pagingEntityIdCollection = this.cacheService.Get<PagingEntityIdCollection>(cacheKey);
			if (pagingEntityIdCollection == null)
			{
				System.Collections.Generic.IEnumerable<object> entityIds = this.CreateDAO().FetchTopPrimaryKeys<TEntity>(this.SecondaryMaxRecords, generateSql());
				pagingEntityIdCollection = new PagingEntityIdCollection(entityIds);
				this.cacheService.Add(cacheKey, pagingEntityIdCollection, cachingExpirationTypes);
			}
			System.Collections.Generic.IEnumerable<object> topEntityIds = pagingEntityIdCollection.GetTopEntityIds(topNumber);
			return this.PopulateEntitiesByEntityIds<object>(topEntityIds);
		}
		public virtual System.Collections.Generic.IEnumerable<TEntity> PopulateEntitiesByEntityIds<T>(System.Collections.Generic.IEnumerable<T> entityIds)
		{
			TEntity[] array = new TEntity[entityIds.Count<T>()];
			System.Collections.Generic.Dictionary<object, int> dictionary = new System.Collections.Generic.Dictionary<object, int>();
			for (int i = 0; i < entityIds.Count<T>(); i++)
			{
				TEntity tEntity = this.cacheService.Get<TEntity>(Repository<TEntity>.RealTimeCacheHelper.GetCacheKeyOfEntity(entityIds.ElementAt(i)));
				if (tEntity != null)
				{
					array[i] = tEntity;
				}
				else
				{
					array[i] = default(TEntity);
					dictionary[entityIds.ElementAt(i)] = i;
				}
			}
			if (dictionary.Count > 0)
			{
				System.Collections.Generic.IEnumerable<TEntity> enumerable = this.CreateDAO().FetchByPrimaryKeys<TEntity>(dictionary.Keys);
				foreach (TEntity current in enumerable)
				{
					array[dictionary[current.EntityId]] = current;
					if (Repository<TEntity>.RealTimeCacheHelper.EnableCache && current != null)
					{
						if (Repository<TEntity>.RealTimeCacheHelper.PropertyNameOfBody != null && Repository<TEntity>.RealTimeCacheHelper.PropertyNameOfBody != null)
						{
							Repository<TEntity>.RealTimeCacheHelper.PropertyNameOfBody.SetValue(current, null, null);
						}
						this.cacheService.Set(Repository<TEntity>.RealTimeCacheHelper.GetCacheKeyOfEntity(current.EntityId), current, Repository<TEntity>.RealTimeCacheHelper.CachingExpirationType);
					}
				}
			}
			System.Collections.Generic.List<TEntity> list = new System.Collections.Generic.List<TEntity>();
			TEntity[] array2 = array;
			for (int j = 0; j < array2.Length; j++)
			{
				TEntity tEntity2 = array2[j];
				if (tEntity2 != null && !tEntity2.IsDeletedInDatabase)
				{
					list.Add(tEntity2);
				}
			}
			return list;
		}
		protected virtual void OnInserted(TEntity entity)
		{
			if (Repository<TEntity>.RealTimeCacheHelper.EnableCache)
			{
				Repository<TEntity>.RealTimeCacheHelper.IncreaseListCacheVersion(entity);
				if (Repository<TEntity>.RealTimeCacheHelper.PropertyNameOfBody != null)
				{
					string value = Repository<TEntity>.RealTimeCacheHelper.PropertyNameOfBody.GetValue(entity, null) as string;
					this.cacheService.Add(Repository<TEntity>.RealTimeCacheHelper.GetCacheKeyOfEntityBody(entity.EntityId), value, Repository<TEntity>.RealTimeCacheHelper.CachingExpirationType);
					Repository<TEntity>.RealTimeCacheHelper.PropertyNameOfBody.SetValue(entity, null, null);
				}
				this.cacheService.Add(Repository<TEntity>.RealTimeCacheHelper.GetCacheKeyOfEntity(entity.EntityId), entity, Repository<TEntity>.RealTimeCacheHelper.CachingExpirationType);
			}
		}
		protected virtual void OnUpdated(TEntity entity)
		{
			if (Repository<TEntity>.RealTimeCacheHelper.EnableCache)
			{
				Repository<TEntity>.RealTimeCacheHelper.IncreaseEntityCacheVersion(entity.EntityId);
				Repository<TEntity>.RealTimeCacheHelper.IncreaseListCacheVersion(entity);
				if (Repository<TEntity>.RealTimeCacheHelper.PropertyNameOfBody != null && Repository<TEntity>.RealTimeCacheHelper.PropertyNameOfBody.GetValue(entity, null) != null)
				{
					string value = Repository<TEntity>.RealTimeCacheHelper.PropertyNameOfBody.GetValue(entity, null) as string;
					this.cacheService.Set(Repository<TEntity>.RealTimeCacheHelper.GetCacheKeyOfEntityBody(entity.EntityId), value, Repository<TEntity>.RealTimeCacheHelper.CachingExpirationType);
					Repository<TEntity>.RealTimeCacheHelper.PropertyNameOfBody.SetValue(entity, null, null);
				}
				this.cacheService.Set(Repository<TEntity>.RealTimeCacheHelper.GetCacheKeyOfEntity(entity.EntityId), entity, Repository<TEntity>.RealTimeCacheHelper.CachingExpirationType);
			}
		}
		protected virtual void OnDeleted(TEntity entity)
		{
			if (Repository<TEntity>.RealTimeCacheHelper.EnableCache)
			{
				Repository<TEntity>.RealTimeCacheHelper.IncreaseEntityCacheVersion(entity.EntityId);
				Repository<TEntity>.RealTimeCacheHelper.IncreaseListCacheVersion(entity);
				this.cacheService.MarkDeletion(Repository<TEntity>.RealTimeCacheHelper.GetCacheKeyOfEntity(entity.EntityId), entity, CachingExpirationType.SingleObject);
			}
		}
	}
}
