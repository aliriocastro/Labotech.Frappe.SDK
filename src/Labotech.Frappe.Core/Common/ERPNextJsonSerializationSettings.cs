using System.Globalization;
using System.Text.Json;

namespace Labotech.Frappe.Core.Common
{
    public static class ERPNextJsonSerializationSettings
    {
        public static readonly JsonSerializerOptions Settings = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }
}

