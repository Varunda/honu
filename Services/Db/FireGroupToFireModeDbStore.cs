using Microsoft.Extensions.Logging;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Db {

    public class FireGroupToFireModeDbStore : IStaticDbStore<FireGroupToFireMode> {

        private readonly ILogger<FireGroupToFireModeDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<FireGroupToFireMode> _Reader;

        public FireGroupToFireModeDbStore(ILogger<FireGroupToFireModeDbStore> logger,
            IDbHelper dbHelper, IDataReader<FireGroupToFireMode> reader) {

            _Logger = logger;
            _DbHelper = dbHelper;
            _Reader = reader;
        }

        public async Task<List<FireGroupToFireMode>> GetAll() {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM fire_group_to_fire_mode;
            ");

            List<FireGroupToFireMode> modes = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return modes;
        }

        public async Task Upsert(FireGroupToFireMode param) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO fire_group_to_fire_mode (
                    fire_group_id, fire_mode_id, fire_mode_index
                ) VALUES (
                    @GroupID, @ModeID, @ModeIndex
                ) ON CONFLICT (fire_group_id, fire_mode_id, fire_mode_index) DO NOTHING;
            ");

            cmd.AddParameter("GroupID", param.FireGroupID);
            cmd.AddParameter("ModeID", param.FireModeID);
            cmd.AddParameter("ModeIndex", param.FireModeIndex);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
