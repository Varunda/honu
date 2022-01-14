using Npgsql;
using System.Data;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.PSB;

namespace watchtower.Services.Db.Readers.PSB {

    public class PsbAccountNoteReader : IDataReader<PsbAccountNote> {

        public override PsbAccountNote? ReadEntry(NpgsqlDataReader reader) {
            PsbAccountNote note = new PsbAccountNote();

            note.ID = reader.GetInt64("id");
            note.AccountID = reader.GetInt64("account_id");
            note.HonuID = reader.GetInt64("honu_id");
            note.Timestamp = reader.GetDateTime("timestamp");
            note.Message = reader.GetString("message");
            note.DeletedAt = reader.GetNullableDateTime("deleted_at");
            note.DeletedBy = reader.GetNullableInt64("deleted_by");
            note.EditedAt = reader.GetNullableDateTime("edited_at");

            return note;
        }

    }
}
