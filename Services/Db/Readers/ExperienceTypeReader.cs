using Npgsql;
using System.Data;
using watchtower.Models.Census;

namespace watchtower.Services.Db.Readers {

    public class ExperienceTypeReader : IDataReader<ExperienceType> {

        public override ExperienceType? ReadEntry(NpgsqlDataReader reader) {
            ExperienceType type = new ExperienceType();

            type.ID = reader.GetInt32("id");
            type.Name = reader.GetString("name");
            type.Amount = reader.GetDouble("amount");

            return type;
        }

    }
}
