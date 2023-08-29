using Microsoft.Extensions.Logging;
using Npgsql;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.PSB;

namespace watchtower.Services.Db {

    public class PsbParsedReservationDbStore {

        private readonly ILogger<PsbParsedReservationDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<PsbParsedReservationMetadata> _Reader;

        public PsbParsedReservationDbStore(ILogger<PsbParsedReservationDbStore> logger,
            IDbHelper dbHelper, IDataReader<PsbParsedReservationMetadata> reader) {

            _Logger = logger;
            _DbHelper = dbHelper;
            _Reader = reader;
        }

        /// <summary>
        ///     Get a single <see cref="PsbParsedReservationMetadata"/> by its <see cref="PsbParsedReservationMetadata.MessageID"/>
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public async Task<PsbParsedReservationMetadata?> GetByID(ulong ID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM psb_parsed_reservations
                    WHERE message_id = @ID;
            ");

            cmd.AddParameter("ID", ID);

            PsbParsedReservationMetadata? meta = await _Reader.ReadSingle(cmd);
            await conn.CloseAsync();

            return meta;
        }

        public async Task Upsert(PsbParsedReservationMetadata meta) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO psb_parsed_reservations (
                    message_id, account_sheet_id, account_sheet_approved_by, booking_approved_by, override_by
                ) VALUES (
                    @MessageID, @AccountSheetID, @AccountSheetApproved, @BookingsApprovedBy, @OverrideBy
                ) ON CONFLICT (message_id)
                    DO UPDATE set 
                        account_sheet_id = @AccountSheetID,
                        account_sheet_approved_by = @AccountSheetApproved,
                        booking_approved_by = @BookingsApprovedBy,
                        override_by = @OverrideBy;
            ");

            cmd.AddParameter("MessageID", meta.MessageID);
            cmd.AddParameter("AccountSheetID", meta.AccountSheetId);
            cmd.AddParameter("AccountSheetApproved", meta.AccountSheetApprovedById);
            cmd.AddParameter("BookingsApprovedBy", meta.BookingApprovedById);
            cmd.AddParameter("OverrideBy", meta.OverrideById);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

        public async Task<PsbParsedReservationMetadata> GetOrCreate(ulong msgID) {
            PsbParsedReservationMetadata? meta = await GetByID(msgID);
            if (meta == null) {
                meta = new PsbParsedReservationMetadata() {
                    MessageID = msgID
                };
                await Upsert(meta);
            }

            return meta;
        }

    }
}
