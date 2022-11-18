using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    [Route("api/vehicle")]
    [ApiController]
    public class VehicleApiController : ApiControllerBase {

        private readonly VehicleRepository _VehicleRepository;

        public VehicleApiController(VehicleRepository vehicleRepository) {
            _VehicleRepository = vehicleRepository;
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

    }
}
