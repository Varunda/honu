using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Code.ExtensionMethods {

    public static class NpgsqlCommandExtensionMethod {

        /// <summary>
        ///     Add a parameter to a <see cref="NpgsqlCommand"/>
        /// </summary>
        /// <remarks>
        ///     Useful, as <see cref="NpgsqlParameterCollection.AddWithValue(string, object)"/> doesn't
        ///     allow nullable values for the value parameter. Null is allowed, but it's safer to pass
        ///     <see cref="DBNull.Value"/>, which this extension method checks for and does
        /// </remarks>
        /// <param name="command">Extension instance</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        public static void AddParameter(this NpgsqlCommand command, string name, object? value) {
            if (value != null) {
                command.Parameters.AddWithValue(name, value);
            } else {
                command.Parameters.AddWithValue(name, DBNull.Value);
            }
        }

    }
}
