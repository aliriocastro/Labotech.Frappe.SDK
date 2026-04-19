using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Labotech.Frappe.Connector.Exceptions;

namespace Labotech.Frappe.Connector.Extensions
{
    public static class HttpResponseMessageExtension
    {
        public static async Task EnsureERPNextSuccessStatusCodeAsync(this HttpResponseMessage response, CancellationToken cancellationToken = default)
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

        private const string TracebackPattern = @"\<pre\>\s*(Traceback.*?)\<\/pre\>";
        private static readonly TimeSpan TracebackRegexTimeout = TimeSpan.FromMilliseconds(200);

        static string ExtractERPNextTraceback(string htmlContent)
        {
            try
            {
                var match = Regex.Match(htmlContent, TracebackPattern, RegexOptions.Singleline, TracebackRegexTimeout);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }
            catch (RegexMatchTimeoutException)
            {
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }

            return string.Empty;
        }
    }
}