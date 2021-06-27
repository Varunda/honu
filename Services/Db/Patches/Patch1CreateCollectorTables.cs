using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch1CreateCollectorTables : IDbPatch {

        public int MinVersion => 1;
        public string Name => "Create collector tables";

        public async Task Execute(IDbHelper helper) {
            using (NpgsqlConnection conn = helper.Connection()) {
                using NpgsqlCommand cmd = await helper.Command(conn, @"
                    CREATE TABLE IF NOT EXISTS wt_kills (
                        id bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                        world_id smallint NOT NULL,
                        zone_id int NOT NULL,
                        attacker_character_id varchar NOT NULL,
                        attacker_loadout_id smallint NOT NULL,
                        attacker_faction_id smallint NOT NULL,
                        attacker_team_id smallint NOT NULL,
                        attacker_fire_mode_id int NOT NULL,
                        attacker_vehicle_id int NOT NULL,
                        killed_character_id varchar NOT NULL,
                        killed_loadout_id smallint NOT NULL,
                        killed_faction_id smallint NOT NULL,
                        killed_team_id smallint NOT NULL,
                        revived_event_id bigint NULL,
                        weapon_id varchar NOT NULL,
                        is_headshot boolean NOT NULL,
                        timestamp timestamptz NOT NULL DEFAULT (NOW() at time zone 'utc')
                    );
                ");
                await cmd.ExecuteNonQueryAsync();
            }

            using (NpgsqlConnection conn = helper.Connection()) {
                using NpgsqlCommand cmd = await helper.Command(conn, @"
                    CREATE TABLE IF NOT EXISTS wt_exp (
                        id bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                        world_id smallint NOT NULL,
                        zone_id int NOT NULL,
                        source_character_id varchar NOT NULL,
                        experience_id int NOT NULL,
                        source_loadout_id smallint NOT NULL,
                        source_faction_id smallint NOT NULL,
                        other_id varchar NOT NULL,
                        amount int NOT NULL,
                        timestamp timestamptz NOT NULL DEFAULT (NOW() at time zone 'utc')
                    );
                ");
                await cmd.ExecuteNonQueryAsync();
            }

        }

    }
}
