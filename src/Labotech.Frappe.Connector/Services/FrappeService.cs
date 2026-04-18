using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Labotech.Frappe.Connector.Contracts;
using Labotech.Frappe.Connector.Core;
using Labotech.Frappe.Connector.Core.Common;
using Labotech.Frappe.Connector.Extensions;

namespace Labotech.Frappe.Connector.Services
{
    public partial class FrappeService
        : IFrappeService
    {
        private readonly IFrappeHttpClient _frappeClient;

        public FrappeService(IFrappeHttpClient frappeClient)
        {
            _frappeClient = frappeClient;
        }

        public async Task<IEnumerable<TEntity>> GetListAsync<TEntity>(
            string docType,
            string fields = "[\"*\"]",
            string filters = null,
            string parent = null,
            string orderBy = null,
            int limitStart = 0,
            int limitPageLength = 20) where TEntity : IFrappeBaseEntity
        {
            var getListMethodName = "frappe.client.get_list";

            var response = await _frappeClient.MethodGetRequestAsync(getListMethodName,
                ("doctype", docType),
                ("fields", fields),
                ("filters", filters),
                ("parent", parent),
                ("order_by", orderBy),
                ("limit_start", limitStart.ToString()),
                ("limit_page_length", limitPageLength.ToString()));

            await response.EnsureERPNextSuccessStatusCodeAsync();

            var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var result = FrappeEntityExtensions.FromJson<IEnumerable<TEntity>>(doc.RootElement.GetProperty("message").ToString());

            return result;
        }

        public async Task<int> GetCountAsync(string docType, string filters = null)
        {
            var getCountMethodName = "frappe.client.get_count";

            var response = await _frappeClient.MethodGetRequestAsync(getCountMethodName,
                ("doctype", docType),
                ("filters", filters));

            await response.EnsureERPNextSuccessStatusCodeAsync();

            var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return doc.RootElement.GetProperty("message").GetInt32();
        }

        public async Task<T> GetDocByFilterAsync<T>(string docType, string filters, string parent = null)
        {
            throw new NotImplementedException();
        }

        public async Task<T> GetDocByNameAsync<T>(string docType, string docName, string parent = null)
        {
            throw new NotImplementedException();
        }

        public async Task<T> GetDocAsync<T>(string docType, string name, dynamic filters, string parent = null)
        {
            throw new NotImplementedException();
        }

        public async Task<TField> GetFieldValueAsync<TField>(string docType, string fieldName, string filters, string parent = null)
        {
            throw new NotImplementedException();
        }

        public async Task<T> GetSingleValueAsync<T>()
        {
            throw new NotImplementedException();
        }

        public async Task<string> RenameDocAsync(string docType, string oldName, string newName, bool merge = false)
        {
            //doctype, old_name, new_name, merge=False
            const string renameDocMethodName = "frappe.client.rename_doc";

            using (var content = new MultipartFormDataContent())
            {
                content.Add(new StringContent(docType), "doctype");
                content.Add(new StringContent(oldName), "old_name");
                content.Add(new StringContent(newName), "new_name");
                if (merge) content.Add(new StringContent("true"), "merge");

                var response = await _frappeClient.MethodPostRequestAsync(renameDocMethodName, content);
                await response.EnsureERPNextSuccessStatusCodeAsync();

                var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                var result = doc.RootElement.GetProperty("message").GetString();

                return result;
            }
        }

        public async Task<TEntity> SaveDocAsync<TEntity>(TEntity entity) where TEntity : IFrappeBaseEntity
        {
            const string updateMethodName = "frappe.client.save";

            using (var content = new MultipartFormDataContent())
            {
                content.Add(new StringContent(JsonSerializer.Serialize(entity, ERPNextJsonSerializationSettings.Settings)), "doc");

                var response = await _frappeClient.MethodPostRequestAsync(updateMethodName, content);
                await response.EnsureERPNextSuccessStatusCodeAsync();

                return (TEntity)FrappeEntityExtensions.FromJson<TEntity>(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task DeleteDocAsync(string docType, string docName)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> HasPermissionAsync(string docType, string docName, FrappeDocPermission permType)
        {
            throw new NotImplementedException();
        }

        public async Task<TEntity> InsertAsync<TEntity>(TEntity entity) where TEntity : IFrappeBaseEntity
        {
            const string insertMethodName = "frappe.client.insert";

            EnsureEntitesHasDoctype(entity);

            using (var content = new MultipartFormDataContent())
            {
                content.Add(new StringContent(JsonSerializer.Serialize(entity)), "doc");

                var response = await _frappeClient.MethodPostRequestAsync(insertMethodName, content);
                await response.EnsureERPNextSuccessStatusCodeAsync();

                var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                var result = FrappeEntityExtensions.FromJson<TEntity>(doc.RootElement.GetProperty("message").ToString());

                return result;
            }
        }

        /// <summary>
        /// Send a POST request to <i>frappe.client.insert_many</i> for batch insertion.
        /// </summary>
        /// <typeparam name="T">Any type of ERPNext DocType Entity.</typeparam>
        /// <param name="entities">Entities to be inserted. ERPNext may limit the amount of rows inserted at once.</param>
        /// <returns>IDs of entities inserted.</returns>
        public async Task<IEnumerable<string>> InsertManyAsync<T>(IEnumerable<T> entities) where T : IFrappeBaseEntity
        {
            const string insertManyMethodName = "frappe.client.insert_many";

            EnsureEntitesHasDoctype(entities);

            using (var content = new MultipartFormDataContent())
            {
                var serializerSettings = new JsonSerializerOptions()
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                content.Add(new StringContent(JsonSerializer.Serialize(entities, serializerSettings)), "docs");

                var response = await _frappeClient.MethodPostRequestAsync(insertManyMethodName, content);
                await response.EnsureERPNextSuccessStatusCodeAsync();

                var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                var result = FrappeEntityExtensions.FromJson<IEnumerable<string>>(doc.RootElement.GetProperty("message").ToString());

                return result;
            }
        }

        /// <summary>
        /// Send a POST request to <i>frappe.client.insert_many</i> for batch insertion.
        /// </summary>
        /// <typeparam name="T">Any type of ERPNext DocType Entity.</typeparam>
        /// <param name="entities">Entities to be inserted. ERPNext may limit the amount of rows inserted at once.</param>
        /// <returns>IDs of entities inserted.</returns>
        [Obsolete]
        public Task<IEnumerable<string>> DeferredInsertManyAsync<T>(List<T> entities) where T : IFrappeEntity
        {
            throw new NotImplementedException();
        }

        private void EnsureEntitesHasDoctype<TEntity>(IEnumerable<TEntity> entities) where TEntity : IFrappeBaseEntity
        {
            if (entities.ToList().FindAll(f => string.IsNullOrEmpty(f.Doctype)).Count > 0)
                throw new ArgumentException(
                    "ERPNext requires that all entities has the property 'doctype' set when inserting in batch.");
        }

        private void EnsureEntitesHasDoctype<TEntity>(TEntity entity) where TEntity : IFrappeBaseEntity
        {
            EnsureEntitesHasDoctype(new TEntity[] {entity}.AsEnumerable());
        }

        public async Task<TEntity> UpdateAsync<TEntity>(TEntity entity) where TEntity : IFrappeBaseEntity
        {
            return await UpdateAsync(entity.Doctype, entity.Name, entity);
        }

        public async Task<IDictionary<dynamic, string>> BulkUpdateAsync<TEntity>(IEnumerable<TEntity> entitiesToUpdate) where TEntity : IFrappeBaseEntity
        {
            throw new NotImplementedException();
        }

        public async Task DeferredInsertAsync<TEntity>(IEnumerable<TEntity> entitiesToInsert, string docType = null) where TEntity : IFrappeBaseEntity
        {
            const string deferredInsertMethod = "frappe.deferred_insert.deferred_insert";

            if (string.IsNullOrEmpty(docType))
                EnsureEntitesHasDoctype(entitiesToInsert);

            using (var content = new MultipartFormDataContent())
            {
                content.Add(new StringContent(docType ?? ""), "doctype");
                content.Add(new StringContent(JsonSerializer.Serialize(entitiesToInsert)), "records");

                var response = await _frappeClient.MethodPostRequestAsync(deferredInsertMethod, content);
                await response.EnsureERPNextSuccessStatusCodeAsync();
            }

        }

        public async Task<TEntity> UpdateAsync<TEntity>(string docType, string docName, TEntity entity)
        {
            var response = await _frappeClient.PutAsJsonResourceRequestAsync(docType, docName, entity);
            await response.EnsureERPNextSuccessStatusCodeAsync();

            var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var result = FrappeEntityExtensions.FromJson<TEntity>(doc.RootElement.GetProperty("data").ToString());

            return result;
        }

        public async Task<HttpResponseMessage> BulkUpdateRequestAsync<T>(IEnumerable<T> entitiesToUpdate)
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

        public async Task<string> GetResourceAsRawAsync(string docType, string docName)
        {
            var response = await _frappeClient.GetResourceRequestAsync(docType, docName);
            await response.EnsureERPNextSuccessStatusCodeAsync();

            var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

            return doc.RootElement.GetProperty("data").ToString();
        }

    }
}