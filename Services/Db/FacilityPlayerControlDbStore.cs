using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Events;

namespace watchtower.Services.Db {

    public class FacilityPlayerControlDbStore {

        private readonly ILogger<FacilityPlayerControlDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<PlayerControlEvent> _Reader;

        public FacilityPlayerControlDbStore(ILogger<FacilityPlayerControlDbStore> logger,
            IDbHelper helper, IDataReader<PlayerControlEvent> reader) {

            _Logger = logger;
            _DbHelper = helper;
            _Reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public async Task Insert(long controlID, PlayerControlEvent ev) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO wt_ledger_player (
                    control_id, character_id, outfit_id, facility_id, timestamp
                ) VALUES (
                    @ControlID, @CharacterID, @OutfitID, @FacilityID, @Timestamp
                );
            ");

            cmd.AddParameter("ControlID", controlID);
            cmd.AddParameter("CharacterID", ev.CharacterID);
            cmd.AddParameter("OutfitID", ev.OutfitID);
            cmd.AddParameter("FacilityID", ev.FacilityID);
            cmd.AddParameter("Timestamp", ev.Timestamp);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

        public async Task<List<PlayerControlEvent>> GetByCharacterIDPeriod(string charID, DateTime start, DateTime end) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT p.*, l.old_faction_id, l.new_faction_id, l.world_id, l.zone_id
                    FROM wt_ledger_player p
                        INNER JOIN wt_ledger l ON l.id = p.control_id
                    WHERE p.character_id = @CharacterID
                        AND l.timestamp BETWEEN @PeriodStart AND @PeriodEnd
            ");

            cmd.AddParameter("CharacterID", charID);
            cmd.AddParameter("PeriodStart", start);
            cmd.AddParameter("PeriodEnd", end);

            List<PlayerControlEvent> evs = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return evs;
        }

        public async Task<List<PlayerControlEvent>> GetByCharacterIDsPeriod(List<string> IDs, DateTime start, DateTime end) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT p.*, l.old_faction_id, l.new_faction_id, l.world_id, l.zone_id
                    FROM wt_ledger_player p
                        INNER JOIN wt_ledger l ON l.id = p.control_id
                    WHERE p.character_id = ANY(@IDs)
                        AND l.timestamp BETWEEN @PeriodStart AND @PeriodEnd
            ");

            cmd.AddParameter("IDs", IDs);
            cmd.AddParameter("PeriodStart", start);
            cmd.AddParameter("PeriodEnd", end);

            cmd.CommandTimeout = 300;

            List<PlayerControlEvent> evs = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return evs;
        }

        public async Task InsertMany(long controlID, List<PlayerControlEvent> ev) {
            if (ev.Count == 0) {
                return;
            }

            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @$"
                INSERT INTO wt_ledger_player (
                    control_id, character_id, outfit_id, facility_id, timestamp
                ) VALUES
                    {string.Join(",\n", ev.Select((_, index) => $"(@ControlID, @CharacterID_{index}, @OutfitID_{index}, @FacilityID, @Timestamp_{index})"))}
                ;
            ");

            cmd.AddParameter("ControlID", controlID);
            cmd.AddParameter("FacilityID", ev[0].FacilityID);

            // Hopefully there's no limit to parameters lol
            for (int i = 0; i < ev.Count; ++i) {
                cmd.AddParameter($"CharacterID_{i}", ev[i].CharacterID);
                cmd.AddParameter($"OutfitID_{i}", ev[i].OutfitID);
                cmd.AddParameter($"Timestamp_{i}", ev[i].Timestamp);
            }

            //_Logger.LogDebug(cmd.Print());

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

        public async Task<List<PlayerControlEvent>> GetByEventID(long controlID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT p.*, l.old_faction_id, l.new_faction_id, l.world_id, l.zone_id
                    FROM wt_ledger_player p
                        INNER JOIN wt_ledger l ON l.id = p.control_id
                    WHERE l.id = @ControlID
            ");

            cmd.AddParameter("ControlID", controlID);

            List<PlayerControlEvent> evs = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return evs;
        }

        public async Task<List<PlayerControlEvent>> LoadWrapped(string charID, DateTime year) {
            string db = $"wrapped_{year:yyyy}";

            using NpgsqlConnection conn = _DbHelper.Connection(db);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, $@"
                SELECT p.*, l.old_faction_id, l.new_faction_id, l.world_id, l.zone_id
                    FROM wt_ledger_player_{year:yyyy} p
                        INNER JOIN wt_ledger l ON l.id = p.control_id
                    WHERE character_id = @CharID;
            ");

            cmd.AddParameter("CharID", charID);

            List<PlayerControlEvent> evs = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return evs;
        }

    }
}
