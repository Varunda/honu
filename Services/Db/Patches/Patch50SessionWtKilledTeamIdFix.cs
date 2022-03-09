using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch50SessionWtKilledTeamIdFix : IDbPatch {
        public int MinVersion => 50;
        public string Name => "fix killed_team_id";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                ALTER TABLE wt_session
                    ADD COLUMN IF NOT EXISTS needs_fix boolean DEFAULT false;

                CREATE INDEX IF NOT EXISTS idx_wt_session_needs_fix
                    ON wt_session(needs_fix);

                UPDATE wt_session
                    SET needs_fix = true
                    WHERE character_id IN (SELECT id FROM wt_character WHERE wt_character.faction_id = 4);
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
