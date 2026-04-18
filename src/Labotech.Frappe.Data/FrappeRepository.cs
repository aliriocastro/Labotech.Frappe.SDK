using Labotech.Frappe.Connector.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using Labotech.Frappe.Connector.Contracts;

namespace Labotech.Frappe.Data
{
    public class FrappeRepository<TEntity>
        : IFrappeRepository<TEntity> where TEntity : class, IFrappeBaseEntity, new()
    {

        private readonly IDataContext _dataContext;
        private readonly IFrappeService? _frappeService;

        public string Doctype => new TEntity().Doctype;

        public FrappeRepository(IDataContext dataContext, IFrappeService frappeService)
        {
            _dataContext = dataContext;
            _frappeService = frappeService;
        }

        public FrappeRepository(IDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        #region Queries

        public async Task<IEnumerable<TEntity>> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> func = null)
        {
            async Task<IEnumerable<TEntity>> getAllAsync()
            {
                var query = Table;
                query = (func != null) ? func(query) : query;

                return await query.ToListAsync();
            }

            return await getAllAsync();

        }

        public async Task<IEnumerable<TResult>> GetAllAsync<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> func)
        {
            async Task<IEnumerable<TResult>> getAllAsync()
            {
                return await func(Table).ToListAsync();
            }

            return await getAllAsync();

        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await Table.ToListAsync();
        }

        public async Task<TEntity> GetByIdAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            // get entity
            var entity = await Table.FirstOrDefaultAsync(e => e.Name == name);

            return entity;
        }

        public TEntity GetById(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var entity = Table.FirstOrDefault(e => e.Name == name);

            return entity;
        }

        public async Task<IEnumerable<TEntity>> GetByIdsAsync(IEnumerable<string> names)
        {
            if (!names?.Any() ?? true)
                return await Task.FromResult(Enumerable.Empty<TEntity>());

            // get entities
            var entities = await Table.Where(c => names.Contains(c.Name)).ToListAsync();

            return entities;

        }

        #endregion


        #region Commands

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            return await _frappeService.InsertAsync(entity);
        }

        public async Task InsertAsync(IEnumerable<TEntity> entities)
        {
            _ = await _frappeService.InsertManyAsync(entities);
        }

        public async Task UpdateAsync<TEntityModel>(TEntityModel entity) where TEntityModel : IFrappeBaseEntity
        {
            await _frappeService.UpdateAsync(entity);
        }

        public async Task UpdateAsync<TEntityModel>(IEnumerable<TEntityModel> entities) where TEntityModel : IFrappeBaseEntity
        {
            await _frappeService.BulkUpdateAsync(entities);
        }

        public async Task DeleteAsync<TEntityModel>(TEntityModel entity) where TEntityModel : IFrappeBaseEntity
        {
            await _frappeService.DeleteResourceAsync(entity);
        }

        public async Task DeleteByNameAsync(string name)
        {
            await DeleteAsync(new TEntity() { Name = name });
        }

        public async Task DeleteAsync<TEntityModel>(IEnumerable<TEntityModel> entities) where TEntityModel : IFrappeBaseEntity
        {
            foreach (var entity in entities)
            {
                await DeleteAsync(entity);
            }
        }

        #endregion


        #region Database Mutation

        public async Task InsertOnDatabaseAsync(TEntity entity)
        {
            _ = await GetClonedDataContext().InsertAsync(entity);

        }

        public async Task InsertManyOnDatabaseAsync(IEnumerable<TEntity> entities)
        {
            _ = await GetClonedDataContext().GetTable<TEntity>().BulkCopyAsync(entities);
        }

        public async Task RenameAsync(TEntity entity, string newName, bool merge = false)
        {
            await _frappeService.RenameDocAsync(entity.Doctype, entity.Name, newName, merge);
        }

        #endregion

        private IDataContext GetClonedDataContext()
        {
            return _dataContext; //_dataContext.Clone(true);
        }

        #region Properties

        public virtual IQueryable<TEntity> Table => GetClonedDataContext().GetTable<TEntity>();

        #endregion

    }
}
