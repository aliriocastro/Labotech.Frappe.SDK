using System.Net.Http;
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
            int limitPageLength);

        Task<HttpResponseMessage> GetResourceRequestAsync(
            string docType,
            string docName);

        Task<HttpResponseMessage> PostResourceRequestAsync(
            string docType,
            HttpContent content);

        Task<HttpResponseMessage> PostAsJsonResourceRequestAsync<T>(
            string docType,
            T content);

        Task<HttpResponseMessage> PutAsJsonResourceRequestAsync<T>(
            string docType,
            string docName,
            T content);

        Task<HttpResponseMessage> PutResourceRequestAsync<T>(
            string docType,
            string docName,
            HttpContent content);

        Task<HttpResponseMessage> DeleteResourceRequestAsync(
            string docType,
            string docName);

        Task<HttpResponseMessage> MethodGetRequestAsync(string methodName, params (string key, string value)[] parameters);
        Task<HttpResponseMessage> MethodPostRequestAsync(string methodName, HttpContent content);

        Task<HttpResponseMessage> MethodPostAsJsonRequestAsync<TPayload>(string methodName, TPayload payload);

        Task<HttpResponseMessage> ControllerMethodPostRequestAsync(string docType, string docName, string methodName, HttpContent content);
        Task<HttpResponseMessage> ControllerMethodGetRequestAsync(string docType, string docName, string methodName, params (string key, string value)[] parameters);

    }
}
