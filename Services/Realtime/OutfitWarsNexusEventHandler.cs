using Microsoft.Extensions.Logging;
using watchtower.Constants;
using watchtower.Models.Events;
using watchtower.Models.OutfitWarsNexus;
using watchtower.Services.Repositories;

namespace watchtower.Services.Realtime {

    public class OutfitWarsNexusEventHandler {

        private readonly ILogger<OutfitWarsNexusEventHandler> _Logger;

        private readonly OutfitWarsMatchRepository _MatchRepository;

        public OutfitWarsNexusEventHandler(ILogger<OutfitWarsNexusEventHandler> logger,
            OutfitWarsMatchRepository matchRepository) {

            _Logger = logger;
            _MatchRepository = matchRepository;
        }

        public void HandleKill(KillEvent ev) {
            OutfitWarsMatch? match = _MatchRepository.Get(ev.WorldID, ev.ZoneID);
            if (match == null) {
                return;
            }

            OutfitWarsTeam? attackerTeam = GetTeam(match, ev.AttackerCharacterID);
            OutfitWarsTeam? killedTeam = GetTeam(match, ev.KilledCharacterID);

            if (attackerTeam == null || killedTeam == null) {
                return;
            }

            if (attackerTeam.OutfitID == killedTeam.OutfitID) {
                return;
            }

            attackerTeam.Kills.Add(ev);
            killedTeam.Deaths.Add(ev);
        }

        public void HandleExp(ExpEvent ev) {
            OutfitWarsMatch? match = _MatchRepository.Get(ev.WorldID, ev.ZoneID);
            if (match == null) {
                return;
            }

            OutfitWarsTeam? sourceTeam = GetTeam(match, ev.SourceID);

            if (sourceTeam == null) {
                return;
            }

            int expID = ev.ExperienceID;

            if (Experience.IsAssist(expID) || Experience.IsHeal(expID)
                    || Experience.IsMaxRepair(expID) || Experience.IsResupply(expID)
                    || Experience.IsRevive(expID) || Experience.IsShieldRepair(expID)
                    || Experience.IsSpawn(expID) || Experience.IsVehicleRepair(expID)
                    || Experience.IsVehicleResupply(expID)) {

                sourceTeam.Experience.Add(ev);
            }
        }

        public void HandleFacilityControl(FacilityControlEvent ev) {
            OutfitWarsMatch? match = _MatchRepository.Get(ev.WorldID, ev.ZoneID);
            if (match == null) {
                return;
            }

        }

        public void HandleVehicleDestroy(VehicleDestroyEvent ev) {
            OutfitWarsMatch? match = _MatchRepository.Get(ev.WorldID, ev.ZoneID);
            if (match == null) {
                return;
            }

        }

        private OutfitWarsTeam? GetTeam(OutfitWarsMatch match, string charID) {
            if (match.TeamAlpha?.Members.Contains(charID) == true) {
                return match.TeamAlpha;
            }

            if (match.TeamOmega?.Members.Contains(charID) == true) {
                return match.TeamOmega;
            }

            return null;
        }

    }
}
