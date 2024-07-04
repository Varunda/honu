using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MoreLinq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Models.Api;
using watchtower.Models.Census;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    [Route("api/vehicle")]
    [ApiController]
    public class VehicleApiController : ApiControllerBase {

        private readonly ILogger<VehicleApiController> _Logger;

        private readonly VehicleRepository _VehicleRepository;
        private readonly VehicleUsageDbStore _VehicleUsageDb;

        public VehicleApiController( ILogger<VehicleApiController> logger,
            VehicleRepository vehicleRepository, VehicleUsageDbStore vehicleUsageDb) {

            _Logger = logger;

            _VehicleRepository = vehicleRepository;
            _VehicleUsageDb = vehicleUsageDb;
        }

        /// <summary>
        ///     Get a list of all <see cref="PsVehicle"/>s that exist
        /// </summary>
        /// <response code="200">
        ///     The response will contain a list of <see cref="PsVehicle"/>
        /// </response>
        [HttpGet]
        public async Task<ApiResponse<List<PsVehicle>>> GetAll() {
            List<PsVehicle> vehicles = await _VehicleRepository.GetAll();

            return ApiOk(vehicles);
        }

        /// <summary>
        ///     get the <see cref="VehicleUsageData"/> that was stored in the DB by its <see cref="VehicleUsageData.ID"/>
        /// </summary>
        /// <param name="ID">ID of the <see cref="VehicleUsageData"/> to get</param>
        /// <param name="includeVehicles">optional, will <see cref="VehicleUsageEntry.Vehicle"/> be populated or not?</param>
        /// <response code="200">
        ///     the response will contain the <see cref="VehicleUsageData"/> with <see cref="VehicleUsageData.ID"/> of <paramref name="ID"/>
        /// </response>
        /// <response code="204">
        ///     no <see cref="VehicleUsageData"/> with <see cref="VehicleUsageData.ID"/> of <paramref name="ID"/> exists
        /// </response>
        [HttpGet("usage/{ID}")]
        public async Task<ApiResponse<VehicleUsageData>> GetUsage(ulong ID, [FromQuery] bool includeVehicles = true) {
            VehicleUsageData? data = await _VehicleUsageDb.GetByID(ID);
            if (data == null) {
                return ApiNoContent<VehicleUsageData>();
            }

            if (includeVehicles == true) {
                Dictionary<int, PsVehicle> vehicles = (await _VehicleRepository.GetAll()).ToDictionary(iter => iter.ID);

                _UpdateVehicleFields(data.Vs, vehicles);
                _UpdateVehicleFields(data.Nc, vehicles);
                _UpdateVehicleFields(data.Tr, vehicles);
                _UpdateVehicleFields(data.Other, vehicles);
            }

            return ApiOk(data);
        }

        private static void _UpdateVehicleFields(VehicleUsageFaction faction, Dictionary<int, PsVehicle> dict) {
            foreach (KeyValuePair<int, VehicleUsageEntry> kvp in faction.Usage) {
                if (kvp.Key == -1) {
                    kvp.Value.VehicleName = "unknown";
                } else if (kvp.Key == 0) {
                    kvp.Value.VehicleName = "none";
                } else {
                    kvp.Value.Vehicle = dict.GetValueOrDefault(kvp.Key);
                    kvp.Value.VehicleName = kvp.Value.Vehicle?.Name ?? $"<missing {kvp.Key}>";
                }
            }
        }

    }
}
