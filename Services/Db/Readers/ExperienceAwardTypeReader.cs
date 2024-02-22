using Npgsql;
using System.Data;
using watchtower.Models.Census;

namespace watchtower.Services.Db.Readers {

    public class ExperienceAwardTypeReader : IDataReader<ExperienceAwardType> {

        public override ExperienceAwardType? ReadEntry(NpgsqlDataReader reader) {
            ExperienceAwardType type = new();

            type.ID = reader.GetInt32("id");
            type.Name = reader.GetString("name");

            return type;
        }

    }
}
