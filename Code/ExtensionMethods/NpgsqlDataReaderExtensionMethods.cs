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

    }
}
