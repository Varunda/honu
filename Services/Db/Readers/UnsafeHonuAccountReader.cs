using Npgsql;
using System.Data;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Internal;

namespace watchtower.Services.Db.Readers {

    public class UnsafeHonuAccountReader : IDataReader<UnsafeHonuAccount> {

        public override UnsafeHonuAccount? ReadEntry(NpgsqlDataReader reader) {
            UnsafeHonuAccount acc = new UnsafeHonuAccount();

            acc.ID = reader.GetInt64("id");
            acc.Name = reader.GetString("name");
            acc.Timestamp = reader.GetDateTime("timestamp");
            acc.Email = reader.GetString("email");
            acc.Discord = "<hidded in reader>";
            acc.DiscordID = reader.GetUInt64("discord_id");
            acc.DeletedOn = reader.GetNullableDateTime("deleted_on");
            acc.DeletedBy = reader.GetNullableInt64("deleted_by");

            return acc;
        }

    }
}
