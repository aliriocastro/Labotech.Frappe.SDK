using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Labotech.Frappe.Connector.Exceptions;
using Microsoft.Extensions.Logging;

namespace Labotech.Frappe.Connector.Extensions
{
    public static class HttpResponseMessageExtension
    {
        private static readonly ILogger<FrappeHttpRequestException> _logger;

        public static async Task EnsureERPNextSuccessStatusCodeAsync(this HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var traceBack = ExtractERPNextTraceback(content);

                throw new FrappeHttpRequestException(
                    $"ERPNext Client returned a non successful HTTP code: {(int)response.StatusCode}",
                    traceBack, response);
            }
        }

        static string ExtractERPNextTraceback(string htmlContent)
        {

            try
            {
                const string pattern = @"\<pre\>\s*(Traceback.*)\<\/pre\>";
                var matches = Regex.Matches(htmlContent, pattern, RegexOptions.Singleline);

                if (matches.Count > 0)
                {
                    return matches[0].Groups[1].Value;
                }
            }
            catch 
            {
                return string.Empty;
            }

            return string.Empty;

        }
    }
}