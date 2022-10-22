using Npgsql;
using System.Data;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Readers {

    public class PopulationReader : IDataReader<PopulationEntry> {

        public override PopulationEntry? ReadEntry(NpgsqlDataReader reader) {
            PopulationEntry pop = new();

            pop.ID = reader.GetInt64("id");
            pop.Timestamp = reader.GetDateTime("timestamp");
            pop.Duration = reader.GetInt32("duration");

            pop.WorldID = reader.GetInt16("world_id");
            pop.FactionID = reader.GetInt16("faction_id");

            pop.Total = reader.GetInt32("total");
            pop.Logins = reader.GetInt32("logins");
            pop.Logouts = reader.GetInt32("logouts");

            pop.UniqueCharacters = reader.GetInt32("unique_characters");
            pop.SecondsPlayed = reader.GetInt64("seconds_played");
            pop.AverageSessionLength = reader.GetInt32("average_session_length");

            return pop;
        }

    }
}
