using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Labotech.Frappe.Connector.Core;
using Labotech.Frappe.Connector.Services;
using Labotech.Frappe.Core;

namespace Labotech.Frappe.Connector.Contracts
{
    public interface IFrappeQueryFluent<T> where T : IFrappeEntity
    {
        List<string> Fields { get; set; }
        string DocType { get; set; }
        string Parent { get; set; }
        int TakeValue { get; set; }
        int SkipValue { get; set; }
        List<ValueTuple<string, object, object>> Conditions { get; set; }
        List<string> OrderBy { get; set; }
        bool IncludeDefaultFields { get; set; }
        JsonSerializerOptions DefaultJsonSerializerSettings { get; }

        /// <summary>
        /// Will fetch all fields available for the DocType. When no fields are specified, ERPNext will return 'name' by default.
        /// </summary>
        /// <returns></returns>
        FrappeQueryFluent<T> WithAllFields();

        /// <summary>
        /// Specify the field names to be retreived. When no fields are specified, ERPNext will return 'name' by default.
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        FrappeQueryFluent<T> WithFields(params string[] fields);

        /// <summary>
        /// Specify the field names to be retreived. When no fields are specified, ERPNext will return 'name' by default.
        /// This method will allow only to retreive the specified fields. Default will not be included in the query.
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        FrappeQueryFluent<T> OnlyWithFields(params string[] fields);

        /// <summary>
        /// Add a condition for the specified field name. If <see cref="FrappeFilterOperator.Between"/>, <see cref="FrappeFilterOperator.In"/> or <see cref="FrappeFilterOperator.NotIn"/>
        /// is specified, it will throw a <see cref="ArgumentException"/> since value handling for those operators are different. Instead use <see cref="FrappeQueryFluent{T}.AddBetweenCondition"/>,
        /// <see cref="FrappeQueryFluent{T}.AddInCondition"/> or <see cref="FrappeQueryFluent{T}.AddNotInCondition"/> respectively for those conditions.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// </exception>
        /// <returns></returns>
        FrappeQueryFluent<T> AddCondition(string fieldName, FrappeFilterOperator op, string value);

        /// <summary>
        /// Adds a condition with the specified parameters. Operator must be one of =, !=, &gt;, &lt;, &gt;=, &lt;=, like, not like, in,
        /// not in, is, between, descendants of, ancestors of, not descendants of, not ancestors of, previous, next.
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        FrappeQueryFluent<T> AddCondition(string fieldName, string op, string value);

        /// <summary>
        /// Add a condition with <see cref="FrappeFilterOperator.EqualTo"/> operator.
        /// </summary>
        /// <returns></returns>
        FrappeQueryFluent<T> AddCondition(string fieldName, string value);

        /// <summary>
        /// Add a condition to the collection with the specified parameters. This method does not perform any validation to the parameters. If <paramref name="listValues"/> has only
        /// one element, it will be treated as a <see cref="String"/> instead of a list of values.
        /// </summary>
        /// <param name="op">Operator must be one of =, !=, &gt;, &lt;, &gt;=, &lt;=, like, not like, in, not in, is, between, descendants of, ancestors of, not descendants of, not ancestors of, previous, next</param>
        /// <returns></returns>
        FrappeQueryFluent<T> AddCondition(string fieldName, string op, params string[] listValues);

        /// <summary>
        /// Add a <see cref="FrappeFilterOperator.Between"/> condition, with an upper and a lower limit values.
        /// </summary>
        /// <returns></returns>
        FrappeQueryFluent<T> AddBetweenCondition(string fieldName, dynamic lowerLimit, dynamic upperLimit);

        /// <summary>
        /// Add a <see cref="FrappeFilterOperator.In"/> condition. It will return the row only if the row field value matches with any of <paramref name="listValues"/>.
        /// </summary>
        /// <returns></returns>
        FrappeQueryFluent<T> AddInCondition(string fieldName, params string[] listValues);

        /// <summary>
        /// Add a <see cref="FrappeFilterOperator.NotIn"/> condition. It will return the row only if the row field value does not match with any of <paramref name="listValues"/>.
        /// </summary>
        /// <returns></returns>
        FrappeQueryFluent<T> AddNotInCondition(string fieldName, params string[] listValues);

        /// <summary>
        /// Add a Order By clause to the query. Please note that ASC is the default orderby clause if not specified.
        /// </summary>
        /// <returns></returns>
        FrappeQueryFluent<T> AddOrderBy(string fieldName, bool asc = true);

        /// <summary>
        /// Indicates how many rows must be fetched.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        FrappeQueryFluent<T> Take(int value);

        /// <summary>
        /// Specify to fetch all rows available (limit_page_length=0)
        /// </summary>
        /// <returns></returns>
        FrappeQueryFluent<T> AllRows();

        /// <summary>
        /// Indicates how many rows must be skipped.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        FrappeQueryFluent<T> Skip(int value);

        /// <summary>
        /// Specify the parent DocType. This option is need it when fetching data from Child Tables.
        /// </summary>
        /// <param name="parentDocType"></param>
        /// <returns></returns>
        FrappeQueryFluent<T> WithParent(string parentDocType);

        Task<IEnumerable<T>> FetchAsync(CancellationToken cancellationToken = default);

    }
}