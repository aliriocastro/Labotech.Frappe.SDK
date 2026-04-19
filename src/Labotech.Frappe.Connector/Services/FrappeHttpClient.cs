using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Labotech.Frappe.Connector.Contracts;
using Labotech.Frappe.Core.Common;

namespace Labotech.Frappe.Connector.Services
{
    public sealed class FrappeHttpClient
        : IFrappeHttpClient
    {

        private readonly string _baseUrl;

        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initialization method providing HttpClient, base URL and access token in base64 format.
        /// </summary>
        public FrappeHttpClient(HttpClient httpClient, string baseUrl, string base64Token)
        {
            _baseUrl = baseUrl;
            _httpClient = httpClient;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Token);
        }

        /// <summary>
        /// Initialization method providing HttpClient, base URL, username and password.
        /// </summary>
        public FrappeHttpClient(HttpClient httpClient, string baseUrl, string username, string password)
            : this(httpClient, baseUrl, Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"))) { }

        public Task<HttpResponseMessage> GetResourcesRequestAsync(string docType,
            string fields,
            string filters,
            string parent,
            string orderBy,
            int limitStart,
            int limitPageLength,
            CancellationToken cancellationToken = default)
        {
            var pairs = new List<(string key, string value)>();
            if (fields != null) pairs.Add(("fields", fields));
            if (limitPageLength != 0) pairs.Add(("limit_page_length", limitPageLength.ToString()));
            if (limitStart != 0) pairs.Add(("limit_start", limitStart.ToString()));
            if (filters != null) pairs.Add(("filters", filters));
            if (parent != null) pairs.Add(("parent", parent));
            if (orderBy != null) pairs.Add(("order_by", orderBy));

            var url = $"{_baseUrl}/api/resource/{docType}{BuildQueryString(pairs)}";
            return _httpClient.GetAsync(url, cancellationToken);
        }

        /// <summary>
        /// Send a GET request to obtain a <paramref name="docType"/> resource identified by its <paramref name="docName"/>.
        /// </summary>
        public Task<HttpResponseMessage> GetResourceRequestAsync(string docType, string docName, CancellationToken cancellationToken = default)
        {
            var url = $"{_baseUrl}/api/resource/{docType}/{docName}";
            return _httpClient.GetAsync(url, cancellationToken);
        }

        /// <summary>
        /// Send a POST request to INSERT a new <paramref name="docType"/> resource.
        /// </summary>
        public Task<HttpResponseMessage> PostResourceRequestAsync(string docType, HttpContent content, CancellationToken cancellationToken = default)
        {
            var url = $"{_baseUrl}/api/resource/{docType}";
            return _httpClient.PostAsync(url, content, cancellationToken);
        }

        /// <summary>
        /// Send a POST as JSON request to INSERT a new <paramref name="docType"/> resource.
        /// </summary>
        public Task<HttpResponseMessage> PostAsJsonResourceRequestAsync<T>(string docType, T content, CancellationToken cancellationToken = default)
        {
            var url = $"{_baseUrl}/api/resource/{docType}";
            return _httpClient.PostAsJsonAsync(url, content, ERPNextJsonSerializationSettings.Settings, cancellationToken);
        }

        /// <summary>
        /// Send a PUT as JSON request to UPDATE a <paramref name="docType"/> resource identified by its <paramref name="docName"/>
        /// </summary>
        public Task<HttpResponseMessage> PutAsJsonResourceRequestAsync<T>(string docType, string docName, T content, CancellationToken cancellationToken = default)
        {
            var url = $"{_baseUrl}/api/resource/{docType}/{docName}";
            return _httpClient.PutAsJsonAsync(url, content, ERPNextJsonSerializationSettings.Settings, cancellationToken);
        }

        /// <summary>
        /// Send a PUT request to UPDATE a <paramref name="docType"/> resource identified by its <paramref name="docName"/>
        /// </summary>
        public Task<HttpResponseMessage> PutResourceRequestAsync<T>(string docType, string docName, HttpContent content, CancellationToken cancellationToken = default)
        {
            var url = $"{_baseUrl}/api/resource/{docType}/{docName}";
            return _httpClient.PutAsync(url, content, cancellationToken);
        }

        /// <summary>
        /// Send a DELETE request to DELETE a <paramref name="docType"/> resource identified by its <paramref name="docName"/>
        /// </summary>
        public Task<HttpResponseMessage> DeleteResourceRequestAsync(string docType, string docName, CancellationToken cancellationToken = default)
        {
            var url = $"{_baseUrl}/api/resource/{docType}/{docName}";
            return _httpClient.DeleteAsync(url, cancellationToken);
        }

        /// <summary>
        /// Send a GET request to an ERPNext whitelisted Method.
        /// </summary>
        public Task<HttpResponseMessage> MethodGetRequestAsync(string methodName, params (string key, string value)[] parameters)
            => MethodGetRequestAsync(methodName, default, parameters);

        public Task<HttpResponseMessage> MethodGetRequestAsync(string methodName, CancellationToken cancellationToken, params (string key, string value)[] parameters)
            => MethodRequestAsync(methodName, string.Empty, string.Empty, false, null, cancellationToken, parameters);

        /// <summary>
        /// Send a POST request to a ERPNext whitelisted Method.
        /// </summary>
        public Task<HttpResponseMessage> MethodPostRequestAsync(string methodName, HttpContent content, CancellationToken cancellationToken = default)
            => MethodRequestAsync(methodName, string.Empty, string.Empty, true, content, cancellationToken, null);

        public Task<HttpResponseMessage> MethodPostAsJsonRequestAsync<TPayload>(string methodName, TPayload payload, CancellationToken cancellationToken = default)
        {
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(payload, ERPNextJsonSerializationSettings.Settings),
                Encoding.UTF8,
                "application/json");
            return MethodRequestAsync(methodName, string.Empty, string.Empty, true, jsonContent, cancellationToken, null);
        }

        /// <summary>
        /// Send a POST request to a specific ERPNext Controller whitelisted Method.
        /// </summary>
        public Task<HttpResponseMessage> ControllerMethodPostRequestAsync(string docType, string docName, string methodName, HttpContent content, CancellationToken cancellationToken = default)
            => MethodRequestAsync(methodName, docType, docName, true, content, cancellationToken, null);

        /// <summary>
        /// Send a GET request to a specific ERPNext Controller whitelisted Method.
        /// </summary>
        public Task<HttpResponseMessage> ControllerMethodGetRequestAsync(string docType, string docName, string methodName,
            params (string key, string value)[] parameters)
            => ControllerMethodGetRequestAsync(docType, docName, methodName, default, parameters);

        public Task<HttpResponseMessage> ControllerMethodGetRequestAsync(string docType, string docName, string methodName,
            CancellationToken cancellationToken, params (string key, string value)[] parameters)
            => MethodRequestAsync(methodName, docType, docName, false, null, cancellationToken, parameters);

        private async Task<HttpResponseMessage> MethodRequestAsync(string methodName, string docType, string docName, bool isPostRequest, HttpContent content, CancellationToken cancellationToken, params (string key, string value)[] parameters)
        {
            string baseUrl;
            var extraPairs = new List<(string key, string value)>();

            if (!string.IsNullOrEmpty(docType) && !string.IsNullOrEmpty(docName))
            {
                baseUrl = $"{_baseUrl}/api/resource/{docType}/{docName}";
                extraPairs.Add(("run_method", methodName));
            }
            else
            {
                baseUrl = $"{_baseUrl}/api/method/{methodName}";
            }

            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    if (!string.IsNullOrEmpty(p.value))
                        extraPairs.Add((p.key, p.value));
                }
            }

            var url = $"{baseUrl}{BuildQueryString(extraPairs)}";

#if DEBUG
            Debug.WriteLine($@"ERPNext Method Request
Name: {methodName}
DocType: {docType}
URL: {url}");
#endif

            return isPostRequest
                ? await _httpClient.PostAsync(url, content, cancellationToken)
                : await _httpClient.GetAsync(url, cancellationToken);
        }

        private static string BuildQueryString(IEnumerable<(string key, string value)> pairs)
        {
            var nonEmpty = pairs.Where(p => !string.IsNullOrEmpty(p.value)).ToList();
            if (nonEmpty.Count == 0) return string.Empty;

            var encoded = nonEmpty
                .Select(p => $"{Uri.EscapeDataString(p.key)}={Uri.EscapeDataString(p.value)}");
            return "?" + string.Join("&", encoded);
        }
    }
}
