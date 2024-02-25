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
        private readonly VehicleUsageRepository _VehicleUsageRepository;

        public WorldApiController(ILogger<WorldApiController> logger,
            WorldOverviewRepository worldOverviewRepository, VehicleRepository vehicleRepository,
            VehicleUsageRepository vehicleUsageRepository) {

            _Logger = logger;

            _WorldOverviewRepository = worldOverviewRepository;
            _VehicleRepository = vehicleRepository;
            _VehicleUsageRepository = vehicleUsageRepository;
        }

        /// <summary>
        ///     Get an overview of all worlds and each zone
        /// </summary>
        [HttpGet("overview")]
        public ApiResponse<List<WorldOverview>> GetOverview() {
            return ApiOk(_WorldOverviewRepository.Build());
        }

        /// <summary>
        ///     get the <see cref="VehicleUsageData"/> that honu thinks is current. optionally, a world filter and zone filter
        ///     can be specified using <paramref name="worldID"/> and <paramref name="zoneID"/>
        /// </summary>
        /// <param name="worldID">optional, if non-null, filter the returned data based on the world ID of the characters</param>
        /// <param name="zoneID">optional, if non-null, filter the returned data based on the zone ID of the characters</param>
        /// <param name="includeVehicles">if the vehicle information from census will be included in the response or not</param>
        /// <response code="200">
        ///     the response will contain a <see cref="VehicleUsageData"/> that provides estimates of how many people are in each
        ///     vehicle
        /// </response>
        [HttpGet("vehicle-data")]
        public async Task<ApiResponse<VehicleUsageData>> GetVehicleData(
                [FromQuery] short? worldID = null,
                [FromQuery] uint? zoneID = null,
                [FromQuery] bool includeVehicles = true
            ) {

            VehicleUsageData data = await _VehicleUsageRepository.Get(worldID: worldID, zoneID: zoneID, includeVehicles: includeVehicles);

            return ApiOk(data);
        }

    }
}
