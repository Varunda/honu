using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Readers {

    public class OutfitPopulationReader : IDataReader<OutfitPopulation> {

        public override OutfitPopulation ReadEntry(NpgsqlDataReader reader) {
            OutfitPopulation pop = new();

            pop.FactionID = reader.GetInt16("team_id");
            pop.OutfitID = reader.GetNullableString("outfit_id");
            if (pop.OutfitID == null || pop.OutfitID == "0") {
                pop.OutfitName = $"No outfit {pop.FactionID}";
            } else {
                pop.OutfitName = reader.GetString("name");
            }

            pop.OutfitTag = reader.GetNullableString("tag");
            pop.Count = reader.GetInt32("count");

            return pop;
        }

    }
}
