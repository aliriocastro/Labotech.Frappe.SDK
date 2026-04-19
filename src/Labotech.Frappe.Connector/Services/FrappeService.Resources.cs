using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Labotech.Frappe.Connector.Extensions;
using Labotech.Frappe.Core;

namespace Labotech.Frappe.Connector.Services
{
    public sealed partial class FrappeService
    {
        // GET
        public async Task<TEntity> GetResourceAsync<TEntity>(string docType, string docName, CancellationToken cancellationToken = default) where TEntity : IFrappeBaseEntity
        {
            var response = await _frappeClient.GetResourceRequestAsync(docType, docName, cancellationToken);
            await response.EnsureERPNextSuccessStatusCodeAsync(cancellationToken);

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return FrappeEntityExtensions.FromJson<TEntity>(doc.RootElement.GetProperty("data").ToString());
        }

        public async Task<IEnumerable<T>> GetResourcesAsync<T>(
            string docType,
            string fields = "[\"*\"]",
            string filters = null,
            string parent = null,
            string orderBy = null,
            int limitStart = 0,
            int limitPageLength = 20,
            CancellationToken cancellationToken = default) where T : IFrappeBaseEntity
        {
            var response = await _frappeClient.GetResourcesRequestAsync(docType, fields, filters, parent, orderBy, limitStart, limitPageLength, cancellationToken);
            await response.EnsureERPNextSuccessStatusCodeAsync(cancellationToken);

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return FrappeEntityExtensions.FromJson<IEnumerable<T>>(doc.RootElement.GetProperty("data").ToString());
        }

        // POST
        public Task<T> PostResourceAsync<T>(T entity, CancellationToken cancellationToken = default) where T : IFrappeBaseEntity
        {
            return PostResourceAsync(entity, entity.Doctype, cancellationToken);
        }

        public async Task<T> PostResourceAsync<T>(T entity, string docType, CancellationToken cancellationToken = default)
        {
            var response = await _frappeClient.PostAsJsonResourceRequestAsync(docType, entity, cancellationToken);
            await response.EnsureERPNextSuccessStatusCodeAsync(cancellationToken);

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return FrappeEntityExtensions.FromJson<T>(doc.RootElement.GetProperty("data").ToString());
        }

        // PUT
        public Task<TEntity> PutResourceAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : IFrappeBaseEntity
        {
            return PutResourceAsync(entity, entity.Doctype, entity.Name, cancellationToken);
        }

        public async Task<TEntity> PutResourceAsync<TEntity>(TEntity entity, string docType, string name, CancellationToken cancellationToken = default)
        {
            var response = await _frappeClient.PutAsJsonResourceRequestAsync(docType, name, entity, cancellationToken);
            await response.EnsureERPNextSuccessStatusCodeAsync(cancellationToken);

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return FrappeEntityExtensions.FromJson<TEntity>(doc.RootElement.GetProperty("data").ToString());
        }


        // DELETE
        public async Task DeleteResourceAsync(string docType, string docName, CancellationToken cancellationToken = default)
        {
            var response = await _frappeClient.DeleteResourceRequestAsync(docType, docName, cancellationToken);
            await response.EnsureERPNextSuccessStatusCodeAsync(cancellationToken);
        }

        public Task DeleteResourceAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : IFrappeBaseEntity
        {
            return DeleteResourceAsync(entity.Doctype, entity.Name, cancellationToken);
        }

    }
}
