using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Labotech.Frappe.Connector.Core;
using Labotech.Frappe.Connector.Extensions;

namespace Labotech.Frappe.Connector.Services
{
    public partial class FrappeService
    {
        // GET
        public async Task<TEntity> GetResourceAsync<TEntity>(string docType, string docName) where TEntity : IFrappeBaseEntity
        {
            var response = await _frappeClient.GetResourceRequestAsync(docType, docName);
            await response.EnsureERPNextSuccessStatusCodeAsync();

            var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var result = FrappeEntityExtensions.FromJson<TEntity>(doc.RootElement.GetProperty("data").ToString());

            return result;
        }
        public async Task<IEnumerable<T>> GetResourcesAsync<T>(
            string docType,
            string fields = "[\"*\"]",
            string filters = null,
            string parent = null,
            string orderBy = null,
            int limitStart = 0,
            int limitPageLength = 20) where T : IFrappeBaseEntity
        {
            var response = await _frappeClient.GetResourcesRequestAsync(docType, fields, filters, parent, orderBy, limitStart, limitPageLength);
            await response.EnsureERPNextSuccessStatusCodeAsync();

            var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var result = FrappeEntityExtensions.FromJson<IEnumerable<T>>(doc.RootElement.GetProperty("data").ToString());

            return result;
        }

        // POST
        public async Task<T> PostResourceAsync<T>(T entity) where T : IFrappeBaseEntity
        {
            return await PostResourceAsync(entity, entity.Doctype);
        }

        public async Task<T> PostResourceAsync<T>(T entity, string docType)
        {
            var response = await _frappeClient.PostAsJsonResourceRequestAsync(docType, entity);
            await response.EnsureERPNextSuccessStatusCodeAsync();

            var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var result = FrappeEntityExtensions.FromJson<T>(doc.RootElement.GetProperty("data").ToString());

            return result;
        }

        // PUT
        public async Task<TEntity> PutResourceAsync<TEntity>(TEntity entity) where TEntity : IFrappeBaseEntity
        {
            return await PutResourceAsync(entity, entity.Doctype, entity.Name);
        }

        public async Task<TEntity> PutResourceAsync<TEntity>(TEntity entity, string docType, string name)
        {
            var response = await _frappeClient.PutAsJsonResourceRequestAsync(docType, name, entity);
            await response.EnsureERPNextSuccessStatusCodeAsync();

            var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var result = FrappeEntityExtensions.FromJson<TEntity>(doc.RootElement.GetProperty("data").ToString());

            return result;
        }


        // DELETE
        public async Task DeleteResourceAsync(string docType, string docName)
        {
            var response = await _frappeClient.DeleteResourceRequestAsync(docType, docName);
            await response.EnsureERPNextSuccessStatusCodeAsync();
        }

        public async Task DeleteResourceAsync<TEntity>(TEntity entity) where TEntity : IFrappeBaseEntity
        {
            await DeleteResourceAsync(entity.Doctype, entity.Name);
        }

    }
}