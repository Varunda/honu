using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Db;

namespace watchtower.Services.Db {

    public class PopulationDbStore {

        private readonly ILogger<PopulationDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<PopulationEntry> _Reader;
        private readonly IDataReader<PopulationCount> _CountReader;

        public PopulationDbStore(ILogger<PopulationDbStore> logger,
            IDbHelper dbHelper, IDataReader<PopulationEntry> reader,
            IDataReader<PopulationCount> countReader) {

            _Logger = logger;
            _DbHelper = dbHelper;
            _Reader = reader;
            _CountReader = countReader;
        }

        /// <summary>
        ///     Get all <see cref="PopulationEntry"/>s that occured at a specific time
        /// </summary>
        /// <param name="timestamp">
        ///     Timestamp to check. No truncation takes place, so if population data is aligned to an hour,
        ///     <paramref name="timestamp"/> must be aligned as well, with no minutes, seconds and milliseconds
        /// </param>
        /// <returns>
        ///     A list of <see cref="PopulationEntry"/>s with <see cref="PopulationEntry.Timestamp"/> of <paramref name="timestamp"/>
        /// </returns>
        public async Task<List<PopulationEntry>> GetByTimestamp(DateTime timestamp) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM population
                    WHERE timestamp = @Timestamp;
            ");

            cmd.AddParameter("Timestamp", timestamp);

            List<PopulationEntry> entries = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return entries;
        }

        public async Task<List<PopulationEntry>> GetByTimestampRange(DateTime start, DateTime end) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM population
                    WHERE timestamp BETWEEN @Start AND @End;
            ");

            cmd.AddParameter("Start", start);
            cmd.AddParameter("End", end);

            List<PopulationEntry> entries = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return entries;

        }

        /// <summary>
        ///     Insert a new <see cref="PopulationEntry"/> into the DB, returning the ID of the row
        /// </summary>
        /// <param name="entry">Parameters used to insert</param>
        /// <returns>
        ///     The ID of the <see cref="PopulationEntry"/> that was just inserted
        /// </returns>
        public async Task<long> Insert(PopulationEntry entry) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO population (
                    timestamp, duration,
                    world_id, faction_id,
                    total, logins, logouts,
                    unique_characters, seconds_played, average_session_length
                ) VALUES (
                    @Timestamp, @Duration,
                    @WorldID, @FactionID,
                    @Total, @Logins, @Logouts,
                    @UniqueCharacters, @SecondsPlayed, @AverageSessionLength
                ) RETURNING id;
            ");

            cmd.AddParameter("Timestamp", entry.Timestamp);
            cmd.AddParameter("Duration", entry.Duration);
            cmd.AddParameter("WorldID", entry.WorldID);
            cmd.AddParameter("FactionID", entry.FactionID);
            cmd.AddParameter("Total", entry.Total);
            cmd.AddParameter("Logins", entry.Logins);
            cmd.AddParameter("Logouts", entry.Logouts);
            cmd.AddParameter("UniqueCharacters", entry.UniqueCharacters);
            cmd.AddParameter("SecondsPlayed", entry.SecondsPlayed);
            cmd.AddParameter("AverageSessionLength", entry.AverageSessionLength);

            long id = await cmd.ExecuteInt64(CancellationToken.None);

            return id;
        }

        /// <summary>
        ///     Get how many <see cref="PopulationEntry"/>s exist at each timestamp stored
        ///     in the table. Useful when generating population data to check what timestamps
        ///     already have data
        /// </summary>
        /// <returns>
        ///     A list of <see cref="PopulationCount"/>s, which represent how many <see cref="PopulationEntry"/>s
        ///     exist at each <see cref="PopulationCount.Timestamp"/>
        /// </returns>
        public async Task<List<PopulationCount>> GetCounts() {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT timestamp, count(*) as count
                    FROM population
                    GROUP BY 1 ORDER BY 1 DESC;
            ");

            List<PopulationCount> counts = await _CountReader.ReadList(cmd);
            await conn.CloseAsync();

            return counts;
        }

    }
}
