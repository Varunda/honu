using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Models.Db;
using watchtower.Services.Db;

namespace watchtower.Services.Hosted {

    /// <summary>
    ///     builds <see cref="WorldZonePopulation"/> periodically for use in world data
    /// </summary>
    public class HostedWorldZonePopulationBuilder : BackgroundService {

        private readonly ILogger<HostedWorldZonePopulationBuilder> _Logger;
        private readonly WorldZonePopulationDbStore _Db;
        private readonly watchtower.Realtime.IEventHandler _EventHandler;

        private const int DELAY_MS = 30 * 1000;

        private DateTime _PreviousTimestamp;

        public HostedWorldZonePopulationBuilder(ILogger<HostedWorldZonePopulationBuilder> logger,
            WorldZonePopulationDbStore db, watchtower.Realtime.IEventHandler eventHandler) {

            _Logger = logger;
            _Db = db;
            _EventHandler = eventHandler;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            while (stoppingToken.IsCancellationRequested == false) {
                DateTime timestamp = _EventHandler.MostRecentProcess();

                try {
                    _Logger.LogInformation($"starting world zone population data [Timestamp={DateTime.UtcNow:u}]...");
                    if (timestamp - _PreviousTimestamp <= TimeSpan.FromSeconds(15)) {
                        _Logger.LogWarning($"skipping world zone population [_PreviousTimestamp={_PreviousTimestamp:u}] [timestamp={timestamp:u}]");
                        await Task.Delay(DELAY_MS, stoppingToken);
                        continue;
                    }

                    Stopwatch timer = Stopwatch.StartNew();

                    await _ProcessInsert(DateTime.UtcNow, stoppingToken);
                    long insertMs = timer.ElapsedMilliseconds; timer.Restart();

                    await _ProcessDelete(DateTime.UtcNow, stoppingToken);
                    long deleteMs = timer.ElapsedMilliseconds; timer.Restart();

                    _Logger.LogInformation($"took {insertMs + deleteMs}ms to update world zone population data [Insert={insertMs}ms] [Delete={deleteMs}ms]");
                } catch (Exception ex) {
                    _Logger.LogError(ex, $"failed to build world zone population data");
                }

                _PreviousTimestamp = timestamp;
                await Task.Delay(DELAY_MS, stoppingToken);
            }
        }

        private async Task _ProcessInsert(DateTime timestamp, CancellationToken cancel) {

            Dictionary<string, WorldZonePopulation> map = new();
            foreach (short worldID in World.PcStreams) {
                foreach (uint zoneID in Zone.StaticZones) {
                    map.Add($"{worldID}-{zoneID}", new WorldZonePopulation() {
                        WorldID = worldID,
                        ZoneID = zoneID,
                        Timestamp = timestamp
                    });
                }

                map.Add($"{worldID}-0", new WorldZonePopulation() {
                    WorldID = worldID,
                    ZoneID = 0,
                    Timestamp = timestamp
                });
            }

            lock (CharacterStore.Get().Players) {
                foreach (KeyValuePair<string, TrackedPlayer> iter in CharacterStore.Get().Players) {
                    TrackedPlayer player = iter.Value;

                    if (player.Online == false || World.PcStreams.Contains(player.WorldID) == false) {
                        continue;
                    }

                    uint zoneID = player.ZoneID;
                    if (Zone.StaticZones.Contains(player.ZoneID) == false) {
                        zoneID = 0;
                    }
                    string key = $"{player.WorldID}-{zoneID}";

                    if (map.ContainsKey(key) == false) {
                        _Logger.LogWarning($"missing key {key}");
                        map.Add(key, new WorldZonePopulation() {
                            WorldID = player.WorldID,
                            ZoneID = zoneID,
                            Timestamp = timestamp
                        });
                    }

                    WorldZonePopulation pop = map[key];

                    if (player.FactionID == Faction.VS) {
                        ++pop.FactionVs;
                    } else if (player.FactionID == Faction.NC) {
                        ++pop.FactionNc;
                    } else if (player.FactionID == Faction.TR) {
                        ++pop.FactionTr;
                    } else if (player.FactionID == Faction.NS) {
                        ++pop.FactionNs;
                    }

                    if (player.TeamID == Faction.VS) {
                        ++pop.TeamVs;
                    } else if (player.TeamID == Faction.NC) {
                        ++pop.TeamNc;
                    } else if (player.TeamID == Faction.TR) {
                        ++pop.TeamTr;
                    } else if (player.TeamID == Faction.NS) {
                        ++pop.TeamUnknown;
                    }

                    pop.Total = pop.TeamVs + pop.TeamNc + pop.TeamTr + pop.TeamUnknown;

                    map[key] = pop;
                }
            }

            List<WorldZonePopulation> pops = map.Values.ToList();
            foreach (WorldZonePopulation pop in pops) {
                //_Logger.LogDebug($"inserting data for {pop.WorldID}-{pop.ZoneID} @{pop.Timestamp:u}");
                await _Db.Insert(pop);
            }

        }

        private async Task _ProcessDelete(DateTime timestamp, CancellationToken cancel) {
            await _Db.DeleteBefore(timestamp - TimeSpan.FromHours(2));
        }

    }
}
