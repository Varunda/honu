using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Readers {

    public class KillDbOutfitEntryReader : IDataReader<KillDbOutfitEntry> {

        public override KillDbOutfitEntry ReadEntry(NpgsqlDataReader reader) {
            KillDbOutfitEntry entry = new KillDbOutfitEntry();

            entry.OutfitID = reader.GetString("outfit_id");
            entry.Kills = reader.GetInt32("kills");
            entry.Deaths = reader.GetInt32("deaths");
            entry.Members = reader.GetInt32("members");

            return entry;
        }

    }
}
