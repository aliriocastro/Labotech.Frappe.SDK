using System.Collections.Generic;
using System.Text.Json;
using Labotech.Frappe.Connector.Core;
using Labotech.Frappe.Connector.Core.Common;

namespace Labotech.ERPNext.Core
{
    public static class FrappeEntityExtensions
    {
        /// <summary>
        /// Deserializes from a JSON string to <typeparamref name="T"/> (a <see cref="IFrappeEntity"/>) by using <see cref="ERPNextJsonSerializationSettings.Settings"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="payload"></param>
        /// <returns></returns>
        public static T FromJsonToFrappeEntity<T>(this string payload) where T : IFrappeEntity
        {
            return JsonSerializer.Deserialize<T>(payload, ERPNextJsonSerializationSettings.Settings);
        }

        /// <summary>
        /// Deserializes from a JSON string to <see cref="List{T}"/> (a <see cref="IFrappeEntity"/>) by using <see cref="ERPNextJsonSerializationSettings.Settings"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="payload"></param>
        /// <returns></returns>
        public static IEnumerable<T> FromJsonToFrappeEntityList<T>(this string payload) where T : IFrappeEntity
        {
            return JsonSerializer.Deserialize<IEnumerable<T>>(payload, ERPNextJsonSerializationSettings.Settings);
        }

    }
}