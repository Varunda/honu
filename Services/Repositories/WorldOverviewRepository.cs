using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using watchtower.Code;
using watchtower.Code.Constants;
using watchtower.Constants;
using watchtower.Models;

namespace watchtower.Services.Repositories {

    public class WorldOverviewRepository {

        private readonly ILogger<WorldOverviewRepository> _Logger;

        private readonly IMemoryCache _Cache;

        private const string CACHE_KEY = "WorldOverview.All";

        public WorldOverviewRepository(ILogger<WorldOverviewRepository> logger,
            IMemoryCache cache) {

            _Logger = logger;
            _Cache = cache;
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

                lock (CharacterStore.Get().Players) {
                    foreach (KeyValuePair<string, TrackedPlayer> iter in CharacterStore.Get().Players) {
                        TrackedPlayer c = iter.Value;

                        if (c.Online == false) {
                            continue;
                        }

                        if (c.WorldID == World.Connery) {
                            ++connery.PlayersOnline;
                        } else if (c.WorldID == World.Cobalt) {
                            ++cobalt.PlayersOnline;
                        } else if (c.WorldID == World.Emerald) {
                            ++emerald.PlayersOnline;
                        } else if (c.WorldID == World.Jaeger) {
                            ++jaeger.PlayersOnline;
                        } else if (c.WorldID == World.Miller) {
                            ++miller.PlayersOnline;
                        } else if (c.WorldID == World.SolTech) {
                            ++soltech.PlayersOnline;
                        }
                    }
                }

                foreach (WorldOverview world in worlds) {
                    lock (ZoneStateStore.Get().Zones) {
                        ZoneState? indar = ZoneStateStore.Get().GetZone(world.WorldID, Zone.Indar);
                        ZoneState? hossin = ZoneStateStore.Get().GetZone(world.WorldID, Zone.Hossin);
                        ZoneState? amerish = ZoneStateStore.Get().GetZone(world.WorldID, Zone.Amerish);
                        ZoneState? esamir = ZoneStateStore.Get().GetZone(world.WorldID, Zone.Esamir);
                        ZoneState? oshur = ZoneStateStore.Get().GetZone(world.WorldID, Zone.Oshur);

                        if (indar != null) { world.Zones.Add(indar); }
                        if (hossin != null) { world.Zones.Add(hossin); }
                        if (amerish != null) { world.Zones.Add(amerish); }
                        if (esamir != null) { world.Zones.Add(esamir); }
                        if (oshur != null) { world.Zones.Add(oshur); }
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
