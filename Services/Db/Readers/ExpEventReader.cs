using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Db;
using watchtower.Models.Events;

namespace watchtower.Services.Db.Readers {

    public class ExpEventReader : IDataReader<ExpEvent> {

        public override ExpEvent ReadEntry(NpgsqlDataReader reader) {
            ExpEvent ev = new ExpEvent();

            ev.SourceID = reader.GetString("source_character_id");
            ev.ExperienceID = reader.GetInt32("experience_id");
            ev.LoadoutID = reader.GetInt16("source_loadout_id");
            ev.TeamID = reader.GetInt16("source_team_id");

            ev.OtherID = reader.GetString("other_id");
            ev.Amount = reader.GetInt32("amount");

            ev.Timestamp = reader.GetDateTime("timestamp");
            ev.ZoneID = reader.GetInt32("zone_id");
            ev.WorldID = reader.GetInt16("world_id");

            return ev;
        }

    }
}
