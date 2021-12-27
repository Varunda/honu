
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace watchtower.Services.Db {

    public abstract class BaseStaticDbStore<T> : IStaticDbStore<T> where T : class {

        private readonly ILogger _Logger;
        private readonly IDataReader<T> _Reader;
        private readonly IDbHelper _DbHelper;

        private readonly string _TableName;

        public BaseStaticDbStore(string tableName, ILoggerFactory loggerFactory,
            IDataReader<T> reader, IDbHelper helper) {

            _TableName = tableName;
            _Logger = loggerFactory.CreateLogger($"StaticDbStore<{typeof(T).Name}>");

            _Reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _DbHelper = helper;
        }

        public async Task<List<T>> GetAll() {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, $@"
                SELECT *
                    FROM {_TableName};
            ");

            List<T> entries = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return entries;
        }

        public async Task Upsert(T param) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, "");
            SetupUpsertCommand(cmd, param);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

        /// <summary>
        ///     Setup the command used to perform the upsert, adding the command text
        ///     and parameters
        /// </summary>
        /// <param name="cmd">Command to setup to upsert</param>
        /// <param name="param">Object that contains the parameters</param>
        internal abstract void SetupUpsertCommand(NpgsqlCommand cmd, T param);

    }

}