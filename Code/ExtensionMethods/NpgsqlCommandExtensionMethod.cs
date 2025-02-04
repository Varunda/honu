using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Services.Db;

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
                } else if (value.GetType() == typeof(ulong)) {
                    command.Parameters.AddWithValue(name, unchecked((long)((ulong)value)));
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

        /// <summary>
        ///     Execute a command as a scalar, converting the returned object to an Int32, then close the connection
        /// </summary>
        /// <param name="cmd">Command to execute as a scalar</param>
        /// <param name="cancel">Cancellation token</param>
        /// <returns>
        ///     The result of executing <see cref="DbCommand.ExecuteScalarAsync()"/> parsed to an int
        /// </returns>
        /// <exception cref="NullReferenceException">
        ///     Throw if the result from <see cref="DbCommand.ExecuteScalarAsync()"/> is null.
        ///     SQL commands executed with this method must return a scalar
        /// </exception>
        /// <exception cref="InvalidCastException">
        ///     Throw if the result from <see cref="DbCommand.ExecuteScalarAsync()"/> could
        ///     not be parsed to a valid int
        /// </exception>
        public static async Task<int> ExecuteInt32(this NpgsqlCommand cmd, CancellationToken cancel) {
            object? objID = await cmd.ExecuteScalarAsync(cancel);

            if (objID == null) {
                throw new NullReferenceException(nameof(objID));
            }

            if (int.TryParse(objID.ToString(), out int ID) == true) {
                await (cmd.Connection?.CloseAsync() ?? Task.CompletedTask);
                return ID;
            } else {
                throw new InvalidCastException($"Missing or bad type on 'id': {objID} {objID?.GetType()}");
            }
        }

        /// <summary>
        ///     Execute a command as a scalar, converting the returned object to an Int64, then close the connection
        /// </summary>
        /// <param name="cmd">Command to execute as a scalar</param>
        /// <param name="cancel">Cancellation token</param>
        /// <returns>
        ///     The result of executing <see cref="DbCommand.ExecuteScalarAsync()"/> parsed to a long
        /// </returns>
        /// <exception cref="NullReferenceException">
        ///     Throw if the result from <see cref="DbCommand.ExecuteScalarAsync()"/> is null.
        ///     SQL commands executed with this method must return a scalar
        /// </exception>
        /// <exception cref="InvalidCastException">
        ///     Throw if the result from <see cref="DbCommand.ExecuteScalarAsync()"/> could
        ///     not be parsed to a valid long
        /// </exception>
        public static async Task<long> ExecuteInt64(this NpgsqlCommand cmd, CancellationToken cancel) {
            object? objID = await cmd.ExecuteScalarAsync(cancel);

            if (objID == null) {
                throw new NullReferenceException(nameof(objID));
            }

            if (long.TryParse(objID.ToString(), out long ID) == true) {
                await (cmd.Connection?.CloseAsync() ?? Task.CompletedTask);
                return ID;
            } else {
                throw new InvalidCastException($"Missing or bad type on 'id': {objID} {objID?.GetType()}");
            }
        }

        /// <summary>
        ///     Execute a command as a scalar, converting the returned object to a UInt64, then close the connection
        /// </summary>
        /// <param name="cmd">Command to execute as a scalar</param>
        /// <param name="cancel">Cancellation token</param>
        /// <returns>
        ///     The result of executing <see cref="DbCommand.ExecuteScalarAsync()"/> parsed to a ulong
        /// </returns>
        /// <exception cref="NullReferenceException">
        ///     Throw if the result from <see cref="DbCommand.ExecuteScalarAsync()"/> is null.
        ///     SQL commands executed with this method must return a scalar
        /// </exception>
        /// <exception cref="InvalidCastException">
        ///     Throw if the result from <see cref="DbCommand.ExecuteScalarAsync()"/> could
        ///     not be parsed to a valid ulong
        /// </exception>
        public static async Task<ulong> ExecuteUInt64(this NpgsqlCommand cmd, CancellationToken cancel) {
            object? objID = await cmd.ExecuteScalarAsync(cancel);

            if (objID == null) {
                throw new NullReferenceException(nameof(objID));
            }

            if (ulong.TryParse(objID.ToString(), out ulong ID) == true) {
                await (cmd.Connection?.CloseAsync() ?? Task.CompletedTask);
                return ID;
            } else {
                throw new InvalidCastException($"Missing or bad type on 'id': {objID} {objID?.GetType()}");
            }
        }

        /// <summary>
        ///     Execute a single read from a command, using the reader to turn it into <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">What type will be returned</typeparam>
        /// <param name="cmd">Command to be executed</param>
        /// <param name="reader">Reader used to read the data</param>
        /// <param name="cancel">Cancellation token</param>
        /// <returns>
        ///     A single read from <paramref name="reader"/> performed on <paramref name="cmd"/>.
        ///     If there was no data to be read, <c>null</c> is instead returned
        /// </returns>
        public static async Task<T?> ExecuteReadSingle<T>(this NpgsqlCommand cmd, IDataReader<T> reader, CancellationToken cancel) where T : class {
            T? entry = await reader.ReadSingle(cmd, cancel);
            await (cmd.Connection?.CloseAsync() ?? Task.CompletedTask);

            return entry;
        }

        /// <summary>
        ///     Execute a list read from a command, using <paramref name="reader"/> to turn it into <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">What type the generic list will be</typeparam>
        /// <param name="cmd">Extension instance</param>
        /// <param name="reader">Reader that can turn a row of data into <typeparamref name="T"/></param>
        /// <param name="cancel">Cancellation token</param>
        /// <returns>
        ///     A list of all rows returned by executing <paramref name="cmd"/>,
        ///     transformed into <typeparamref name="T"/> by <paramref name="reader"/>
        /// </returns>
        public static async Task<List<T>> ExecuteReadList<T>(this NpgsqlCommand cmd, IDataReader<T> reader, CancellationToken cancel) where T : class {
            List<T> entries = await reader.ReadList(cmd, cancel);
            await (cmd.Connection?.CloseAsync() ?? Task.CompletedTask);

            return entries;
        }

    }
}
