using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Labotech.Frappe.Connector.Contracts;

namespace Labotech.Frappe.Connector.Services
{
    public class FrappeHttpClient
    : IFrappeHttpClient
    {

        private readonly string _baseUrl;

        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initialization method providing HttpClient, base URL and access token in base64 format.
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="baseUrl"></param>
        /// <param name="base64Token"></param>
        public FrappeHttpClient(HttpClient httpClient, string baseUrl, string base64Token)
        {
            _baseUrl = baseUrl;
            _httpClient = httpClient;
            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Token);           
        }

        /// <summary>
        /// Initialization method providing HttpClient, base URL, username and password.
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="baseUrl"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public FrappeHttpClient(HttpClient httpClient, string baseUrl, string username, string password)
        : this(httpClient, baseUrl, Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"))) { }

        public async Task<HttpResponseMessage> GetResourcesRequestAsync(string docType,
            string fields,
            string filters,
            string parent,
            string orderBy,
            int limitStart,
            int limitPageLength)
        {
            var baseUri = new UriBuilder($"{_baseUrl}/api/resource/{docType}");

            var query = HttpUtility.ParseQueryString(baseUri.Query);

            if (fields != null) query["fields"] = fields;
            if (limitPageLength != 0) query["limit_page_length"] = limitPageLength.ToString();
            if (limitStart != 0) query["limit_start"] = limitStart.ToString();
            if (filters != null) query["filters"] = filters;
            if (parent != null) query["parent"] = parent;
            if (orderBy != null) query["order_by"] = orderBy;

            baseUri.Query = HttpUtility.UrlDecode(query.ToString());

            return await _httpClient.GetAsync(baseUri.ToString());
        }

        /// <summary>
        /// Send a GET request to obtain a <paramref name="docType"/> resource identified by its <paramref name="docName"/>.
        /// </summary>
        /// <param name="docType"></param>
        /// <param name="docName"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> GetResourceRequestAsync(string docType, string docName)
        {
            var baseUri = new UriBuilder($"{_baseUrl}/api/resource/{docType}/{docName}");

            return await _httpClient.GetAsync(baseUri.ToString());
        }

        /// <summary>
        /// Send a POST request to INSERT a new <paramref name="docType"/> resource.
        /// </summary>
        /// <param name="docType"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PostResourceRequestAsync(string docType, HttpContent content)
        {
            var baseUri = new UriBuilder($"{_baseUrl}/api/resource/{docType}");

            // test with PostAsJsonAsync();
            return await _httpClient.PostAsync(baseUri.ToString(), content);
        }

        /// <summary>
        /// Send a POST as JSON request to INSERT a new <paramref name="docType"/> resource.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="docType"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PostAsJsonResourceRequestAsync<T>(string docType, T content)
        {
            var baseUri = new UriBuilder($"{_baseUrl}/api/resource/{docType}");

            return await _httpClient.PostAsJsonAsync(baseUri.ToString(), content);
        }

        /// <summary>
        /// Send a PUT as JSON request to UPDATE a <paramref name="docType"/> resource identified by its <paramref name="docName"/>
        /// </summary>
        public async Task<HttpResponseMessage> PutAsJsonResourceRequestAsync<T>(string docType, string docName, T content)
        {
            var baseUri = new UriBuilder($"{_baseUrl}/api/resource/{docType}/{docName}");

            return await _httpClient.PutAsJsonAsync(baseUri.ToString(), content);
        }

        /// <summary>
        /// Send a PUT request to UPDATE a <paramref name="docType"/> resource identified by its <paramref name="docName"/>
        /// </summary>
        public async Task<HttpResponseMessage> PutResourceRequestAsync<T>(string docType, string docName, HttpContent content)
        {
            var baseUri = new UriBuilder($"{_baseUrl}/api/resource/{docType}/{docName}");

            return await _httpClient.PutAsync(baseUri.ToString(), content);
        }

        /// <summary>
        /// Send a DELETE request to DELETE a <paramref name="docType"/> resource identified by its <paramref name="docName"/>
        /// </summary>
        public async Task<HttpResponseMessage> DeleteResourceRequestAsync(string docType, string docName)
        {
            var baseUri = new UriBuilder($"{_baseUrl}/api/resource/{docType}/{docName}");

            return await _httpClient.DeleteAsync(baseUri.ToString());
        }

        /// <summary>
        /// Send a GET request to an ERPNext whitelisted Method.
        /// </summary>
        /// <param name="methodName">Name of the method to call (check in ERPNext its location)</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> MethodGetRequestAsync(string methodName, params (string key, string value)[] parameters)
        {
            return await MethodRequestAsync(methodName, string.Empty, string.Empty, false, null, parameters);
        }

        /// <summary>
        /// Send a POST request to a ERPNext whitelisted Method.
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> MethodPostRequestAsync(string methodName, HttpContent content)
        {
            return await MethodRequestAsync(methodName, string.Empty, string.Empty, true, content, null);
        }

        public async Task<HttpResponseMessage> MethodPostAsJsonRequestAsync<TPayload>(string methodName, TPayload payload)
        {
            return await MethodRequestAsync(methodName, string.Empty, string.Empty, true,
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));
        }

        /// <summary>
        /// Send a POST request to a specific ERPNext Controller whitelisted Method.
        /// </summary>
        /// <param name="docType"></param>
        /// <param name="docName"></param>
        /// <param name="methodName"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> ControllerMethodPostRequestAsync(string docType, string docName, string methodName, HttpContent content)
        {
            return await MethodRequestAsync(methodName, docType, docName, true, content, null);
        }

        /// <summary>
        /// Send a GET request to a specific ERPNext Controller whitelisted Method.
        /// </summary>
        /// <param name="docType"></param>
        /// <param name="docName"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> ControllerMethodGetRequestAsync(string docType, string docName, string methodName,
            params (string key, string value)[] parameters)
        {
            return await MethodRequestAsync(methodName, docType, docName, false, null, parameters);
        }

        private async Task<HttpResponseMessage> MethodRequestAsync(string methodName, string docType, string docName, bool isPostRequest, HttpContent content, params (string key, string value)[] parameters)
        {
            UriBuilder baseUri;
            
            if (!string.IsNullOrEmpty(docType) && !string.IsNullOrEmpty(docName))
                baseUri = new UriBuilder($"{_baseUrl}/api/resource/{docType}/{docName}?run_method={methodName}");
            else
                baseUri = new UriBuilder($"{_baseUrl}/api/method/{methodName}");

            var query = HttpUtility.ParseQueryString(baseUri.Query);

            if(parameters != null) foreach (var valueTuple in parameters)
            {
                if(!string.IsNullOrEmpty(valueTuple.value))
                    query[valueTuple.key] = valueTuple.value;
            }

            baseUri.Query = HttpUtility.UrlDecode(query.ToString());

#if DEBUG

            Debug.WriteLine($@"ERPNext Method Request
Name: {methodName}
DocType: {docType}
Query String: {baseUri.Query?.ToString()}");

#endif

            if (isPostRequest)
            {
                return await _httpClient.PostAsync(baseUri.ToString(), content);
            }

            //var c = await (_httpClient.GetAsync(""));
            //c.Content.ReadAsAsync<dynamic>()

            return await _httpClient.GetAsync(baseUri.ToString());
        }
    }
}
