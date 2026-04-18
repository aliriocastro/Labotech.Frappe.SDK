using System.Collections.Generic;
using System.Threading.Tasks;
using Labotech.Frappe.Connector.Core;

namespace Labotech.Frappe.Connector.Contracts
{
    public interface IFrappeService
    {

        Task<IEnumerable<TEntity>> GetListAsync<TEntity>(
            string docType,
            string fields,
            string filters,
            string parent,
            string orderBy,
            int limitStart,
            int limitPageLength) where TEntity : IFrappeBaseEntity;

        Task<int> GetCountAsync(
            string docType,
            string filters);

        Task<TEntity> GetDocByFilterAsync<TEntity>(
            string docType,
            string filters,
            string parent);

        Task<TEntity> GetDocByNameAsync<TEntity>(
            string docType,
            string docName,
            string parent);

        Task<TField> GetFieldValueAsync<TField>(
            string docType,
            string fieldName,
            string filters,
            string parent);

        Task<TEntity> GetResourceAsync<TEntity>(
            string docType,
            string docName) where TEntity : IFrappeBaseEntity;

        Task<string> GetResourceAsRawAsync(
            string docType,
            string docName);

        Task<IEnumerable<TEntity>> GetResourcesAsync<TEntity>(
            string docType,
            string fields,
            string filters,
            string parent,
            string orderBy,
            int limitStart,
            int limitPageLength) where TEntity : IFrappeBaseEntity;

        Task<TEntity> PostResourceAsync<TEntity>(
            TEntity content) where TEntity : IFrappeBaseEntity;

        Task<T> PostResourceAsync<T>(T entity, string docType);

        Task DeleteResourceAsync(
            string docType,
            string docName);

        Task DeleteResourceAsync<TEntity>(
            TEntity entity) where TEntity : IFrappeBaseEntity;

        Task<TEntity> PutResourceAsync<TEntity>(
            TEntity content) where TEntity : IFrappeBaseEntity;

        Task<TEntity> PutResourceAsync<TEntity>(TEntity entity, string docType, string name);

        Task<TResult> GetSingleValueAsync<TResult>();

        Task<string> RenameDocAsync(
            string docType,
            string oldName,
            string newName,
            bool merge);

        Task<TEntity> SaveDocAsync<TEntity>(
            TEntity entity) where TEntity : IFrappeBaseEntity;

        Task DeleteDocAsync(
            string docType,
            string docName);

        Task<bool> HasPermissionAsync(
            string docType,
            string docName,
            FrappeDocPermission permType);

        Task<TEntity> InsertAsync<TEntity>(
            TEntity entity) where TEntity : IFrappeBaseEntity;

        Task<IEnumerable<string>> InsertManyAsync<TEntity>(IEnumerable<TEntity> entitiesToInsert) where TEntity : IFrappeBaseEntity;

        Task<TEntity> UpdateAsync<TEntity>(
            string docType,
            string docName,
            TEntity entity);

        Task<TEntity> UpdateAsync<TEntity>(
            TEntity entity) where TEntity : IFrappeBaseEntity;

        Task<IDictionary<dynamic, string>> BulkUpdateAsync<TEntity>(IEnumerable<TEntity> entitiesToUpdate) where TEntity : IFrappeBaseEntity;

        Task DeferredInsertAsync<TEntity>(IEnumerable<TEntity> entitiesToInsert, string docType) where TEntity : IFrappeBaseEntity;
    }
}