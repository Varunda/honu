using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Health;

namespace watchtower.Services.Db {

    public class RealtimeReconnectDbStore {

        private readonly ILogger<RealtimeReconnectDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<RealtimeReconnectEntry> _Reader;

        public RealtimeReconnectDbStore(ILogger<RealtimeReconnectDbStore> logger,
            IDbHelper dbHelper, IDataReader<RealtimeReconnectEntry> reader) {

            _Logger = logger;
            _DbHelper = dbHelper;
            _Reader = reader;
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

        /// <summary>
        ///     Get the entries for a world within an interval
        /// </summary>
        /// <param name="worldID">ID of the world</param>
        /// <param name="start">Start period</param>
        /// <param name="end">End period</param>
        /// <returns>
        ///     A list of <see cref="RealtimeReconnectEntry"/>s with <see cref="RealtimeReconnectEntry.WorldID"/> of <paramref name="worldID"/>,
        ///     and a <see cref="RealtimeReconnectEntry.Timestamp"/> between <paramref name="start"/> and <paramref name="end"/>
        /// </returns>
        public async Task<List<RealtimeReconnectEntry>> GetByInterval(short worldID, DateTime start, DateTime end) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM realtime_reconnect
                    WHERE
                        world_id = @WorldID
                        AND timestamp BETWEEN @PeriodStart AND @PeriodEnd;
            ");

            cmd.AddParameter("WorldID", worldID);
            cmd.AddParameter("PeriodStart", start);
            cmd.AddParameter("PeriodEnd", end);

            List<RealtimeReconnectEntry> entries = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return entries;
        }

    }
}
