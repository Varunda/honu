using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MoreLinq;
using System;
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
        private readonly VehicleUsageRepository _VehicleUsageRepository;

        public VehicleApiController(ILogger<VehicleApiController> logger,
            VehicleRepository vehicleRepository, VehicleUsageDbStore vehicleUsageDb,
            VehicleUsageRepository vehicleUsageRepository) {

            _Logger = logger;

            _VehicleRepository = vehicleRepository;
            _VehicleUsageDb = vehicleUsageDb;
            _VehicleUsageRepository = vehicleUsageRepository;
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
                await _VehicleUsageRepository.AddVehicles(data);
            }

            return ApiOk(data);
        }

        /// <summary>
        ///     get historical saved <see cref="VehicleUsageData"/> data, up to 7 days
        /// </summary>
        /// <param name="start">where the range will start (inclusive)</param>
        /// <param name="end">where the range will end (exclusive)</param>
        /// <param name="worldID">optional, filters based on <see cref="VehicleUsageData.WorldID"/></param>
        /// <param name="includeVehicles">if the <see cref="VehicleUsageEntry.Vehicle"/> will be populated</param>
        /// <response code="200">
        ///     the response will contain a list of <see cref="VehicleUsageData"/>
        ///     where <see cref="VehicleUsageData.Timestamp"/> is between
        ///     <paramref name="start"/> and <paramref name="end"/>
        /// </response>
        /// <response code="400">
        ///     one of the following validation errors occured:
        ///     <ul>
        ///         <li><paramref name="end"/> came before <paramref name="start"/></li>
        ///         <li><paramref name="end"/> and <paramref name="start"/> are more than 7 days apart</li>
        ///     </ul>
        /// </response>
        [HttpGet("usage/history")]
        public async Task<ApiResponse<List<VehicleUsageData>>> GetHistoric(
            [FromQuery] DateTime start, [FromQuery] DateTime end,
            [FromQuery] int? worldID = null, [FromQuery] bool includeVehicles = true) {

            if (end - start > TimeSpan.FromDays(7)) {
                return ApiBadRequest<List<VehicleUsageData>>($"{nameof(start)} and {nameof(end)} can only be 7 days apart");
            }

            if (end > start) {
                return ApiBadRequest<List<VehicleUsageData>>($"{nameof(start)} must come before {nameof(end)}");
            }

            List<VehicleUsageData> data = await _VehicleUsageDb.GetByTimestamp(start, end);
            if (worldID != null) {
                data = data.Where(iter => iter.WorldID == worldID.Value).ToList();
            }

            if (includeVehicles == true) {
                await _VehicleUsageRepository.AddVehicles(data);
            }

            return ApiOk(data);
        }

    }
}
