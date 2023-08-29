using Npgsql;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.PSB;

namespace watchtower.Services.Db.Readers.PSB {

    public class PsbParsedReservationReader : IDataReader<PsbParsedReservationMetadata> {

        public override PsbParsedReservationMetadata? ReadEntry(NpgsqlDataReader reader) {
            PsbParsedReservationMetadata metadata = new();

            metadata.MessageID = reader.GetUInt64("message_id");
            metadata.AccountSheetId = reader.GetNullableString("account_sheet_id");
            metadata.AccountSheetApprovedById = reader.GetNullableUInt64("account_sheet_approved_by");
            metadata.BookingApprovedById = reader.GetNullableUInt64("booking_approved_by");
            metadata.OverrideById = reader.GetNullableUInt64("override_by");

            return metadata;
        }

    }
}
