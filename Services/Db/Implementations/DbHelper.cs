using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
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

        public NpgsqlConnection Connection() {
            string connStr = $"Host={_DbOptions.ServerUrl};Username={_DbOptions.Username};Password={_DbOptions.Password};Database={_DbOptions.DatabaseName};Include Error Detail=true;ApplicationName=watchtower";

            NpgsqlConnection conn = new NpgsqlConnection(connStr);

            return conn;
        }

        public async Task<NpgsqlCommand> Command(NpgsqlConnection connection, string text) {
            await connection.OpenAsync();

            NpgsqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = text;

            return cmd;
        }

    }
}
