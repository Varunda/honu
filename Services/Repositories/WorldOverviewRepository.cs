using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using watchtower.Code;
using watchtower.Code.Constants;
using watchtower.Constants;
using watchtower.Models;

namespace watchtower.Services.Repositories {

    public class WorldOverviewRepository {

        private readonly ILogger<WorldOverviewRepository> _Logger;
        private readonly IMemoryCache _Cache;

        private readonly MapRepository _MapRepository;

        private const string CACHE_KEY = "WorldOverview.All";

        public WorldOverviewRepository(ILogger<WorldOverviewRepository> logger,
            IMemoryCache cache, MapRepository mapRepository) {

            _Logger = logger;
            _Cache = cache;
            _MapRepository = mapRepository;
        }

        public List<WorldOverview> Build() {
            if (_Cache.TryGetValue(CACHE_KEY, out List<WorldOverview> worlds) == false) {
                WorldOverview connery = new() { WorldID = World.Connery, WorldName = "Connery" };
                WorldOverview cobalt = new() { WorldID = World.Cobalt, WorldName = "Cobalt" };
                WorldOverview jaeger = new() { WorldID = World.Jaeger, WorldName = "Jaeger" };
                WorldOverview miller = new() { WorldID = World.Miller, WorldName = "Miller" };
                WorldOverview emerald = new() { WorldID = World.Emerald, WorldName = "Emerald" };
                WorldOverview soltech = new() { WorldID = World.SolTech, WorldName = "SolTech" };

                worlds = new List<WorldOverview>() {
                    connery, cobalt, jaeger, miller, emerald, soltech
                };

                foreach (WorldOverview world in worlds) {
                    lock (ZoneStateStore.Get().Zones) {
                        foreach (uint zoneID in Zone.All) {
                            ZoneState? zs = ZoneStateStore.Get().GetZone(world.WorldID, zoneID);
                            if (zs != null) {
                                zs.UnstableState = _MapRepository.GetUnstableState(world.WorldID, zoneID);
                                world.Zones.Add(zs);
                            }
                        }
                    }
                }

                void UpdateZoneCount(WorldOverview wo, ref TrackedPlayer player) {
                    uint zoneID = player.ZoneID;

                    ZoneState? state = wo.Zones.FirstOrDefault(iter => iter.ZoneID == zoneID);
                    if (state != null) {
                        ++state.PlayerCount;

                        if (player.TeamID == Faction.VS) {
                            ++state.VsCount;
                        } else if (player.TeamID == Faction.NC) {
                            ++state.NcCount;
                        } else if (player.TeamID == Faction.TR) {
                            ++state.TrCount;
                        } else {
                            ++state.OtherCount;
                        }
                    }
                }

                lock (CharacterStore.Get().Players) {
                    foreach (KeyValuePair<string, TrackedPlayer> iter in CharacterStore.Get().Players) {
                        TrackedPlayer c = iter.Value;

                        if (c.Online == false) {
                            continue;
                        }

                        if (c.WorldID == World.Connery) {
                            ++connery.PlayersOnline;
                            UpdateZoneCount(connery, ref c);
                        } else if (c.WorldID == World.Cobalt) {
                            ++cobalt.PlayersOnline;
                            UpdateZoneCount(cobalt, ref c);
                        } else if (c.WorldID == World.Emerald) {
                            ++emerald.PlayersOnline;
                            UpdateZoneCount(emerald, ref c);
                        } else if (c.WorldID == World.Jaeger) {
                            ++jaeger.PlayersOnline;
                            UpdateZoneCount(jaeger, ref c);
                        } else if (c.WorldID == World.Miller) {
                            ++miller.PlayersOnline;
                            UpdateZoneCount(miller, ref c);
                        } else if (c.WorldID == World.SolTech) {
                            ++soltech.PlayersOnline;
                            UpdateZoneCount(soltech, ref c);
                        }
                    }
                }

                _Cache.Set(CACHE_KEY, worlds, new MemoryCacheEntryOptions() {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(4)
                });
            }

            return worlds;
        }

    }
}
