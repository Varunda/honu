using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db {

    public interface IDbHelper {

        /// <summary>
        /// Create a new connection to the database given in the db options
        /// </summary>
        /// <returns>A new connection to use</returns>
        NpgsqlConnection Connection();

        /// <summary>
        /// Create a new command using the connection passed
        /// </summary>
        /// <param name="connection">Connection the command will be executed on</param>
        /// <param name="text">Text of the command</param>
        Task<NpgsqlCommand> Command(NpgsqlConnection connection, string text);

    }
}
