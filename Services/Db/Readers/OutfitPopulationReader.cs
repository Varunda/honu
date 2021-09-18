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
            OutfitPopulation pop = new OutfitPopulation();

            pop.OutfitID = reader.GetString("outfit_id");
            pop.OutfitTag = reader.GetNullableString("tag");
            pop.OutfitName = reader.GetString("name");
            pop.Count = reader.GetInt32("count");
            pop.FactionID = reader.GetInt16("faction_id");

            return pop;
        }

    }
}
