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

        /// <summary>
        ///     From an object json, get the value of a field within the object
        /// </summary>
        /// <typeparam name="T">Type to get. Usually an unmanaged type</typeparam>
        /// <param name="elem">Extension instance</param>
        /// <param name="name">Case-sensitive name of field within <paramref name="elem"/></param>
        /// <returns>
        ///     A nullable value of <typeparamref name="T"/>. If the field within <paramref name="elem"/> does not exist,
        ///     or has a <c>null</c> value, then <c>null</c> will be returned
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     If <paramref name="elem"/> has a <see cref="JsonElement.ValueKind"/> of <see cref="JsonValueKind.Undefined"/>
        /// </exception>
        public static T? GetValue<T>(this JsonElement elem, string name) {
            if (elem.ValueKind == JsonValueKind.Undefined) {
                throw new InvalidOperationException($"Element {elem} is of ValueKind Undefined, cannot get properties of it");
            }

            if (elem.TryGetProperty(name, out JsonElement child) == false) {
                return default(T);
            }

            string? value = child.Deserialize<string>();
            if (value == null) {
                return default(T);
            }

            TypeConverter conv = TypeDescriptor.GetConverter(typeof(T));
            return (T?)conv.ConvertFrom(value) ?? default(T);
        }

        public static T GetValueOrDefault<T>(this JsonElement elem, string name, T def) {
            return elem.GetValue<T>(name) ?? def;
        }

        public static JsonElement? GetChild(this JsonElement elem, string name) {
            if (elem.TryGetProperty(name, out JsonElement child) == true) {
                if (child.ValueKind == JsonValueKind.Undefined) {
                    return null;
                }
                return child;
            }
            return null;
        }

        /// <summary>
        ///     Get a string from a token. If there is no field in <paramref name="token"/> matching <paramref name="name"/>,
        ///     or the field is a null value, a <see cref="ArgumentNullException"/> will be thrown
        /// </summary>
        /// <param name="token">Extension instance</param>
        /// <param name="name">Name of the field within <paramref name="token"/> to read from</param>
        /// <returns>
        ///     A string 
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="token"/> does not contain a field named <paramref name="name"/>,
        ///     or the field with name <paramref name="name"/> has a null/undefined value
        /// </exception>
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

        /// <summary>
        ///     get a <see cref="long"/> value from a <see cref="JsonElement"/>, using <paramref name="fallback"/> if not found
        /// </summary>
        /// <param name="token">extension instance</param>
        /// <param name="name">name of the element within the token</param>
        /// <param name="fallback">fallback value if the token does not have the value</param>
        /// <returns></returns>
        public static long GetInt64(this JsonElement token, string name, short fallback) {
            return token.GetValue<long?>(name) ?? fallback;
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

        public static ushort GetUInt16(this JsonElement token, string field) {
            return token.GetValue<ushort?>(field) ?? 0;
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
