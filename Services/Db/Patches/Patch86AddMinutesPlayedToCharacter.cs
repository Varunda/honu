using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch86AddMinutesPlayedToCharacter : IDbPatch {
        public int MinVersion => 86;

        public string Name => "add minutes_played to wt_character";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                ALTER TABLE wt_character
                    ADD COLUMN IF NOT EXISTS minutes_played bigint NOT NULL DEFAULT 0;
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }
    }
}
