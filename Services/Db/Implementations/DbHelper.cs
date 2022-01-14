using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Implementations {

    public class DbHelper : IDbHelper {

        private readonly ILogger<DbHelper> _Logger;
        private readonly DbOptions _DbOptions;

        public DbHelper(ILogger<DbHelper> logger, IOptions<DbOptions> options) {
            _Logger = logger;
            _DbOptions = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        ///     Create a new connection to the Honu database
        /// </summary>
        /// <remarks>
        ///     The following additional properties are set on the connection:
        ///         <br/>
        ///         'Include Error Detail'=true
        ///         <br/>
        ///         ApplicationName='Honu'
        ///         <br/>
        ///         Timezone=UTC
        /// </remarks>
        /// <returns>
        ///     A new <see cref="NpgsqlConnection"/>
        /// </returns>
        public NpgsqlConnection Connection() {
            string connStr = $"Host={_DbOptions.ServerUrl};"
                + $"Username={_DbOptions.Username};"
                + $"Password={_DbOptions.Password};" 
                + $"Database={_DbOptions.DatabaseName};"
                + $"Include Error Detail=true;"
                + $"ApplicationName=honu;"
                + $"Timezone=UTC";

            NpgsqlConnection conn = new NpgsqlConnection(connStr);

            return conn;
        }

        /// <summary>
        ///     Create a new command, using the connection passed
        /// </summary>
        /// <remarks>
        ///     The resulting <see cref="NpgsqlCommand"/> will have <see cref="DbCommand.CommandType"/> of <see cref="CommandType.Text"/>,
        ///     and <see cref="DbCommand.CommandText"/> of <paramref name="text"/>
        /// </remarks>
        /// <param name="connection">Connection to create the command on</param>
        /// <param name="text">Command text</param>
        /// <returns>
        ///     A new <see cref="NpgsqlCommand"/> ready to be used
        /// </returns>
        public async Task<NpgsqlCommand> Command(NpgsqlConnection connection, string text) {
            await connection.OpenAsync();

            NpgsqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = text;

            return cmd;
        }

    }
}
