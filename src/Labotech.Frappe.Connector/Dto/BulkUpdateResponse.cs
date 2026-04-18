using System.Collections.Generic;
using Labotech.Frappe.Connector.Core;

namespace Labotech.Frappe.Connector.Dto
{
    public class BulkUpdateResponse<T> where T : IFrappeEntity
    {
        public IDictionary<dynamic, string> FailedDocs { get; set; }
    }
}