using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.CharacterViewer.WeaponStats;

namespace watchtower.Services.Db.Implementations {

    public class CharacterWeaponStatDbStore : IDataReader<WeaponStatEntry>, ICharacterWeaponStatDbStore {

        private readonly ILogger<CharacterWeaponStatDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        public CharacterWeaponStatDbStore(ILogger<CharacterWeaponStatDbStore> logger,
                IDbHelper helper) {

            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _DbHelper = helper ?? throw new ArgumentNullException(nameof(helper));
        }

        public async Task<List<WeaponStatEntry>> GetByCharacterID(string charID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT weapon_stats.*
	                FROM weapon_stats
		                LEFT JOIN wt_item i ON CAST(weapon_stats.item_id as int) = i.id
	                WHERE weapon_stats.character_id = @CharID
		                AND (weapon_stats.kills > 0 OR weapon_stats.seconds_with > 299)
            ");

            cmd.AddParameter("CharID", charID);

            List<WeaponStatEntry> entry = await ReadList(cmd);
            await conn.CloseAsync();

            return entry;
        }

        public async Task<List<WeaponStatEntry>> GetTopEntries(string itemID, string column, List<short> worlds, List<short> factions, int minKills = 1159) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @$"
                WITH top AS (
                    SELECT *
                        FROM weapon_stats
                        WHERE item_id = @ItemID
                            AND kills > @MinKills
                        ORDER BY {column} DESC
                        LIMIT 100
                )
                SELECT *
                    FROM top ws
                        INNER JOIN wt_character c ON ws.character_id = c.id
                    WHERE item_id = @ItemID
                        {(worlds.Count > 0 ? "AND c.world_id = ANY(@WorldID) " : "")}
                        {(factions.Count > 0 ? "AND c.faction_id = ANY(@FactionID) " : "")}
            ");

            cmd.AddParameter("ItemID", itemID);
            cmd.AddParameter("WorldID", worlds.Count == 0 ? null : worlds);
            cmd.AddParameter("FactionID", factions.Count == 0 ? null : factions);
            cmd.AddParameter("MinKills", minKills);

            List<WeaponStatEntry> entry = await ReadList(cmd);
            await conn.CloseAsync();

            return entry;
        }

        public async Task<List<WeaponStatEntry>> GetByItemID(string itemID, int? minKills) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM weapon_stats
                    WHERE item_id = @ItemID
                        AND kills > @MinKills
            ");

            cmd.AddParameter("ItemID", itemID);
            cmd.AddParameter("MinKills", minKills ?? 0);

            List<WeaponStatEntry> entry = await ReadList(cmd);
            await conn.CloseAsync();

            return entry;
        }

        public async Task Upsert(WeaponStatEntry entry) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO weapon_stats (
                    character_id, item_id, kills, deaths, shots, shots_hit, headshots, vehicle_kills, seconds_with, kd, kpm, acc, hsr, vkpm
                ) VALUES (
                    @CharID, @ItemID, @Kills, @Deaths, @Shots, @ShotsHit, @Headshots, @VehicleKills, @SecondsWith, @KD, @KPM, @ACC, @HSR, @VKPM
                ) ON CONFLICT (character_id, item_id) DO
                    UPDATE SET kills = @Kills,
                        deaths = @Deaths,
                        shots = @Shots,
                        shots_hit = @ShotsHit,
                        headshots = @Headshots,
                        vehicle_kills = @VehicleKills,
                        seconds_with = @SecondsWith,
                        timestamp = (NOW() at time zone 'utc'),
                        kd = @KD,
                        kpm = @KPM,
                        acc = @ACC,
                        hsr = @HSR,
                        vkpm = @VKPM
                    WHERE
                        weapon_stats.character_id = @CharID
                        AND weapon_stats.item_id = @ItemID
            ");

            decimal kd = entry.Kills / Math.Max(1m, entry.Deaths);
            decimal kpm = entry.Kills / (Math.Max(1m, entry.SecondsWith) / 60m);
            decimal acc = entry.ShotsHit / Math.Max(1m, entry.Shots);
            decimal hsr = entry.Headshots / Math.Max(1m, entry.Kills);
            decimal vkpm = entry.VehicleKills / (Math.Max(1m, entry.SecondsWith) / 60m);

            cmd.AddParameter("CharID", entry.CharacterID);
            cmd.AddParameter("ItemID", entry.WeaponID);
            cmd.AddParameter("Kills", entry.Kills);
            cmd.AddParameter("Deaths", entry.Deaths);
            cmd.AddParameter("Shots", entry.Shots);
            cmd.AddParameter("ShotsHit", entry.ShotsHit);
            cmd.AddParameter("Headshots", entry.Headshots);
            cmd.AddParameter("VehicleKills", entry.VehicleKills);
            cmd.AddParameter("SecondsWith", entry.SecondsWith);
            cmd.AddParameter("KD", kd);
            cmd.AddParameter("KPM", kpm);
            cmd.AddParameter("ACC", acc);
            cmd.AddParameter("HSR", hsr);
            cmd.AddParameter("VKPM", vkpm);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

        public override WeaponStatEntry ReadEntry(NpgsqlDataReader reader) {
            WeaponStatEntry entry = new WeaponStatEntry();

            entry.CharacterID = reader.GetString("character_id");
            entry.WeaponID = reader.GetString("item_id");
            entry.Kills = reader.GetInt32("kills");
            entry.Deaths = reader.GetInt32("deaths");
            entry.Shots = reader.GetInt32("shots");
            entry.ShotsHit = reader.GetInt32("shots_hit");
            entry.Headshots = reader.GetInt32("headshots");
            entry.VehicleKills = reader.GetInt32("vehicle_kills");
            entry.SecondsWith = reader.GetInt32("seconds_with");
            entry.Timestamp = reader.GetDateTime("timestamp");

            entry.KillDeathRatio = entry.Kills / Math.Max(1d, entry.Deaths);
            entry.KillsPerMinute = entry.Kills / (Math.Max(1d, entry.SecondsWith) / 60d);
            entry.Accuracy = entry.ShotsHit / Math.Max(1d, entry.Shots);
            entry.HeadshotRatio = entry.Headshots / Math.Max(1d, entry.Kills);
            entry.VehicleKillsPerMinute = entry.VehicleKills / (Math.Max(1d, entry.SecondsWith) / 60d);

            return entry;
        }

    }

}
