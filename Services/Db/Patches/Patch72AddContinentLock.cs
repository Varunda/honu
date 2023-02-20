using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch72AddContinentLock : IDbPatch {
        public int MinVersion => 72;
        public string Name => "add continent_lock";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS continent_lock (
                    zone_id int NOT NULL,
                    world_id smallint NOT NULL,

                    timestamp timestamptz NOT NULL,

                    CONSTRAINT pk_continent_lock PRIMARY KEY (zone_id, world_id)

                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }
    }
}
