using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace watchtower.Services.Db {

    /// <summary>
    /// A generic data reader that wraps reading a single entry and list safey 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class IDataReader<T> where T : class {

        /// <summary>
        ///     Read a single row of data and turn it into the generic type <typeparamref name="T"/>
        /// </summary>
        /// <param name="reader">Reader the read is being performed on</param>
        /// <returns>
        ///     The row in <paramref name="reader"/> represents as the generic type, or <c>null</c>
        ///     if the data within the reader could not produce a valid output
        /// </returns>
        public abstract T? ReadEntry(NpgsqlDataReader reader);

        /// <summary>
        /// Read a list of rows from a <see cref="NpgsqlCommand"/>
        /// </summary>
        /// <param name="cmd">Command to read the list from</param>
        /// <returns>A list of type <typeparamref name="T"/>. If no rows are read, an empty list is returned, not null</returns>
        public Task<List<T>> ReadList(NpgsqlCommand cmd) {
            return ReadList(cmd, CancellationToken.None);
        }

        /// <summary>
        ///     Read a list of rows from a <see cref="NpgsqlCommand"/>
        /// </summary>
        /// <param name="cmd">Command to read the list from</param>
        /// <param name="cancel">Cancel token</param>
        /// <returns>
        ///     A list of type <typeparamref name="T"/>. If no rows are read, an empty list is returned, not null
        /// </returns>
        public async Task<List<T>> ReadList(NpgsqlCommand cmd, CancellationToken cancel) {
            List<T> entries = new List<T>();

            using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancel);
            while (await reader.ReadAsync(cancel) == true) {
                T? entry = ReadEntry(reader);
                if (entry != null) {
                    entries.Add(entry);
                }
            }

            return entries;
        }

        /// <summary>
        ///     Read a single entry from a <see cref="NpgsqlCommand"/>
        /// </summary>
        /// <param name="cmd">Command to read the entry from</param>
        /// <returns>
        ///     The value read from the command or null if no rows were returned
        /// </returns>
        public Task<T?> ReadSingle(NpgsqlCommand cmd) {
            return ReadSingle(cmd, CancellationToken.None);
        }

        /// <summary>
        ///     Read a single entry from a <see cref="NpgsqlCommand"/>
        /// </summary>
        /// <param name="cmd">Command to read the entry from</param>
        /// <param name="cancel">Cancel token</param>
        /// <returns>
        ///     The value read from the command or null if no rows were returned
        /// </returns>
        public async Task<T?> ReadSingle(NpgsqlCommand cmd, CancellationToken cancel) {
            T? entry = null;

            using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancel);
            if (await reader.ReadAsync(cancel) == true) {
                entry = ReadEntry(reader);
            }

            return entry;
        }

    }
}
