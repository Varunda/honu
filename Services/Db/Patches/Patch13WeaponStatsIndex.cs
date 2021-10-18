using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch13WeaponStatsIndex : IDbPatch {

        public int MinVersion => 13;
        public string Name => "WeaponStats index";

        public async Task Execute(IDbHelper helper) {
            if (await helper.HasIndex("weapon_stats", "idx_weapon_stats_item_id") == false) {
                using NpgsqlConnection conn = helper.Connection();
                using NpgsqlCommand cmd = await helper.Command(conn, @"
                    CREATE INDEX idx_weapon_stats_item_id ON weapon_stats(item_id, kills);
                ");
                await cmd.ExecuteNonQueryAsync();
                await conn.CloseAsync();
            }
        }

    }
}
