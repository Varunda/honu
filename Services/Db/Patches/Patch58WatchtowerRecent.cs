using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch58WatchtowerRecent : IDbPatch {
        public int MinVersion => 58;

        public string Name => "Create wt_recent tables";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS wt_recent_kills (
                    id bigint NOT NULL PRIMARY KEY,
                    world_id smallint NOT NULL,
                    zone_id integer NOT NULL,
                    attacker_character_id varchar NOT NULL,
                    attacker_loadout_id smallint NOT NULL,
                    attacker_faction_id smallint NOT NULL,
                    attacker_team_id smallint NOT NULL,
                    attacker_fire_mode_id integer NOT NULL,
                    attacker_vehicle_id integer NOT NULL,
                    killed_character_id varchar NOT NULL,
                    killed_loadout_id smallint NOT NULL,
                    killed_faction_id smallint NOT NULL,
                    killed_team_id smallint NOT NULL,
                    revived_event_id bigint,
                    weapon_id integer NOT NULL,
                    is_headshot boolean NOT NULL,
                    timestamp timestamptz NOT NULL
                );

                CREATE INDEX IF NOT EXISTS idx_wt_recent_kills_attacker_character_id ON wt_recent_kills(attacker_character_id);

                CREATE INDEX IF NOT EXISTS idx_wt_recent_kills_world_id ON wt_recent_kills(world_id);
            
                CREATE INDEX IF NOT EXISTS idx_wt_recent_kills_weapon_id ON wt_recent_kills(weapon_id);

                CREATE TABLE IF NOT EXISTS wt_recent_exp (
                    id bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                    world_id smallint NOT NULL,
                    zone_id integer NOT NULL,
                    source_character_id varchar NOT NULL,
                    experience_id integer NOT NULL,
                    source_loadout_id smallint NOT NULL,
                    source_faction_id smallint NOT NULL,
                    other_id varchar NOT NULL,
                    amount integer NOT NULL,
                    timestamp timestamptz NOT NULL,
                    source_team_id smallint NOT NULL
                );

                CREATE INDEX IF NOT EXISTS idx_wt_recent_exp_world_id ON wt_recent_exp(world_id);
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }
    }
}
