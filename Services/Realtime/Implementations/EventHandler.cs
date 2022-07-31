using Microsoft.AspNetCore.SignalR;
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
using watchtower.Code.Hubs;
using watchtower.Code.Hubs.Implementations;
using watchtower.Code.Tracking;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Models.Alert;
using watchtower.Models.Census;
using watchtower.Models.Events;
using watchtower.Models.Queues;
using watchtower.Services.Census;
using watchtower.Services.Db;
using watchtower.Services.Queues;
using watchtower.Services.Realtime;
using watchtower.Services.Repositories;

namespace watchtower.Realtime {

    public class EventHandler : IEventHandler {

        private readonly ILogger<EventHandler> _Logger;

        private readonly KillEventDbStore _KillEventDb;
        private readonly ExpEventDbStore _ExpEventDb;
        private readonly SessionRepository _SessionRepository;
        private readonly FacilityControlDbStore _ControlDb;
        private readonly IBattleRankDbStore _BattleRankDb;
        private readonly FacilityPlayerControlDbStore _FacilityPlayerDb;
        private readonly VehicleDestroyDbStore _VehicleDestroyDb;
        private readonly ItemAddedDbStore _ItemAddedDb;
        private readonly AchievementEarnedDbStore _AchievementEarnedDb;
        private readonly AlertDbStore _AlertDb;
        private readonly AlertPlayerDataRepository _ParticipantDataRepository;

        private readonly CharacterCacheQueue _CacheQueue;
        private readonly SessionStarterQueue _SessionQueue;
        private readonly CharacterUpdateQueue _WeaponQueue;
        private readonly DiscordMessageQueue _MessageQueue;
        private readonly LogoutUpdateBuffer _LogoutQueue;
        private readonly JaegerSignInOutQueue _JaegerQueue;

        private readonly CharacterRepository _CharacterRepository;
        private readonly MapCollection _MapCensus;
        private readonly ItemRepository _ItemRepository;
        private readonly MapRepository _MapRepository;
        private readonly FacilityRepository _FacilityRepository;

        private readonly IHubContext<RealtimeMapHub> _MapHub;
        private readonly WorldTagManager _TagManager;

        private readonly List<string> _Recent;
        private DateTime _MostRecentProcess = DateTime.UtcNow;

        public EventHandler(ILogger<EventHandler> logger,
            KillEventDbStore killEventDb, ExpEventDbStore expDb,
            CharacterCacheQueue cacheQueue, CharacterRepository charRepo,
            SessionStarterQueue sessionQueue, SessionRepository sessionRepository,
            DiscordMessageQueue msgQueue, MapCollection mapColl,
            FacilityControlDbStore controlDb, CharacterUpdateQueue weaponQueue,
            IBattleRankDbStore rankDb, LogoutUpdateBuffer logoutQueue,
            FacilityPlayerControlDbStore fpDb, VehicleDestroyDbStore vehicleDestroyDb,
            ItemRepository itemRepo, MapRepository mapRepo,
            JaegerSignInOutQueue jaegerQueue, FacilityRepository facRepo,
            IHubContext<RealtimeMapHub> mapHub, AlertDbStore alertDb,
            AlertPlayerDataRepository participantDataRepository, WorldTagManager tagManager,
            ItemAddedDbStore itemAddedDb, AchievementEarnedDbStore achievementEarnedDb) { 

            _Logger = logger;

            _Recent = new List<string>();

            _KillEventDb = killEventDb ?? throw new ArgumentNullException(nameof(killEventDb));
            _ExpEventDb = expDb ?? throw new ArgumentNullException(nameof(expDb));
            _ControlDb = controlDb ?? throw new ArgumentNullException(nameof(controlDb));
            _BattleRankDb = rankDb ?? throw new ArgumentNullException(nameof(rankDb));
            _FacilityPlayerDb = fpDb ?? throw new ArgumentNullException(nameof(fpDb));
            _VehicleDestroyDb = vehicleDestroyDb ?? throw new ArgumentNullException(nameof(vehicleDestroyDb));
            _AlertDb = alertDb ?? throw new ArgumentNullException(nameof(alertDb));

            _CacheQueue = cacheQueue ?? throw new ArgumentNullException(nameof(cacheQueue));
            _SessionQueue = sessionQueue ?? throw new ArgumentNullException(nameof(sessionQueue));
            _MessageQueue = msgQueue ?? throw new ArgumentNullException(nameof(msgQueue));
            _WeaponQueue = weaponQueue ?? throw new ArgumentNullException(nameof(weaponQueue));
            _LogoutQueue = logoutQueue ?? throw new ArgumentNullException(nameof(logoutQueue));
            _JaegerQueue = jaegerQueue ?? throw new ArgumentNullException(nameof(jaegerQueue));

            _CharacterRepository = charRepo ?? throw new ArgumentNullException(nameof(charRepo));
            _MapCensus = mapColl ?? throw new ArgumentNullException(nameof(mapColl));
            _ItemRepository = itemRepo ?? throw new ArgumentNullException(nameof(itemRepo));
            _MapRepository = mapRepo ?? throw new ArgumentNullException(nameof(mapRepo));
            _FacilityRepository = facRepo ?? throw new ArgumentNullException(nameof(facRepo));

            _MapHub = mapHub;
            _ParticipantDataRepository = participantDataRepository;
            _TagManager = tagManager;
            _ItemAddedDb = itemAddedDb;
            _AchievementEarnedDb = achievementEarnedDb;
            _SessionRepository = sessionRepository;
        }

        public DateTime MostRecentProcess() {
            return _MostRecentProcess;
        }

        public async Task Process(JToken ev) {
            // The default == for tokens seems like it's by reference, not value. Since the order of the keys in the JSON
            //      object is fixed and hasn't changed in the last 7 months, this is safe.
            // If the order of keys changes, this method of detecting duplicate events will have to change, as it relies
            //      on the key order being the same for duplicate events
            //
            // For example:
            //      { id: 1, value: "howdy" }
            //      { value: "howdy", id: 1 }
            //  
            //      The strings for these JTokens would be different, but they represent the same object. The current duplicate
            //      event checking would not handle this correctly
            //
            if (_Recent.Contains(ev.ToString())) {
                //_Logger.LogError($"Skipping duplicate event {ev}");
                return;
            }

            _Recent.Add(ev.ToString());
            if (_Recent.Count > 50) {
                _Recent.RemoveAt(0);
            }

            using var processTrace = HonuActivitySource.Root.StartActivity("EventProcess");

            string? type = ev.Value<string?>("type");
            processTrace?.AddTag("type", type);

            if (type == "serviceMessage") {
                JToken? payloadToken = ev.SelectToken("payload");
                if (payloadToken == null) {
                    _Logger.LogWarning($"Missing 'payload' from {ev}");
                    return;
                }

                if (payloadToken.Value<int?>("timestamp") != null) {
                    _MostRecentProcess = payloadToken.CensusTimestamp("timestamp");
                }

                string? eventName = payloadToken.Value<string?>("event_name");
                processTrace?.AddTag("eventName", eventName);

                if (eventName == null) {
                    _Logger.LogWarning($"Missing 'event_name' from {ev}");
                } else if (eventName == "PlayerLogin") {
                    _ProcessPlayerLogin(payloadToken);
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
                    await _ProcessMetagameEvent(payloadToken);
                } else if (eventName == "VehicleDestroy") {
                    await _ProcessVehicleDestroy(payloadToken);
                } else if (eventName == "ItemAdded") {
                    await _ProcessItemAdded(payloadToken);
                } else if (eventName == "AchievementEarned") {
                    await _ProcessAchievementEarned(payloadToken);
                } else {
                    _Logger.LogWarning($"Untracked event_name: '{eventName}': {payloadToken}");
                }
            } else if (type == "heartbeat") {
                //_Logger.LogInformation($"Heartbeat: {ev}");
            } else if (type == "connectionStateChanged") {
                //_Logger.LogInformation($"connectionStateChanged: {ev}");
            } else if (type == "serviceStateChanged") {
                //_Logger.LogInformation($"serviceStateChanged: {ev}");
            } else if (type == "" || type == null) {
                //_Logger.LogInformation($": {ev}");
            } else {
                _Logger.LogWarning($"Unchecked message type: '{type}'");
            }
        }

        private async Task _ProcessVehicleDestroy(JToken payload) {
            //_Logger.LogDebug($"{payload}");

            short attackerLoadoutID = payload.GetInt16("attacker_loadout_id", -1);
            short attackerFactionID = Loadout.GetFaction(attackerLoadoutID);

            VehicleDestroyEvent ev = new() {
                AttackerCharacterID = payload.GetRequiredString("attacker_character_id"),
                AttackerLoadoutID = attackerLoadoutID,
                AttackerVehicleID = payload.GetString("attacker_vehicle_id", "0"),
                AttackerFactionID = attackerFactionID,
                // AttackerTeamID - This is set from CharacterStore
                AttackerWeaponID = payload.GetInt32("attacker_weapon_id", 0),

                KilledCharacterID = payload.GetRequiredString("character_id"),
                KilledFactionID = payload.GetInt16("faction_id", Faction.UNKNOWN),
                KilledVehicleID = payload.GetString("vehicle_id", "0"),
                // KilledTeamID - This is set from CharacterStore

                ZoneID = payload.GetZoneID(),
                WorldID = payload.GetWorldID(),
                FacilityID = payload.GetString("facility_id", "0"),
                Timestamp = payload.CensusTimestamp("timestamp")
            };

            if (ev.AttackerCharacterID == "0" && ev.KilledCharacterID == "0") {
                return;
            }

            if (ev.AttackerCharacterID == ev.KilledCharacterID) {
                ev.KilledFactionID = ev.AttackerFactionID;
            }

            lock (CharacterStore.Get().Players) {
                // The default value for Online must be false, else when a new TrackedPlayer is constructed,
                //      the session will never start, as the handler already sees the character online,
                //      so no need to start a new session
                TrackedPlayer attacker = CharacterStore.Get().Players.GetOrAdd(ev.AttackerCharacterID, new TrackedPlayer() {
                    ID = ev.AttackerCharacterID,
                    FactionID = ev.AttackerFactionID,
                    TeamID = (ev.AttackerFactionID == 4) ? Faction.NS : ev.AttackerFactionID,
                    Online = false,
                    WorldID = ev.WorldID
                });

                if (attacker.ID != "0" && attacker.Online == false) {
                    _SessionQueue.Queue(new CharacterSessionStartQueueEntry() {
                        CharacterID = attacker.ID,
                        LastEvent = ev.Timestamp
                    });
                }

                _CacheQueue.Queue(attacker.ID);

                attacker.ZoneID = ev.ZoneID;

                if (attacker.FactionID == Faction.UNKNOWN) {
                    attacker.FactionID = ev.AttackerFactionID; // If a tracked player was made from a login, no faction ID is given
                    attacker.TeamID = ev.AttackerTeamID;
                }

                ev.AttackerTeamID = attacker.TeamID;

                // See above for why false is used for the Online value, instead of true
                TrackedPlayer killed = CharacterStore.Get().Players.GetOrAdd(ev.KilledCharacterID, new TrackedPlayer() {
                    ID = ev.KilledCharacterID,
                    FactionID = ev.KilledFactionID,
                    TeamID = (ev.KilledFactionID == 4) ? Faction.NS : ev.KilledFactionID,
                    Online = false,
                    WorldID = ev.WorldID
                });

                _CacheQueue.Queue(killed.ID);

                // Ensure that 2 sessions aren't started if the attacker and killed are the same
                if (killed.ID != "0" && killed.Online == false && attacker.ID != killed.ID) {
                    _SessionQueue.Queue(new CharacterSessionStartQueueEntry() {
                        CharacterID = killed.ID,
                        LastEvent = ev.Timestamp
                    });
                }

                killed.ZoneID = ev.ZoneID;
                if (killed.FactionID == Faction.UNKNOWN) {
                    killed.FactionID = ev.KilledFactionID;
                    killed.TeamID = killed.FactionID;
                }

                ev.KilledTeamID = killed.TeamID;

                long nowSeconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                attacker.LatestEventTimestamp = nowSeconds;
                killed.LatestEventTimestamp = nowSeconds;
            }

            //_Logger.LogDebug($"\n{payload.ToString(Newtonsoft.Json.Formatting.None)}\n=>\n{JToken.FromObject(ev).ToString(Newtonsoft.Json.Formatting.None)}");

            if (World.IsTrackedWorld(ev.WorldID)) {
                await _VehicleDestroyDb.Insert(ev, CancellationToken.None);
            }

            if (ev.KilledVehicleID == Vehicle.SUNDERER && ev.WorldID == World.Jaeger) {
                List<ExpEvent> possibleEvents = RecentSundererDestroyExpStore.Get().GetList();
                ExpEvent? expEvent = null;

                foreach (ExpEvent exp in possibleEvents) {
                    if (exp.SourceID == ev.AttackerCharacterID && exp.Timestamp == ev.Timestamp) {
                        expEvent = exp;
                        break;
                    }

                    // Easily break in case there are many events to look at
                    if (exp.Timestamp > ev.Timestamp) {
                        break;
                    }
                }

                TrackedNpc? bus = null;

                if (expEvent != null && expEvent.OtherID != "0") {
                    lock (NpcStore.Get().Npcs) {
                        NpcStore.Get().Npcs.TryGetValue(expEvent.OtherID, out bus);
                    }
                }

                new Thread(async () => {
                    try {
                        PsCharacter? attacker = await _CharacterRepository.GetByID(ev.AttackerCharacterID);
                        PsCharacter? owner = (bus != null) ? await _CharacterRepository.GetByID(bus.OwnerID) : null;
                        PsCharacter? killed = await _CharacterRepository.GetByID(ev.KilledCharacterID);
                        PsItem? attackerItem = await _ItemRepository.GetByID(ev.AttackerWeaponID);

                        string msg = $"A bus has been blown up at **{ev.Timestamp:u}** on **{Zone.GetName(ev.ZoneID)}\n**";
                        msg += $"Attacker: **{attacker?.GetDisplayName() ?? $"<missing {ev.AttackerCharacterID}>"}**. Faction: **{Faction.GetName(ev.AttackerFactionID)}**, Team: **{Faction.GetName(ev.AttackerTeamID)}\n**";
                        msg += $"Weapon: **{attackerItem?.Name} ({ev.AttackerWeaponID})\n**";
                        msg += $"Owner: **{killed?.GetDisplayName() ?? $"<missing {ev.KilledCharacterID}>"}**. Faction **{Faction.GetName(ev.KilledFactionID)}**, Team: **{Faction.GetName(ev.KilledTeamID)}\n**";

                        if (bus != null) {
                            msg += $"Owner EXP: {owner?.GetDisplayName() ?? $"<missing {bus.OwnerID}>"}\n";
                            DateTime lastUsed = DateTimeOffset.FromUnixTimeMilliseconds(bus.LatestEventAt).UtcDateTime;

                            msg += $"First spawn at: {bus.FirstSeenAt:u} UTC\n";
                            msg += $"Spawns: {bus.SpawnCount}\n";
                            msg += $"Last used: {(int) (DateTime.UtcNow - lastUsed).TotalSeconds} seconds ago";
                        }

                        _MessageQueue.Queue(msg);
                    } catch (Exception ex) {
                        _Logger.LogError(ex, $"error in background tracked sunderer death handler");
                    }
                }).Start();
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

            //_Logger.LogTrace($"Facility control: {payload}");

            ushort defID = (ushort) (ev.ZoneID & 0xFFFF);
            ushort instanceID = (ushort) ((ev.ZoneID & 0xFFFF0000) >> 4);

            // Exclude flips that aren't interesting
            if (defID == 95 // A tutorial area
                || defID == 364 // Another tutorial area (0x16C)
                ) {

                return;
            }

            _MapRepository.Set(ev.WorldID, ev.ZoneID, ev.FacilityID, ev.NewFactionID);

            try {
                PsZone? zone = _MapRepository.GetZone(ev.WorldID, ev.ZoneID);
                if (zone != null) {
                    _ = _MapHub.Clients.Group($"RealtimeMap.{ev.WorldID}.{ev.ZoneID}").SendAsync("UpdateMap", zone);
                }
            } catch (Exception ex) {
                _Logger.LogError(ex, $"failed to send 'UpdateMap' event to signalR for worldID {ev.WorldID}, zone ID {ev.ZoneID}");
            }

            // Set the map repository before we discard server events, such as a continent unlock, to keep the map repo in sync with live
            if (World.IsTrackedWorld(ev.WorldID) == false || ev.OldFactionID == 0 || ev.NewFactionID == 0) {
                return;
            }

            //_Logger.LogDebug($"CONTROL> {ev.FacilityID} :: {ev.Players}, {ev.OldFactionID} => {ev.NewFactionID}, {ev.WorldID}:{instanceID:X}.{defID:X}, state: {ev.UnstableState}, {ev.Timestamp}");
            //_Logger.LogDebug($"CONTROL> {ev.FacilityID} {ev.OldFactionID} => {ev.NewFactionID}, {ev.WorldID}:{instanceID:X}.{defID:X}");

            new Thread(async () => {
                try {
                    PsFacility? fac = await _FacilityRepository.GetByID(ev.FacilityID);
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
                    UnstableState state = _MapRepository.GetUnstableState(ev.WorldID, ev.ZoneID);
                    timer.Stop();

                    ev.UnstableState = state;

                    long ID = await _ControlDb.Insert(ev);

                    timer.Restart();
                    await _FacilityPlayerDb.InsertMany(ID, events);
                    //_Logger.LogTrace($"CONTROL> Took {timer.ElapsedMilliseconds}ms to insert {events.Count} entries");
                    //_Logger.LogDebug($"CONTROL> {ev.FacilityID} :: with {ev.Players} players, {ev.OldFactionID} => {ev.NewFactionID}, {ev.WorldID}:{instanceID:X}.{defID:X}, state: {ev.UnstableState}/{stat2}, {ev.Timestamp}");

                    RecentFacilityControlStore.Get().Add(ev.WorldID, ev);
                } catch (Exception ex) {
                    _Logger.LogError(ex, "error in background thread of control event");
                }
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

            if (World.IsTrackedWorld(ev.WorldID) == false) {
                return;
            }

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

            if (World.IsTrackedWorld(ev.WorldID) == false) {
                return;
            }

            // Inserted into the DB after the facility control event is generated, and the ID is known
            lock (PlayerFacilityControlStore.Get().Events) {
                PlayerFacilityControlStore.Get().Events.Add(ev);
            }
        }

        private void _ProcessPlayerLogin(JToken payload) {
            //_Logger.LogTrace($"Processing login: {payload}");

            string? charID = payload.Value<string?>("character_id");
            if (charID == null || charID == "0") {
                return;
            }

            using Activity? logoutRoot = HonuActivitySource.Root.StartActivity("PlayerLogin");

            _CacheQueue.Queue(charID);

            DateTime timestamp = payload.CensusTimestamp("timestamp");
            short worldID = payload.GetWorldID();
            if (worldID == World.Jaeger) {
                _JaegerQueue.QueueSignIn(new JaegerSigninoutEntry() {
                    CharacterID = charID,
                    Timestamp = timestamp
                });
            }

            TrackedPlayer p;
            lock (CharacterStore.Get().Players) {
                // The FactionID and TeamID are updated as part of caching the character
                p = CharacterStore.Get().Players.GetOrAdd(charID, new TrackedPlayer() {
                    ID = charID,
                    WorldID = worldID,
                    ZoneID = 0,
                    FactionID = Faction.UNKNOWN,
                    TeamID = Faction.UNKNOWN,
                    Online = false
                });

                p.LastLogin = DateTime.UtcNow;
            }

            if (World.IsTrackedWorld(worldID)) {
                _SessionQueue.Queue(new CharacterSessionStartQueueEntry() {
                    CharacterID = p.ID,
                    LastEvent = timestamp
                });
            }

            p.LatestEventTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        private async Task _ProcessPlayerLogout(JToken payload) {
            string? charID = payload.Value<string?>("character_id");
            if (charID == null) {
                return;
            }

            using Activity? logoutRoot = HonuActivitySource.Root.StartActivity("PlayerLogout");
            logoutRoot?.AddTag("CharacterID", charID);

            _CacheQueue.Queue(charID);

            DateTime timestamp = payload.CensusTimestamp("timestamp");

            short worldID = payload.GetWorldID();
            if (worldID == World.Jaeger) {
                _JaegerQueue.QueueSignOut(new JaegerSigninoutEntry() {
                    CharacterID = charID,
                    Timestamp = timestamp
                });
            }

            TrackedPlayer? p;
            lock (CharacterStore.Get().Players) {
                _ = CharacterStore.Get().Players.TryGetValue(charID, out p);
            }

            if (p != null) {
                if (World.IsTrackedWorld(p.WorldID)) {
                    // Null if Honu was started when the character was online
                    if (p.LastLogin != null) {
                        // Intentionally discard, we do not care about the result of this
                        _ = _LogoutQueue.Queue(new LogoutBufferEntry() {
                            CharacterID = charID,
                            LoginTime = p.LastLogin.Value
                        });
                    } else {
                        _WeaponQueue.Queue(charID);
                    }
                    using (Activity? db = HonuActivitySource.Root.StartActivity("db")) {
                        await _SessionRepository.End(p.ID, timestamp);
                    }
                }

                // Reset team of the NSO player as they're now offline
                if (p.FactionID == Faction.NS) {
                    p.TeamID = Faction.NS;
                }
            }
        }

        private async Task _ProcessMetagameEvent(JToken payload) {
            short worldID = payload.GetWorldID();
            uint zoneID = payload.GetZoneID();
            int instanceID = payload.GetInt32("instance_id", 0);
            string metagameEventName = payload.GetString("metagame_event_state_name", "missing");
            int metagameEventID = payload.GetInt32("metagame_event_id", 0);
            DateTime timestamp = payload.CensusTimestamp("timestamp");

            _Logger.LogDebug($"metagame event payload: {payload}");

            if (MetagameEvent.IsAerialAnomaly(metagameEventID) == false) {
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
                        state.AlertStart = timestamp;

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

                            string s = $"ALERT ended in {worldID}, current owners:\n";

                            foreach (uint zoneID in Zone.All) {
                                short? owner = _MapRepository.GetZoneMapOwner(worldID, zoneID);

                                s += $"{zoneID} => {owner}\n";

                                if (owner == null) {
                                    ZoneStateStore.Get().UnlockZone(worldID, zoneID);
                                }
                            }

                            _Logger.LogDebug(s);
                        }).Start();
                    }

                    ZoneStateStore.Get().SetZone(worldID, zoneID, state);
                }
            }

            _Logger.LogInformation($"METAGAME in world {worldID} zone {zoneID} metagame: {metagameEventName}/{metagameEventID}");

            if (metagameEventName == "started") {
                TimeSpan? duration = MetagameEvent.GetDuration(metagameEventID);
                PsZone? zone = _MapRepository.GetZone(worldID, zoneID);

                if (duration == null) {
                    _Logger.LogWarning($"Failed to find a duration for MetagameEvent {metagameEventID}");
                }

                PsAlert alert = new PsAlert();
                alert.Timestamp = timestamp;
                alert.ZoneID = zoneID;
                alert.WorldID = worldID;
                alert.AlertID = metagameEventID;
                alert.InstanceID = payload.GetInt32("instance_id", 0);
                alert.Duration = ((int?)duration?.TotalSeconds) ?? (60 * 90); // default to 90 minute alerts if unknown
                alert.ZoneFacilityCount = zone?.Facilities.Count ?? 1;

                if (zone != null) {
                    List<PsFacility> facs = (await _FacilityRepository.GetAll())
                        .Where(iter => iter.ZoneID == alert.ZoneID)
                        .Where(iter => iter.TypeID == 7) // 7 = warpgate
                        .ToList();

                    _Logger.LogDebug($"Found {facs.Count} warpgates in {alert.ZoneID}, finding owners");

                    foreach (PsFacility fac in facs) {
                        PsFacilityOwner? owner = zone.GetFacilityOwner(fac.FacilityID);
                        if (owner != null) {
                            if (owner.Owner == Faction.VS) {
                                alert.WarpgateVS = owner.FacilityID;
                            } else if (owner.Owner == Faction.NC) {
                                alert.WarpgateNC = owner.FacilityID;
                            } else if (owner.Owner == Faction.TR) {
                                alert.WarpgateTR = owner.FacilityID;
                            } else {
                                _Logger.LogWarning($"In alert end, world {alert.WorldID}, zone {alert.ZoneID}: facility {fac.FacilityID} was unowned by 1|2|3, current owner: {owner.Owner}");
                            }
                        } else {
                            _Logger.LogWarning($"In alert end, world {alert.WorldID}, zone {alert.ZoneID}: failed to get owner of {fac.FacilityID}, zone missing facility");
                        }
                    }
                }

                AlertStore.Get().AddAlert(alert);

                if (World.IsTrackedWorld(alert.WorldID)) {
                    try {
                        alert.ID = await _AlertDb.Insert(alert);
                    } catch (Exception ex) {
                        _Logger.LogError(ex, $"Failed to insert alert in {worldID} in zone {zoneID}");
                    }
                }
            } else if (metagameEventName == "ended") {
                List<PsAlert> alerts = AlertStore.Get().GetAlerts();

                PsAlert? toRemove = null;
                foreach (PsAlert alert in alerts) {
                    if (alert.ZoneID == zoneID && alert.WorldID == worldID) {
                        _Logger.LogInformation($"Metagame event {alert.ID}/{alert.Name} finished on world {worldID} in zone {zoneID}");
                        toRemove = alert;
                        break;
                    }
                    if (alert.InstanceID == instanceID && alert.WorldID == worldID) {
                        _Logger.LogInformation($"Metagame event {alert.ID}/{alert.Name} finished on world {worldID} in zone {zoneID}");
                        toRemove = alert;
                        break;
                    }
                }

                if (toRemove != null && World.IsTrackedWorld(toRemove.WorldID) == false) {
                    return;
                }

                if (toRemove != null) {
                    AlertStore.Get().RemoveByID(toRemove.ID);

                    decimal countVS = payload.GetDecimal("faction_vs", 0m);
                    decimal countNC = payload.GetDecimal("faction_nc", 0m);
                    decimal countTR = payload.GetDecimal("faction_tr", 0m);

                    toRemove.CountVS = (int)countVS;
                    toRemove.CountNC = (int)countNC;
                    toRemove.CountTR = (int)countTR;

                    // Update the winner faction ID
                    decimal winnerCount = 0;
                    short factionID = 0;

                    if (countVS > winnerCount) {
                        winnerCount = countVS;
                        factionID = Faction.VS;
                    }
                    if (countNC > winnerCount) {
                        winnerCount = countNC;
                        factionID = Faction.NC;
                    } 
                    if (countTR > winnerCount) {
                        winnerCount = countTR;
                        factionID = Faction.TR;
                    }

                    if ((countVS == countNC && (factionID == Faction.VS || factionID == Faction.NC)) // VS and NC tied
                        || (countVS == countTR && (factionID == Faction.VS || factionID == Faction.TR)) // VS and TR tied
                        || (countNC == countTR && (factionID == Faction.NC || factionID == Faction.TR)) // NC and TR tied
                        ) {

                        factionID = 0;
                    }

                    toRemove.VictorFactionID = factionID;

                    // Aerial anomalies can end early, update the duration if needed
                    if (MetagameEvent.IsAerialAnomaly(metagameEventID) == true) {
                        TimeSpan duration = timestamp - toRemove.Timestamp;
                        _Logger.LogDebug($"Aerial anomaly {toRemove.WorldID}-{toRemove.InstanceID} lasted {(int)duration.TotalSeconds} seconds");

                        toRemove.Duration = (int)duration.TotalSeconds;
                        await _AlertDb.UpdateByID(toRemove.ID, toRemove);
                    }

                    // Get the count of each faction if it's not an aerial anomaly, a lockdown alert
                    if (MetagameEvent.IsAerialAnomaly(metagameEventID) == false) {
                        PsZone? zone = _MapRepository.GetZone(worldID, zoneID);
                        if (zone != null) {
                            int factionVS = zone.GetFacilities().Where(iter => iter.Owner == Faction.VS).Count();
                            int factionNC = zone.GetFacilities().Where(iter => iter.Owner == Faction.NC).Count();
                            int factionTR = zone.GetFacilities().Where(iter => iter.Owner == Faction.TR).Count();

                            decimal scoreVS = decimal.Round(toRemove.ZoneFacilityCount * countVS / 100);
                            decimal scoreNC = decimal.Round(toRemove.ZoneFacilityCount * countNC / 100);
                            decimal scoreTR = decimal.Round(toRemove.ZoneFacilityCount * countTR / 100);

                            /*
                            _Logger.LogDebug($"VS own {factionVS}, have {toRemove.ZoneFacilityCount * countVS / 100}/{scoreVS}");
                            _Logger.LogDebug($"NC own {factionNC}, have {toRemove.ZoneFacilityCount * countNC / 100}/{scoreNC}");
                            _Logger.LogDebug($"TR own {factionTR}, have {toRemove.ZoneFacilityCount * countTR / 100}/{scoreTR}");
                            */

                            toRemove.CountVS = (int)scoreVS;
                            toRemove.CountNC = (int)scoreNC;
                            toRemove.CountTR = (int)scoreTR;
                            await _AlertDb.UpdateByID(toRemove.ID, toRemove);
                        } else {
                            _Logger.LogWarning($"Cannot assign score for alert {toRemove.WorldID}-{toRemove.InstanceID} (in zone {toRemove.ZoneID}), missing zone");
                        }
                    }

                    new Thread(async () => {
                        try {
                            _Logger.LogInformation($"Alert {toRemove.ID}/{toRemove.WorldID}-{toRemove.InstanceID} ended, creating participation data...");
                            List<AlertPlayerDataEntry> parts = await _ParticipantDataRepository.GetByAlert(toRemove, CancellationToken.None);

                            toRemove.Participants = parts.Count;
                            await _AlertDb.UpdateByID(toRemove.ID, toRemove);
                            _Logger.LogInformation($"Alert {toRemove.ID}/{toRemove.WorldID}-{toRemove.InstanceID} ended, {parts.Count} participant data created");
                        } catch (Exception ex) {
                            _Logger.LogError(ex, $"failed to create alert data for {toRemove.ID}/{toRemove.WorldID}-{toRemove.InstanceID}");
                        }
                    }).Start();
                } else {
                    _Logger.LogWarning($"Failed to find alert to finish for world {worldID} in zone {zoneID}\nCurrent alerts: {string.Join(", ", alerts.Select(iter => $"{iter.WorldID}.{iter.ZoneID}"))}");
                }
            } else {
                _Logger.LogError($"Unchecked value of {nameof(metagameEventName)} '{metagameEventName}'");
            }
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
            using Activity? traceDeath = HonuActivitySource.Root.StartActivity("Death");

            string attackerID = payload.Value<string?>("attacker_character_id") ?? "0";
            string charID = payload.Value<string?>("character_id") ?? "0";

            if (attackerID == "0" && charID == "0") {
                _Logger.LogTrace($"why does this exist? {payload}");
                return;
            }

            int timestamp = payload.Value<int?>("timestamp") ?? 0;
            uint zoneID = payload.GetZoneID();
            short attackerLoadoutID = payload.Value<short?>("attacker_loadout_id") ?? -1;
            short loadoutID = payload.Value<short?>("character_loadout_id") ?? -1;

            short attackerFactionID = Loadout.GetFaction(attackerLoadoutID);
            short factionID = Loadout.GetFaction(loadoutID);

            KillEvent ev = new KillEvent() {
                AttackerCharacterID = attackerID,
                AttackerLoadoutID = attackerLoadoutID,
                AttackerTeamID = attackerFactionID,
                KilledCharacterID = charID,
                KilledLoadoutID = loadoutID,
                KilledTeamID = factionID,
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime,
                WeaponID = payload.GetInt32("attacker_weapon_id", 0),
                WorldID = payload.GetWorldID(),
                ZoneID = payload.GetZoneID(),
                AttackerFireModeID = payload.GetInt32("attacker_fire_mode_id", 0),
                AttackerVehicleID = payload.GetInt32("attacker_vehicle_id", 0),
                IsHeadshot = (payload.Value<string?>("is_headshot") ?? "0") != "0"
            };

            traceDeath?.AddTag("World", ev.WorldID);
            traceDeath?.AddTag("Zone", ev.ZoneID);

            if (World.IsTrackedWorld(ev.WorldID)) {
                _CacheQueue.Queue(charID);
                _CacheQueue.Queue(attackerID);
            }

            //_Logger.LogTrace($"Processing death: {payload}");

            using Activity? processDeath = HonuActivitySource.Root.StartActivity("process CharacterStore");
            lock (CharacterStore.Get().Players) {
                // The default value for Online must be false, else when a new TrackedPlayer is constructed,
                //      the session will never start, as the handler already sees the character online,
                //      so no need to start a new session
                TrackedPlayer attacker = CharacterStore.Get().Players.GetOrAdd(ev.AttackerCharacterID, new TrackedPlayer() {
                    ID = ev.AttackerCharacterID,
                    FactionID = attackerFactionID,
                    TeamID = (attackerFactionID == 4) ? Faction.NS : attackerFactionID,
                    Online = false,
                    WorldID = ev.WorldID
                });

                if (attacker.ID != "0" && attacker.Online == false) {
                    _SessionQueue.Queue(new CharacterSessionStartQueueEntry() {
                        CharacterID = attacker.ID,
                        LastEvent = ev.Timestamp
                    });
                }

                _CacheQueue.Queue(attacker.ID);

                attacker.ZoneID = zoneID;

                if (attacker.FactionID == Faction.UNKNOWN) {
                    attacker.FactionID = attackerFactionID; // If a tracked player was made from a login, no faction ID is given
                    attacker.TeamID = ev.AttackerTeamID;
                }

                if (attacker.FactionID == Faction.NS) {
                    ev.AttackerTeamID = attacker.TeamID;
                }

                // See above for why false is used for the Online value, instead of true
                TrackedPlayer killed = CharacterStore.Get().Players.GetOrAdd(ev.KilledCharacterID, new TrackedPlayer() {
                    ID = ev.KilledCharacterID,
                    FactionID = factionID,
                    TeamID = (factionID == 4) ? Faction.NS : factionID,
                    Online = false,
                    WorldID = ev.WorldID
                });

                _CacheQueue.Queue(killed.ID);

                // Ensure that 2 sessions aren't started if the attacker and killed are the same
                if (killed.ID != "0" && killed.Online == false && attacker.ID != killed.ID) {
                    _SessionQueue.Queue(new CharacterSessionStartQueueEntry() {
                        CharacterID = killed.ID,
                        LastEvent = ev.Timestamp
                    });
                }

                killed.ZoneID = zoneID;

                if (killed.FactionID == Faction.UNKNOWN) {
                    killed.FactionID = factionID;
                    killed.TeamID = ev.KilledTeamID;
                }

                if (killed.FactionID == Faction.NS) {
                    ev.KilledTeamID = killed.TeamID;
                }

                long nowSeconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                attacker.LatestEventTimestamp = nowSeconds;

                killed.LatestEventTimestamp = nowSeconds;
                killed.LatestDeath = ev;
            }
            processDeath?.Stop();

            if (World.IsTrackedWorld(ev.WorldID)) {
                using Activity? insertDeath = HonuActivitySource.Root.StartActivity("insert");
                ev.ID = await _KillEventDb.Insert(ev);
                insertDeath?.Stop();
            }

            //await _TagManager.OnKillHandler(ev);
        }

        private async Task _ProcessExperience(JToken payload) {
            //_Logger.LogInformation($"Processing exp: {payload}");

            string? charID = payload.Value<string?>("character_id");
            if (charID == null || charID == "0") {
                return;
            }

            using Activity? rootExp = HonuActivitySource.Root.StartActivity("GainExperience");

            Stopwatch timer = Stopwatch.StartNew();

            _CacheQueue.Queue(charID);
            long queueMs = timer.ElapsedMilliseconds; timer.Restart();

            int expId = payload.GetInt32("experience_id", -1);
            short loadoutId = payload.GetInt16("loadout_id", -1);
            short worldID = payload.GetWorldID();
            int timestamp = payload.Value<int?>("timestamp") ?? 0;
            uint zoneID = payload.GetZoneID();
            string otherID = payload.GetString("other_id", "0");

            short factionID = Loadout.GetFaction(loadoutId);

            long readValuesMs = timer.ElapsedMilliseconds; timer.Restart();

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

            long createEventMs = timer.ElapsedMilliseconds; timer.Restart();

            TrackedPlayer? otherPlayer = null;

            using Activity? processExp = HonuActivitySource.Root.StartActivity("process CharacterStore");
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
                    _SessionQueue.Queue(new CharacterSessionStartQueueEntry() {
                        CharacterID = p.ID,
                        LastEvent = ev.Timestamp
                    });
                }

                p.LatestEventTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                p.ZoneID = zoneID;

                if (p.FactionID == Faction.UNKNOWN) {
                    p.FactionID = factionID;
                    p.TeamID = factionID;
                }

                // If the event could only happen if two characters are on the same faction, update the team_id field
                if (Experience.IsRevive(expId) || Experience.IsHeal(expId) || Experience.IsResupply(expId)) {
                    // If either character was not NSO, update the team_id of the character
                    // If both are NSO, this field is not updated, as one bad team_id could then spread to other NSOs, messing up tracking
                    if (CharacterStore.Get().Players.TryGetValue(otherID, out otherPlayer)) {
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

                // If the character is NS, and we know what team they're currently on (not -1 or 0), update the event
                if (p.FactionID == Faction.NS && p.TeamID != Faction.UNKNOWN && p.TeamID != 0) {
                    ev.TeamID = p.TeamID;
                }
            }
            processExp?.Stop();

            long processCharMs = timer.ElapsedMilliseconds; timer.Restart();

            long ID = 0;
            if (World.IsTrackedWorld(ev.WorldID)) {
                ID = await _ExpEventDb.Insert(ev);
            } else {
                _Logger.LogTrace($"not inserting exp event for world {ev.WorldID}");
            }
            long dbInsertMs = timer.ElapsedMilliseconds; timer.Restart();

            // If this event was a revive, get the latest death of the character who died and set the revived id
            if ((ev.ExperienceID == Experience.REVIVE || ev.ExperienceID == Experience.SQUAD_REVIVE)
                && otherPlayer != null && otherPlayer.LatestDeath != null) {

                TimeSpan diff = ev.Timestamp - otherPlayer.LatestDeath.Timestamp;

                if (diff > TimeSpan.FromSeconds(50)) {
                    //_Logger.LogTrace($"death {otherPlayer.LatestDeath.ID} is too old {diff}");
                    otherPlayer.LatestDeath = null;
                } else {
                    //_Logger.LogTrace($"using death {otherPlayer.LatestDeath.ID} at {otherPlayer.LatestDeath.Timestamp:u}, exp ID {ID}, occured {diff} ago");
                    if (World.IsTrackedWorld(ev.WorldID)) {
                        await _KillEventDb.SetRevived(otherPlayer.LatestDeath.ID, ID);
                    }
                }
            } else if ((ev.ExperienceID == Experience.REVIVE || ev.ExperienceID == Experience.SQUAD_REVIVE)
                && (otherPlayer == null || otherPlayer.LatestDeath == null)) {

                //_Logger.LogTrace($"no death for exp {ID}, missing other? {otherPlayer == null}, missing death? {otherPlayer?.LatestDeath == null}");
            }

            long reviveMs = timer.ElapsedMilliseconds; timer.Restart();

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
                RecentSundererDestroyExpStore.Get().Add(ev);
            }

            long total = queueMs + readValuesMs + createEventMs + processCharMs + dbInsertMs + reviveMs;

            if (total > 100 && Logging.EventProcess == true) {
                _Logger.LogDebug($"Total: {total}\nQueue: {queueMs}, Read: {readValuesMs}, create: {createEventMs}, process: {processCharMs}, DB {dbInsertMs}, revive {reviveMs}");
            }

        }

        private async Task _ProcessItemAdded(JToken payload) {
            ItemAddedEvent ev = new ItemAddedEvent();
            ev.CharacterID = payload.GetRequiredString("character_id");
            ev.Context = payload.GetString("context", "");
            ev.ItemCount = payload.GetInt32("item_count", 0);
            ev.ItemID = payload.GetRequiredInt32("item_id");
            ev.Timestamp = payload.CensusTimestamp("timestamp");
            ev.WorldID = payload.GetWorldID();
            ev.ZoneID = payload.GetZoneID();

            await _ItemAddedDb.Insert(ev);

            /*
                {
                    "character_id": "5429269171559531473",
                    "context": "SkillGrantItemLine",
                    "event_name": "ItemAdded",
                    "item_count": "1",
                    "item_id": "6013812",
                    "timestamp": "1659246325",
                    "world_id": "1",
                    "zone_id": "131434"
                }
             */
        }

        private async Task _ProcessAchievementEarned(JToken payload) {
            AchievementEarnedEvent ev = new AchievementEarnedEvent();

            ev.CharacterID = payload.GetRequiredString("character_id");
            ev.Timestamp = payload.CensusTimestamp("timestamp");
            ev.AchievementID = payload.GetRequiredInt32("achievement_id");
            ev.ZoneID = payload.GetZoneID();
            ev.WorldID = payload.GetWorldID();

            await _AchievementEarnedDb.Insert(ev);

            /*
                {
                    "achievement_id": "90020",
                    "character_id": "5429292002801114593",
                    "event_name": "AchievementEarned",
                    "timestamp": "1659246271",
                    "world_id": "1",
                    "zone_id": "4"
                }
            */
        }

    }
}
