using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Labotech.Frappe.Connector.Contracts;
using Labotech.Frappe.Connector.Core;
using Labotech.Frappe.Connector.Extensions;
using Labotech.Frappe.Core;
using Labotech.Frappe.Core.Common;

namespace Labotech.Frappe.Connector.Services
{
    public sealed partial class FrappeService
        : IFrappeService
    {
        private readonly IFrappeHttpClient _frappeClient;

        public FrappeService(IFrappeHttpClient frappeClient)
        {
            _frappeClient = frappeClient ?? throw new ArgumentNullException(nameof(frappeClient));
        }

        public async Task<IEnumerable<TEntity>> GetListAsync<TEntity>(
            string docType,
            string fields = "[\"*\"]",
            string filters = null,
            string parent = null,
            string orderBy = null,
            int limitStart = 0,
            int limitPageLength = 20,
            CancellationToken cancellationToken = default) where TEntity : IFrappeBaseEntity
        {
            const string getListMethodName = "frappe.client.get_list";

            var response = await _frappeClient.MethodGetRequestAsync(getListMethodName, cancellationToken,
                ("doctype", docType),
                ("fields", fields),
                ("filters", filters),
                ("parent", parent),
                ("order_by", orderBy),
                ("limit_start", limitStart.ToString()),
                ("limit_page_length", limitPageLength.ToString()));

            await response.EnsureERPNextSuccessStatusCodeAsync(cancellationToken);

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return FrappeEntityExtensions.FromJson<IEnumerable<TEntity>>(doc.RootElement.GetProperty("message").ToString());
        }

        public async Task<int> GetCountAsync(string docType, string filters = null, CancellationToken cancellationToken = default)
        {
            const string getCountMethodName = "frappe.client.get_count";

            var response = await _frappeClient.MethodGetRequestAsync(getCountMethodName, cancellationToken,
                ("doctype", docType),
                ("filters", filters));

            await response.EnsureERPNextSuccessStatusCodeAsync(cancellationToken);

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return doc.RootElement.GetProperty("message").GetInt32();
        }

        [Obsolete("Not implemented yet — will be added in a future release. Do not call.", error: false)]
        public Task<T> GetDocByFilterAsync<T>(string docType, string filters, string parent = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Not implemented yet — will be added in a future release. Do not call.", error: false)]
        public Task<T> GetDocByNameAsync<T>(string docType, string docName, string parent = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Not implemented yet — will be added in a future release. Do not call.", error: false)]
        public Task<TField> GetFieldValueAsync<TField>(string docType, string fieldName, string filters, string parent = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Not implemented yet — will be added in a future release. Do not call.", error: false)]
        public Task<T> GetSingleValueAsync<T>(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<string> RenameDocAsync(string docType, string oldName, string newName, bool merge = false, CancellationToken cancellationToken = default)
        {
            const string renameDocMethodName = "frappe.client.rename_doc";

            using var content = new MultipartFormDataContent
            {
                { new StringContent(docType), "doctype" },
                { new StringContent(oldName), "old_name" },
                { new StringContent(newName), "new_name" }
            };
            if (merge) content.Add(new StringContent("true"), "merge");

            var response = await _frappeClient.MethodPostRequestAsync(renameDocMethodName, content, cancellationToken);
            await response.EnsureERPNextSuccessStatusCodeAsync(cancellationToken);

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return doc.RootElement.GetProperty("message").GetString();
        }

        public async Task<TEntity> SaveDocAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : IFrappeBaseEntity
        {
            const string updateMethodName = "frappe.client.save";

            using var content = new MultipartFormDataContent
            {
                { new StringContent(JsonSerializer.Serialize(entity, ERPNextJsonSerializationSettings.Settings)), "doc" }
            };

            var response = await _frappeClient.MethodPostRequestAsync(updateMethodName, content, cancellationToken);
            await response.EnsureERPNextSuccessStatusCodeAsync(cancellationToken);

            return (TEntity)FrappeEntityExtensions.FromJson<TEntity>(await response.Content.ReadAsStringAsync());
        }

        [Obsolete("Not implemented yet — will be added in a future release. Do not call.", error: false)]
        public Task DeleteDocAsync(string docType, string docName, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Not implemented yet — will be added in a future release. Do not call.", error: false)]
        public Task<bool> HasPermissionAsync(string docType, string docName, FrappeDocPermission permType, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<TEntity> InsertAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : IFrappeBaseEntity
        {
            const string insertMethodName = "frappe.client.insert";

            EnsureEntitiesHaveDoctype(entity);

            using var content = new MultipartFormDataContent
            {
                { new StringContent(JsonSerializer.Serialize(entity, ERPNextJsonSerializationSettings.Settings)), "doc" }
            };

            var response = await _frappeClient.MethodPostRequestAsync(insertMethodName, content, cancellationToken);
            await response.EnsureERPNextSuccessStatusCodeAsync(cancellationToken);

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return FrappeEntityExtensions.FromJson<TEntity>(doc.RootElement.GetProperty("message").ToString());
        }

        /// <summary>
        /// Send a POST request to <i>frappe.client.insert_many</i> for batch insertion.
        /// </summary>
        /// <typeparam name="T">Any type of ERPNext DocType Entity.</typeparam>
        /// <param name="entities">Entities to be inserted. ERPNext may limit the amount of rows inserted at once.</param>
        /// <returns>IDs of entities inserted.</returns>
        public async Task<IEnumerable<string>> InsertManyAsync<T>(IEnumerable<T> entities, CancellationToken cancellationToken = default) where T : IFrappeBaseEntity
        {
            const string insertManyMethodName = "frappe.client.insert_many";

            var materialized = entities as IReadOnlyCollection<T> ?? entities.ToList();
            EnsureEntitiesHaveDoctype(materialized);

            using var content = new MultipartFormDataContent
            {
                { new StringContent(JsonSerializer.Serialize(materialized, ERPNextJsonSerializationSettings.Settings)), "docs" }
            };

            var response = await _frappeClient.MethodPostRequestAsync(insertManyMethodName, content, cancellationToken);
            await response.EnsureERPNextSuccessStatusCodeAsync(cancellationToken);

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return FrappeEntityExtensions.FromJson<IEnumerable<string>>(doc.RootElement.GetProperty("message").ToString());
        }

        /// <summary>
        /// Send a POST request to <i>frappe.client.insert_many</i> for batch insertion.
        /// </summary>
        [Obsolete("Use InsertManyAsync instead. This overload is not implemented.", error: false)]
        public Task<IEnumerable<string>> DeferredInsertManyAsync<T>(List<T> entities) where T : IFrappeEntity
        {
            throw new NotImplementedException();
        }

        private static void EnsureEntitiesHaveDoctype<TEntity>(IEnumerable<TEntity> entities) where TEntity : IFrappeBaseEntity
        {
            if (entities.Any(e => string.IsNullOrEmpty(e.Doctype)))
                throw new ArgumentException(
                    "ERPNext requires that all entities have the property 'doctype' set when inserting in batch.");
        }

        private static void EnsureEntitiesHaveDoctype<TEntity>(TEntity entity) where TEntity : IFrappeBaseEntity
        {
            if (string.IsNullOrEmpty(entity.Doctype))
                throw new ArgumentException(
                    "ERPNext requires that the entity has the property 'doctype' set.");
        }

        public Task<TEntity> UpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : IFrappeBaseEntity
        {
            return UpdateAsync(entity.Doctype, entity.Name, entity, cancellationToken);
        }

        [Obsolete("Not implemented yet — will be added in a future release. Do not call.", error: false)]
        public Task<IDictionary<dynamic, string>> BulkUpdateAsync<TEntity>(IEnumerable<TEntity> entitiesToUpdate, CancellationToken cancellationToken = default) where TEntity : IFrappeBaseEntity
        {
            throw new NotImplementedException();
        }

        public async Task DeferredInsertAsync<TEntity>(IEnumerable<TEntity> entitiesToInsert, string docType = null, CancellationToken cancellationToken = default) where TEntity : IFrappeBaseEntity
        {
            const string deferredInsertMethod = "frappe.deferred_insert.deferred_insert";

            var materialized = entitiesToInsert as IReadOnlyCollection<TEntity> ?? entitiesToInsert.ToList();

            if (string.IsNullOrEmpty(docType))
                EnsureEntitiesHaveDoctype(materialized);

            using var content = new MultipartFormDataContent
            {
                { new StringContent(docType ?? string.Empty), "doctype" },
                { new StringContent(JsonSerializer.Serialize(materialized, ERPNextJsonSerializationSettings.Settings)), "records" }
            };

            var response = await _frappeClient.MethodPostRequestAsync(deferredInsertMethod, content, cancellationToken);
            await response.EnsureERPNextSuccessStatusCodeAsync(cancellationToken);
        }

        public async Task<TEntity> UpdateAsync<TEntity>(string docType, string docName, TEntity entity, CancellationToken cancellationToken = default)
        {
            var response = await _frappeClient.PutAsJsonResourceRequestAsync(docType, docName, entity, cancellationToken);
            await response.EnsureERPNextSuccessStatusCodeAsync(cancellationToken);

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return FrappeEntityExtensions.FromJson<TEntity>(doc.RootElement.GetProperty("data").ToString());
        }

        [Obsolete("Use BulkUpdateAsync instead. This overload is not implemented.", error: false)]
        public Task<HttpResponseMessage> BulkUpdateRequestAsync<T>(IEnumerable<T> entitiesToUpdate)
        {
            throw new NotImplementedException();
        }

        public FrappeQueryFluent<T> Query<T>(string docType) where T : IFrappeEntity
        {
            return new FrappeQueryFluent<T>(docType, this);
        }

        public FrappeQueryFluent<T> Query<T>() where T : IFrappeEntity, new()
        {
            var instance = new T();
            if (string.IsNullOrEmpty(instance.Doctype))
                throw new ArgumentException("DocType value cannot be null.");

            return new FrappeQueryFluent<T>(instance.Doctype, this);
        }

        public async Task<string> GetResourceAsRawAsync(string docType, string docName, CancellationToken cancellationToken = default)
        {
            var response = await _frappeClient.GetResourceRequestAsync(docType, docName, cancellationToken);
            await response.EnsureERPNextSuccessStatusCodeAsync(cancellationToken);

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return doc.RootElement.GetProperty("data").ToString();
        }

    }
}
