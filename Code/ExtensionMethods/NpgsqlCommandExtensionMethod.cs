using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        ///     
        ///     Unsigned ints are turned into ints, as unsigned ints aren't supported
        /// </remarks>
        /// <param name="command">Extension instance</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        public static void AddParameter(this NpgsqlCommand command, string name, object? value) {
            if (value != null) {
                // PostgreSQL as far as I can tell, doesn't have a type for unsigned ints, so to handle this,
                //      it's first unboxed to a uint, then cast to an int byte for byte
                // not including the unchecked would mean values that couldn't be represents as ints,
                //      but could be represented by uints, would throw an exception
                if (value.GetType() == typeof(uint)) {
                    command.Parameters.AddWithValue(name, unchecked((int)((uint)value)));
                } else {
                    command.Parameters.AddWithValue(name, value);
                }
            } else {
                command.Parameters.AddWithValue(name, DBNull.Value);
            }
        }

        /// <summary>
        /// Turn a command into text that's useful
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static string Print(this NpgsqlCommand cmd) {
            string s = cmd.CommandText + "\n";

            foreach (NpgsqlParameter param in cmd.Parameters) {
                Type? valueType = param.Value?.GetType();

                s += $"{param.ParameterName} => ";

                if (valueType != null && valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(List<>)) {
                    Type listType = valueType.GetGenericArguments()[0];
                    MethodInfo? boundPrint = typeof(NpgsqlCommandExtensionMethod).GetMethod("IterateUnboundGeneric")?.MakeGenericMethod(listType);
                    if (boundPrint == null) {
                        s += "<failed to get IterateUnboundGeneric>";
                    } else {
                        object? ret = boundPrint.Invoke(null, new[] { param.Value });
                        s += (string?)ret;
                    }
                } else {
                    s += param.Value?.ToString();
                }

                s += "\n";
            }

            return s;
        }

        public static string IterateUnboundGeneric<T>(object o) {
            List<T> list = (List<T>) o;
            return string.Join(", ", list);
        }

    }
}
