using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch85KeyCharacterStats : IDbPatch {
        public int MinVersion => 85;
        public string Name => "add a key for character_stats in the character DB";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE UNIQUE INDEX IF NOT EXISTS unq_character_stats ON character_stats (character_id, stat_name, profile_id);
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
