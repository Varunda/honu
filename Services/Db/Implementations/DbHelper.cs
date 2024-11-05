using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Implementations {

    public class DbHelper : IDbHelper {

        private readonly ILogger<DbHelper> _Logger;
        private readonly IConfiguration _Configuration;

        private static ConcurrentDictionary<string, NpgsqlDataSource> _DataSources = new();

        public DbHelper(ILogger<DbHelper> logger, IConfiguration config) {
            _Logger = logger;
            _Configuration = config;

            // create all data sources up top, and name them so the otel metrics have nice names (instead of the connection string)
            IConfigurationSection allStrings = _Configuration.GetSection("ConnectionStrings");
            foreach (KeyValuePair<string, string?> conn in allStrings.AsEnumerable()) {
                _Logger.LogInformation($"creating data source [name={conn.Key}] [value={conn.Value}]");
                if (string.IsNullOrEmpty(conn.Value)) {
                    _Logger.LogWarning($"skipping DB with no connection string [name={conn.Key}]");
                    continue;
                }
                string dbName = conn.Key.Split(":")[1];
                NpgsqlDataSource ds = new NpgsqlDataSourceBuilder(conn.Value) {
                    Name = dbName
                }.Build();

                _DataSources.TryAdd(dbName, ds);

            }
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
        public NpgsqlConnection Connection(string server = Dbs.EVENTS, string? task = null, bool enlist = true) {

            NpgsqlDataSource ds = _DataSources.GetValueOrDefault(server) ??
                throw new Exception($"No connection string for {server} exists. Currently have [{string.Join(", ", _DataSources.Keys)}]. "
                    + $"Set this value in config, or by using 'dotnet user-secrets set ConnectionStrings:{server} {{connection string}}");

            return ds.CreateConnection();

            /*
            IConfigurationSection allStrings = _Configuration.GetSection("ConnectionStrings");
            string? connStr = allStrings[server];
            foreach (KeyValuePair<string, string?> sec in allStrings.AsEnumerable()) {
                _Logger.LogDebug($"{sec.Key}=>{sec.Value}");
            }

            if (string.IsNullOrEmpty(connStr)) {
                throw new Exception($"No connection string for {server} exists. Currently have [{string.Join(", ", allStrings.GetChildren().ToList().Select(iter => iter.Path))}]. "
                    + $"Set this value in config, or by using 'dotnet user-secrets set ConnectionStrings:{server} {{connection string}}");
            }

            if (enlist == false) {
                connStr += ";Enlist=false";
            }

            NpgsqlDataSource ds = new NpgsqlDataSourceBuilder(connStr) {
                Name = server
            }.Build();

            NpgsqlConnection conn = ds.CreateConnection();

            //NpgsqlConnection conn = new(connStr);

            return conn;
            */
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
        /// <param name="cancel">cancellation token. defaults to <see cref="CancellationToken.None"/> if not given</param>
        /// <returns>
        ///     A new <see cref="NpgsqlCommand"/> ready to be used
        /// </returns>
        public async Task<NpgsqlCommand> Command(NpgsqlConnection connection, string text, CancellationToken cancel = default) {
            if (connection.State == ConnectionState.Closed) {
                await connection.OpenAsync(cancel);
            }

            NpgsqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = text;

            return cmd;
        }

    }

}
