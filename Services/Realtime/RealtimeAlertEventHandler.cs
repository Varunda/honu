using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Models.Events;
using watchtower.Models.RealtimeAlert;
using watchtower.Services.Repositories;

namespace watchtower.Services.Realtime {

    public class RealtimeAlertEventHandler {

        private readonly ILogger<RealtimeAlertEventHandler> _Logger;

        private readonly RealtimeAlertRepository _MatchRepository;

        public RealtimeAlertEventHandler(ILogger<RealtimeAlertEventHandler> logger,
            RealtimeAlertRepository matchRepository) {

            _Logger = logger;
            _MatchRepository = matchRepository;
        }

        public void HandleKill(KillEvent ev) {
            if (ev.AttackerTeamID == ev.KilledTeamID) {
                return;
            }

            RealtimeAlert? match = _MatchRepository.Get(ev.WorldID, ev.ZoneID);
            if (match == null) {
                return;
            }

            RealtimeAlertTeam? attackerTeam = GetTeamByID(match, ev.AttackerTeamID);
            if (attackerTeam != null) {
                attackerTeam.KillDeathEvents.Add(ev);
                ++attackerTeam.Kills;
            }

            RealtimeAlertTeam? killedTeam = GetTeamByID(match, ev.KilledTeamID);
            if (killedTeam != null) {
                killedTeam.KillDeathEvents.Add(ev);
                ++killedTeam.Deaths;
            }
        }

        public void HandleExp(ExpEvent ev) {
            RealtimeAlert? match = _MatchRepository.Get(ev.WorldID, ev.ZoneID);
            if (match == null) {
                return;
            }

            RealtimeAlertTeam? sourceTeam = GetTeamByID(match, ev.TeamID);

            if (sourceTeam == null) {
                return;
            }

            int expID = ev.ExperienceID;

            if (sourceTeam.Experience.ContainsKey(expID) == false) {
                sourceTeam.Experience.Add(expID, 0);
            }

            sourceTeam.Experience[expID] = ++sourceTeam.Experience[expID];
            sourceTeam.ExpEvents.Add(ev);

            if (Experience.IsRevive(expID)) {
                --sourceTeam.Deaths;
            }
        }

        public void HandleFacilityControl(FacilityControlEvent ev) {
            RealtimeAlert? match = _MatchRepository.Get(ev.WorldID, ev.ZoneID);
            if (match == null) {
                return;
            }

            //_Logger.LogDebug($"{JToken.FromObject(ev)} {ev.WorldID}.{ev.ZoneID}");

            match.Zone.SetFacilityOwner(ev.FacilityID, ev.NewFactionID);
            match.Facilities = match.Zone.GetFacilities();
            //_Logger.LogDebug($"{JToken.FromObject(match.Zone.GetFacilities())}");
        }

        public void HandleVehicleDestroy(VehicleDestroyEvent ev) {
            if (ev.AttackerCharacterID == ev.KilledCharacterID || ev.AttackerCharacterID == "0") {
                return;
            }

            RealtimeAlert? match = _MatchRepository.Get(ev.WorldID, ev.ZoneID);
            if (match == null) {
                return;
            }

            RealtimeAlertTeam? attackerTeam = GetTeamByID(match, ev.AttackerTeamID);
            if (attackerTeam != null) {
                attackerTeam.VehicleDestroyEvents.Add(ev);
                if (ev.AttackerTeamID != ev.KilledTeamID) {
                    ++attackerTeam.VehicleKills;
                }
            }

            RealtimeAlertTeam? killedTeam = GetTeamByID(match, ev.KilledTeamID);
            if (killedTeam != null) {
                killedTeam.VehicleDestroyEvents.Add(ev);
                if (ev.AttackerTeamID != ev.KilledTeamID) {
                    ++killedTeam.VehicleDeaths;
                }
            }
        }

        private RealtimeAlertTeam? GetTeamByID(RealtimeAlert match, short teamID) {
            if (teamID == Faction.VS) {
                return match.VS;
            } else if (teamID == Faction.NC) {
                return match.NC;
            } else if (teamID == Faction.TR) {
                return match.TR;
            }

            return null;
        }

        private RealtimeAlertTeam? GetTeam(RealtimeAlert match, string charID) {
            TrackedPlayer? p = CharacterStore.Get().GetByCharacterID(charID);
            if (p == null) {
                return null;
            }

            if (p.TeamID == 1) {
                return match.VS;
            } else if (p.TeamID == 2) {
                return match.NC;
            } else if (p.TeamID == 3) {
                return match.TR;
            }

            return null;
        }

    }
}
