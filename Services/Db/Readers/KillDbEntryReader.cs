using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Readers {

    public class KillDbEntryReader : IDataReader<KillDbEntry> {

        public override KillDbEntry ReadEntry(NpgsqlDataReader reader) {
            KillDbEntry entry = new KillDbEntry();

            entry.CharacterID = reader.GetString("attacker_character_id");
            entry.Kills = reader.GetInt32("kills");
            entry.Deaths = reader.GetInt32("deaths");

            return entry;
        }

    }
}
