using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Labotech.Frappe.Core;
using Labotech.Frappe.Core.Common;

namespace Labotech.Frappe.Connector.Extensions
{
    public static class FrappeEntityExtensions
    {
        /// <summary>
        /// Serializes from a <typeparamref name="TEntity"/> entity to a JSON string using <see cref="ERPNextJsonSerializationSettings.Settings"/>
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static string ToJson<TEntity>(this TEntity entity)
        {
            return JsonSerializer.Serialize(entity, ERPNextJsonSerializationSettings.Settings);
        }


        /// <summary>
        /// Deserializes from a JSON string to <typeparamref name="TEntity"/> (a <see cref="IFrappeBaseEntity"/>) by using <see cref="ERPNextJsonSerializationSettings.Settings"/>
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="payload"></param>
        /// <returns></returns>
        public static TEntity FromJson<TEntity>(this string payload) 
        {
            return JsonSerializer.Deserialize<TEntity>(payload,  ERPNextJsonSerializationSettings.Settings);
        }

    }
}