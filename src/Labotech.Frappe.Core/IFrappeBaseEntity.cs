using System.ComponentModel.DataAnnotations;

namespace Labotech.Frappe.Connector.Core
{
    public interface IFrappeBaseEntity
    {
        /// <summary>
        /// Gets the <c>name</c> property of the Frappe Document. It is the Document ID. You may not modify this property directly, instead use renaming methods provided by the API.
        /// </summary>
        [Key]
        string Name { get; set; }

        /// <summary>
        /// Gets the <c>doctype</c> property of the ERPNext Document. Represents de Document Type. Sets this property when you want to insert multiple rows,
        /// ERPNext uses this property to identify the <c>doctype</c> for each row independently, so you are able to insert rows from different DocType in the
        /// same batch request.
        /// </summary>
        string Doctype { get; set; }
    }
}