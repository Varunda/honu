using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch60IndexReconcile : IDbPatch {

        public int MinVersion => 60;
        public string Name => "fix mismatch indexes";

        public async Task Execute(IDbHelper helper) {
            /*
             * List of indexes changed. This can take a very long time, so if the DB has already been running for a while,
             *      it's recommended that you run each one manually concurrently to keep Honu up
             * Ex:
             *      Instead of:
             *          > CREATE INDEX IF NOT EXISTS idx_wt_kills_attacker_character_id ON wt_kills(attacker_character_id);
             *          
             *      Run:
             *          > CREATE CONCURRENTLY INDEX IF NOT EXISTS idx_wt_kills_attacker_character_id ON wt_kills(attacker_character_id);
             *                   ^^^^^^^^^^^^
             * 
             * wt_kills:
             *      +attacker_character_id
             *      +killed_character_id
             *      +timestamp (brin)
             *      +weapon_id
             *      -world_id
             *      
             * wt_recent_kills:
             *      +killed_character_id
             *      
             * wt_exp:
             *      +experience_id
             *      +source_character_id
             *      +timestamp (brin)
             *      -team_id
             *      
             * wt_recent_exp:
             *      +source_character_id
             *      
             * wt_session:
             *      +character_id
             *      +start (brin)
             */

            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE INDEX IF NOT EXISTS idx_wt_kills_attacker_character_id ON wt_kills(attacker_character_id);

                CREATE INDEX IF NOT EXISTS idx_wt_kills_killed_character_id ON wt_kills(killed_character_id);

                CREATE INDEX IF NOT EXISTS idx_wt_kills_timestamp_brin ON wt_kills USING BRIN(timestamp);

                CREATE INDEX IF NOT EXISTS idx_wt_kills_weapon_id ON wt_kills(weapon_id);
            
                DROP INDEX IF EXISTS idx_wt_kills_world_id;

                CREATE INDEX IF NOT EXISTS idx_wt_recent_kills_killed_character_id ON wt_recent_kills(killed_character_id);

                CREATE INDEX IF NOT EXISTS idx_wt_exp_experience_id ON wt_exp(experience_id);

                CREATE INDEX IF NOT EXISTS idx_wt_exp_source_character_id ON wt_exp(source_character_id);

                CREATE INDEX IF NOT EXISTS idx_wt_exp_timestamp_brin ON wt_exp USING BRIN (timestamp);

                DROP INDEX IF EXISTS wt_exp_team_id;

                CREATE INDEX IF NOT EXISTS idx_wt_recent_exp_source_character_id ON wt_recent_exp(source_character_id);

                CREATE INDEX IF NOT EXISTS idx_wt_session_character_id ON wt_session(character_id);

                CREATE INDEX IF NOT EXISTS idx_wt_session_start ON wt_session USING brin(start);
            ");
            cmd.CommandTimeout = 3000;

            await cmd.ExecuteNonQueryAsync();
        }

    }
}
