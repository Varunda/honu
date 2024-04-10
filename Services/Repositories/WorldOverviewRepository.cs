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
            if (_Cache.TryGetValue(CACHE_KEY, out List<WorldOverview>? worlds) == false || worlds == null) {
                WorldOverview connery = new() { WorldID = World.Connery, WorldName = "Connery" };
                WorldOverview cobalt = new() { WorldID = World.Cobalt, WorldName = "Cobalt" };
                WorldOverview jaeger = new() { WorldID = World.Jaeger, WorldName = "Jaeger" };
                WorldOverview miller = new() { WorldID = World.Miller, WorldName = "Miller" };
                WorldOverview emerald = new() { WorldID = World.Emerald, WorldName = "Emerald" };
                WorldOverview soltech = new() { WorldID = World.SolTech, WorldName = "SolTech" };
                WorldOverview genudine = new() { WorldID = World.Genudine, WorldName = "Genudine" };
                WorldOverview ceres = new() { WorldID = World.Ceres, WorldName = "Ceres" };
                WorldOverview apex = new() { WorldID = World.Apex, WorldName = "Apex" };

                worlds = new List<WorldOverview>() {
                    connery, miller, cobalt, emerald, apex, jaeger, soltech, genudine, ceres
                };

                foreach (WorldOverview world in worlds) {
                    lock (ZoneStateStore.Get().Zones) {
                        foreach (uint zoneID in Zone.StaticZones) {
                            ZoneState? zs = ZoneStateStore.Get().GetZone(world.WorldID, zoneID);

                            if (zs != null) {
                                zs.UnstableState = _MapRepository.GetUnstableState(world.WorldID, zoneID);
                                ZoneState copy = new ZoneState(zs);
                                copy.Players = new PlayerCount();
                                copy.PlayerCount = 0;
                                world.Zones.Add(copy);
                            }
                        }
                    }

                    foreach (uint zoneID in Zone.StaticZones) {
                        PsZone? zone = _MapRepository.GetZone(world.WorldID, zoneID);
                        ZoneState? state = world.Zones.FirstOrDefault(iter => iter.ZoneID == zoneID);
                        if (zone != null && state != null) {
                            List<PsFacilityOwner> owners = zone.GetFacilities();
                            state.TerritoryControl.VS = owners.Where(iter => iter.Owner == Faction.VS).Count();
                            state.TerritoryControl.NC = owners.Where(iter => iter.Owner == Faction.NC).Count();
                            state.TerritoryControl.TR = owners.Where(iter => iter.Owner == Faction.TR).Count();
                            state.TerritoryControl.Total = owners.Count;
                        }
                    }
                }

                void UpdateZoneCount(WorldOverview wo, ref TrackedPlayer player) {
                    uint zoneID = player.ZoneID;

                    ZoneState? state = wo.Zones.FirstOrDefault(iter => iter.ZoneID == zoneID);
                    if (state != null) {
                        ++state.PlayerCount;
                        ++state.Players.All;

                        if (player.TeamID == Faction.VS) {
                            ++state.Players.VS;
                        } else if (player.TeamID == Faction.NC) {
                            ++state.Players.NC;
                        } else if (player.TeamID == Faction.TR) {
                            ++state.Players.TR;
                        } else {
                            ++state.Players.Unknown;
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
                        } else if (c.WorldID == World.Genudine) {
                            ++genudine.PlayersOnline;
                            UpdateZoneCount(genudine, ref c);
                        } else if (c.WorldID == World.Ceres) {
                            ++ceres.PlayersOnline;
                            UpdateZoneCount(ceres, ref c);
                        } else if (c.WorldID == World.Apex) {
                            ++apex.PlayersOnline;
                            UpdateZoneCount(apex, ref c);
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
