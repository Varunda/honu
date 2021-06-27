using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db {

    /// <summary>
    /// A generic data reader that wraps reading a single entry and list safey 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class IDataReader<T> where T : class {

        /// <summary>
        /// Read a single row of data and turn it into the generic type <typeparamref name="T"/>
        /// </summary>
        /// <param name="reader">Reader the read is being performed on</param>
        /// <returns>The row in <paramref name="reader"/> represents as the generic type</returns>
        public abstract T ReadEntry(NpgsqlDataReader reader);

        /// <summary>
        /// Read a list of rows from a <see cref="NpgsqlCommand"/>
        /// </summary>
        /// <param name="cmd">Command to read the list from</param>
        /// <returns>A list of type <typeparamref name="T"/>. If no rows are read, an empty list is returned, not null</returns>
        public async Task<List<T>> ReadList(NpgsqlCommand cmd) {
            List<T> entries = new List<T>();

            using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync() == true) {
                entries.Add(ReadEntry(reader));
            }

            return entries;
        }

        /// <summary>
        /// Read a single entry from a <see cref="NpgsqlCommand"/>
        /// </summary>
        /// <param name="cmd">Command to read the entry from</param>
        /// <returns>The value read from the command or null if no rows were returned</returns>
        public async Task<T?> ReadSingle(NpgsqlCommand cmd) {
            T? entry = null;

            using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync() == true) {
                entry = ReadEntry(reader);
            }

            return entry;
        }

    }
}
