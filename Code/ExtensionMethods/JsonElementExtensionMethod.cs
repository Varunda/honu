using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace watchtower.Code.ExtensionMethods {

    /// <summary>
    /// Helper extension methods for getting types used frequently, or specific fields used a lot
    /// </summary>
    public static class JsonElementExtensionMethods {

        public static T? GetValue<T>(this JsonElement elem, string name) {
            if (elem.TryGetProperty(name, out JsonElement child) == false) {
                return default(T);
            }

            string? value = child.Deserialize<string>();
            if (value == null) {
                return default(T);
            }

            TypeConverter conv = TypeDescriptor.GetConverter(typeof(T));
            return (T?)conv.ConvertFrom(value) ?? default(T);

            //return (T)Convert.ChangeType(value, typeof(T)) ?? default(T);
            //return child.Deserialize<T>();
        }

        public static T GetValueOrDefault<T>(this JsonElement elem, string name, T def) {
            return elem.GetValue<T>(name) ?? def;
        }

        public static JsonElement? GetChild(this JsonElement elem, string name) {
            if (elem.TryGetProperty(name, out JsonElement child) == true) {
                return child;
            }
            return null;
        }

        public static string GetRequiredString(this JsonElement token, string name) {
            return token.GetValue<string?>(name) ?? throw new ArgumentNullException($"Failed to get required field with name of '{name}', from {token}");
        }

        public static int GetRequiredInt32(this JsonElement token, string name) {
            return token.GetValue<int?>(name) ?? throw new ArgumentNullException($"Failed to get required field with name of '{name}', from {token}");
        }

        public static string GetString(this JsonElement token, string name, string fallback) {
            return token.GetValue<string?>(name) ?? fallback;
        }

        public static int GetInt32(this JsonElement token, string name, int fallback) {
            return token.GetValue<int?>(name) ?? fallback;
        }

        public static string? NullableString(this JsonElement token, string name) {
            return token.GetValue<string?>(name);
        }

        public static short GetInt16(this JsonElement token, string name, short fallback) {
            return token.GetValue<short?>(name) ?? fallback;
        }

        public static short GetRequiredInt16(this JsonElement token, string name) {
            return token.GetValue<short?>(name) ?? throw new ArgumentNullException($"Failed to get required field with name of '{name}' from {token}");
        }

        public static DateTime GetRequiredDateTime(this JsonElement token, string name) {
            string input = token.GetRequiredString(name);
            if (DateTime.TryParse(input, out DateTime d) == false) {
                throw new InvalidCastException($"Failed to parse {input} to a valid DateTime");
            }
            return d;
        }

        /// <summary>
        ///     Get the 'world_id' field from a JsonElement, or -1 if it isn't in the token
        /// </summary>
        /// <param name="token">Extension instance</param>
        /// <returns>The Int16 in the field named 'world_id', or -1 if that field doesn't exist</returns>
        public static short GetWorldID(this JsonElement token) {
            return token.GetInt16("world_id", -1);
        }

        public static uint GetZoneID(this JsonElement token) {
            return token.GetValue<uint?>("zone_id") ?? 0;
        }

        public static uint GetUInt32(this JsonElement token, string field) {
            return token.GetValue<uint?>(field) ?? 0;
        }

        public static bool GetBoolean(this JsonElement token, string name, bool fallback) {
            return token.GetValue<bool?>(name) ?? fallback;
        }

        public static decimal GetDecimal(this JsonElement token, string name, decimal fallback) {
            return token.GetValue<decimal?>(name) ?? fallback;
        }

        public static DateTime CensusTimestamp(this JsonElement token, string name) {
            return DateTimeOffset.FromUnixTimeSeconds(token.GetValue<int?>(name) ?? throw new ArgumentNullException(nameof(name))).UtcDateTime;
        }

    }
}
