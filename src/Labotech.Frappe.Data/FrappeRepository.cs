using Labotech.Frappe.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using Labotech.Frappe.Connector.Contracts;

namespace Labotech.Frappe.Data
{
    public sealed class FrappeRepository<TEntity>
        : IFrappeRepository<TEntity>, IFrappeDirectDbRepository<TEntity>
        where TEntity : class, IFrappeBaseEntity, new()
    {

        private readonly IDataContext _dataContext;
        private readonly IFrappeService _frappeService;

        public string Doctype { get; }

        public FrappeRepository(IDataContext dataContext, IFrappeService frappeService)
        {
            _dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            _frappeService = frappeService ?? throw new ArgumentNullException(nameof(frappeService));
            Doctype = new TEntity().Doctype;
        }

        #region Queries

        public async Task<IEnumerable<TEntity>> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> func = null, CancellationToken cancellationToken = default)
        {
            var query = Table;
            query = (func != null) ? func(query) : query;

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<TResult>> GetAllAsync<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> func, CancellationToken cancellationToken = default)
        {
            return await func(Table).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await Table.ToListAsync(cancellationToken);
        }

        public async Task<TEntity> GetByIdAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            return await Table.FirstOrDefaultAsync(e => e.Name == name, cancellationToken);
        }

        public TEntity GetById(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            return Table.FirstOrDefault(e => e.Name == name);
        }

        public async Task<IEnumerable<TEntity>> GetByIdsAsync(IEnumerable<string> names, CancellationToken cancellationToken = default)
        {
            if (!names?.Any() ?? true)
                return Enumerable.Empty<TEntity>();

            return await Table.Where(c => names.Contains(c.Name)).ToListAsync(cancellationToken);
        }

        #endregion


        #region Commands

        public async Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return await _frappeService.InsertAsync(entity, cancellationToken);
        }

        public async Task InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            _ = await _frappeService.InsertManyAsync(entities, cancellationToken);
        }

        public async Task UpdateAsync<TEntityModel>(TEntityModel entity, CancellationToken cancellationToken = default) where TEntityModel : IFrappeBaseEntity
        {
            await _frappeService.UpdateAsync(entity, cancellationToken);
        }

        [Obsolete("Bulk update is not implemented in IFrappeService yet. Iterate and call UpdateAsync(entity) instead until BulkUpdateAsync ships.", error: false)]
        public async Task UpdateAsync<TEntityModel>(IEnumerable<TEntityModel> entities, CancellationToken cancellationToken = default) where TEntityModel : IFrappeBaseEntity
        {
#pragma warning disable CS0618
            await _frappeService.BulkUpdateAsync(entities, cancellationToken);
#pragma warning restore CS0618
        }

        public async Task DeleteAsync<TEntityModel>(TEntityModel entity, CancellationToken cancellationToken = default) where TEntityModel : IFrappeBaseEntity
        {
            await _frappeService.DeleteResourceAsync(entity, cancellationToken);
        }

        public async Task DeleteByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            await DeleteAsync(new TEntity() { Name = name }, cancellationToken);
        }

        public async Task DeleteAsync<TEntityModel>(IEnumerable<TEntityModel> entities, CancellationToken cancellationToken = default) where TEntityModel : IFrappeBaseEntity
        {
            foreach (var entity in entities)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await DeleteAsync(entity, cancellationToken);
            }
        }

        public async Task RenameAsync(TEntity entity, string newName, bool merge = false, CancellationToken cancellationToken = default)
        {
            await _frappeService.RenameDocAsync(entity.Doctype, entity.Name, newName, merge, cancellationToken);
        }

        #endregion


        #region Direct Database Mutation (bypasses Frappe API)

        public async Task InsertOnDatabaseAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            _ = await GetDataContext().InsertAsync(entity, token: cancellationToken);
        }

        public async Task InsertManyOnDatabaseAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            _ = await GetDataContext().GetTable<TEntity>().BulkCopyAsync(entities, cancellationToken);
        }

        #endregion

        private IDataContext GetDataContext() => _dataContext;

        public IQueryable<TEntity> Table => GetDataContext().GetTable<TEntity>();

    }
}
