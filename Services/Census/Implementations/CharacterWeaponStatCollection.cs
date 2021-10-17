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

namespace watchtower.Services.Census.Implementations {

    public class CharacterWeaponStatCollection : ICharacterWeaponStatCollection {

        private readonly ILogger<CharacterWeaponStatCollection> _Logger;

        private readonly ICensusQueryFactory _Census;

        public CharacterWeaponStatCollection(ILogger<CharacterWeaponStatCollection> logger,
                ICensusQueryFactory census) {

            _Logger = logger;
            _Census = census;
        }

        public async Task<List<WeaponStatEntry>> GetByCharacterID(string charID) {
            List<WeaponStat> weaponStats = await GetWeaponStatByCharacterIDAsync(charID);
            List<WeaponStatByFactionEntry> byFaction = await GetWeaponStatByFactionByCharacterIDAsync(charID);

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
                }
            }

            //entry.VehicleKillsPerMinute = entry.VehicleKills / (Math.Max(1m, entry.SecondsWith) / 60m);

            foreach (WeaponStatByFactionEntry stat in byFaction) {
                if (entries.TryGetValue(stat.ItemID, out WeaponStatEntry? entry) == false) {
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
                }
            }

            return entries.Values.ToList();
        }

        private async Task<List<WeaponStat>> GetWeaponStatByCharacterIDAsync(string charID) {
            List<WeaponStat> weaponStats; // = new List<WeaponStat>();

            CensusQuery query = _Census.Create("characters_weapon_stat");
            query.Where("character_id").Equals(charID);
            query.SetLimit(10000);
            query.SetLanguage(CensusLanguage.English);
            query.ShowFields("character_id", "stat_name", "item_id", "value");

            try {
                Uri uri = query.GetUri();
                IEnumerable<JToken> result = await query.GetListAsync();

                weaponStats = new List<WeaponStat>(result.Count());

                foreach (JToken t in result) {
                    WeaponStat? s = _ParseWeaponStat(t);
                    if (s != null) {
                        weaponStats.Add(s);
                    }
                }
            } catch (Exception ex) {
                weaponStats = new List<WeaponStat>();
                _Logger.LogError(ex, "Failed to get {CharID}", charID);
            }

            return weaponStats;
        }

        private async Task<List<WeaponStatByFactionEntry>> GetWeaponStatByFactionByCharacterIDAsync(string charID) {
            List<WeaponStatByFactionEntry> weaponStats; // = new List<WeaponStat>();

            CensusQuery query = _Census.Create("characters_weapon_stat_by_faction");
            query.Where("character_id").Equals(charID);
            query.SetLimit(10000);
            query.SetLanguage(CensusLanguage.English);
            query.ShowFields("character_id", "stat_name", "item_id", "value_vs", "value_nc", "value_tr", "vehicle_id");

            try {
                Uri uri = query.GetUri();
                IEnumerable<JToken> result = await query.GetListAsync();

                weaponStats = new List<WeaponStatByFactionEntry>(result.Count());

                foreach (JToken t in result) {
                    WeaponStatByFactionEntry? s = _ParseWeaponByFaction(t);
                    if (s != null) { 
                        weaponStats.Add(s);
                    }
                }
            } catch (Exception ex) {
                weaponStats = new List<WeaponStatByFactionEntry>();
                _Logger.LogError(ex, "Failed to get {CharID}", charID);
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

            //_Logger.LogInformation($"{token} {stat.CharacterID} {stat.StatName} {stat.ItemID} {stat.VehicleID} {stat.ValueVS} {stat.ValueNC} {stat.ValueTR}");

            return stat;
        }

    }
}
