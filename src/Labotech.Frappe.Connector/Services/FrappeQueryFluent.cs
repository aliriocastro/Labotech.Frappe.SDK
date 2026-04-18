using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Labotech.Frappe.Connector.Contracts;
using Labotech.Frappe.Connector.Core;

namespace Labotech.Frappe.Connector.Services
{
    public partial class FrappeQueryFluent<T> : IFrappeQueryFluent<T> where T : IFrappeEntity
    {
        private readonly IFrappeService _frappeService;

        public List<string> Fields { get; set; }
        public string DocType { get; set; }
        public string Parent { get; set; }
        public int TakeValue { get; set; }
        public int SkipValue { get; set; }
        public List<ValueTuple<string, dynamic, dynamic>> Conditions { get; set; }
        public List<string> OrderBy { get; set; }
        public bool IncludeDefaultFields { get; set; }

        private readonly string[] _defaultFields = new string[] { "name", "modified_by", "modified", "creation", "docstatus" };

        public FrappeQueryFluent(string docType, IFrappeService frappeService)
        {
            // No Default Fields, ERPNext will only return the name.

            _frappeService = frappeService;
            DocType = docType;
            Fields = new List<string>();
            Conditions = new List<(string, dynamic, dynamic)>();
            OrderBy = new List<string>();
            TakeValue = 20;
            IncludeDefaultFields = true;
        }

        /// <summary>
        /// Will fetch all fields available for the DocType. When no fields are specified, ERPNext will return 'name' by default.
        /// </summary>
        /// <returns></returns>
        public FrappeQueryFluent<T> WithAllFields()
        {
            Fields.Clear();
            Fields.Add("*");
            return this;
        }

        /// <summary>
        /// Specify the field names to be retreived. When no fields are specified, ERPNext will return 'name' by default.
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public FrappeQueryFluent<T> WithFields(params string[] fields)
        {
            Fields.AddRange(fields);
            return this;
        }

        /// <summary>
        /// Specify the field names to be retreived. When no fields are specified, ERPNext will return 'name' by default.
        /// This method will allow only to retreive the specified fields. Default will not be included in the query.
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public FrappeQueryFluent<T> OnlyWithFields(params string[] fields)
        {
            IncludeDefaultFields = false;
            Fields.AddRange(fields);
            return this;
        }

        /// <summary>
        /// Add a condition for the specified field name. If <see cref="FrappeFilterOperator.Between"/>, <see cref="FrappeFilterOperator.In"/> or <see cref="FrappeFilterOperator.NotIn"/>
        /// is specified, it will throw a <see cref="ArgumentException"/> since value handling for those operators are different. Instead use <see cref="AddBetweenCondition{TValue}(string, TValue, TValue)"/>,
        /// <see cref="AddInCondition(string, string[])"/> or <see cref="AddNotInCondition(string, string[])"/> respectively for those conditions.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// </exception>
        /// <returns></returns>
        public FrappeQueryFluent<T> AddCondition(string fieldName, FrappeFilterOperator op, string value)
        {
            if (op == FrappeFilterOperator.Between || op == FrappeFilterOperator.In ||
                op == FrappeFilterOperator.NotIn)
            {
                throw new ArgumentException(
                    "Operators Between, In or NotIn, must be used with their respective methods.");
            }

            Conditions.Add((fieldName, op, value));
            return this;
        }

        /// <summary>
        /// Adds a condition with the specified parameters. Operator must be one of =, !=, &gt;, &lt;, &gt;=, &lt;=, like, not like, in,
        /// not in, is, between, descendants of, ancestors of, not descendants of, not ancestors of, previous, next.
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public FrappeQueryFluent<T> AddCondition(string fieldName, string op, string value)
        {
            Conditions.Add((fieldName, op, value));
            return this;
        }

        /// <summary>
        /// Add a condition with <see cref="FrappeFilterOperator.EqualTo"/> operator.
        /// </summary>
        /// <returns></returns>
        public FrappeQueryFluent<T> AddCondition(string fieldName, string value)
        {
            Conditions.Add((fieldName, FrappeFilterOperator.EqualTo, value));
            return this;
        }

        /// <summary>
        /// Add a condition to the collection with the specified parameters. This method does not perform any validation to the parameters. If <paramref name="listValues"/> has only
        /// one element, it will be treated as a <see cref="String"/> instead of a list of values.
        /// </summary>
        /// <param name="op">Operator must be one of =, !=, &gt;, &lt;, &gt;=, &lt;=, like, not like, in, not in, is, between, descendants of, ancestors of, not descendants of, not ancestors of, previous, next</param>
        /// <returns></returns>
        public FrappeQueryFluent<T> AddCondition(string fieldName, string op, params string[] listValues)
        {
            Conditions.Add((fieldName, op, listValues));
            return this;
        }

        /// <summary>
        /// Add a <see cref="FrappeFilterOperator.Between"/> condition, with an upper and a lower limit values.
        /// </summary>
        /// <returns></returns>
        public FrappeQueryFluent<T> AddBetweenCondition(string fieldName, dynamic lowerLimit, dynamic upperLimit)
        {
            Conditions.Add((fieldName, FrappeFilterOperator.Between, new dynamic[] { lowerLimit, upperLimit }));
            return this;
        }

        /// <summary>
        /// Add a <see cref="FrappeFilterOperator.In"/> condition. It will return the row only if the row field value matches with any of <paramref name="listValues"/>.
        /// </summary>
        /// <returns></returns>
        public FrappeQueryFluent<T> AddInCondition(string fieldName, params string[] listValues)
        {
            Conditions.Add((fieldName, FrappeFilterOperator.In, listValues));
            return this;
        }

        /// <summary>
        /// Add a <see cref="FrappeFilterOperator.NotIn"/> condition. It will return the row only if the row field value does not match with any of <paramref name="listValues"/>.
        /// </summary>
        /// <returns></returns>
        public FrappeQueryFluent<T> AddNotInCondition(string fieldName, params string[] listValues)
        {
            Conditions.Add((fieldName, FrappeFilterOperator.NotIn, listValues));
            return this;
        }

        /// <summary>
        /// Add a Order By clause to the query. Please note that ASC is the default orderby clause if not specified.
        /// </summary>
        /// <returns></returns>
        public FrappeQueryFluent<T> AddOrderBy(string fieldName, bool asc = true)
        {
            OrderBy.Add($"{fieldName} {(asc ? "ASC" : "DESC")}");
            return this;
        }


        /// <summary>
        /// Indicates how many rows must be fetched.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public FrappeQueryFluent<T> Take(int value)
        {
            TakeValue = value;
            return this;
        }

        /// <summary>
        /// Specify to fetch all rows available (limit_page_length=0)
        /// </summary>
        /// <returns></returns>
        public FrappeQueryFluent<T> AllRows()
        {
            TakeValue = 0;
            return this;
        }

        /// <summary>
        /// Indicates how many rows must be skipped.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public FrappeQueryFluent<T> Skip(int value)
        {
            SkipValue = value;
            return this;
        }

        /// <summary>
        /// Specify the parent DocType. This option is need it when fetching data from Child Tables.
        /// </summary>
        /// <param name="parentDocType"></param>
        /// <returns></returns>
        public FrappeQueryFluent<T> WithParent(string parentDocType)
        {
            Parent = parentDocType;
            return this;
        }


        public async Task<IEnumerable<T>> FetchAsync()
        {
            return await _frappeService.GetListAsync<T>(
                DocType,
                FieldsAsString(),
                ConditionsAsString(),
                Parent,
                OrderByAsString(),
                SkipValue,
                TakeValue);
        }

        private string FieldsAsString()
        {
            if (IncludeDefaultFields)
                Fields.AddRange(_defaultFields);

            return JsonSerializer.Serialize(Fields.Distinct(), DefaultJsonSerializerSettings);
        }

        private string OrderByAsString()
        {
            // They way ERPNext interprets order_by is completely different.
            // Ej. order_by=field1 ASC, field2 ASC
            return OrderBy.Count > 1 ? string.Join(", ", OrderBy.ToArray()) : string.Empty;
        }

        private string ConditionsAsString()
        {
            if (Conditions.Count == 0)
                return string.Empty;

            var formattedFilterList = new List<List<dynamic>>();

            Conditions.ForEach(((string f, dynamic o, dynamic v) item) =>
            {
                var op = item.o is FrappeFilterOperator filterOperator
                    ? (string)filterOperator.ToStringOperand()
                    : (string)item.o;
                dynamic value;

                if (item.v is string[] arr)
                {
                    if (arr.Length == 1)
                        value = arr[0];
                    else
                        value = arr;
                }
                else
                    value = item.v;

                formattedFilterList.Add(new List<dynamic>() { item.f, op, value });
            });


            return JsonSerializer.Serialize(formattedFilterList, DefaultJsonSerializerSettings);

        }

        public JsonSerializerOptions DefaultJsonSerializerSettings => new JsonSerializerOptions
        {
            Converters = { new HtmlEscapingConverter() }
        };
    }
}