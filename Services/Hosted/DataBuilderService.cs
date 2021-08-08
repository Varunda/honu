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
using watchtower.Constants;
using watchtower.Code.Hubs.Implementations;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Events;
using watchtower.Services.Db;
using watchtower.Services.Repositories;
using watchtower.Code.Hubs;
using watchtower.Code.Constants;
using watchtower.Code;

namespace watchtower.Services {

    public class DataBuilderService : BackgroundService {

        private const int _RunDelay = 5; // seconds

        private const string SERVICE_NAME = "data_builder";

        private readonly ILogger<DataBuilderService> _Logger;
        private readonly IServiceHealthMonitor _ServiceHealthMonitor;

        private readonly IKillEventDbStore _KillEventDb;
        private readonly IExpEventDbStore _ExpEventDb;
        private readonly IWorldTotalDbStore _WorldTotalDb;

        private readonly ICharacterRepository _CharacterRepository;
        private readonly IOutfitRepository _OutfitRepository;
        private readonly IWorldDataRepository _WorldDataRepository;
        private readonly IItemRepository _ItemRepository;

        private readonly IBackgroundCharacterCacheQueue _CharacterCacheQueue;

        private List<short> _WorldIDs = new List<short>() {
            World.Connery, World.Cobalt, World.Emerald, World.Jaeger, World.Miller, World.SolTech
        };

        public DataBuilderService(ILogger<DataBuilderService> logger,
            IBackgroundCharacterCacheQueue charQueue,
            IKillEventDbStore killDb, IExpEventDbStore expDb,
            ICharacterRepository charRepo, IOutfitRepository outfitRepo,
            IWorldTotalDbStore worldTotalDb, IWorldDataRepository worldDataRepo,
            IServiceHealthMonitor healthMon, IItemRepository itemRepo) {

            _Logger = logger;
            _ServiceHealthMonitor = healthMon ?? throw new ArgumentNullException(nameof(healthMon));

            _KillEventDb = killDb ?? throw new ArgumentNullException(nameof(killDb));
            _ExpEventDb = expDb ?? throw new ArgumentNullException(nameof(expDb));
            _WorldTotalDb = worldTotalDb ?? throw new ArgumentNullException(nameof(worldTotalDb));

            _CharacterRepository = charRepo ?? throw new ArgumentNullException(nameof(charRepo));
            _OutfitRepository = outfitRepo ?? throw new ArgumentNullException(nameof(outfitRepo));
            _WorldDataRepository = worldDataRepo ?? throw new ArgumentNullException(nameof(worldDataRepo));
            _ItemRepository = itemRepo ?? throw new ArgumentNullException(nameof(itemRepo));

            _CharacterCacheQueue = charQueue ?? throw new ArgumentNullException(nameof(charQueue));
        }

        public async Task<List<KillData>> GetTopKillers(KillDbOptions options, Dictionary<string, TrackedPlayer> players) {
            List<KillData> data = new List<KillData>();

            List<KillDbEntry> topKillers = await _KillEventDb.GetTopKillers(options);

            foreach (KillDbEntry entry in topKillers) {
                PsCharacter? c = await _CharacterRepository.GetByID(entry.CharacterID);
                bool hasPlayer = players.TryGetValue(entry.CharacterID, out TrackedPlayer? p);

                if (hasPlayer == false && c != null) {
                    _CharacterCacheQueue.Queue(entry.CharacterID);
                }

                //_Logger.LogTrace($"{c?.Name ?? entry.CharacterID} has been online for {entry.SecondsOnline} seconds");

                KillData killDatum = new KillData() {
                    ID = entry.CharacterID,
                    Kills = entry.Kills,
                    Deaths = entry.Deaths,
                    Assists = entry.Assist,
                    Name = c?.GetDisplayName() ?? $"Missing {entry.CharacterID}",
                    Online = p?.Online ?? true,
                    SecondsOnline = (int)entry.SecondsOnline,
                    FactionID = p?.FactionID ?? options.FactionID
                };

                data.Add(killDatum);
            }

            return data;
        }

        private async Task<WeaponKillsBlock> GetTopWeapons(KillDbOptions options) {
            WeaponKillsBlock block = new WeaponKillsBlock();

            List<KillItemEntry> items = (await _KillEventDb.GetTopWeapons(options))
                .OrderByDescending(iter => iter.Kills)
                .Take(12).ToList();

            foreach (KillItemEntry itemIter in items) {
                PsItem? item = await _ItemRepository.GetByID(itemIter.ItemID);

                WeaponKillEntry entry = new WeaponKillEntry() {
                    ItemID = itemIter.ItemID,
                    ItemName = item?.Name ?? $"missing {itemIter.ItemID}",
                    Kills = itemIter.Kills
                };

                block.Entries.Add(entry);
            }

            return block;
        }

        public async Task<OutfitKillBlock> GetTopOutfitKillers(KillDbOptions options) {
            OutfitKillBlock block = new OutfitKillBlock();

            List<KillDbOutfitEntry> topOutfits = await _KillEventDb.GetTopOutfitKillers(options);
            foreach (KillDbOutfitEntry iter in topOutfits) {
                PsOutfit? outfit = await _OutfitRepository.GetByID(iter.OutfitID);

                TrackedOutfit tracked = new TrackedOutfit() {
                    ID = iter.OutfitID,
                    Kills = iter.Kills,
                    Deaths = iter.Deaths,
                    MembersOnline = iter.Members,
                    Members = iter.Members,
                    Name = outfit?.Name ?? $"Missing {iter.OutfitID}",
                    Tag = outfit?.Tag,
                };

                block.Entries.Add(tracked);
            }

            block.Entries = block.Entries.Where(iter => iter.Members > 4)
                .OrderByDescending(iter => iter.Kills / Math.Max(1, iter.MembersOnline))
                .Take(5).ToList();

            return block;
        }

        public async Task<List<BlockEntry>> GetExpBlock(ExpEntryOptions options) {
            List<BlockEntry> blockEntries = new List<BlockEntry>();

            List<ExpDbEntry> entries = await _ExpEventDb.GetEntries(options);
            foreach (ExpDbEntry entry in entries) {
                PsCharacter? c = await _CharacterRepository.GetByID(entry.ID);

                BlockEntry b = new BlockEntry() {
                    ID = entry.ID,
                    Name = c?.GetDisplayName() ?? $"Missing {entry.ID}",
                    Value = entry.Count,
                    FactionID = c?.FactionID ?? options.FactionID
                };

                blockEntries.Add(b);
            }

            return blockEntries;
        }

        public async Task<List<BlockEntry>> GetOutfitExpBlock(ExpEntryOptions options) {
            List<BlockEntry> blockEntries = new List<BlockEntry>();

            List<ExpDbEntry> entries = await _ExpEventDb.GetTopOutfits(options);
            foreach (ExpDbEntry entry in entries) {
                PsOutfit? outfit = await _OutfitRepository.GetByID(entry.ID);

                BlockEntry b = new BlockEntry() {
                    ID = (entry.ID == "") ? "0" : entry.ID,
                    Name = (entry.ID == "") ? "No outfit" : (outfit == null) ? $"Missing {entry.ID}" : $"[{outfit.Tag}] {outfit.Name}",
                    Value = entry.Count
                };

                blockEntries.Add(b);
            }

            return blockEntries.OrderByDescending(iter => iter.Value).Take(5).ToList();
        }

        public async Task<OutfitsOnline> GetOutfitsOnline(Dictionary<string, TrackedPlayer> players, short teamID, short worldID) {
            Dictionary<string, OutfitOnlineEntry> outfits = new Dictionary<string, OutfitOnlineEntry>();

            int total = 0;

            foreach (KeyValuePair<string, TrackedPlayer> entry in players) {
                if (entry.Value.Online == false || entry.Value.TeamID != teamID || entry.Value.WorldID != worldID) {
                    continue;
                }

                string outfitID = entry.Value.OutfitID ?? "0";

                /*
                if (teamID == Faction.VS && worldID == World.Emerald) {
                    _Logger.LogDebug(outfitID);
                }
                */

                if (outfits.TryGetValue(outfitID, out OutfitOnlineEntry? outfit) == false) {
                    PsOutfit? psOutfit;
                    if (outfitID == "0") {
                        psOutfit = new PsOutfit() {
                            ID = outfitID,
                            FactionID = teamID,
                            Name = "No outfit"
                        };
                    } else {
                        psOutfit = await _OutfitRepository.GetByID(outfitID);
                    }

                    outfit = new OutfitOnlineEntry() {
                        Display = $"{(psOutfit?.Tag == null ? $"" : $"[{psOutfit?.Tag}] ")}{psOutfit?.Name}",
                        FactionID = teamID
                    };

                    /*
                    if (teamID == Faction.VS && worldID == World.Emerald) {
                        _Logger.LogDebug($"Made new outfit {psOutfit?.Name}");
                    }
                    */
                }

                ++outfit.AmountOnline;
                ++total;

                outfits[outfitID] = outfit;
            }

            return new OutfitsOnline() {
                TotalOnline = total,
                Outfits = outfits.Values
                    .OrderByDescending(iter => iter.AmountOnline)
                    .Take(10)
                    .ToList()
            };

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            while (!stoppingToken.IsCancellationRequested) {
                try {
                    Stopwatch timer = Stopwatch.StartNew();

                    ServiceHealthEntry? entry = _ServiceHealthMonitor.Get(SERVICE_NAME);
                    if (entry == null) {
                        entry = new ServiceHealthEntry() {
                            Name = SERVICE_NAME
                        };
                    }

                    // Useful for debugging on my laptop which can't handle running the queries and run vscode at the same time
                    if (entry.Enabled == false) {
                        await Task.Delay(1000, stoppingToken);
                        continue;
                    }

                    foreach (short worldID in _WorldIDs) {
                        lock (ConnectionStore.Get().Connections) {
                            int count = ConnectionStore.Get().Connections.Where(iter => iter.Value.WorldID == worldID).Count();
                            if (count == 0) {
                                continue;
                            }
                        }

                        WorldData data = await _BuildWorldData(worldID, stoppingToken);
                        _WorldDataRepository.Set(worldID, data);

                        if (stoppingToken.IsCancellationRequested) {
                            _Logger.LogDebug($"Stopping token sent, disabling early");
                            entry.Enabled = false;
                            break;
                        }

                        ServiceHealthEntry? iterEntry = _ServiceHealthMonitor.Get(SERVICE_NAME);
                        if (iterEntry != null && iterEntry.Enabled == false) {
                            _Logger.LogInformation($"{SERVICE_NAME} ended early");
                            entry.Enabled = false;
                            break;
                        }
                    }

                    long elapsedTime = timer.ElapsedMilliseconds;

                    entry.RunDuration = elapsedTime;
                    entry.LastRan = DateTime.UtcNow;
                    _ServiceHealthMonitor.Set(SERVICE_NAME, entry);

                    long timeToHold = (_RunDelay * 1000) - elapsedTime;

                    // Don't constantly run the data building, not useful, but if it does take awhile start building again so the data is recent
                    if (timeToHold > 5) {
                        await Task.Delay((int)timeToHold, stoppingToken);
                    }
                } catch (Exception) when (stoppingToken.IsCancellationRequested) {
                    _Logger.LogInformation($"Stopped data builder service");
                } catch (Exception ex) {
                    _Logger.LogError(ex, "Exception in DataBuilderService");
                }
            }
        }

        private async Task<WorldData> _BuildWorldData(short worldID, CancellationToken? stoppingToken = null) {
            Stopwatch time = Stopwatch.StartNew();

            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            WorldData data = new WorldData();

            data.WorldID = worldID;
            data.WorldName = World.GetName(worldID);
            data.ContinentCount = new ContinentCount();

            Dictionary<string, TrackedPlayer> players;
            lock (CharacterStore.Get().Players) {
                players = new Dictionary<string, TrackedPlayer>(CharacterStore.Get().Players);
            }
            players = players.Where(iter => iter.Value.WorldID == worldID).ToDictionary(key => key.Key, value => value.Value);

            long timeToCopyPlayers = time.ElapsedMilliseconds;

            ExpEntryOptions expOptions = new ExpEntryOptions() {
                Interval = 120,
                WorldID = worldID
            };

            ExpEntryOptions vsExpOptions = new ExpEntryOptions() { Interval = 120, WorldID = worldID, FactionID = Faction.VS };
            ExpEntryOptions ncExpOptions = new ExpEntryOptions() { Interval = 120, WorldID = worldID, FactionID = Faction.NC };
            ExpEntryOptions trExpOptions = new ExpEntryOptions() { Interval = 120, WorldID = worldID, FactionID = Faction.TR };

            ncExpOptions.ExperienceIDs = trExpOptions.ExperienceIDs = vsExpOptions.ExperienceIDs = new List<int>() { Experience.HEAL, Experience.SQUAD_HEAL };
            await Task.WhenAll(
                GetExpBlock(vsExpOptions).ContinueWith(t => data.VS.PlayerHeals.Entries = t.Result),
                GetOutfitExpBlock(vsExpOptions).ContinueWith(t => data.VS.OutfitHeals.Entries = t.Result),
                GetExpBlock(ncExpOptions).ContinueWith(t => data.NC.PlayerHeals.Entries = t.Result),
                GetOutfitExpBlock(ncExpOptions).ContinueWith(t => data.NC.OutfitHeals.Entries = t.Result),
                GetExpBlock(trExpOptions).ContinueWith(t => data.TR.PlayerHeals.Entries = t.Result),
                GetOutfitExpBlock(trExpOptions).ContinueWith(t => data.TR.OutfitHeals.Entries = t.Result)
            );
            long timeToGetHealEntries = time.ElapsedMilliseconds;

            // Early stop for a quicker shutdown. Saves seconds per restart!
            if (stoppingToken != null && stoppingToken.Value.IsCancellationRequested) { return data; }

            ncExpOptions.ExperienceIDs = trExpOptions.ExperienceIDs = vsExpOptions.ExperienceIDs = new List<int>() { Experience.REVIVE, Experience.SQUAD_REVIVE };
            await Task.WhenAll(
                GetExpBlock(vsExpOptions).ContinueWith(t => data.VS.PlayerRevives.Entries = t.Result),
                GetOutfitExpBlock(vsExpOptions).ContinueWith(t => data.VS.OutfitRevives.Entries = t.Result),
                GetExpBlock(ncExpOptions).ContinueWith(t => data.NC.PlayerRevives.Entries = t.Result),
                GetOutfitExpBlock(ncExpOptions).ContinueWith(t => data.NC.OutfitRevives.Entries = t.Result),
                GetExpBlock(trExpOptions).ContinueWith(t => data.TR.PlayerRevives.Entries = t.Result),
                GetOutfitExpBlock(trExpOptions).ContinueWith(t => data.TR.OutfitRevives.Entries = t.Result)
            );
            long timeToGetReviveEntries = time.ElapsedMilliseconds;

            // Early stop for a quicker shutdown. Saves seconds per restart!
            if (stoppingToken != null && stoppingToken.Value.IsCancellationRequested) { return data; }

            ncExpOptions.ExperienceIDs = trExpOptions.ExperienceIDs = vsExpOptions.ExperienceIDs = new List<int>() { Experience.RESUPPLY, Experience.SQUAD_RESUPPLY };
            await Task.WhenAll(
                GetExpBlock(vsExpOptions).ContinueWith(t => data.VS.PlayerResupplies.Entries = t.Result),
                GetOutfitExpBlock(vsExpOptions).ContinueWith(t => data.VS.OutfitResupplies.Entries = t.Result),
                GetExpBlock(ncExpOptions).ContinueWith(t => data.NC.PlayerResupplies.Entries = t.Result),
                GetOutfitExpBlock(ncExpOptions).ContinueWith(t => data.NC.OutfitResupplies.Entries = t.Result),
                GetExpBlock(trExpOptions).ContinueWith(t => data.TR.PlayerResupplies.Entries = t.Result),
                GetOutfitExpBlock(trExpOptions).ContinueWith(t => data.TR.OutfitResupplies.Entries = t.Result)
            );
            long timeToGetResupplyEntries = time.ElapsedMilliseconds;

            // Early stop for a quicker shutdown. Saves seconds per restart!
            if (stoppingToken != null && stoppingToken.Value.IsCancellationRequested) { return data; }

            ncExpOptions.ExperienceIDs = trExpOptions.ExperienceIDs = vsExpOptions.ExperienceIDs = new List<int>() {
                Experience.SQUAD_SPAWN, Experience.GALAXY_SPAWN_BONUS, Experience.SUNDERER_SPAWN_BONUS,
                Experience.SQUAD_VEHICLE_SPAWN_BONUS, Experience.GENERIC_NPC_SPAWN
            };
            await Task.WhenAll(
                GetExpBlock(vsExpOptions).ContinueWith(t => data.VS.PlayerSpawns.Entries = t.Result),
                GetOutfitExpBlock(vsExpOptions).ContinueWith(t => data.VS.OutfitSpawns.Entries = t.Result),
                GetExpBlock(ncExpOptions).ContinueWith(t => data.NC.PlayerSpawns.Entries = t.Result),
                GetOutfitExpBlock(ncExpOptions).ContinueWith(t => data.NC.OutfitSpawns.Entries = t.Result),
                GetExpBlock(trExpOptions).ContinueWith(t => data.TR.PlayerSpawns.Entries = t.Result),
                GetOutfitExpBlock(trExpOptions).ContinueWith(t => data.TR.OutfitSpawns.Entries = t.Result)
            );
            long timeToGetSpawnEntries = time.ElapsedMilliseconds;

            // Early stop for a quicker shutdown. Saves seconds per restart!
            if (stoppingToken != null && stoppingToken.Value.IsCancellationRequested) { return data; }

            ncExpOptions.ExperienceIDs = trExpOptions.ExperienceIDs = vsExpOptions.ExperienceIDs = Experience.VehicleKillEvents;
            await Task.WhenAll(
                GetExpBlock(vsExpOptions).ContinueWith(t => data.VS.PlayerVehicleKills.Entries = t.Result),
                GetOutfitExpBlock(vsExpOptions).ContinueWith(t => data.VS.OutfitVehicleKills.Entries = t.Result),
                GetExpBlock(ncExpOptions).ContinueWith(t => data.NC.PlayerVehicleKills.Entries = t.Result),
                GetOutfitExpBlock(ncExpOptions).ContinueWith(t => data.NC.OutfitVehicleKills.Entries = t.Result),
                GetExpBlock(trExpOptions).ContinueWith(t => data.TR.PlayerVehicleKills.Entries = t.Result),
                GetOutfitExpBlock(trExpOptions).ContinueWith(t => data.TR.OutfitVehicleKills.Entries = t.Result)
            );
            long timeToGetVehicleKills = time.ElapsedMilliseconds;

            KillDbOptions vsKillOptions = new KillDbOptions() { Interval = 120, WorldID = data.WorldID, FactionID = Faction.VS };
            KillDbOptions ncKillOptions = new KillDbOptions() { Interval = 120, WorldID = data.WorldID, FactionID = Faction.NC };
            KillDbOptions trKillOptions = new KillDbOptions() { Interval = 120, WorldID = data.WorldID, FactionID = Faction.TR };

            await Task.WhenAll(
                GetTopKillers(vsKillOptions, players).ContinueWith(t => data.VS.PlayerKills.Entries = t.Result),
                GetTopOutfitKillers(vsKillOptions).ContinueWith(t => data.VS.OutfitKills = t.Result),
                GetTopWeapons(vsKillOptions).ContinueWith(t => data.VS.WeaponKills = t.Result),

                GetTopKillers(ncKillOptions, players).ContinueWith(t => data.NC.PlayerKills.Entries = t.Result),
                GetTopOutfitKillers(ncKillOptions).ContinueWith(t => data.NC.OutfitKills = t.Result),
                GetTopWeapons(ncKillOptions).ContinueWith(t => data.NC.WeaponKills = t.Result),

                GetTopKillers(trKillOptions, players).ContinueWith(t => data.TR.PlayerKills.Entries = t.Result),
                GetTopOutfitKillers(trKillOptions).ContinueWith(t => data.TR.OutfitKills = t.Result),
                GetTopWeapons(trKillOptions).ContinueWith(t => data.TR.WeaponKills = t.Result)
            );

            long timeToGetTopKills = time.ElapsedMilliseconds;

            // Early stop for a quicker shutdown. Saves seconds per restart!
            if (stoppingToken != null && stoppingToken.Value.IsCancellationRequested) { return data; }

            data.TopSpawns = new SpawnEntries();

            Dictionary<string, TrackedNpc> npcs;
            lock (NpcStore.Get().Npcs) {
                npcs = new Dictionary<string, TrackedNpc>(NpcStore.Get().Npcs);
            }

            long timeToCopyNpcStore = time.ElapsedMilliseconds;

            data.TopSpawns.Entries = npcs.Values
                .Where(iter => iter.WorldID == worldID)
                .OrderByDescending(iter => iter.SpawnCount).Take(8)
                .Select(async iter => {
                    PsCharacter? c = await _CharacterRepository.GetByID(iter.OwnerID);

                    return new SpawnEntry() {
                        FirstSeenAt = iter.FirstSeenAt,
                        NpcType = iter.Type,
                        SecondsAlive = (int)(DateTime.UtcNow - iter.FirstSeenAt).TotalSeconds,
                        SpawnCount = iter.SpawnCount,
                        FactionID = c?.FactionID ?? Faction.UNKNOWN,
                        Owner = (c != null) ? $"{(c.OutfitTag != null ? $"[{c.OutfitTag}] " : "")}{c.Name}" : $"Missing {iter.OwnerID}"
                    };
                }).Select(iter => iter.Result).ToList();

            long timeToGetBiggestSpawns = time.ElapsedMilliseconds;

            WorldTotalOptions totalOptions = new WorldTotalOptions() {
                Interval = 120,
                WorldID = worldID
            };

            // Early stop for a quicker shutdown. Saves seconds per restart!
            if (stoppingToken != null && stoppingToken.Value.IsCancellationRequested) { return data; }

            WorldTotal worldTotal = await _WorldTotalDb.Get(totalOptions);

            // Early stop for a quicker shutdown. Saves seconds per restart!
            if (stoppingToken != null && stoppingToken.Value.IsCancellationRequested) { return data; }

            data.VS.TotalKills = worldTotal.GetValue(WorldTotal.TOTAL_VS_KILLS);
            data.VS.TotalDeaths = worldTotal.GetValue(WorldTotal.TOTAL_VS_DEATHS);
            data.VS.TotalAssists = worldTotal.GetValue(WorldTotal.TOTAL_VS_ASSISTS);
            data.VS.PlayerHeals.Total = worldTotal.GetValue(WorldTotal.TOTAL_VS_HEALS);
            data.VS.OutfitHeals.Total = worldTotal.GetValue(WorldTotal.TOTAL_VS_HEALS);
            data.VS.PlayerRevives.Total = worldTotal.GetValue(WorldTotal.TOTAL_VS_REVIVES);
            data.VS.OutfitRevives.Total = worldTotal.GetValue(WorldTotal.TOTAL_VS_REVIVES);
            data.VS.PlayerResupplies.Total = worldTotal.GetValue(WorldTotal.TOTAL_VS_RESUPPLIES);
            data.VS.OutfitResupplies.Total = worldTotal.GetValue(WorldTotal.TOTAL_VS_RESUPPLIES);
            data.VS.PlayerSpawns.Total = worldTotal.GetValue(WorldTotal.TOTAL_VS_SPAWNS);
            data.VS.OutfitSpawns.Total = worldTotal.GetValue(WorldTotal.TOTAL_VS_SPAWNS);
            data.VS.PlayerVehicleKills.Total = worldTotal.GetValue(WorldTotal.TOTAL_VS_VEHICLE_KILLS);
            data.VS.OutfitVehicleKills.Total = worldTotal.GetValue(WorldTotal.TOTAL_VS_VEHICLE_KILLS);

            data.NC.TotalKills = worldTotal.GetValue(WorldTotal.TOTAL_NC_KILLS);
            data.NC.TotalDeaths = worldTotal.GetValue(WorldTotal.TOTAL_NC_DEATHS);
            data.NC.TotalAssists = worldTotal.GetValue(WorldTotal.TOTAL_NC_ASSISTS);
            data.NC.PlayerHeals.Total = worldTotal.GetValue(WorldTotal.TOTAL_NC_HEALS);
            data.NC.OutfitHeals.Total = worldTotal.GetValue(WorldTotal.TOTAL_NC_HEALS);
            data.NC.PlayerRevives.Total = worldTotal.GetValue(WorldTotal.TOTAL_NC_REVIVES);
            data.NC.OutfitRevives.Total = worldTotal.GetValue(WorldTotal.TOTAL_NC_REVIVES);
            data.NC.PlayerResupplies.Total = worldTotal.GetValue(WorldTotal.TOTAL_NC_RESUPPLIES);
            data.NC.OutfitResupplies.Total = worldTotal.GetValue(WorldTotal.TOTAL_NC_RESUPPLIES);
            data.NC.PlayerSpawns.Total = worldTotal.GetValue(WorldTotal.TOTAL_NC_SPAWNS);
            data.NC.OutfitSpawns.Total = worldTotal.GetValue(WorldTotal.TOTAL_NC_SPAWNS);
            data.NC.PlayerVehicleKills.Total = worldTotal.GetValue(WorldTotal.TOTAL_NC_VEHICLE_KILLS);
            data.NC.OutfitVehicleKills.Total = worldTotal.GetValue(WorldTotal.TOTAL_NC_VEHICLE_KILLS);

            data.TR.TotalKills = worldTotal.GetValue(WorldTotal.TOTAL_TR_KILLS);
            data.TR.TotalDeaths = worldTotal.GetValue(WorldTotal.TOTAL_TR_DEATHS);
            data.TR.TotalAssists = worldTotal.GetValue(WorldTotal.TOTAL_TR_ASSISTS);
            data.TR.PlayerHeals.Total = worldTotal.GetValue(WorldTotal.TOTAL_TR_HEALS);
            data.TR.OutfitHeals.Total = worldTotal.GetValue(WorldTotal.TOTAL_TR_HEALS);
            data.TR.PlayerRevives.Total = worldTotal.GetValue(WorldTotal.TOTAL_TR_REVIVES);
            data.TR.OutfitRevives.Total = worldTotal.GetValue(WorldTotal.TOTAL_TR_REVIVES);
            data.TR.PlayerResupplies.Total = worldTotal.GetValue(WorldTotal.TOTAL_TR_RESUPPLIES);
            data.TR.OutfitResupplies.Total = worldTotal.GetValue(WorldTotal.TOTAL_TR_RESUPPLIES);
            data.TR.PlayerSpawns.Total = worldTotal.GetValue(WorldTotal.TOTAL_TR_SPAWNS);
            data.TR.OutfitSpawns.Total = worldTotal.GetValue(WorldTotal.TOTAL_TR_SPAWNS);
            data.TR.PlayerVehicleKills.Total = worldTotal.GetValue(WorldTotal.TOTAL_TR_VEHICLE_KILLS);
            data.TR.OutfitVehicleKills.Total = worldTotal.GetValue(WorldTotal.TOTAL_TR_VEHICLE_KILLS);

            FactionFocus focus = new FactionFocus();
            focus.VS.NcKills = worldTotal.GetValue(WorldTotal.TOTAL_VS_KILLS_NC);
            focus.VS.TrKills = worldTotal.GetValue(WorldTotal.TOTAL_VS_KILLS_TR);
            focus.NC.VsKills = worldTotal.GetValue(WorldTotal.TOTAL_NC_KILLS_VS);
            focus.NC.TrKills = worldTotal.GetValue(WorldTotal.TOTAL_NC_KILLS_TR);
            focus.TR.VsKills = worldTotal.GetValue(WorldTotal.TOTAL_TR_KILLS_NC);
            focus.TR.NcKills = worldTotal.GetValue(WorldTotal.TOTAL_TR_KILLS_VS);
            data.FactionFocus = focus;

            await Task.WhenAll(
                GetOutfitsOnline(players, Faction.VS, worldID).ContinueWith(result => data.VS.Outfits = result.Result),
                GetOutfitsOnline(players, Faction.NC, worldID).ContinueWith(result => data.NC.Outfits = result.Result),
                GetOutfitsOnline(players, Faction.TR, worldID).ContinueWith(result => data.TR.Outfits = result.Result)
            );

            long timeToGetWorldTotals = time.ElapsedMilliseconds;

            // Early stop for a quicker shutdown. Saves seconds per restart!
            if (stoppingToken != null && stoppingToken.Value.IsCancellationRequested) { return data; }

            foreach (KeyValuePair<string, TrackedPlayer> entry in players) {
                if (entry.Value.Online == true) {
                    ++data.OnlineCount;

                    if (entry.Value.FactionID == Faction.VS) {
                        data.ContinentCount.AddToVS(entry.Value.ZoneID);
                    } else if (entry.Value.FactionID == Faction.NC) {
                        data.ContinentCount.AddToNC(entry.Value.ZoneID);
                    } else if (entry.Value.FactionID == Faction.TR) {
                        data.ContinentCount.AddToTR(entry.Value.ZoneID);
                    } else if (entry.Value.FactionID == Faction.NS) {
                        data.ContinentCount.AddToNS(entry.Value.ZoneID);
                    }
                }
            }

            long timeToUpdateSecondsOnline = time.ElapsedMilliseconds;

            time.Stop();

            /*
            _Logger.LogInformation(
                $"{DateTime.UtcNow} took {time.ElapsedMilliseconds}ms to build world data for {worldID}\n"
            );

                + $"\ttime to copy players: {timeToCopyPlayers}ms\n"
                + $"\ttime to get heal entries: {timeToGetHealEntries}ms\n"
                + $"\ttime to get revive entries: {timeToGetReviveEntries}ms\n"
                + $"\ttime to get resupply entries: {timeToGetResupplyEntries}ms\n"
                + $"\ttime to get spawn entries: {timeToGetSpawnEntries}ms\n"
                + $"\ttime to get top kills: {timeToGetTopKills}ms\n"
                + $"\ttime to copy npc store: {timeToCopyNpcStore}ms\n"
                + $"\ttime to get biggest spawns: {timeToGetBiggestSpawns}ms\n"
                + $"\ttime to get world totals: {timeToGetWorldTotals}ms\n"
                + $"\ttime to update seconds online: {timeToUpdateSecondsOnline}ms\n"
            );
            */

            return data;
        }

    }
}
