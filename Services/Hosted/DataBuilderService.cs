using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Census;
using watchtower.Constants;
using watchtower.Hubs;
using watchtower.Models;
using watchtower.Models.Events;

namespace watchtower.Services {

    public class DataBuilderService : BackgroundService {

        private const int _RunDelay = 5;

        private readonly ILogger<DataBuilderService> _Logger;

        private readonly ICharacterCollection _Characters;

        private readonly IHubContext<DataHub> _DataHub;

        private DateTime _TrackingStart = DateTime.UtcNow;

        public DataBuilderService(ILogger<DataBuilderService> logger,
            ICharacterCollection charColl, IHubContext<DataHub> hub) {

            _Logger = logger;

            _Characters = charColl;
            _DataHub = hub;
        }

        public override Task StopAsync(CancellationToken cancellationToken) {
            _Logger.LogError($"DataBuilder service stopped");
            return base.StopAsync(cancellationToken);
        }

        private Block _BuildPlayerBlock(string name, List<PsEvent> events, List<Character> characters) {
            Dictionary<string, int> entries = new Dictionary<string, int>();
            foreach (PsEvent ev in events) {
                string key = ev.CharacterID;
                if (entries.ContainsKey(key) == false) {
                    entries.Add(key, 0);
                }

                entries[key] = entries[key] + 1;
            }

            return new Block() {
                Name = name,
                Entries = entries.OrderByDescending(i => i.Value).Take(5)
                    .Select(i => {
                        Character? c = characters.Find(iter => iter.ID == i.Key);
                        return new BlockEntry() {
                            ID = i.Key,
                            Name = (c != null) ? $"{(c.OutfitTag != null ? $"[{c.OutfitTag}] " : "")}{c.Name}" : $"Missing {i.Key}",
                            Value = i.Value
                        };
                    }).ToList(),
                Total = events.Count
            };
        }

        private Block _BuildOutfitBlock(string name, List<PsEvent> events, Dictionary<string, Character> characters) {
            Dictionary<string, string> outfits = new Dictionary<string, string> {
                { "-1", "No outfit" }
            };

            Dictionary<string, int> entries = new Dictionary<string, int>();
            foreach (PsEvent ev in events) {
                bool r = characters.TryGetValue(ev.CharacterID, out Character? c);
                if (r == false || c == null) {
                    continue;
                }

                if (c.OutfitID != null) {
                    if (outfits.ContainsKey(c.OutfitID) == false) {
                        outfits.Add(c.OutfitID, $"[{c.OutfitTag}] {c.OutfitName}");
                    }
                }

                string outfitID = c.OutfitID ?? "-1";

                if (entries.ContainsKey(outfitID) == false) {
                    entries.Add(outfitID, 0);
                }
                entries[outfitID] = entries[outfitID] + 1;
            }

            return new Block() {
                Name = name,
                Entries = entries.Select(i => {
                    return new BlockEntry() {
                        ID = i.Key,
                        Name = outfits[i.Key],
                        Value = i.Value
                    };
                }).OrderByDescending(i => i.Value).Take(5).ToList(),
                Total = events.Count
            };
        }

        private Block _BuildBlock(string name, Dictionary<string, int> entries, Dictionary<string, Character> characters) {
            int total = 0;
            foreach (KeyValuePair<string, int> entry in entries) {
                total += entry.Value;
            }

            return new Block() {
                Name = name,
                Entries = entries.OrderByDescending(i => i.Value).Take(5)
                    .Select(i => {
                        characters.TryGetValue(i.Key, out Character? c);
                        return new BlockEntry() {
                            ID = i.Key,
                            Name = (c != null) ? $"{(c.OutfitTag != null ? $"[{c.OutfitTag}] " : "")}{c.Name}" : $"Missing {i.Key}",
                            Value = i.Value
                        };
                    }).ToList(),
                Total = total
            };
        }

        private Block _BuildOutfitBlock(Dictionary<string, int> entries, Dictionary<string, Character> characters) {
            Dictionary<string, string> outfits = new Dictionary<string, string> {
                { "-1", "No outfit" }
            };

            int total = 0;

            Dictionary<string, int> values = new Dictionary<string, int>();

            foreach (KeyValuePair<string, int> entry in entries) {
                bool r = characters.TryGetValue(entry.Key, out Character? c);
                if (r == false || c == null) {
                    continue;
                }

                if (c.OutfitID != null) {
                    if (outfits.ContainsKey(c.OutfitID) == false) {
                        outfits.Add(c.OutfitID, $"[{c.OutfitTag}] {c.OutfitName}");
                    }
                }

                string outfitID = c.OutfitID ?? "-1";

                if (values.ContainsKey(outfitID) == false) {
                    values.Add(outfitID, 0);
                }
                values[outfitID] = values[outfitID] + entry.Value;

                total += entry.Value;
            }

            return new Block() {
                Name = "",
                Entries = values.OrderByDescending(i => i.Value).Take(5)
                    .Select(i => {
                        return new BlockEntry() {
                            ID = i.Key,
                            Name = outfits[i.Key],
                            Value = i.Value
                        };
                    }).ToList(),
                Total = total
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            try {
                while (!stoppingToken.IsCancellationRequested) {
                    Stopwatch time = Stopwatch.StartNew();

                    long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                    WorldData data = new WorldData();

                    data.WorldID = "1";
                    data.WorldName = "Connery";
                    data.TrackingDuration = (_TrackingStart - DateTime.UtcNow).Seconds;
                    data.ContinentCount = new ContinentCount();

                    long timeToMakeBlock = time.ElapsedMilliseconds;

                    Dictionary<string, TrackedPlayer> players;
                    lock (CharacterStore.Get().Players) {
                        players = new Dictionary<string, TrackedPlayer>(CharacterStore.Get().Players);
                    }

                    //players = players.Where(iter => iter.Value.WorldID == data.WorldID).ToDictionary();

                    long timeToCopyPlayers = time.ElapsedMilliseconds;

                    List<Character> cachedCharacters = new List<Character>(_Characters.GetCache());
                    Dictionary<string, Character> characters = new Dictionary<string, Character>(cachedCharacters.Count);
                    foreach (Character ch in cachedCharacters) {
                        //if (ch.WorldID == data.WorldID) {
                            characters.Add(ch.ID, ch);
                        //}
                    }

                    long timeToBuildCharacters = time.ElapsedMilliseconds;

                    Dictionary<string, int> trKills = new Dictionary<string, int>();
                    Dictionary<string, int> trHeals = new Dictionary<string, int>();
                    Dictionary<string, int> trRevives = new Dictionary<string, int>();
                    Dictionary<string, int> trResupplies = new Dictionary<string, int>();
                    Dictionary<string, int> trRepairs = new Dictionary<string, int>();
                    Dictionary<string, int> trSpawns = new Dictionary<string, int>();

                    Dictionary<string, int> ncKills = new Dictionary<string, int>();
                    Dictionary<string, int> ncHeals = new Dictionary<string, int>();
                    Dictionary<string, int> ncRevives = new Dictionary<string, int>();
                    Dictionary<string, int> ncResupplies = new Dictionary<string, int>();
                    Dictionary<string, int> ncRepairs = new Dictionary<string, int>();
                    Dictionary<string, int> ncSpawns = new Dictionary<string, int>();

                    Dictionary<string, int> vsKills = new Dictionary<string, int>();
                    Dictionary<string, int> vsHeals = new Dictionary<string, int>();
                    Dictionary<string, int> vsRevives = new Dictionary<string, int>();
                    Dictionary<string, int> vsResupplies = new Dictionary<string, int>();
                    Dictionary<string, int> vsRepairs = new Dictionary<string, int>();
                    Dictionary<string, int> vsSpawns = new Dictionary<string, int>();

                    KillBlock trPlayerBlock = new KillBlock();
                    KillBlock ncPlayerBlock = new KillBlock();
                    KillBlock vsPlayerBlock = new KillBlock();

                    Dictionary<string, TrackedOutfit> outfits = new Dictionary<string, TrackedOutfit>();
                    OutfitKillBlock trOutfitBlock = new OutfitKillBlock();
                    OutfitKillBlock ncOutfitBlock = new OutfitKillBlock();
                    OutfitKillBlock vsOutfitBlock = new OutfitKillBlock();

                    Dictionary<string, OutfitOnlineEntry> outfitsOnline = new Dictionary<string, OutfitOnlineEntry>();

                    long timeToCtorProcessingVars = time.ElapsedMilliseconds;

                    // Set the start high cause we want to find the minimum, so go down from here
                    int start = 1999999999;

                    foreach (KeyValuePair<string, TrackedPlayer> entry in players) {
                        if (entry.Value.FactionID == Faction.TR) {
                            if (entry.Value.Heals.Count > 0) { trHeals.Add(entry.Key, entry.Value.Heals.Count); }
                            if (entry.Value.Revives.Count > 0) { trRevives.Add(entry.Key, entry.Value.Revives.Count); }
                            if (entry.Value.Repairs.Count > 0) { trRepairs.Add(entry.Key, entry.Value.Repairs.Count); }
                            if (entry.Value.Resupplies.Count > 0) { trResupplies.Add(entry.Key, entry.Value.Resupplies.Count); }
                            if (entry.Value.Spawns.Count > 0) { trSpawns.Add(entry.Key, entry.Value.Spawns.Count); }
                        } else if (entry.Value.FactionID == Faction.NC) {
                            if (entry.Value.Heals.Count > 0) { ncHeals.Add(entry.Key, entry.Value.Heals.Count); }
                            if (entry.Value.Revives.Count > 0) { ncRevives.Add(entry.Key, entry.Value.Revives.Count); }
                            if (entry.Value.Repairs.Count > 0) { ncRepairs.Add(entry.Key, entry.Value.Repairs.Count); }
                            if (entry.Value.Resupplies.Count > 0) { ncResupplies.Add(entry.Key, entry.Value.Resupplies.Count); }
                            if (entry.Value.Spawns.Count > 0) { ncSpawns.Add(entry.Key, entry.Value.Spawns.Count); }
                        } else if (entry.Value.FactionID == Faction.VS) {
                            if (entry.Value.Heals.Count > 0) { vsHeals.Add(entry.Key, entry.Value.Heals.Count); }
                            if (entry.Value.Revives.Count > 0) { vsRevives.Add(entry.Key, entry.Value.Revives.Count); }
                            if (entry.Value.Repairs.Count > 0) { vsRepairs.Add(entry.Key, entry.Value.Repairs.Count); }
                            if (entry.Value.Resupplies.Count > 0) { vsResupplies.Add(entry.Key, entry.Value.Resupplies.Count); }
                            if (entry.Value.Spawns.Count > 0) { vsSpawns.Add(entry.Key, entry.Value.Spawns.Count); }
                        }

                        if (entry.Value.Online == true) {
                            ++data.OnlineCount;

                            // Add the current interval the online has been online for
                            entry.Value.OnlineIntervals.Add(new TimestampPair() {
                                Start = (int)currentTime,
                                End = (int)currentTime + _RunDelay
                            });
                        }

                        int secondsOnline = 0;
                        foreach (TimestampPair pair in entry.Value.OnlineIntervals) {
                            secondsOnline += pair.End - pair.Start;
                        }

                        if (entry.Value.Kills.Count == 0 && entry.Value.Deaths.Count == 0) {
                            continue;
                        }

                        // Get when the most recent event timestamp was, used for KPM calculation when the
                        // tracker hasn't been running for the tracking period (2 hours currently)
                        foreach (int timestamp in entry.Value.Kills) {
                            if (timestamp <= start) {
                                start = timestamp;
                            }
                        }

                        KillData datum = new KillData() {
                            ID = entry.Value.ID,
                            Kills = entry.Value.Kills.Count,
                            Deaths = entry.Value.Deaths.Count,
                            Assists = entry.Value.Assists.Count,
                            Online = entry.Value.Online,
                            SecondsOnline = secondsOnline
                        };

                        bool r = characters.TryGetValue(entry.Value.ID, out Character? c);
                        datum.Name = (c != null) ? $"{(c.OutfitTag != null ? $"[{c.OutfitTag}] " : "")}{c.Name}" : $"Missing {entry.Key}";

                        if (r == true && c != null) {
                            if (outfits.TryGetValue(c.OutfitID ?? "-1", out TrackedOutfit? outfit) == false) { 
                                outfit = new TrackedOutfit() {
                                    ID = c.OutfitID ?? "-1",
                                    Tag = c.OutfitTag,
                                    Name = c.OutfitID == null ? "No outfit" : c.OutfitName ?? "Missing name",
                                    FactionID = c.FactionID,
                                    MembersOnline = 0
                                };

                                outfits.Add(c.OutfitID ?? "-1", outfit);
                            }

                            outfit.Kills += datum.Kills;
                            outfit.Deaths += datum.Deaths;
                            ++outfit.Members;

                            if (entry.Value.Online == true) {
                                ++outfit.MembersOnline;
                            }
                        }

                        if (entry.Value.FactionID == Faction.VS) {
                            vsPlayerBlock.Entries.Add(datum);
                            data.VS.TotalKills += entry.Value.Kills.Count;
                            data.VS.TotalDeaths += entry.Value.Deaths.Count;
                            data.VS.TotalAssists += entry.Value.Assists.Count;

                            if (entry.Value.Online == true) {
                                data.ContinentCount.AddToVS(entry.Value.ZoneID);
                            }
                        } else if (entry.Value.FactionID == Faction.NC) {
                            ncPlayerBlock.Entries.Add(datum);
                            data.NC.TotalKills += entry.Value.Kills.Count;
                            data.NC.TotalDeaths += entry.Value.Deaths.Count;
                            data.NC.TotalAssists += entry.Value.Assists.Count;

                            if (entry.Value.Online == true) {
                                data.ContinentCount.AddToNC(entry.Value.ZoneID);
                            }
                        } else if (entry.Value.FactionID == Faction.TR) {
                            trPlayerBlock.Entries.Add(datum);
                            data.TR.TotalKills += entry.Value.Kills.Count;
                            data.TR.TotalDeaths += entry.Value.Deaths.Count;
                            data.TR.TotalAssists += entry.Value.Assists.Count;

                            if (entry.Value.Online == true) {
                                data.ContinentCount.AddToTR(entry.Value.ZoneID);
                            }
                        }
                    }

                    data.TrackingDuration = (int)(DateTime.UtcNow - DateTimeOffset.FromUnixTimeSeconds(start)).TotalSeconds;

                    long timeToProcessData = time.ElapsedMilliseconds;

                    data.TR.PlayerHeals = _BuildBlock("", trHeals, characters);
                    data.TR.PlayerRevives = _BuildBlock("", trRevives, characters);
                    data.TR.PlayerResupplies = _BuildBlock("", trResupplies, characters);
                    data.TR.PlayerSpawns = _BuildBlock("", trSpawns, characters);
                    data.TR.OutfitHeals = _BuildOutfitBlock(trHeals, characters);
                    data.TR.OutfitRevives = _BuildOutfitBlock(trRevives, characters);
                    data.TR.OutfitResupplies = _BuildOutfitBlock(trResupplies, characters);
                    data.TR.OutfitSpawns = _BuildOutfitBlock(trSpawns, characters);

                    data.NC.PlayerHeals = _BuildBlock("", ncHeals, characters);
                    data.NC.PlayerRevives = _BuildBlock("", ncRevives, characters);
                    data.NC.PlayerResupplies = _BuildBlock("", ncResupplies, characters);
                    data.NC.PlayerSpawns = _BuildBlock("", ncSpawns, characters);
                    data.NC.OutfitHeals = _BuildOutfitBlock(ncHeals, characters);
                    data.NC.OutfitRevives = _BuildOutfitBlock(ncRevives, characters);
                    data.NC.OutfitResupplies = _BuildOutfitBlock(ncResupplies, characters);
                    data.NC.OutfitSpawns = _BuildOutfitBlock(ncSpawns, characters);

                    data.VS.PlayerHeals = _BuildBlock("", vsHeals, characters);
                    data.VS.PlayerRevives = _BuildBlock("", vsRevives, characters);
                    data.VS.PlayerResupplies = _BuildBlock("", vsResupplies, characters);
                    data.VS.PlayerSpawns = _BuildBlock("", vsSpawns, characters);
                    data.VS.OutfitHeals = _BuildOutfitBlock(vsHeals, characters);
                    data.VS.OutfitRevives = _BuildOutfitBlock(vsRevives, characters);
                    data.VS.OutfitResupplies = _BuildOutfitBlock(vsResupplies, characters);
                    data.VS.OutfitSpawns = _BuildOutfitBlock(vsSpawns, characters);

                    long timeToBuildBlocks = time.ElapsedMilliseconds;

                    foreach (KeyValuePair<string, TrackedOutfit> entry in outfits) {
                        if (entry.Value.Members < 5) {
                            continue;
                        }

                        if (entry.Value.FactionID == Faction.VS.ToString()) {
                            vsOutfitBlock.Entries.Add(entry.Value);
                        } else if (entry.Value.FactionID == Faction.NC.ToString()) {
                            ncOutfitBlock.Entries.Add(entry.Value);
                        } else if (entry.Value.FactionID == Faction.TR.ToString()) {
                            trOutfitBlock.Entries.Add(entry.Value);
                        }
                    }
                    long timeToSortKills = time.ElapsedMilliseconds;

                    trOutfitBlock.Entries = trOutfitBlock.Entries.OrderByDescending(i => i.Kills / ((i.Members == 0) ? 1 : i.Members)).Take(5).ToList();
                    ncOutfitBlock.Entries = ncOutfitBlock.Entries.OrderByDescending(i => i.Kills / ((i.Members == 0) ? 1 : i.Members)).Take(5).ToList();
                    vsOutfitBlock.Entries = vsOutfitBlock.Entries.OrderByDescending(i => i.Kills / ((i.Members == 0) ? 1 : i.Members)).Take(5).ToList();

                    long timeToOrderOutfitBlock = time.ElapsedMilliseconds;

                    data.TR.OutfitKills = trOutfitBlock;
                    data.NC.OutfitKills = ncOutfitBlock;
                    data.VS.OutfitKills = vsOutfitBlock;

                    trPlayerBlock.Entries = trPlayerBlock.Entries.OrderByDescending(i => i.Kills).Take(8).ToList();
                    ncPlayerBlock.Entries = ncPlayerBlock.Entries.OrderByDescending(i => i.Kills).Take(8).ToList();
                    vsPlayerBlock.Entries = vsPlayerBlock.Entries.OrderByDescending(i => i.Kills).Take(8).ToList();

                    long timeToBuildPlayerBlock = time.ElapsedMilliseconds;

                    data.TR.PlayerKills = trPlayerBlock;
                    data.NC.PlayerKills = ncPlayerBlock;
                    data.VS.PlayerKills = vsPlayerBlock;

                    long timeToChars = time.ElapsedMilliseconds;

                    data.TopSpawns = new SpawnEntries();

                    Dictionary<string, TrackedNpc> npcs;
                    lock (NpcStore.Get().Npcs) {
                        npcs = new Dictionary<string, TrackedNpc>(NpcStore.Get().Npcs);
                    }

                    data.TopSpawns.Entries = npcs.Values.OrderByDescending(iter => iter.SpawnCount).Take(8).Select(iter => {
                        bool hasOwner = characters.TryGetValue(iter.OwnerID, out Character? c);
                        return new SpawnEntry() {
                            FirstSeenAt = iter.FirstSeenAt,
                            SecondsAlive = (int)(DateTime.UtcNow - iter.FirstSeenAt).TotalSeconds,
                            SpawnCount = iter.SpawnCount,
                            Owner = (c != null) ? $"{(c.OutfitTag != null ? $"[{c.OutfitTag}] " : "")}{c.Name}" : $"Missing {iter.OwnerID}"
                        };
                    }).ToList();

                    time.Stop();

                    _Logger.LogInformation(
                        $"{DateTime.UtcNow} Took {time.ElapsedMilliseconds}ms to build world data\n"
                        + $"\tTime to make WorldData: {timeToMakeBlock}\n"
                        + $"\tTime to copy players: {timeToCopyPlayers}\n"
                        + $"\tTime to build char dict: {timeToBuildCharacters}\n"
                        + $"\tTime to ctor processing dicts: {timeToCtorProcessingVars}\n"
                        + $"\tTime to process data: {timeToProcessData}\n"
                        + $"\tTime to build blocks from data: {timeToBuildBlocks}\n"
                        + $"\tTime to sort kills: {timeToSortKills}\n"
                        + $"\tTime to order outfit kills: {timeToOrderOutfitBlock}\n"
                        + $"\tTime to build player block: {timeToBuildPlayerBlock}\n"
                        + $"\tTime to chars: {timeToChars}\n"
                    );

                    string json = JsonConvert.SerializeObject(data, new JsonSerializerSettings() {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });

                    _ = _DataHub.Clients.All.SendAsync("DataUpdate", json);

                    await Task.Delay(_RunDelay * 1000, stoppingToken);
                }
                _Logger.LogError($"Token cancelled");
            } catch (Exception ex) {
                _Logger.LogError(ex, "Exception in DataBuilderService");
                throw ex;
            }

            return;
        }

    }
}
