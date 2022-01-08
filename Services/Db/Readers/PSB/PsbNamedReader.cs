using Npgsql;
using System.Data;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.PSB;

namespace watchtower.Services.Db.Readers.PSB {

    public class PsbNamedReader : IDataReader<PsbNamedAccount> {

        public override PsbNamedAccount? ReadEntry(NpgsqlDataReader reader) {
            PsbNamedAccount acc = new PsbNamedAccount();

            acc.Tag = reader.GetNullableString("tag");
            acc.Name = reader.GetString("name");
            acc.VsID = reader.GetNullableString("vs_id");
            acc.NcID = reader.GetNullableString("nc_id");
            acc.TrID = reader.GetNullableString("tr_id");
            acc.NsID = reader.GetNullableString("ns_id");
            acc.Notes = reader.GetNullableString("notes");

            return acc;
        }

    }
}
