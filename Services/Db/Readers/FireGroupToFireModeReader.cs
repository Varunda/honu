using Npgsql;
using System.Data;
using watchtower.Models.Census;

namespace watchtower.Services.Db.Readers {

    public class FireGroupToFireModeReader : IDataReader<FireGroupToFireMode> {

        public override FireGroupToFireMode? ReadEntry(NpgsqlDataReader reader) {
            FireGroupToFireMode mode = new();

            mode.FireGroupID = reader.GetInt32("fire_group_id");
            mode.FireModeID = reader.GetInt32("fire_mode_id");
            mode.FireModeIndex = reader.GetInt32("fire_mode_index");

            return mode;
        }

    }
}
