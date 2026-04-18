using System.Net.Http;

namespace Labotech.Frappe.Connector.Exceptions
{

    /// <summary>
    /// Represents an exception that is thrown when an HTTP request to the Frappe API fails.
    /// </summary>
    public class FrappeHttpRequestException : HttpRequestException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FrappeHttpRequestException"/> class with the specified error message, Frappe traceback, and HTTP response.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="frappeTraceback">The traceback from the Frappe error response.</param>
        /// <param name="httpResponse">The HTTP response associated with the exception.</param>
        public FrappeHttpRequestException(string message, string frappeTraceback = null, HttpResponseMessage httpResponse = null)
            : base(message)
        {
            this.FrappeTraceback = frappeTraceback;
            this.ResponseMessage = httpResponse;
        }

        /// <summary>
        /// Gets the traceback from the Frappe error response.
        /// </summary>
        public string FrappeTraceback { get; }

        /// <summary>
        /// Gets or sets the HTTP response associated with the exception.
        /// </summary>
        public HttpResponseMessage ResponseMessage { get; set; }

        /// <summary>
        /// Gets a value indicating whether the current exception has a traceback from the Frappe error response.
        /// </summary>
        public bool HasERPNextTraceback => !string.IsNullOrEmpty(this.FrappeTraceback);
    }
}