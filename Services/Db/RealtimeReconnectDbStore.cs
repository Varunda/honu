using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Health;

namespace watchtower.Services.Db {

    public class RealtimeReconnectDbStore {

        private readonly ILogger<RealtimeReconnectDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        public RealtimeReconnectDbStore(ILogger<RealtimeReconnectDbStore> logger,
            IDbHelper dbHelper) {

            _Logger = logger;
            _DbHelper = dbHelper;
        }

        /// <summary>
        ///     Insert a new <see cref="RealtimeReconnectEntry"/>
        /// </summary>
        /// <param name="entry">Parameters used to insert the new data</param>
        /// <param name="cancel">Cancellation token</param>
        public async Task Insert(RealtimeReconnectEntry entry, CancellationToken cancel) {
            if (entry.Timestamp == default) {
                throw new ArgumentException($"{nameof(entry.Timestamp)} cannot be the default value");
            }

            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO realtime_reconnect (
                    timestamp, world_id, stream_type, failed_count, duration, event_count
                ) VALUES (
                    @Timestamp, @WorldID, @StreamType, @FailedCount, @Duration, @EventCount
                );
            ");

            cmd.AddParameter("Timestamp", entry.Timestamp);
            cmd.AddParameter("WorldID", entry.WorldID);
            cmd.AddParameter("StreamType", entry.StreamType);
            cmd.AddParameter("FailedCount", entry.FailedCount);
            cmd.AddParameter("Duration", entry.Duration);
            cmd.AddParameter("EventCount", entry.EventCount);

            await cmd.ExecuteNonQueryAsync(cancel);
            await conn.CloseAsync();
        }

    }
}
