using System;

namespace Labotech.Frappe.Connector.Core
{

    /// <summary>
    /// Represents a Frappe's Entity aka <c>DocType</c>
    /// </summary>
    public interface IFrappeEntity
    : IFrappeBaseEntity
    {

        int? Idx { get; set; }

        /// <summary>
        /// Gets the <c>docstatus</c> property of the ERPNext Document.
        /// <list type="bullet">
        /// <item>0 - Draft</item>
        /// <item>1 - Submitted</item>
        /// <item>2 - Cancelled</item>
        /// </list>
        /// </summary>
        int? DocStatus { get; set; }

        DateTime Creation { get; set; }
        string ModifiedBy { get; set; }

        DateTime Modified { get; set; }

        string Owner { get; set; }

    }

    /// <summary>
    /// Represents a Frappe's Entity aka <c>DocType</c>
    /// </summary>
    public interface IFrappeChildEntity
    : IFrappeEntity
    {

        string Parent { get; set; }

        /// <summary>
        /// Gets the <c>docstatus</c> property of the ERPNext Document.
        /// <list type="bullet">
        /// <item>0 - Draft</item>
        /// <item>1 - Submitted</item>
        /// <item>2 - Cancelled</item>
        /// </list>
        /// </summary>
     
        string ParentField { get; set; }

        /// <summary>
        /// Gets the <c>parentfield</c> property of the ERPNext Document. Represents de parent's document type.
        /// </summary>
        string ParentType { get; set; }

    }
}