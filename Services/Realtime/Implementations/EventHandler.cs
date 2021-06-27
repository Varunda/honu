using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Models.Events;
using watchtower.Services;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Realtime {

    public class EventHandler : IEventHandler {

        private readonly ILogger<EventHandler> _Logger;

        private readonly IKillEventDbStore _KillEventDb;
        private readonly IExpEventDbStore _ExpEventDb;

        private readonly IBackgroundCharacterCacheQueue _CacheQueue;

        private readonly List<JToken> _Recent;

        public EventHandler(ILogger<EventHandler> logger,
            IKillEventDbStore killEventDb, IExpEventDbStore expDb,
            IBackgroundCharacterCacheQueue cacheQueue) {

            _Logger = logger;

            _Recent = new List<JToken>();

            _KillEventDb = killEventDb ?? throw new ArgumentNullException(nameof(killEventDb));
            _ExpEventDb = expDb ?? throw new ArgumentNullException(nameof(expDb));
            _CacheQueue = cacheQueue ?? throw new ArgumentNullException(nameof(cacheQueue));
        }

        public async Task Process(JToken ev) {
            if (_Recent.Contains(ev)) {
                _Logger.LogInformation($"Skipping duplicate event {ev}");
                return;
            }

            _Recent.Add(ev);
            if (_Recent.Count > 10) {
                _Recent.RemoveAt(0);
            }

            string? type = ev.Value<string?>("type");

            if (type == "serviceMessage") {
                JToken? payloadToken = ev.SelectToken("payload");
                if (payloadToken == null) {
                    _Logger.LogWarning($"Missing 'payload' from {ev}");
                    return;
                }

                string? worldID = payloadToken.Value<string?>("world_id");
                if (worldID != "1") {
                    return;
                }

                string? eventName = payloadToken.Value<string?>("event_name");

                if (eventName == null) {
                    _Logger.LogWarning($"Missing 'event_name' from {ev}");
                } else if (eventName == "PlayerLogin") {
                    _ProcessPlayerLogin(payloadToken);
                } else if (eventName == "PlayerLogout") {
                    _ProcessPlayerLogout(payloadToken);
                } else if (eventName == "GainExperience") {
                    await  _ProcessExperience(payloadToken);
                } else if (eventName == "Death") {
                    await _ProcessDeath(payloadToken);
                } else {
                    _Logger.LogWarning($"Untracked event_name: '{eventName}'");
                }
            }
        }

        private void _ProcessPlayerLogin(JToken payload) {
            //_Logger.LogTrace($"Processing login: {payload}");

            string? charID = payload.Value<string?>("character_id");
            if (charID != null) {
                _CacheQueue.Queue(charID);

                string worldID = payload.Value<string?>("world_id") ?? "0";

                lock (CharacterStore.Get().Players) {
                    TrackedPlayer p = CharacterStore.Get().Players.GetOrAdd(charID, new TrackedPlayer() {
                        ID = charID,
                        WorldID = worldID,
                        ZoneID = "-1",
                        FactionID = -1
                    });

                    p.Online = true;
                    p.LatestEventTimestamp = (int) DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                }
            }
        }

        private void _ProcessPlayerLogout(JToken payload) {
            string? charID = payload.Value<string?>("character_id");
            if (charID != null) {
                _CacheQueue.Queue(charID);

                lock (CharacterStore.Get().Players) {
                    if (CharacterStore.Get().Players.TryGetValue(charID, out TrackedPlayer? p) == true) {
                        if (p != null) {
                            p.Online = false;
                        }
                    }
                }
            }
        }

        private async Task _ProcessDeath(JToken payload) {
            int timestamp = payload.Value<int?>("timestamp") ?? 0;

            string zoneID = payload.Value<string?>("zone_id") ?? "-1";
            string attackerID = payload.Value<string?>("attacker_character_id") ?? "0";
            short attackerLoadoutID = payload.Value<short?>("attacker_loadout_id") ?? -1;
            string charID = payload.Value<string?>("character_id") ?? "0";
            short loadoutID = payload.Value<short?>("character_loadout_id") ?? -1;

            short attackerFactionID = Loadout.GetFaction(attackerLoadoutID);
            short factionID = Loadout.GetFaction(loadoutID);

            _CacheQueue.Queue(charID);
            _CacheQueue.Queue(attackerID);

            KillEvent ev = new KillEvent() {
                AttackerCharacterID = attackerID,
                AttackerLoadoutID = attackerLoadoutID,
                KilledCharacterID = charID,
                KilledLoadoutID = loadoutID,
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime,
                WeaponID = payload.GetString("attacker_weapon_id", "0"),
                WorldID = payload.GetWorldID(),
                ZoneID = payload.GetZoneID(),
                AttackerFireModeID = payload.GetInt32("attacker_fire_mode_id", 0),
                AttackerVehicleID = payload.GetInt32("attacker_vehicle_id", 0),
                IsHeadshot = (payload.Value<string?>("is_headshot") ?? "0") != "0"
            };

            await _KillEventDb.Insert(ev);

            //_Logger.LogTrace($"Processing death: {payload}");

            lock (CharacterStore.Get().Players) {
                TrackedPlayer attacker = CharacterStore.Get().Players.GetOrAdd(attackerID, new TrackedPlayer() {
                    ID = attackerID,
                    FactionID = attackerFactionID,
                    Online = true,
                    WorldID = payload.Value<string?>("world_id") ?? "-1"
                });

                attacker.Online = true;
                attacker.ZoneID = zoneID;
                attacker.FactionID = attackerFactionID; // If a tracked player was made from a login, no faction ID is given

                TrackedPlayer killed = CharacterStore.Get().Players.GetOrAdd(charID, new TrackedPlayer() {
                    ID = charID,
                    FactionID = factionID,
                    Online = true,
                    WorldID = payload.Value<string?>("world_id") ?? "-1"
                });

                killed.Online = true;
                killed.ZoneID = zoneID;
                killed.FactionID = factionID;

                int nowSeconds = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                attacker.LatestEventTimestamp = nowSeconds;
                killed.LatestEventTimestamp = nowSeconds;

                if (attackerFactionID != factionID) {
                    attacker.Kills.Add(timestamp);
                    killed.Deaths.Add(timestamp);
                }
            }
        }

        private async Task _ProcessExperience(JToken payload) {
            //_Logger.LogInformation($"Processing exp: {payload}");

            string? charID = payload.Value<string?>("character_id");
            if (charID == null) {
                return;
            }

            _CacheQueue.Queue(charID);

            int expId = payload.Value<int?>("experience_id") ?? -1;
            short loadoutId = payload.Value<short?>("loadout_id") ?? -1;
            int timestamp = payload.Value<int?>("timestamp") ?? 0;
            int zoneID = payload.Value<int?>("zone_id") ?? -1;
            string otherID = payload.Value<string?>("other_id") ?? "0";

            short factionID = Loadout.GetFaction(loadoutId);

            ExpEvent ev = new ExpEvent() {
                SourceID = charID,
                LoadoutID = loadoutId,
                Amount = payload.Value<int?>("amount") ?? 0,
                ExperienceID = expId,
                OtherID = otherID,
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime,
                WorldID = payload.Value<short?>("world_id") ?? -1,
                ZoneID = zoneID
            };

            long ID = await _ExpEventDb.Insert(ev);

            if (ev.ExperienceID == Experience.REVIVE || ev.ExperienceID == Experience.SQUAD_REVIVE) {
                await _KillEventDb.SetRevivedID(ev.OtherID, ID);
            }

            lock (CharacterStore.Get().Players) {
                TrackedPlayer p = CharacterStore.Get().Players.GetOrAdd(charID, new TrackedPlayer() {
                    ID = charID,
                    FactionID = factionID,
                    Online = true,
                    WorldID = payload.Value<string?>("world_id") ?? "-1"
                });

                p.Online = true;
                p.LatestEventTimestamp = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                p.ZoneID = zoneID.ToString();

                if (expId == Experience.HEAL || expId == Experience.SQUAD_HEAL) {
                    p.Heals.Add(timestamp);
                } else if (expId == Experience.REVIVE || expId == Experience.SQUAD_REVIVE) {
                    p.Revives.Add(timestamp);

                    // Remove the most recent death
                    if (otherID != null && otherID != "0") {
                        if (CharacterStore.Get().Players.TryGetValue(otherID, out TrackedPlayer? player) == true) {
                            if (player != null) {
                                if (player.Deaths.Count > 0) {
                                    player.Deaths.RemoveAt(player.Deaths.Count - 1);
                                }
                            }
                        }
                    }
                } else if (expId == Experience.RESUPPLY || expId == Experience.SQUAD_RESUPPLY) {
                    p.Resupplies.Add(timestamp);
                } else if (Experience.IsSpawn(expId)) {
                    p.Spawns.Add(timestamp);

                    if (expId == Experience.SUNDERER_SPAWN_BONUS) {

                    }
                } else if (Experience.IsAssist(expId)) {
                    p.Assists.Add(timestamp);
                }
            }

            if (expId == Experience.SUNDERER_SPAWN_BONUS && otherID != null && otherID != "0") {
                lock (NpcStore.Get().Npcs) {
                    TrackedNpc npc = NpcStore.Get().Npcs.GetOrAdd(otherID, new TrackedNpc() {
                        OwnerID = charID,
                        FirstSeenAt = DateTime.UtcNow,
                        NpcID = otherID,
                        SpawnCount = 0,
                        Type = "Sundy"
                    });

                    ++npc.SpawnCount;
                    npc.LatestEventAt = (int) DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                };
            }
        }

    }
}
