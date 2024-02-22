using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Db;

namespace watchtower.Services.Db {

    public class WorldZonePopulationDbStore {

        private readonly ILogger<WorldZonePopulationDbStore> _Logger;
        private readonly IDataReader<WorldZonePopulation> _Reader;
        private readonly IDbHelper _DbHelper;

        public WorldZonePopulationDbStore(ILogger<WorldZonePopulationDbStore> logger,
            IDataReader<WorldZonePopulation> reader, IDbHelper dbHelper) {

            _Logger = logger;
            _Reader = reader;
            _DbHelper = dbHelper;
        }

        /// <summary>
        ///     Insert a new <see cref="WorldZonePopulation"/>
        /// </summary>
        /// <param name="pop">Parameters used to insert</param>
        /// <returns>
        ///     A task when the operation is complete
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     If <see cref="WorldZonePopulation.WorldID"/> or <see cref="WorldZonePopulation.ZoneID"/> of <paramref name="pop"/> is equal to 0
        /// </exception>
        public async Task Insert(WorldZonePopulation pop) {
            if (pop.WorldID == 0) {
                throw new ArgumentException($"{nameof(WorldZonePopulation.WorldID)} is 0");
            }

            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.EVENTS);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO world_zone_population (
                    world_id, zone_id, timestamp,
                    total, faction_vs, faction_nc, faction_tr, faction_ns,
                    team_vs, team_nc, team_tr, team_unknown
                ) VALUES (
                    @WorldID, @ZoneID, @Timestamp,
                    @Total, @FactionVs, @FactionNc, @FactionTr, @FactionNs,
                    @TeamVs, @TeamNc, @TeamTr, @TeamUnknown
                );
            ");

            cmd.AddParameter("WorldID", pop.WorldID);
            cmd.AddParameter("ZoneID", pop.ZoneID);
            cmd.AddParameter("Timestamp", pop.Timestamp);
            cmd.AddParameter("Total", pop.Total);
            cmd.AddParameter("FactionVs", pop.FactionVs);
            cmd.AddParameter("FactionNc", pop.FactionNc);
            cmd.AddParameter("FactionTr", pop.FactionTr);
            cmd.AddParameter("FactionNs", pop.FactionNs);
            cmd.AddParameter("TeamVs", pop.TeamVs);
            cmd.AddParameter("TeamNc", pop.TeamNc);
            cmd.AddParameter("TeamTr", pop.TeamTr);
            cmd.AddParameter("TeamUnknown", pop.TeamUnknown);

            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        ///     Get the <see cref="WorldZonePopulation"/> for a specific world over a time period
        /// </summary>
        /// <param name="worldID">ID of the world</param>
        /// <param name="start">start of the period (inclusive)</param>
        /// <param name="end">end period (exclusive)</param>
        /// <returns>
        ///     A list of <see cref="WorldZonePopulation"/>
        /// </returns>
        public async Task<List<WorldZonePopulation>> GetByWorld(short worldID, DateTime start, DateTime end) {
            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.EVENTS);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM world_zone_population
                    WHERE world_id = @WorldID
                        AND timestamp BETWEEN @Start and @End;
            ");

            cmd.AddParameter("WorldID", worldID);
            cmd.AddParameter("Start", start);
            cmd.AddParameter("End", end);

            await cmd.PrepareAsync();

            List<WorldZonePopulation> pops = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return pops;
        }

        /// <summary>
        ///     Delete all entries in the table before or on a certain timestamp
        /// </summary>
        /// <param name="timestamp">Timestamp to delete all entries before</param>
        /// <returns>
        ///     A task for when the operation is complete
        /// </returns>
        public async Task DeleteBefore(DateTime timestamp) {
            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.EVENTS);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                DELETE 
                    FROM world_zone_population
                    WHERE timestamp <= @Timestamp;
            ");

            cmd.AddParameter("Timestamp", timestamp);
            await cmd.PrepareAsync();

            await cmd.ExecuteNonQueryAsync();
        }

    }
}
