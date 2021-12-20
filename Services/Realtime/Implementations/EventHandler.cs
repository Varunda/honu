using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code;
using watchtower.Code.Constants;
using watchtower.Code.ExtensionMethods;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.Events;
using watchtower.Models.Queues;
using watchtower.Services;
using watchtower.Services.Census;
using watchtower.Services.Db;
using watchtower.Services.Queues;
using watchtower.Services.Repositories;

namespace watchtower.Realtime {

    public class EventHandler : IEventHandler {

        private readonly ILogger<EventHandler> _Logger;

        private readonly IKillEventDbStore _KillEventDb;
        private readonly IExpEventDbStore _ExpEventDb;
        private readonly ISessionDbStore _SessionDb;
        private readonly FacilityControlDbStore _ControlDb;
        private readonly IBattleRankDbStore _BattleRankDb;
        private readonly FacilityPlayerControlDbStore _FacilityPlayerDb;

        private readonly IBackgroundCharacterCacheQueue _CacheQueue;
        private readonly IBackgroundSessionStarterQueue _SessionQueue;
        private readonly BackgroundCharacterWeaponStatQueue _WeaponQueue;
        private readonly IDiscordMessageQueue _MessageQueue;
        private readonly BackgroundLogoutBufferQueue _LogoutQueue;

        private readonly ICharacterRepository _CharacterRepository;
        private readonly IMapCollection _MapCensus;

        private readonly List<JToken> _Recent;

        public EventHandler(ILogger<EventHandler> logger,
            IKillEventDbStore killEventDb, IExpEventDbStore expDb,
            IBackgroundCharacterCacheQueue cacheQueue, ICharacterRepository charRepo,
            ISessionDbStore sessionDb, IBackgroundSessionStarterQueue sessionQueue,
            IDiscordMessageQueue msgQueue, IMapCollection mapColl,
            FacilityControlDbStore controlDb, BackgroundCharacterWeaponStatQueue weaponQueue,
            IBattleRankDbStore rankDb, BackgroundLogoutBufferQueue logoutQueue,
            FacilityPlayerControlDbStore fpDb) {

            _Logger = logger;

            _Recent = new List<JToken>();

            _KillEventDb = killEventDb ?? throw new ArgumentNullException(nameof(killEventDb));
            _ExpEventDb = expDb ?? throw new ArgumentNullException(nameof(expDb));
            _SessionDb = sessionDb ?? throw new ArgumentNullException(nameof(sessionDb));
            _ControlDb = controlDb ?? throw new ArgumentNullException(nameof(controlDb));
            _BattleRankDb = rankDb ?? throw new ArgumentNullException(nameof(rankDb));
            _FacilityPlayerDb = fpDb ?? throw new ArgumentNullException(nameof(fpDb));

            _CacheQueue = cacheQueue ?? throw new ArgumentNullException(nameof(cacheQueue));
            _SessionQueue = sessionQueue ?? throw new ArgumentNullException(nameof(sessionQueue));
            _MessageQueue = msgQueue ?? throw new ArgumentNullException(nameof(msgQueue));
            _WeaponQueue = weaponQueue ?? throw new ArgumentNullException(nameof(weaponQueue));
            _LogoutQueue = logoutQueue ?? throw new ArgumentNullException(nameof(logoutQueue));

            _CharacterRepository = charRepo ?? throw new ArgumentNullException(nameof(charRepo));
            _MapCensus = mapColl ?? throw new ArgumentNullException(nameof(mapColl));
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

                string? eventName = payloadToken.Value<string?>("event_name");

                if (eventName == null) {
                    _Logger.LogWarning($"Missing 'event_name' from {ev}");
                } else if (eventName == "PlayerLogin") {
                    await _ProcessPlayerLogin(payloadToken);
                } else if (eventName == "PlayerLogout") {
                    await _ProcessPlayerLogout(payloadToken);
                } else if (eventName == "GainExperience") {
                    await _ProcessExperience(payloadToken);
                } else if (eventName == "Death") {
                    await _ProcessDeath(payloadToken);
                } else if (eventName == "FacilityControl") {
                    _ProcessFacilityControl(payloadToken);
                } else if (eventName == "PlayerFacilityCapture") {
                    _ProcessPlayerCapture(payloadToken);
                } else if (eventName == "PlayerFacilityDefend") {
                    _ProcessPlayerDefend(payloadToken);
                } else if (eventName == "ContinentUnlock") {
                    _ProcessContinentUnlock(payloadToken);
                } else if (eventName == "ContinentLock") {
                    _ProcessContinentLock(payloadToken);
                } else if (eventName == "BattleRankUp") {
                    await _ProcessBattleRankUp(payloadToken);
                } else if (eventName == "MetagameEvent") {
                    _ProcessMetagameEvent(payloadToken);
                } else {
                    _Logger.LogWarning($"Untracked event_name: '{eventName}': {payloadToken}");
                }
            }
        }

        private void _ProcessFacilityControl(JToken payload) {
            FacilityControlEvent ev = new() {
                FacilityID = payload.GetInt32("facility_id", 0),
                DurationHeld = payload.GetInt32("duration_held", 0),
                OutfitID = payload.NullableString("outfit_id"),
                OldFactionID = payload.GetInt16("old_faction_id", 0),
                NewFactionID = payload.GetInt16("new_faction_id", 0),
                ZoneID = payload.GetZoneID(),
                WorldID = payload.GetWorldID(),
                Timestamp = payload.CensusTimestamp("timestamp")
            };

            ushort defID = (ushort) (ev.ZoneID & 0xFFFF);
            ushort instanceID = (ushort) ((ev.ZoneID & 0xFFFF0000) >> 4);

            // Exclude flips that aren't interesting
            if (ev.OldFactionID == 0 || ev.NewFactionID == 0 // Came from a cont unlock
                || defID == 95 // A tutorial area
                || defID == 364 // Another tutorial area
                ) {

                return;
            }

            new Thread(async () => {
                // Wait a second for all the PlayerCapture and PlayerDefend events to come in
                await Task.Delay(1000);

                List<PlayerControlEvent> events;

                lock (PlayerFacilityControlStore.Get().Events) {
                    // Clean up is handled in a period hosted service
                    events = PlayerFacilityControlStore.Get().Events.Where(iter => {
                        return iter.FacilityID == ev.FacilityID
                            && iter.WorldID == ev.WorldID
                            && iter.ZoneID == ev.ZoneID
                            && iter.Timestamp == ev.Timestamp;
                    }).ToList();

                    ev.Players = events.Count;
                }

                if (ev.Players == 0) {
                    return;
                }

                Stopwatch timer = Stopwatch.StartNew();
                UnstableState state = await _MapCensus.GetUnstableState(ev.WorldID, ev.ZoneID);
                if (timer.ElapsedMilliseconds > 1000) {
                    //_Logger.LogTrace($"Took {timer.ElapsedMilliseconds}ms to get unstable state for {ev.WorldID}:{ev.ZoneID}");
                }
                timer.Stop();

                ev.UnstableState = state;

                long ID = await _ControlDb.Insert(ev);

                timer.Restart();
                await _FacilityPlayerDb.InsertMany(ID, events);
                //_Logger.LogTrace($"CONTROL> Took {timer.ElapsedMilliseconds}ms to insert {events.Count} entries");
                //_Logger.LogDebug($"CONTROL> {ev.FacilityID} :: {ev.Players}, {ev.OldFactionID} => {ev.NewFactionID}, {ev.WorldID}:{instanceID:X}.{defID:X}, state: {ev.UnstableState}, {ev.Timestamp}");
            }).Start();
        }

        private async Task _ProcessBattleRankUp(JToken payload) {
            string charID = payload.GetRequiredString("character_id");
            int rank = payload.GetInt32("battle_rank", 0);
            DateTime timestamp = payload.CensusTimestamp("timestamp");

            await _BattleRankDb.Insert(charID, rank, timestamp);
        }

        private void _ProcessPlayerCapture(JToken payload) {
            PlayerControlEvent ev = new() {
                IsCapture = true,
                CharacterID = payload.GetRequiredString("character_id"),
                FacilityID = payload.GetInt32("facility_id", 0),
                OutfitID = payload.NullableString("outfit_id"),
                WorldID = payload.GetWorldID(),
                ZoneID = payload.GetZoneID(),
                Timestamp = payload.CensusTimestamp("timestamp")
            };

            // Inserted into the DB after the facility control event is generated, and the ID is known
            lock (PlayerFacilityControlStore.Get().Events) {
                PlayerFacilityControlStore.Get().Events.Add(ev);
            }
        }

        private void _ProcessPlayerDefend(JToken payload) {
            PlayerControlEvent ev = new() {
                IsCapture = false,
                CharacterID = payload.GetRequiredString("character_id"),
                FacilityID = payload.GetInt32("facility_id", 0),
                OutfitID = payload.NullableString("outfit_id"),
                WorldID = payload.GetWorldID(),
                ZoneID = payload.GetZoneID(),
                Timestamp = payload.CensusTimestamp("timestamp")
            };

            // Inserted into the DB after the facility control event is generated, and the ID is known
            lock (PlayerFacilityControlStore.Get().Events) {
                PlayerFacilityControlStore.Get().Events.Add(ev);
            }
        }

        private async Task _ProcessPlayerLogin(JToken payload) {
            //_Logger.LogTrace($"Processing login: {payload}");

            string? charID = payload.Value<string?>("character_id");
            if (charID != null) {
                _CacheQueue.Queue(charID);

                TrackedPlayer p;

                lock (CharacterStore.Get().Players) {
                    // The FactionID and TeamID are updated as part of caching the character
                    p = CharacterStore.Get().Players.GetOrAdd(charID, new TrackedPlayer() {
                        ID = charID,
                        WorldID = payload.GetWorldID(),
                        ZoneID = 0,
                        FactionID = Faction.UNKNOWN,
                        TeamID = Faction.UNKNOWN,
                        Online = false
                    });

                    p.LastLogin = DateTime.UtcNow;
                }

                await _SessionDb.Start(p);

                p.LatestEventTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
        }

        private async Task _ProcessPlayerLogout(JToken payload) {
            string? charID = payload.Value<string?>("character_id");
            if (charID != null) {
                _CacheQueue.Queue(charID);
                //_WeaponQueue.Queue(charID);

                TrackedPlayer? p;
                lock (CharacterStore.Get().Players) {
                    _ = CharacterStore.Get().Players.TryGetValue(charID, out p);
                }

                if (p != null) {
                    // Null if Honu was started when the character was online
                    if (p.LastLogin != null) {
                        _LogoutQueue.Queue(new LogoutBufferEntry() {
                            CharacterID = charID,
                            LoginTime = p.LastLogin.Value
                        });
                    } else {
                        _WeaponQueue.Queue(charID);
                    }

                    await _SessionDb.End(p);

                    // Reset team of the NSO player as they're now offline
                    if (p.FactionID == Faction.NS) {
                        p.TeamID = Faction.NS;
                    }
                }
            }
        }

        private void _ProcessMetagameEvent(JToken payload) {
            short worldID = payload.GetWorldID();
            uint zoneID = payload.GetZoneID();
            string metagameEventName = payload.GetString("metagame_event_state_name", "missing");
            int metagameEventID = payload.GetInt32("metagame_event_id", 0);

            //_Logger.LogDebug($"metagame event payload: {payload}");

            lock (ZoneStateStore.Get().Zones) {
                ZoneState? state = ZoneStateStore.Get().GetZone(worldID, zoneID);

                if (state == null) {
                    state = new ZoneState() {
                        ZoneID = zoneID,
                        WorldID = worldID,
                        IsOpened = true
                    };
                }

                if (metagameEventName == "started") {
                    state.AlertStart = DateTime.UtcNow;

                    TimeSpan? duration = MetagameEvent.GetDuration(metagameEventID);
                    if (duration == null) {
                        _Logger.LogWarning($"Failed to find duration of metagame event {metagameEventID}\n{payload}");
                    } else {
                        state.AlertEnd = state.AlertStart + duration;
                    }
                } else if (metagameEventName == "ended") {
                    state.AlertStart = null;

                    // Continent unlock events are not sent. To check if a continent is open,
                    //      we get the owner of each continent. If there is no owner, then 
                    //      a continent must be open
                    new Thread(async () => {
                        // Ensure census has times to update
                        await Task.Delay(5000);

                        short? indarOwner = await _MapCensus.GetZoneMapOwner(worldID, Zone.Indar);
                        short? hossinOwner = await _MapCensus.GetZoneMapOwner(worldID, Zone.Hossin);
                        short? amerishOwner = await _MapCensus.GetZoneMapOwner(worldID, Zone.Amerish);
                        short? esamirOwner = await _MapCensus.GetZoneMapOwner(worldID, Zone.Esamir);

                        if (indarOwner == null) { ZoneStateStore.Get().UnlockZone(worldID, Zone.Indar); }
                        if (hossinOwner == null) { ZoneStateStore.Get().UnlockZone(worldID, Zone.Hossin); }
                        if (amerishOwner == null) { ZoneStateStore.Get().UnlockZone(worldID, Zone.Amerish); }
                        if (esamirOwner == null) { ZoneStateStore.Get().UnlockZone(worldID, Zone.Esamir); }

                        _Logger.LogDebug($"ALERT ended in {worldID}, current owners:\nIndar: {indarOwner}\nHossin: {hossinOwner}\nAmerish: {amerishOwner}\nEsamir: {esamirOwner}");
                    }).Start();
                }

                ZoneStateStore.Get().SetZone(worldID, zoneID, state);
            }
            //_Logger.LogInformation($"METAGAME in world {worldID} zone {zoneID} metagame: {metagameEventName}");
        }

        private void _ProcessContinentUnlock(JToken payload) {
            short worldID = payload.GetWorldID();
            uint zoneID = payload.GetZoneID();

            lock (ZoneStateStore.Get().Zones) {
                ZoneState? state = ZoneStateStore.Get().GetZone(worldID, zoneID);

                if (state == null) {
                    state = new() {
                        ZoneID = zoneID,
                        WorldID = worldID,
                    };
                }

                state.IsOpened = true;

                ZoneStateStore.Get().SetZone(worldID, zoneID, state);
            }

            //_Logger.LogDebug($"OPENED In world {worldID} zone {zoneID} was opened");
        }

        private void _ProcessContinentLock(JToken payload) {
            short worldID = payload.GetWorldID();
            uint zoneID = payload.GetZoneID();

            lock (ZoneStateStore.Get().Zones) {
                ZoneState? state = ZoneStateStore.Get().GetZone(worldID, zoneID);

                if (state == null) {
                    state = new() {
                        ZoneID = zoneID,
                        WorldID = worldID,
                    };
                }

                state.IsOpened = false;
                state.AlertEnd = null;
                state.AlertStart = null;

                ZoneStateStore.Get().SetZone(worldID, zoneID, state);
            }

            //_Logger.LogDebug($" CLOSE In world {worldID} zone {zoneID} was closed");
        }

        private async Task _ProcessDeath(JToken payload) {
            int timestamp = payload.Value<int?>("timestamp") ?? 0;

            uint zoneID = payload.GetZoneID();
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
                AttackerTeamID = attackerFactionID,
                KilledCharacterID = charID,
                KilledLoadoutID = loadoutID,
                KilledTeamID = factionID,
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime,
                WeaponID = payload.GetString("attacker_weapon_id", "0"),
                WorldID = payload.GetWorldID(),
                ZoneID = payload.GetZoneID(),
                AttackerFireModeID = payload.GetInt32("attacker_fire_mode_id", 0),
                AttackerVehicleID = payload.GetInt32("attacker_vehicle_id", 0),
                IsHeadshot = (payload.Value<string?>("is_headshot") ?? "0") != "0"
            };

            //_Logger.LogTrace($"Processing death: {payload}");

            lock (CharacterStore.Get().Players) {
                // The default value for Online must be false, else when a new TrackedPlayer is constructed,
                //      the session will never start, as the handler already sees the character online,
                //      so no need to start a new session
                TrackedPlayer attacker = CharacterStore.Get().Players.GetOrAdd(attackerID, new TrackedPlayer() {
                    ID = attackerID,
                    FactionID = attackerFactionID,
                    TeamID = ev.AttackerTeamID,
                    Online = false,
                    WorldID = ev.WorldID
                });

                if (attacker.Online == false) {
                    _SessionQueue.Queue(attacker);
                }

                _CacheQueue.Queue(attacker.ID);

                attacker.ZoneID = zoneID;

                if (attacker.FactionID == Faction.UNKNOWN) {
                    attacker.FactionID = attackerFactionID; // If a tracked player was made from a login, no faction ID is given
                    attacker.TeamID = ev.AttackerTeamID;
                }

                ev.AttackerTeamID = attacker.TeamID;

                // See above for why false is used for the Online value, instead of true
                TrackedPlayer killed = CharacterStore.Get().Players.GetOrAdd(charID, new TrackedPlayer() {
                    ID = charID,
                    FactionID = factionID,
                    TeamID = ev.KilledTeamID,
                    Online = false,
                    WorldID = ev.WorldID
                });

                _CacheQueue.Queue(killed.ID);

                // Ensure that 2 sessions aren't started if the attacker and killed are the same
                if (killed.Online == false && attacker.ID != killed.ID) {
                    _SessionQueue.Queue(attacker);
                }

                killed.ZoneID = zoneID;
                if (killed.FactionID == Faction.UNKNOWN) {
                    killed.FactionID = factionID;
                    killed.TeamID = ev.KilledTeamID;
                }

                ev.KilledTeamID = killed.TeamID;

                long nowSeconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                attacker.LatestEventTimestamp = nowSeconds;
                killed.LatestEventTimestamp = nowSeconds;
            }

            await _KillEventDb.Insert(ev);
        }

        private async Task _ProcessExperience(JToken payload) {
            //_Logger.LogInformation($"Processing exp: {payload}");

            string? charID = payload.Value<string?>("character_id");
            if (charID == null) {
                return;
            }

            _CacheQueue.Queue(charID);

            int expId = payload.GetInt32("experience_id", -1);
            short loadoutId = payload.GetInt16("loadout_id", -1);
            short worldID = payload.GetWorldID();
            int timestamp = payload.Value<int?>("timestamp") ?? 0;
            uint zoneID = payload.GetZoneID();
            string otherID = payload.GetString("other_id", "0");

            short factionID = Loadout.GetFaction(loadoutId);

            ExpEvent ev = new ExpEvent() {
                SourceID = charID,
                LoadoutID = loadoutId,
                TeamID = factionID,
                Amount = payload.Value<int?>("amount") ?? 0,
                ExperienceID = expId,
                OtherID = otherID,
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime,
                WorldID = worldID,
                ZoneID = zoneID
            };

            lock (CharacterStore.Get().Players) {
                // Default false for |Online| to ensure a session is started
                TrackedPlayer p = CharacterStore.Get().Players.GetOrAdd(charID, new TrackedPlayer() {
                    ID = charID,
                    FactionID = factionID,
                    TeamID = factionID,
                    Online = false,
                    WorldID = worldID
                });

                if (p.Online == false) {
                    _SessionQueue.Queue(p);
                }

                p.LatestEventTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                p.ZoneID = zoneID;

                if (p.FactionID == Faction.UNKNOWN) {
                    p.FactionID = factionID;
                    p.TeamID = factionID;
                }

                // Update the team_id field if needed
                if (Experience.IsRevive(expId) || Experience.IsHeal(expId) || Experience.IsResupply(expId)) {
                    // If either character was not NSO, update the team_id of the character
                    // If both are NSO, this field is not updated, as one bad team_id could then spread to other NSOs, messing up tracking
                    if (CharacterStore.Get().Players.TryGetValue(otherID, out TrackedPlayer? otherPlayer)) {
                        if (p.FactionID == Faction.NS && otherPlayer.FactionID != Faction.NS
                            && otherPlayer.FactionID != Faction.UNKNOWN && p.TeamID != otherPlayer.FactionID) {

                            //_Logger.LogDebug($"Robot {p.ID} supported (exp {expId}, loadout {loadoutId}, faction {factionID}) non-robot {otherPlayer.ID}, setting robot team ID to {otherPlayer.FactionID} from {p.TeamID}");
                            p.TeamID = otherPlayer.FactionID;
                        }

                        if (p.FactionID != Faction.NS && p.FactionID != Faction.UNKNOWN
                            && otherPlayer.FactionID == Faction.NS && otherPlayer.TeamID != p.FactionID) {

                            //_Logger.LogDebug($"Non-robot {p.ID} supported (exp {expId}, loadout {loadoutId}, faction {factionID}) robot {otherPlayer.ID}, setting robot team ID to {p.FactionID}, from {otherPlayer.TeamID}");
                            otherPlayer.TeamID = p.FactionID;
                        }
                    }
                }

                ev.TeamID = p.TeamID;
            }

            long ID = await _ExpEventDb.Insert(ev);

            if (ev.ExperienceID == Experience.REVIVE || ev.ExperienceID == Experience.SQUAD_REVIVE) {
                await _KillEventDb.SetRevivedID(ev.OtherID, ID);
            }

            // Track the sundy and how many spawns it has
            if (expId == Experience.SUNDERER_SPAWN_BONUS && otherID != null && otherID != "0") {
                lock (NpcStore.Get().Npcs) {
                    TrackedNpc npc = NpcStore.Get().Npcs.GetOrAdd(otherID, new TrackedNpc() {
                        OwnerID = charID,
                        FirstSeenAt = DateTime.UtcNow,
                        NpcID = otherID,
                        SpawnCount = 0,
                        Type = NpcType.Sunderer,
                        WorldID = worldID
                    });

                    ++npc.SpawnCount;
                    npc.LatestEventAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                };
            } else if (expId == Experience.GENERIC_NPC_SPAWN && otherID != null && otherID != "0") {
                lock (NpcStore.Get().Npcs) {
                    TrackedNpc npc = NpcStore.Get().Npcs.GetOrAdd(otherID, new TrackedNpc() {
                        OwnerID = charID,
                        FirstSeenAt = DateTime.UtcNow,
                        NpcID = otherID,
                        SpawnCount = 0,
                        Type = NpcType.Router,
                        WorldID = worldID
                    });

                    ++npc.SpawnCount;
                    npc.LatestEventAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                }
            }

            if (expId == Experience.VKILL_SUNDY && worldID == World.Jaeger) {
            //if (expId == Experience.VKILL_SUNDY) {// && worldID == World.Jaeger) {
                TrackedNpc? bus = null;

                if (otherID != null && otherID != "0") {
                    lock (NpcStore.Get().Npcs) {
                        NpcStore.Get().Npcs.TryGetValue(otherID, out bus);
                    }
                }

                new Thread(async () => {
                    PsCharacter? attacker = await _CharacterRepository.GetByID(charID);
                    PsCharacter? owner = (bus != null) ? await _CharacterRepository.GetByID(bus.OwnerID) : null;

                    string msg = $"A bus was blown up by {attacker?.GetDisplayName() ?? $"<Missing {charID}>"}\n";
                    msg += $"Owner: {owner?.GetDisplayName() ?? $"<Missing {bus?.OwnerID}>"}\n";

                    if (bus != null) {
                        DateTime lastUsed = DateTimeOffset.FromUnixTimeMilliseconds(bus.LatestEventAt).UtcDateTime;

                        msg += $"First spawn at: {bus.FirstSeenAt} UTC\n";
                        msg += $"Spawns: {bus.SpawnCount}\n";
                        msg += $"Last used: {(int) (DateTime.UtcNow - lastUsed).TotalSeconds} seconds ago";
                    }

                    _MessageQueue.Queue(msg);
                }).Start();
            }

        }

    }
}
