using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Labotech.Frappe.Connector.Contracts
{
    public interface IFrappeHttpClient
    {
        Task<HttpResponseMessage> GetResourcesRequestAsync(
            string docType,
            string fields,
            string filters,
            string parent,
            string orderBy,
            int limitStart,
            int limitPageLength,
            CancellationToken cancellationToken = default);

        Task<HttpResponseMessage> GetResourceRequestAsync(
            string docType,
            string docName,
            CancellationToken cancellationToken = default);

        Task<HttpResponseMessage> PostResourceRequestAsync(
            string docType,
            HttpContent content,
            CancellationToken cancellationToken = default);

        Task<HttpResponseMessage> PostAsJsonResourceRequestAsync<T>(
            string docType,
            T content,
            CancellationToken cancellationToken = default);

        Task<HttpResponseMessage> PutAsJsonResourceRequestAsync<T>(
            string docType,
            string docName,
            T content,
            CancellationToken cancellationToken = default);

        Task<HttpResponseMessage> PutResourceRequestAsync<T>(
            string docType,
            string docName,
            HttpContent content,
            CancellationToken cancellationToken = default);

        Task<HttpResponseMessage> DeleteResourceRequestAsync(
            string docType,
            string docName,
            CancellationToken cancellationToken = default);

        Task<HttpResponseMessage> MethodGetRequestAsync(string methodName, params (string key, string value)[] parameters);
        Task<HttpResponseMessage> MethodGetRequestAsync(string methodName, CancellationToken cancellationToken, params (string key, string value)[] parameters);

        Task<HttpResponseMessage> MethodPostRequestAsync(string methodName, HttpContent content, CancellationToken cancellationToken = default);

        Task<HttpResponseMessage> MethodPostAsJsonRequestAsync<TPayload>(string methodName, TPayload payload, CancellationToken cancellationToken = default);

        Task<HttpResponseMessage> ControllerMethodPostRequestAsync(string docType, string docName, string methodName, HttpContent content, CancellationToken cancellationToken = default);

        Task<HttpResponseMessage> ControllerMethodGetRequestAsync(string docType, string docName, string methodName, params (string key, string value)[] parameters);
        Task<HttpResponseMessage> ControllerMethodGetRequestAsync(string docType, string docName, string methodName, CancellationToken cancellationToken, params (string key, string value)[] parameters);

    }
}
