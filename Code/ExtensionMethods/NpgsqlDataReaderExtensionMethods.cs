using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Code.ExtensionMethods {

    public static class NpgsqlDataReaderExtensionMethods {

        /// <summary>
        /// Get a string value from a column that may ben ull
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static string? GetNullableString(this NpgsqlDataReader reader, string field) {
            if (reader.IsDBNull(field)) {
                return null;
            }
            return reader.GetString(field);
        }

        public static DateTime? GetNullableDateTime(this NpgsqlDataReader reader, string field) {
            if (reader.IsDBNull(field)) {
                return null;
            }
            return reader.GetDateTime(field);
        }

        /// <summary>
        /// Get a <see cref="uint"/> from a column
        /// </summary>
        /// <param name="reader">Extension instance</param>
        /// <param name="field">Name of that field that has the <c>uint</c></param>
        /// <returns>The <c>uint</c> in the column, cast from an int32</returns>
        public static uint GetUInt32(this NpgsqlDataReader reader, string field) {
            return unchecked((uint)reader.GetInt32(field));
        }

    }
}
