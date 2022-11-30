using Npgsql;
using System.Data;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.PSB;

namespace watchtower.Services.Db.Readers.PSB {

    public class PsbNamedReader : IDataReader<PsbNamedAccount> {

        public override PsbNamedAccount? ReadEntry(NpgsqlDataReader reader) {
            PsbNamedAccount acc = new PsbNamedAccount();

            acc.ID = reader.GetInt64("id");
            acc.AccountType = reader.GetInt16("account_type");
            acc.Tag = reader.GetNullableString("tag");
            acc.Name = reader.GetString("name");
            acc.PlayerName = reader.GetString("player_name");
            acc.Timestamp = reader.GetDateTime("timestamp");

            acc.VsID = reader.GetNullableString("vs_id");
            acc.NcID = reader.GetNullableString("nc_id");
            acc.TrID = reader.GetNullableString("tr_id");
            acc.NsID = reader.GetNullableString("ns_id");

            acc.VsStatus = reader.GetInt32("vs_status");
            acc.NcStatus = reader.GetInt32("nc_status");
            acc.TrStatus = reader.GetInt32("tr_status");
            acc.NsStatus = reader.GetInt32("ns_status");

            acc.SecondsUsage = reader.GetInt32("play_time");

            acc.DeletedBy = reader.GetNullableInt64("deleted_by");
            acc.DeletedAt = reader.GetNullableDateTime("deleted_at");

            return acc;
        }

    }
}
