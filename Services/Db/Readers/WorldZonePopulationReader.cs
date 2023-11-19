using Npgsql;
using System.Data;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Readers {

    public class WorldZonePopulationReader : IDataReader<WorldZonePopulation> {

        public override WorldZonePopulation? ReadEntry(NpgsqlDataReader reader) {
            WorldZonePopulation pop = new();

            pop.WorldID = reader.GetInt16("world_id");
            pop.ZoneID = reader.GetUInt32("zone_id");
            pop.Timestamp = reader.GetDateTime("timestamp");

            pop.Total = reader.GetInt32("total");
            pop.FactionVs = reader.GetInt32("faction_vs");
            pop.FactionNc = reader.GetInt32("faction_nc");
            pop.FactionTr = reader.GetInt32("faction_tr");
            pop.FactionNs = reader.GetInt32("faction_ns");

            pop.TeamVs = reader.GetInt32("team_vs");
            pop.TeamNc = reader.GetInt32("team_nc");
            pop.TeamTr = reader.GetInt32("team_tr");
            pop.TeamUnknown = reader.GetInt32("team_unknown");

            return pop;
        }

    }
}
