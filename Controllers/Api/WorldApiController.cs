using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Models.Api;
using watchtower.Models.Census;
using watchtower.Realtime;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("/api/world")]
    public class WorldApiController : ApiControllerBase {

        private readonly ILogger<WorldApiController> _Logger;

        private readonly WorldOverviewRepository _WorldOverviewRepository;
        private readonly VehicleRepository _VehicleRepository;
        private readonly IEventHandler _EventHandler;

        public WorldApiController(ILogger<WorldApiController> logger,
            WorldOverviewRepository worldOverviewRepository, VehicleRepository vehicleRepository,
            IEventHandler eventHandler) {

            _Logger = logger;

            _WorldOverviewRepository = worldOverviewRepository;
            _VehicleRepository = vehicleRepository;
            _EventHandler = eventHandler;
        }

        /// <summary>
        ///     Get an overview of all worlds and each zone
        /// </summary>
        [HttpGet("overview")]
        public ApiResponse<List<WorldOverview>> GetOverview() {
            return ApiOk(_WorldOverviewRepository.Build());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="worldID"></param>
        /// <param name="zoneID"></param>
        /// <returns></returns>
        [HttpGet("vehicle-data/{worldID}/{zoneID}")]
        public async Task<ApiResponse<VehicleUsageData>> GetVehicleData(short worldID, uint zoneID, [FromQuery] bool includeVehicles = true) {
            VehicleUsageData data = new();
            data.WorldID = worldID;
            data.ZoneID = zoneID;
            data.Timestamp = DateTime.UtcNow;
            data.LastEvent = _EventHandler.MostRecentProcess();

            // the ! is here cause c# doesn't like that a PsVehicle can become a PsVehicle?, which is pretty valid actually
            Dictionary<int, PsVehicle?> vehicles = (await _VehicleRepository.GetAll()).ToDictionary(iter => iter.ID)!;

            lock (CharacterStore.Get().Players) {
                foreach (KeyValuePair<string, TrackedPlayer> iter in CharacterStore.Get().Players) {
                    TrackedPlayer p = iter.Value;

                    if (p.Online == false || p.WorldID != worldID || p.ZoneID != zoneID) {
                        continue;
                    }

                    VehicleUsageFaction fact;
                    if (p.TeamID == Faction.NC) {
                        fact = data.Nc;
                    } else if (p.TeamID == Faction.VS) {
                        fact = data.Vs;
                    } else if (p.TeamID == Faction.TR) {
                        fact = data.Tr;
                    } else { 
                        fact = data.Other;
                    }

                    if (fact.Usage.ContainsKey(p.PossibleVehicleID) == false) {
                        VehicleUsageEntry entry = new();
                        entry.VehicleID = p.PossibleVehicleID;
                        if (includeVehicles == true) {
                            entry.Vehicle = vehicles.GetValueOrDefault(p.PossibleVehicleID, null);
                        }

                        if (p.PossibleVehicleID == -1) {
                            entry.VehicleName = "unknown";
                        } else if (p.PossibleVehicleID == 0) {
                            entry.VehicleName = "none";
                        } else {
                            entry.VehicleName = vehicles.GetValueOrDefault(p.PossibleVehicleID, null)?.Name ?? $"<missing {p.PossibleVehicleID}>";
                        }

                        fact.Usage.Add(p.PossibleVehicleID, entry);
                    }

                    fact.Usage[p.PossibleVehicleID].Count += 1;
                }
            }

            return ApiOk(data);
        }

    }
}
