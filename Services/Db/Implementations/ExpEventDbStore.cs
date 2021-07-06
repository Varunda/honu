using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Constants;
using watchtower.Models.Db;
using watchtower.Models.Events;

namespace watchtower.Services.Db.Implementations {

    public class ExpEventDbStore : IDataReader<ExpDbEntry>, IExpEventDbStore {

        private readonly ILogger<ExpEventDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        public ExpEventDbStore(ILogger<ExpEventDbStore> logger,
            IDbHelper dbHelper) {

            _Logger = logger;
            _DbHelper = dbHelper;
        }

        public async Task<long> Insert(ExpEvent ev) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO wt_exp (
                    source_character_id, experience_id, source_loadout_id, source_faction_id, source_team_id,
                    other_id,
                    amount,
                    world_id, zone_id,
                    timestamp
                ) VALUES (
                    @SourceCharacterID, @ExperienceID, @SourceLoadoutID, @SourceFactionID, @SourceTeamID,
                    @OtherID,
                    @Amount,
                    @WorldID, @ZoneID,
                    @Timestamp
                ) RETURNING id;
            ");

            cmd.AddParameter("SourceCharacterID", ev.SourceID);
            cmd.AddParameter("ExperienceID", ev.ExperienceID);
            cmd.AddParameter("SourceLoadoutID", ev.LoadoutID);
            cmd.AddParameter("SourceFactionID", Loadout.GetFaction(ev.LoadoutID));
            cmd.AddParameter("SourceTeamID", ev.TeamID);
            cmd.AddParameter("OtherID", ev.OtherID);
            cmd.AddParameter("Amount", ev.Amount);
            cmd.AddParameter("WorldID", ev.WorldID);
            cmd.AddParameter("ZoneID", ev.ZoneID);
            cmd.AddParameter("Timestamp", ev.Timestamp);

            object? objID = await cmd.ExecuteScalarAsync();
            if (objID != null && int.TryParse(objID.ToString(), out int ID) == true) {
                return ID;
            } else {
                throw new Exception($"Missing or bad type on 'id': {objID} {objID?.GetType()}");
            }
        }

        public async Task<List<ExpDbEntry>> GetEntries(ExpEntryOptions parameters) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT source_character_id AS id, COUNT(source_character_id) AS count
	                FROM wt_exp
	                WHERE world_id = @WorldID
                        AND (timestamp + (@Interval || ' minutes')::INTERVAl) >= NOW() at time zone 'utc'
		                AND experience_id = ANY(@ExperienceIDs)
                        AND source_team_id = @FactionID
	                GROUP BY source_character_id
	                ORDER BY COUNT(source_character_id) DESC
	                LIMIT 5;
            ");

            cmd.AddParameter("Interval", parameters.Interval);
            cmd.AddParameter("WorldID", parameters.WorldID);
            cmd.AddParameter("ExperienceIDs", parameters.ExperienceIDs);
            cmd.AddParameter("FactionID", parameters.FactionID);

            List<ExpDbEntry> entries = await ReadList(cmd);

            return entries;
        }

        public async Task<List<ExpDbEntry>> GetTopOutfits(ExpEntryOptions options) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                WITH evs AS (
                    SELECT e.id, e.world_id, e.zone_id, COALESCE(c.outfit_id, '') AS outfit_id
                        FROM wt_exp e
                            JOIN wt_character c ON e.source_character_id = c.id
                        WHERE (e.timestamp + (@Interval || ' minutes')::INTERVAL) >= NOW() at time zone 'utc'
                            AND e.world_id = @WorldID
                            AND e.source_team_id = @FactionID
                            AND e.experience_id = ANY(@ExperienceIDs)
                ), outfits AS (
                    SELECT outfit_id
                        FROM evs
                        GROUP BY outfit_id
                )
                SELECT outfits.outfit_id AS id,
                    (SELECT COUNT(*) FROM evs e WHERE e.outfit_id = outfits.outfit_id) AS count
                    FROM outfits
                    ORDER BY count DESC
            ");

            cmd.AddParameter("Interval", options.Interval);
            cmd.AddParameter("WorldID", options.WorldID);
            cmd.AddParameter("ExperienceIDs", options.ExperienceIDs);
            cmd.AddParameter("FactionID", options.FactionID);

            List<ExpDbEntry> entries = await ReadList(cmd);

            return entries;
        }

        public override ExpDbEntry ReadEntry(NpgsqlDataReader reader) {
            ExpDbEntry entry = new ExpDbEntry();

            entry.ID = reader.GetString("id");
            entry.Count = reader.GetInt32("count");

            return entry;
        }

    }
}
