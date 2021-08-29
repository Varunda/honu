using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Commands;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Services.Census;

namespace watchtower.Code.Commands {

    [Command]
    public class ZoneCommand {

        private readonly ILogger<ZoneCommand> _Logger;
        private readonly IMapCollection _MapCollection;

        public ZoneCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<ZoneCommand>>();
            _MapCollection = services.GetRequiredService<IMapCollection>();
        }

        public void Print() {
            string s = $"Zone metadata currently stored:";

            lock (ZoneStateStore.Get().Zones) {
                foreach (KeyValuePair<string, ZoneState> zone in ZoneStateStore.Get().Zones) {
                    s += $"\n\t{zone.Key} => {JToken.FromObject(zone.Value)}";
                }
            }

            _Logger.LogInformation(s);
        }

        public void StartAlert(short worldID, uint zoneID, int duration) {
            lock (ZoneStateStore.Get().Zones) {
                ZoneState? zone = ZoneStateStore.Get().GetZone(worldID, zoneID);

                if (zone == null) {
                    zone = new ZoneState() {
                        ZoneID = zoneID,
                        WorldID = worldID,
                        IsOpened = true
                    };
                }

                zone.AlertStart = DateTime.UtcNow;
                zone.AlertEnd = zone.AlertStart + TimeSpan.FromMinutes(duration);

                ZoneStateStore.Get().SetZone(worldID, zoneID, zone);
            }
        }

        public void EndAlert(short worldID, uint zoneID) {
            lock (ZoneStateStore.Get().Zones) {
                ZoneState zone = ZoneStateStore.Get().GetZone(worldID, zoneID) ?? new() { ZoneID = zoneID, WorldID = worldID };

                zone.AlertStart = null;
                zone.AlertEnd = null;

                ZoneStateStore.Get().SetZone(worldID, zoneID, zone);
            }
        }

        public void Lock(short worldID, uint zoneID) {
            lock (ZoneStateStore.Get().Zones) {
                ZoneState zone = ZoneStateStore.Get().GetZone(worldID, zoneID) ?? new() { ZoneID = zoneID, WorldID = worldID };

                zone.IsOpened = false;

                ZoneStateStore.Get().SetZone(worldID, zoneID, zone);
            }
        }

        public void Unlock(short worldID, uint zoneID) {
            lock (ZoneStateStore.Get().Zones) {
                ZoneState zone = ZoneStateStore.Get().GetZone(worldID, zoneID) ?? new() { ZoneID = zoneID, WorldID = worldID };

                zone.IsOpened = true;

                ZoneStateStore.Get().SetZone(worldID, zoneID, zone);
            }
        }

        public async Task Map(short worldID, uint zoneID) {
            List<PsMap> regions = await _MapCollection.GetZoneMap(worldID, zoneID);

            foreach (PsMap region in regions) {
                _Logger.LogInformation($"{region.RegionID} => {region.FactionID}");
            }
        }

        public async Task Owner(short worldID, uint zoneID) {
            short? ownerFactionID = await _MapCollection.GetZoneMapOwner(worldID, zoneID);
            _Logger.LogInformation($"Owners of {worldID}:{zoneID} => {ownerFactionID}");
        }

    }
}
