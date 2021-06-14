using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Census;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Models.Events;

namespace watchtower.Realtime {

    public class EventHandler : IEventHandler {

        private readonly ILogger<EventHandler> _Logger;

        private readonly ICharacterCollection _Characters;

        private readonly List<JToken> _Recent;

        public EventHandler(ILogger<EventHandler> logger,
            ICharacterCollection charCollection) {

            _Logger = logger;

            _Recent = new List<JToken>();

            _Characters = charCollection ?? throw new ArgumentNullException(nameof(charCollection));
        }

        public void Process(JToken ev) {
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
                    _ProcessExperience(payloadToken);
                } else if (eventName == "Death") {
                    _ProcessDeath(payloadToken);
                } else {
                    _Logger.LogWarning($"Untracked event_name: '{eventName}'");
                }
            }
        }

        private void _ProcessPlayerLogin(JToken payload) {
            string? charID = payload.Value<string?>("character_id");
            if (charID != null) {
                _Characters.Cache(charID);

                lock (CharacterStore.Get().Players) {
                    if (CharacterStore.Get().Players.TryGetValue(charID, out TrackedPlayer? p) == true) {
                        if (p != null) {
                            p.Online = true;
                        }
                    }
                }
            }
        }

        private void _ProcessPlayerLogout(JToken payload) {
            string? charID = payload.Value<string?>("character_id");
            if (charID != null) {
                _Characters.Cache(charID);

                lock (CharacterStore.Get().Players) {
                    if (CharacterStore.Get().Players.TryGetValue(charID, out TrackedPlayer? p) == true) {
                        if (p != null) {
                            p.Online = false;
                        }
                    }
                }
            }
        }

        private void _ProcessDeath(JToken payload) {
            int timestamp = payload.Value<int?>("timestamp") ?? 0;

            string attackerID = payload.Value<string?>("attacker_character_id") ?? "0";
            string attackerLoadoutID = payload.Value<string?>("attacker_loadout_id") ?? "-1";
            string charID = payload.Value<string?>("character_id") ?? "0";
            string loadoutID = payload.Value<string?>("character_loadout_id") ?? "-1";

            string attackerFactionID = Loadout.GetFaction(attackerLoadoutID);
            string factionID = Loadout.GetFaction(loadoutID);

            _Characters.Cache(attackerID);
            _Characters.Cache(charID);

            if (attackerFactionID == factionID) {
                return;
            }

            lock (CharacterStore.Get().Players) {
                TrackedPlayer attacker = CharacterStore.Get().Players.GetOrAdd(attackerID, new TrackedPlayer() {
                    ID = attackerID,
                    FactionID = attackerFactionID,
                    Online = true
                });

                TrackedPlayer killed = CharacterStore.Get().Players.GetOrAdd(charID, new TrackedPlayer() {
                    ID = charID,
                    FactionID = factionID,
                    Online = true
                });

                attacker.Kills.Add(timestamp);
                killed.Deaths.Add(timestamp);
            }

        }

        private void _ProcessExperience(JToken payload) {
            //_Logger.LogInformation($"Processing exp: {payload}");

            string? charID = payload.Value<string?>("character_id");
            if (charID == null) {
                return;
            }
            _Characters.Cache(charID);

            string expId = payload.Value<string?>("experience_id") ?? "-1";
            string loadoutId = payload.Value<string?>("loadout_id") ?? "-1";
            int timestamp = payload.Value<int?>("timestamp") ?? 0;

            string factionID = Loadout.GetFaction(loadoutId);

            lock (CharacterStore.Get().Players) {
                TrackedPlayer p = CharacterStore.Get().Players.GetOrAdd(charID, new TrackedPlayer() {
                    ID = charID,
                    FactionID = factionID
                });

                if (expId == Experience.HEAL || expId == Experience.SQUAD_HEAL) {
                    p.Heals.Add(timestamp);
                } else if (expId == Experience.REVIVE || expId == Experience.SQUAD_REVIVE) {
                    p.Revives.Add(timestamp);

                    // Remove the most recent death
                    string? targetID = payload.Value<string?>("other_id");
                    if (targetID != null && targetID != "0") {
                        if (CharacterStore.Get().Players.TryGetValue(targetID, out TrackedPlayer? player) == true) {
                            if (player != null) {
                                if (player.Deaths.Count > 0) {
                                    player.Deaths.RemoveAt(player.Deaths.Count - 1);
                                }
                            }
                        }
                    }
                } else if (expId == Experience.RESUPPLY || expId == Experience.SQUAD_RESUPPLY) {
                    p.Resupplies.Add(timestamp);
                } else if (expId == Experience.MAX_REPAIR || expId == Experience.SQUAD_MAX_REPAIR) {
                    p.Resupplies.Add(timestamp);
                } else if (Experience.IsSpawn(expId)) {
                    p.Spawns.Add(timestamp);
                } else if (Experience.IsAssist(expId)) {
                    p.Assists.Add(timestamp);
                }
            }
        }

    }
}
