using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.CharacterViewer.WeaponStats;

namespace watchtower.Services.Db {

    /// <summary>
    ///     Service to interact with the character_weapon table
    /// </summary>
    public class CharacterWeaponStatDbStore : IDataReader<WeaponStatEntry> {

        private readonly ILogger<CharacterWeaponStatDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        public CharacterWeaponStatDbStore(ILogger<CharacterWeaponStatDbStore> logger,
                IDbHelper helper) {

            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _DbHelper = helper ?? throw new ArgumentNullException(nameof(helper));
        }

        /// <summary>
        ///     Get the <see cref="WeaponStatEntry"/>s for a character
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <returns>
        ///     A list of all the <see cref="WeaponStatEntry"/> with <see cref="WeaponStatEntry.CharacterID"/>
        ///     of <paramref name="charID"/>
        /// </returns>
        public async Task<List<WeaponStatEntry>> GetByCharacterID(string charID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT weapon_stats.*
	                FROM weapon_stats
		                LEFT JOIN wt_item i ON CAST(weapon_stats.item_id as int) = i.id
	                WHERE weapon_stats.character_id = @CharID
		                AND (weapon_stats.kills > 0 OR weapon_stats.seconds_with > 0)
            ");

            cmd.AddParameter("CharID", charID);

            List<WeaponStatEntry> entry = await ReadList(cmd);
            await conn.CloseAsync();

            return entry;
        }

        /// <summary>
        ///     Get the top performers with a weapon. This is meant to be used internally and isn't commented :)
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="column"></param>
        /// <param name="worlds"></param>
        /// <param name="factions"></param>
        /// <param name="minKills"></param>
        /// <returns></returns>
        public async Task<List<WeaponStatEntry>> GetTopEntries(string itemID, string column, List<short> worlds, List<short> factions, int minKills = 1159) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @$"
                SELECT weapon_stats.*
                    FROM weapon_stats
                        {(worlds.Count > 0 || factions.Count > 0 ? "INNER JOIN wt_character c ON c.id = weapon_stats.character_id" : "")}
                    WHERE weapon_stats.item_id = @ItemID
                        AND kills > @MinKills
                        {(worlds.Count > 0 ? "AND c.world_id = ANY(@WorldID) " : "")}
                        {(factions.Count > 0 ? "AND c.faction_id = ANY(@FactionID) " : "")}
                    ORDER BY {column} DESC
                    LIMIT 100;
            ");

            cmd.AddParameter("ItemID", itemID);
            cmd.AddParameter("WorldID", worlds.Count == 0 ? null : worlds);
            cmd.AddParameter("FactionID", factions.Count == 0 ? null : factions);
            cmd.AddParameter("MinKills", minKills);

            //_Logger.LogDebug(cmd.Print());

            List<WeaponStatEntry> entry = await ReadList(cmd);
            await conn.CloseAsync();

            return entry;
        }

        /// <summary>
        ///     Get all the <see cref="WeaponStatEntry"/> for a weapon
        /// </summary>
        /// <param name="itemID">ID of the weapon</param>
        /// <param name="minKills">Minimum number of kills to be included</param>
        /// <returns>
        ///     A list of all <see cref="WeaponStatEntry"/> with <see cref="WeaponStatEntry.WeaponID"/> of <paramref name="itemID"/>
        /// </returns>
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

        /// <summary>
        ///     Update or insert an entry
        /// </summary>
        /// <param name="entry">Entry to upsert</param>
        public async Task Upsert(WeaponStatEntry entry) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO weapon_stats (
                    character_id, item_id, vehicle_id, kills, deaths, shots, shots_hit, headshots, vehicle_kills, seconds_with, kd, kpm, acc, hsr, vkpm
                ) VALUES (
                    @CharID, @ItemID, @VehicleID, @Kills, @Deaths, @Shots, @ShotsHit, @Headshots, @VehicleKills, @SecondsWith, @KD, @KPM, @ACC, @HSR, @VKPM
                ) ON CONFLICT (character_id, item_id, vehicle_id) DO
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
                        AND weapon_stats.vehicle_id = @VehicleID;
            ");

            decimal kd = entry.Kills / Math.Max(1m, entry.Deaths);
            decimal kpm = entry.Kills / (Math.Max(1m, entry.SecondsWith) / 60m);
            decimal acc = entry.ShotsHit / Math.Max(1m, entry.Shots);
            decimal hsr = entry.Headshots / Math.Max(1m, entry.Kills);
            decimal vkpm = entry.VehicleKills / (Math.Max(1m, entry.SecondsWith) / 60m);

            cmd.AddParameter("CharID", entry.CharacterID);
            cmd.AddParameter("ItemID", entry.WeaponID);
            cmd.AddParameter("VehicleID", entry.VehicleID);
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

        public async Task UpsertMany(string characterID, List<WeaponStatEntry> entries) {
            if (entries.Count == 0) {
                return;
            }

            List<string> existingIDs = entries.Select(iter => iter.WeaponID).ToList();
            List<int> vehicleIDs = entries.Select(iter => iter.VehicleID).Distinct().ToList();

            /*
                item_id IN ('0', {string.Join(",", existingIDs.Select(iter => $"'{iter}'"))})
                OR vehicle_id IN (0, {string.Join(",", vehicleIDs.Select(iter => iter))})
            */

            string GetDeleteEntry(WeaponStatEntry entry) {
                if (entry.WeaponID != "0") {
                    return $"(item_id = '{entry.WeaponID}')";
                }

                return $"(item_id = '0' AND vehicle_id = {entry.VehicleID})";
            }

            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, $@"
                BEGIN;

                DELETE FROM weapon_stats
                    WHERE character_id = @CharacterID
                        AND (
                            {string.Join(" \nOR ", entries.Select(iter => GetDeleteEntry(iter)))}
                        );

                INSERT INTO weapon_stats (
                    character_id, item_id, vehicle_id,
                    kills, deaths,
                    shots, shots_hit, headshots, vehicle_kills, seconds_with,
                    kd, kpm, acc, hsr, vkpm
                ) VALUES    
                    {string.Join(",", entries.Select((iter, index) => $"(@CharacterID, @ItemID_{index}, @VehicleID_{index}, "
                        + $"@Kills_{index}, @Deaths_{index}, "
                        + $"@Shots_{index}, @ShotsHit_{index}, @Headshots_{index}, @VehicleKills_{index}, @SecondsWith_{index}, "
                        + $"@KD_{index}, @KPM_{index}, @Accuracy_{index}, @HeadshotRatio_{index}, @VKPM_{index})\n"
                    ))}
                ;

                COMMIT;
            ");

            cmd.AddParameter("CharacterID", characterID);

            /*
            if (entries.Count < 200) {
                string s = $"";
                foreach (WeaponStatEntry entry in entries) {
                    s += $"{entry.WeaponID}:{entry.VehicleID}\n";
                }
                _Logger.LogDebug(s);
            }
            */

            for (int i = 0; i < entries.Count; ++i) {
                WeaponStatEntry entry = entries[i];

                decimal kd = entry.Kills / Math.Max(1m, entry.Deaths);
                decimal kpm = entry.Kills / (Math.Max(1m, entry.SecondsWith) / 60m);
                decimal acc = entry.ShotsHit / Math.Max(1m, entry.Shots);
                decimal hsr = entry.Headshots / Math.Max(1m, entry.Kills);
                decimal vkpm = entry.VehicleKills / (Math.Max(1m, entry.SecondsWith) / 60m);

                cmd.AddParameter($"ItemID_{i}", entry.WeaponID);
                cmd.AddParameter($"VehicleID_{i}", entry.VehicleID);
                cmd.AddParameter($"Kills_{i}", entry.Kills);
                cmd.AddParameter($"Deaths_{i}", entry.Deaths);
                cmd.AddParameter($"Shots_{i}", entry.Shots);
                cmd.AddParameter($"ShotsHit_{i}", entry.ShotsHit);
                cmd.AddParameter($"Headshots_{i}", entry.Headshots);
                cmd.AddParameter($"VehicleKills_{i}", entry.VehicleKills);
                cmd.AddParameter($"SecondsWith_{i}", entry.SecondsWith);
                cmd.AddParameter($"KD_{i}", kd);
                cmd.AddParameter($"KPM_{i}", kpm);
                cmd.AddParameter($"Accuracy_{i}", acc);
                cmd.AddParameter($"HeadshotRatio_{i}", hsr);
                cmd.AddParameter($"VKPM_{i}", vkpm);
            }

            /*
            if (entries.Count < 200) {
                _Logger.LogDebug(cmd.Print());
            }
            */

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

        public override WeaponStatEntry ReadEntry(NpgsqlDataReader reader) {
            WeaponStatEntry entry = new WeaponStatEntry();

            entry.CharacterID = reader.GetString("character_id");
            entry.WeaponID = reader.GetString("item_id");
            entry.VehicleID = reader.GetInt32("vehicle_id");
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

    public static class ICharacterWeaponStatDbStoreExtensionMethods {

        public static Task<List<WeaponStatEntry>> GetTopKD(this CharacterWeaponStatDbStore repo, string itemID, List<short> worlds, List<short> factions, int minKills = 1159) {
            return repo.GetTopEntries(itemID, "kd", worlds, factions, minKills);
        }

        public static Task<List<WeaponStatEntry>> GetTopKPM(this CharacterWeaponStatDbStore repo, string itemID, List<short> worlds, List<short> factions, int minKills = 1159) {
            return repo.GetTopEntries(itemID, "kpm", worlds, factions, minKills);
        }

        public static Task<List<WeaponStatEntry>> GetTopAccuracy(this CharacterWeaponStatDbStore repo, string itemID, List<short> worlds, List<short> factions, int minKills = 1159) {
            return repo.GetTopEntries(itemID, "acc", worlds, factions, minKills);
        }

        public static Task<List<WeaponStatEntry>> GetTopHeadshotRatio(this CharacterWeaponStatDbStore repo, string itemID, List<short> worlds, List<short> factions, int minKills = 1159) {
            return repo.GetTopEntries(itemID, "hsr", worlds, factions, minKills);
        }

        public static Task<List<WeaponStatEntry>> GetTopKills(this CharacterWeaponStatDbStore repo, string itemID, List<short> worlds, List<short> factions) {
            return repo.GetTopEntries(itemID, "kills", worlds, factions, 0);
        }
    }

}
