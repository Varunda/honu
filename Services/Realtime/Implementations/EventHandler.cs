using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code;
using watchtower.Code.ExtensionMethods;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Models.Census;
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
        private readonly ICharacterRepository _CharacterRepository;

        private readonly List<JToken> _Recent;

        public EventHandler(ILogger<EventHandler> logger,
            IKillEventDbStore killEventDb, IExpEventDbStore expDb,
            IBackgroundCharacterCacheQueue cacheQueue, ICharacterRepository charRepo) {

            _Logger = logger;

            _Recent = new List<JToken>();

            _KillEventDb = killEventDb ?? throw new ArgumentNullException(nameof(killEventDb));
            _ExpEventDb = expDb ?? throw new ArgumentNullException(nameof(expDb));
            _CacheQueue = cacheQueue ?? throw new ArgumentNullException(nameof(cacheQueue));
            _CharacterRepository = charRepo ?? throw new ArgumentNullException(nameof(charRepo));
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

                lock (CharacterStore.Get().Players) {
                    // The FactionID and TeamID are updated as part of caching the character
                    TrackedPlayer p = CharacterStore.Get().Players.GetOrAdd(charID, new TrackedPlayer() {
                        ID = charID,
                        WorldID = payload.GetWorldID(),
                        ZoneID = "-1",
                        FactionID = Faction.UNKNOWN,
                        TeamID = Faction.UNKNOWN
                    });

                    p.Online = true;
                    p.LatestEventTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
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

                            if (p.FactionID == Faction.NS) {
                                p.TeamID = Faction.NS;
                            }
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
                TrackedPlayer attacker = CharacterStore.Get().Players.GetOrAdd(attackerID, new TrackedPlayer() {
                    ID = attackerID,
                    FactionID = attackerFactionID,
                    TeamID = ev.AttackerTeamID,
                    Online = true,
                    WorldID = ev.WorldID
                });

                attacker.Online = true;
                attacker.ZoneID = zoneID;

                if (attacker.FactionID == Faction.UNKNOWN) {
                    attacker.FactionID = attackerFactionID; // If a tracked player was made from a login, no faction ID is given
                    attacker.TeamID = ev.AttackerTeamID;
                }

                ev.AttackerTeamID = attacker.TeamID;

                TrackedPlayer killed = CharacterStore.Get().Players.GetOrAdd(charID, new TrackedPlayer() {
                    ID = charID,
                    FactionID = factionID,
                    TeamID = ev.KilledTeamID,
                    Online = true,
                    WorldID = ev.WorldID
                });

                killed.Online = true;
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
            int timestamp = payload.Value<int?>("timestamp") ?? 0;
            int zoneID = payload.GetZoneID();
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
                WorldID = payload.Value<short?>("world_id") ?? -1,
                ZoneID = zoneID
            };

            lock (CharacterStore.Get().Players) {
                TrackedPlayer p = CharacterStore.Get().Players.GetOrAdd(charID, new TrackedPlayer() {
                    ID = charID,
                    FactionID = factionID,
                    TeamID = factionID,
                    Online = true,
                    WorldID = payload.GetWorldID()
                });

                p.Online = true;
                p.LatestEventTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                p.ZoneID = zoneID.ToString();

                if (p.FactionID == Faction.UNKNOWN) {
                    p.FactionID = factionID;
                    p.TeamID = factionID;
                }

                // Update the team_id field if needed
                if (Experience.IsRevive(expId) || Experience.IsHeal(expId) || Experience.IsResupply(expId)) {
                    // If either character was not NSO, update the team_id of the character
                    // If both are NSO, this field is not updated, as one bad team_id could then spread to other NSOs, messing up tracking
                    if (CharacterStore.Get().Players.TryGetValue(otherID, out TrackedPlayer? otherPlayer)) {
                        if (p.FactionID == Faction.NS && otherPlayer.FactionID != Faction.NS && otherPlayer.FactionID != Faction.UNKNOWN && p.TeamID != otherPlayer.FactionID) {
                            _Logger.LogDebug($"Robot {p.ID} supported (exp {expId}, loadout {loadoutId}, faction {factionID}) non-robot {otherPlayer.ID}, setting robot team ID to {otherPlayer.FactionID} from {p.TeamID}");
                            p.TeamID = otherPlayer.FactionID;
                        }

                        if (p.FactionID != Faction.NS && p.FactionID != Faction.UNKNOWN && otherPlayer.FactionID == Faction.NS && otherPlayer.TeamID != p.FactionID) {
                            _Logger.LogDebug($"Non-robot {p.ID} supported (exp {expId}, loadout {loadoutId}, faction {factionID}) robot {otherPlayer.ID}, setting robot team ID to {p.FactionID}, from {otherPlayer.TeamID}");
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
                        Type = "Sundy"
                    });

                    ++npc.SpawnCount;
                    npc.LatestEventAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                };
            }

        }

    }
}
