using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;

namespace watchtower.Services.Db {

    public interface IDbHelper {

        /// <summary>
        /// Create a new connection to the database given in the db options
        /// </summary>
        /// <param name="server">Name of the server. Currently setup for 'events' and 'character'</param>
        /// <param name="task">Optional name to use about the application, defaults to 'honu'</param>
        /// <param name="enlist">Will this connection enlist to the TransactionScope?</param>
        /// <returns>A new connection to use</returns>
        NpgsqlConnection Connection(string server = Dbs.EVENTS, string? task = null, bool enlist = true);

        /// <summary>
        ///     Create a new command using the connection passed
        /// </summary>
        /// <param name="connection">Connection the command will be executed on</param>
        /// <param name="text">Text of the command</param>
        /// <param name="cancel">cancellation token</param>
        /// <returns>
        ///     a <see cref="NpgsqlCommand"/> with an opened connection that can be executed
        /// </returns>
        Task<NpgsqlCommand> Command(NpgsqlConnection connection, string text, CancellationToken cancel = default);

    }

    /// <summary>
    ///     Names of connection strings
    /// </summary>
    public static class Dbs {

        /// <summary>
        ///     The old DB that has everything
        /// </summary>
        public const string EVENTS = "events";

        /// <summary>
        ///     Faster DB
        /// </summary>
        public const string CHARACTER = "character";

        /// <summary>
        ///     DB for wrapped for 2022
        /// </summary>
        public const string WRAPPED_2022 = "wrapped_2022";

        /// <summary>
        ///     DB for wrapped for 2023
        /// </summary>
        public const string WRAPPED_2023 = "wrapped_2023";

    }

    public static class IDbHelperExtensions {

        /// <summary>
        ///     Check if an index exists
        /// </summary>
        /// <param name="instance">Extension instance</param>
        /// <param name="tableName">Table the index is for</param>
        /// <param name="indexName">Index name on the table</param>
        /// <returns>
        ///     A boolean value if the index exists on that table
        /// </returns>
        public static async Task<bool> HasIndex(this IDbHelper instance, string tableName, string indexName) {
            using NpgsqlConnection conn = instance.Connection();
            using NpgsqlCommand cmd = await instance.Command(conn, @"
                SELECT
                    t.relname as table_name, i.relname as index_name, a.attname as column_name
                FROM
                    pg_class t, pg_class i, pg_index ix, pg_attribute a
                WHERE
                    t.oid = ix.indrelid AND i.oid = ix.indexrelid AND a.attrelid = t.oid AND a.attnum = ANY(ix.indkey)
                    AND t.relkind = 'r' AND t.relname = @TableName AND i.relname = @IndexName
            ");

            cmd.AddParameter("TableName", tableName);
            cmd.AddParameter("IndexName", indexName);

            using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

            bool hasIndex = await reader.ReadAsync();
            await conn.CloseAsync();

            return hasIndex;
        }

    }

}
