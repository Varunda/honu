using DaybreakGames.Census;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.CharacterViewer.WeaponStats;

namespace watchtower.Services.Census {

    /// <summary>
    ///     Service to get data from the /characters_weapon_stat
    /// </summary>
    public class CharacterWeaponStatCollection {

        private readonly ILogger<CharacterWeaponStatCollection> _Logger;
        private readonly ICensusQueryFactory _Census;

        public CharacterWeaponStatCollection(ILogger<CharacterWeaponStatCollection> logger,
                ICensusQueryFactory census) {

            _Logger = logger;
            _Census = census;
        }

        /// <summary>
        ///     Get all the <see cref="WeaponStatEntry"/>s for a character
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <returns>
        ///     A list from census of all the <see cref="WeaponStatEntry"/>s that exist
        /// </returns>
        public async Task<List<WeaponStatEntry>> GetByCharacterID(string charID) {
            List<WeaponStat> weaponStats = await GetWeaponStatByCharacterIDAsync(charID);
            List<WeaponStatByFactionEntry> byFaction = await GetWeaponStatByFactionByCharacterIDAsync(charID);

            //_Logger.LogDebug($"Got {weaponStats.Count} weapon stats, got {byFaction.Count} by faction");

            if (weaponStats.Count == 0 && byFaction.Count != 0) {
                _Logger.LogError($"Failed to get weapon_stats for {charID}, but have {byFaction.Count} faction stats");
            }

            Dictionary<string, WeaponStatEntry> entries = new Dictionary<string, WeaponStatEntry>();

            foreach (WeaponStat stat in weaponStats) {
                if (entries.TryGetValue(stat.ItemID, out WeaponStatEntry? entry) == false) {
                    entry = new WeaponStatEntry {
                        CharacterID = charID,
                        WeaponID = stat.ItemID
                    };
                    entries.Add(stat.ItemID, entry);
                }

                if (stat.StatName == "weapon_play_time") {
                    entry.SecondsWith = stat.Value;
                } else if (stat.StatName == "weapon_hit_count") {
                    entry.ShotsHit = stat.Value;
                    entry.Accuracy = entry.ShotsHit / Math.Max(1d, entry.Shots);
                } else if (stat.StatName == "weapon_fire_count") {
                    entry.Shots = stat.Value;
                    entry.Accuracy = entry.ShotsHit / Math.Max(1d, entry.Shots);
                } else if (stat.StatName == "weapon_deaths") {
                    entry.Deaths = stat.Value;
                } else {
                    throw new ArgumentException($"Invalid StatName '{stat.StatName}' passed. Expencted 'weapon_play_time' | 'weapon_hit_count' | 'weapon_fire_count' | 'weapon_deaths'");
                }
            }

            //entry.VehicleKillsPerMinute = entry.VehicleKills / (Math.Max(1m, entry.SecondsWith) / 60m);

            foreach (WeaponStatByFactionEntry stat in byFaction) {
                // There are legit cases where the _by_faction stats may contain a value while characters_weapon_stat would not,
                //      such as getting a kill with a weapon of a different faction. For example, 5428990295173600849 is an NC
                //      character, and has a kill with the ML-7
                if (entries.TryGetValue(stat.ItemID, out WeaponStatEntry? entry) == false) {
                    //_Logger.LogError($"Missing weapon_stats for: {stat.ItemID}, from {JToken.FromObject(stat)}");
                    entry = new WeaponStatEntry {
                        CharacterID = charID,
                        WeaponID = stat.ItemID
                    };
                    entries.Add(stat.ItemID, entry);
                }

                if (stat.StatName == "weapon_kills") {
                    entry.Kills = stat.ValueNC + stat.ValueVS + stat.ValueTR;
                    entry.KillDeathRatio = entry.Kills / Math.Max(1d, entry.Deaths);
                    entry.KillsPerMinute = entry.Kills / (Math.Max(1d, entry.SecondsWith) / 60d);
                } else if (stat.StatName == "weapon_headshots") {
                    entry.Headshots = stat.ValueNC + stat.ValueVS + stat.ValueTR;
                    entry.HeadshotRatio = entry.Headshots / Math.Max(1d, entry.Kills);
                } else if (stat.StatName == "weapon_vehicle_kills") {
                    entry.VehicleKills = stat.ValueNC + stat.ValueVS + stat.ValueTR;
                    entry.VehicleKillsPerMinute = entry.VehicleKills / (Math.Max(1d, entry.SecondsWith) / 60d);
                } else {
                    throw new ArgumentException($"Invalid StatName '{stat.StatName}' passed. Expected 'weapon_kills' | 'weapon_headshots' | 'weapon_vehicle_kills'");
                }
            }

            return entries.Values.ToList();
        }

        private async Task<List<WeaponStat>> GetWeaponStatByCharacterIDAsync(string charID) {
            List<WeaponStat> weaponStats; // = new List<WeaponStat>();

            // Gives deaths, shots fired, shots hit, play time
            CensusQuery query = _Census.Create("characters_weapon_stat");
            query.Where("character_id").Equals(charID);
            query.Where("stat_name").Equals("weapon_deaths");
            query.Where("stat_name").Equals("weapon_fire_count");
            query.Where("stat_name").Equals("weapon_hit_count");
            query.Where("stat_name").Equals("weapon_play_time");
            query.SetLimit(10000);
            query.SetLanguage(CensusLanguage.English);
            query.ShowFields("character_id", "stat_name", "item_id", "value");

            //_Logger.LogTrace($"characters_weapon_stat: {query.GetUri()}");

            Uri uri = query.GetUri();
            IEnumerable<JToken> result = await query.GetListAsync();

            weaponStats = new List<WeaponStat>(result.Count());

            foreach (JToken t in result) {
                WeaponStat? s = _ParseWeaponStat(t);
                if (s != null) {
                    weaponStats.Add(s);
                }
            }

            return weaponStats;
        }

        private async Task<List<WeaponStatByFactionEntry>> GetWeaponStatByFactionByCharacterIDAsync(string charID) {
            List<WeaponStatByFactionEntry> weaponStats; // = new List<WeaponStat>();

            // Gives headshot count, kills and vehicle kills
            CensusQuery query = _Census.Create("characters_weapon_stat_by_faction");
            query.Where("character_id").Equals(charID);
            query.Where("stat_name").Equals("weapon_headshots");
            query.Where("stat_name").Equals("weapon_kills");
            query.Where("stat_name").Equals("weapon_vehicle_kills");
            query.SetLimit(10000);
            query.SetLanguage(CensusLanguage.English);
            query.ShowFields("character_id", "stat_name", "item_id", "value_vs", "value_nc", "value_tr", "vehicle_id");

            //_Logger.LogTrace($"characters_weapon_stat_by_faction: {query.GetUri()}");

            Uri uri = query.GetUri();
            IEnumerable<JToken> result = await query.GetListAsync();

            weaponStats = new List<WeaponStatByFactionEntry>(result.Count());

            foreach (JToken t in result) {
                WeaponStatByFactionEntry? s = _ParseWeaponByFaction(t);
                if (s != null) { 
                    weaponStats.Add(s);
                }
            }

            return weaponStats;
        }

        private WeaponStat? _ParseWeaponStat(JToken token) {
            if (token == null) {
                return null;
            }

            WeaponStat stat = new WeaponStat {
                CharacterID = token.GetString("character_id", "0"),
                StatName = token.GetString("stat_name", ""),
                ItemID = token.GetString("item_id", "0"),
                VehicleID = token.GetString("vehicle_id", "0"),
                Value = token.GetInt32("value", 0)
            };

            //_Logger.LogInformation($"{token} => {stat.CharacterID} {stat.StatName} {stat.ItemID} {stat.VehicleID} {stat.Value}");

            return stat;
        }

        public WeaponStatByFactionEntry? _ParseWeaponByFaction(JToken token) {
            if (token == null) {
                return null;
            }

            WeaponStatByFactionEntry stat = new WeaponStatByFactionEntry {
                CharacterID = token.GetString("character_id", "0"),
                StatName = token.GetString("stat_name", ""),
                ItemID = token.GetString("item_id", ""),
                VehicleID = token.GetString("vehicle_id", "0"),
                ValueVS = int.Parse(token.Value<string?>("value_vs") ?? "0"),
                ValueNC = int.Parse(token.Value<string?>("value_nc") ?? "0"),
                ValueTR = int.Parse(token.Value<string?>("value_tr") ?? "0"),
            };

            //_Logger.LogInformation($"{token} => {stat.CharacterID} {stat.StatName} {stat.ItemID} {stat.VehicleID} {stat.ValueVS} {stat.ValueNC} {stat.ValueTR}");

            return stat;
        }

    }
}
