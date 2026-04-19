using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Labotech.Frappe.Connector.Core;
using Labotech.Frappe.Core;

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
            int limitPageLength,
            CancellationToken cancellationToken = default) where TEntity : IFrappeBaseEntity;

        Task<int> GetCountAsync(
            string docType,
            string filters,
            CancellationToken cancellationToken = default);

        [Obsolete("Not implemented yet — will be added in a future release. Do not call.", error: false)]
        Task<TEntity> GetDocByFilterAsync<TEntity>(
            string docType,
            string filters,
            string parent,
            CancellationToken cancellationToken = default);

        [Obsolete("Not implemented yet — will be added in a future release. Do not call.", error: false)]
        Task<TEntity> GetDocByNameAsync<TEntity>(
            string docType,
            string docName,
            string parent,
            CancellationToken cancellationToken = default);

        [Obsolete("Not implemented yet — will be added in a future release. Do not call.", error: false)]
        Task<TField> GetFieldValueAsync<TField>(
            string docType,
            string fieldName,
            string filters,
            string parent,
            CancellationToken cancellationToken = default);

        Task<TEntity> GetResourceAsync<TEntity>(
            string docType,
            string docName,
            CancellationToken cancellationToken = default) where TEntity : IFrappeBaseEntity;

        Task<string> GetResourceAsRawAsync(
            string docType,
            string docName,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<TEntity>> GetResourcesAsync<TEntity>(
            string docType,
            string fields,
            string filters,
            string parent,
            string orderBy,
            int limitStart,
            int limitPageLength,
            CancellationToken cancellationToken = default) where TEntity : IFrappeBaseEntity;

        Task<TEntity> PostResourceAsync<TEntity>(
            TEntity content,
            CancellationToken cancellationToken = default) where TEntity : IFrappeBaseEntity;

        Task<T> PostResourceAsync<T>(T entity, string docType, CancellationToken cancellationToken = default);

        Task DeleteResourceAsync(
            string docType,
            string docName,
            CancellationToken cancellationToken = default);

        Task DeleteResourceAsync<TEntity>(
            TEntity entity,
            CancellationToken cancellationToken = default) where TEntity : IFrappeBaseEntity;

        Task<TEntity> PutResourceAsync<TEntity>(
            TEntity content,
            CancellationToken cancellationToken = default) where TEntity : IFrappeBaseEntity;

        Task<TEntity> PutResourceAsync<TEntity>(TEntity entity, string docType, string name, CancellationToken cancellationToken = default);

        [Obsolete("Not implemented yet — will be added in a future release. Do not call.", error: false)]
        Task<TResult> GetSingleValueAsync<TResult>(CancellationToken cancellationToken = default);

        Task<string> RenameDocAsync(
            string docType,
            string oldName,
            string newName,
            bool merge,
            CancellationToken cancellationToken = default);

        Task<TEntity> SaveDocAsync<TEntity>(
            TEntity entity,
            CancellationToken cancellationToken = default) where TEntity : IFrappeBaseEntity;

        [Obsolete("Not implemented yet — will be added in a future release. Do not call.", error: false)]
        Task DeleteDocAsync(
            string docType,
            string docName,
            CancellationToken cancellationToken = default);

        [Obsolete("Not implemented yet — will be added in a future release. Do not call.", error: false)]
        Task<bool> HasPermissionAsync(
            string docType,
            string docName,
            FrappeDocPermission permType,
            CancellationToken cancellationToken = default);

        Task<TEntity> InsertAsync<TEntity>(
            TEntity entity,
            CancellationToken cancellationToken = default) where TEntity : IFrappeBaseEntity;

        Task<IEnumerable<string>> InsertManyAsync<TEntity>(IEnumerable<TEntity> entitiesToInsert, CancellationToken cancellationToken = default) where TEntity : IFrappeBaseEntity;

        Task<TEntity> UpdateAsync<TEntity>(
            string docType,
            string docName,
            TEntity entity,
            CancellationToken cancellationToken = default);

        Task<TEntity> UpdateAsync<TEntity>(
            TEntity entity,
            CancellationToken cancellationToken = default) where TEntity : IFrappeBaseEntity;

        [Obsolete("Not implemented yet — will be added in a future release. Do not call.", error: false)]
        Task<IDictionary<dynamic, string>> BulkUpdateAsync<TEntity>(IEnumerable<TEntity> entitiesToUpdate, CancellationToken cancellationToken = default) where TEntity : IFrappeBaseEntity;

        Task DeferredInsertAsync<TEntity>(IEnumerable<TEntity> entitiesToInsert, string docType, CancellationToken cancellationToken = default) where TEntity : IFrappeBaseEntity;
    }
}
