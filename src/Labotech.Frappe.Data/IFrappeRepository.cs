using Labotech.Frappe.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Labotech.Frappe.Data
{
    public interface IFrappeRepository<TEntity>
        where TEntity : class, IFrappeBaseEntity, new()
    {
        /// <summary>
        /// Get entity entry by its name
        /// </summary>
        Task<TEntity> GetByIdAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get entity entry by its name (synchronous)
        /// </summary>
        TEntity GetById(string name);

        /// <summary>
        /// Get entity entries by their names
        /// </summary>
        Task<IEnumerable<TEntity>> GetByIdsAsync(IEnumerable<string> names, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all entity entries with an optional query shaper
        /// </summary>
        Task<IEnumerable<TEntity>> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all entity entries
        /// </summary>
        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Projects all entity entries through the supplied query.
        /// </summary>
        Task<IEnumerable<TResult>> GetAllAsync<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> func, CancellationToken cancellationToken = default);

        /// <summary>
        /// Insert entity entry through the Frappe API.
        /// </summary>
        Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Insert entity entries through the Frappe API.
        /// </summary>
        Task InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update entity entry through the Frappe API.
        /// </summary>
        Task UpdateAsync<TEntityModel>(TEntityModel entity, CancellationToken cancellationToken = default) where TEntityModel : IFrappeBaseEntity;

        /// <summary>
        /// Update entity entries through the Frappe API.
        /// </summary>
        [Obsolete("Bulk update is not implemented in IFrappeService yet. Iterate and call UpdateAsync(entity) instead until BulkUpdateAsync ships.", error: false)]
        Task UpdateAsync<TEntityModel>(IEnumerable<TEntityModel> entities, CancellationToken cancellationToken = default) where TEntityModel : IFrappeBaseEntity;

        /// <summary>
        /// Delete the entity entry through the Frappe API.
        /// </summary>
        Task DeleteAsync<TEntityModel>(TEntityModel entity, CancellationToken cancellationToken = default) where TEntityModel : IFrappeBaseEntity;

        /// <summary>
        /// Delete the entity entry by its name through the Frappe API.
        /// </summary>
        Task DeleteByNameAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete entity entries through the Frappe API.
        /// </summary>
        Task DeleteAsync<TEntityModel>(IEnumerable<TEntityModel> entities, CancellationToken cancellationToken = default) where TEntityModel : IFrappeBaseEntity;

        IQueryable<TEntity> Table { get; }

    }

    /// <summary>
    /// Direct database access for an entity. Bypasses the Frappe HTTP API entirely.
    /// <para>
    /// <b>Warning:</b> implementations write straight to the underlying database and therefore
    /// skip Frappe validation, document hooks, permissions, and audit trail. Use only for trusted,
    /// bulk-import scenarios where the caller accepts responsibility for data integrity.
    /// </para>
    /// </summary>
    public interface IFrappeDirectDbRepository<TEntity>
        where TEntity : class, IFrappeBaseEntity, new()
    {
        /// <summary>
        /// Insert a single row directly into the database, bypassing Frappe.
        /// </summary>
        Task InsertOnDatabaseAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulk-copy entities directly into the database, bypassing Frappe.
        /// </summary>
        Task InsertManyOnDatabaseAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    }
}
