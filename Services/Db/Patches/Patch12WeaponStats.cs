using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch12WeaponStats : IDbPatch {

        public int MinVersion => 12;

        public string Name => "Add weapon stat tables";

        public async Task Execute(IDbHelper helper) {
            using (NpgsqlConnection conn = helper.Connection()) {
                using NpgsqlCommand cmd = await helper.Command(conn, @"
                    CREATE TABLE IF NOT EXISTS weapon_stats (
                        character_id varchar NOT NULL,
                        item_id varchar NOT NULL,

                        timestamp timestamptz NOT NULL DEFAULT (NOW() at time zone 'utc'),

                        kills int NOT NULL,
                        deaths int NOT NULL,
                        headshots int NOT NULL,
                        shots int NOT NULL,
                        shots_hit int NOT NULL,
                        vehicle_kills int NOT NULL,
                        seconds_with int NOT NULL,

                        kd real NOT NULL,
                        kpm real NOT NULL,
                        vkpm real NOT NULL,
                        acc real NOT NULL,
                        hsr real NOT NULL,

                        PRIMARY KEY (character_id, item_id)
                    );
                ");

                await cmd.ExecuteNonQueryAsync();
            }

            using (NpgsqlConnection conn = helper.Connection()) {
                using NpgsqlCommand cmd = await helper.Command(conn, @"
                    CREATE TABLE IF NOT EXISTS percentile_cache_type (
                        id smallint NOT NULL PRIMARY KEY,
                        name varchar NOT NULL
                    );

                    INSERT INTO percentile_cache_type (id, name) VALUES
                        (1, 'kd'),
                        (2, 'kpm'),
                        (3, 'vkpm'),
                        (4, 'acc'),
                        (5, 'hsr') 
                    ON CONFLICT DO NOTHING;
                ");

                await cmd.ExecuteNonQueryAsync();
            }

            using (NpgsqlConnection conn = helper.Connection()) {
                using NpgsqlCommand cmd = await helper.Command(conn, @"
                    CREATE TABLE IF NOT EXISTS percentile_cache (
                        item_id varchar NOT NULL,
                        type smallint NOT NULL,
                        timestamp timestamptz NOT NULL DEFAULT (NOW() at time zone 'utc'),
                        q0 real NOT NULL,
                        q5 real NOT NULL,
                        q10 real NOT NULL,
                        q15 real NOT NULL,
                        q20 real NOT NULL,
                        q25 real NOT NULL,
                        q30 real NOT NULL,
                        q35 real NOT NULL,
                        q40 real NOT NULL,
                        q45 real NOT NULL,
                        q50 real NOT NULL,
                        q55 real NOT NULL,
                        q60 real NOT NULL,
                        q65 real NOT NULL,
                        q70 real NOT NULL,
                        q75 real NOT NULL,
                        q80 real NOT NULL,
                        q85 real NOT NULL,
                        q90 real NOT NULL,
                        q95 real NOT NULL,
                        q100 real NOT NULL,

                        PRIMARY KEY (item_id, type)
                    );
                ");

                await cmd.ExecuteNonQueryAsync();
            }

        }

    }
}
