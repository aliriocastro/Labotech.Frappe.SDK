using Labotech.Frappe.Connector.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Labotech.Frappe.Data
{
    public interface IFrappeRepository<TEntity>
        where TEntity : class, IFrappeBaseEntity, new()
    {
        /// <summary>
        /// Get entity entry by its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<TEntity> GetByIdAsync(string name);

         /// <summary>
        /// Get entity entry by its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        TEntity GetById(string name);

        /// <summary>
        /// Get entity entries by its names
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetByIdsAsync(IEnumerable<string> name);

        /// <summary>
        /// Gets all entity entries
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> query);

        /// <summary>
        /// Insert entity entry
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<TEntity> InsertAsync(TEntity entity);

        /// <summary>
        /// Insert entity entries
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task InsertAsync(IEnumerable<TEntity> entities);

        /// <summary>
        /// Update entity entry
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task UpdateAsync<TEntityModel>(TEntityModel entity) where TEntityModel : IFrappeBaseEntity;

        /// <summary>
        /// Update entity entries 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task UpdateAsync<TEntityModel>(IEnumerable<TEntityModel> entities) where TEntityModel : IFrappeBaseEntity;

        /// <summary>
        /// Delete the entity entry
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task DeleteAsync<TEntityModel>(TEntityModel entity) where TEntityModel : IFrappeBaseEntity;

        /// <summary>
        /// Delete the entity entry by its name
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task DeleteByNameAsync(string name);

        /// <summary>
        /// Delete entity entries
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task DeleteAsync<TEntityModel>(IEnumerable<TEntityModel> entities) where TEntityModel : IFrappeBaseEntity;
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task InsertOnDatabaseAsync(TEntity entity);
        Task InsertManyOnDatabaseAsync(IEnumerable<TEntity> entities);
        Task<IEnumerable<TResult>> GetAllAsync<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> func);

        IQueryable<TEntity> Table { get; }

    }
}
