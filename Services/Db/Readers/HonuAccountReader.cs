using Npgsql;
using System.Data;
using watchtower.Code.ExtensionMethods;
using watchtower.Models;

namespace watchtower.Services.Db.Readers {

    public class HonuAccountReader : IDataReader<HonuAccount> {

        public override HonuAccount? ReadEntry(NpgsqlDataReader reader) {
            HonuAccount acc = new HonuAccount();

            acc.ID = reader.GetInt64("id");
            acc.Name = reader.GetString("name");
            acc.Timestamp = reader.GetDateTime("timestamp");
            acc.Email = "<hidden in reader>";
            acc.Discord = "<hidded in reader>";
            acc.DiscordID = reader.GetUInt64("discord_id");
            acc.DeletedOn = reader.GetNullableDateTime("deleted_on");
            acc.DeletedBy = reader.GetNullableInt64("deleted_by");

            return acc;
        }

    }
}
