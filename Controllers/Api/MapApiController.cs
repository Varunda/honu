using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Models.Api;
using watchtower.Models.Census;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    /// <summary>
    ///     Endpoints used to get the lattices and hexes in a zone
    /// </summary>
    [ApiController]
    [Route("/api/map")]
    public class MapApiController : ApiControllerBase {

        private readonly ILogger<MapApiController> _Logger;
        private readonly MapRepository _MapRepository;

        public MapApiController(ILogger<MapApiController> logger,
            MapRepository mapRepo) {

            _Logger = logger;
            _MapRepository = mapRepo ?? throw new ArgumentNullException(nameof(mapRepo));
        }

        /// <summary>
        ///     Get all the information about a zone in one handy endpoint
        /// </summary>
        /// <param name="zoneID">ID of the zone</param>
        /// <response code="200">
        ///     The response will contain the zone information
        /// </response>
        [HttpGet("{zoneID}")]
        public async Task<ApiResponse<ZoneMap>> GetMap(uint zoneID) {
            List<PsMapHex> hexes = (await _MapRepository.GetHexes()).Where(iter => iter.ZoneID == zoneID).ToList();
            List<PsFacilityLink> links = (await _MapRepository.GetFacilityLinks()).Where(iter => iter.ZoneID == zoneID).ToList();
            List<PsFacility> facs = (await _MapRepository.GetFacilities()).Where(iter => iter.ZoneID == zoneID).ToList();

            ZoneMap map = new ZoneMap() {
                Hexes = hexes,
                Facilities = facs,
                Links = links
            };

            return ApiOk(map);
        }

        /// <summary>
        ///     Get all the hexes for all zones
        /// </summary>
        [HttpGet("hexes")]
        public async Task<ApiResponse<List<PsMapHex>>> GetHexes() {
            List<PsMapHex> hexes = await _MapRepository.GetHexes();
            return ApiOk(hexes);
        }

        /// <summary>
        ///     Get the facility links for all zones
        /// </summary>
        [HttpGet("links")]
        public async Task<ApiResponse<List<PsFacilityLink>>> GetLinks() {
            List<PsFacilityLink> links = await _MapRepository.GetFacilityLinks();
            return ApiOk(links);
        }

        /// <summary>
        ///     Get all facilities
        /// </summary>
        [HttpGet("facilities")]
        public async Task<ApiResponse<List<PsFacility>>> GetFacilities() {
            List<PsFacility> facs = await _MapRepository.GetFacilities();
            return ApiOk(facs);
        }

    }
}
