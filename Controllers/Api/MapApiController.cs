using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Api;
using watchtower.Models.Census;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("/api/map")]
    public class MapApiController : ControllerBase {

        private readonly ILogger<MapApiController> _Logger;
        private readonly IMapRepository _MapRepository;

        public MapApiController(ILogger<MapApiController> logger,
            IMapRepository mapRepo) {

            _Logger = logger;
            _MapRepository = mapRepo ?? throw new ArgumentNullException(nameof(mapRepo));
        }

        [HttpGet("{zoneID}")]
        public async Task<ActionResult<ZoneMap?>> GetMap(uint zoneID) {
            List<PsMapHex> hexes = (await _MapRepository.GetHexes()).Where(iter => iter.ZoneID == zoneID).ToList();
            List<PsFacilityLink> links = (await _MapRepository.GetFacilityLinks()).Where(iter => iter.ZoneID == zoneID).ToList();
            List<PsFacility> facs = (await _MapRepository.GetFacilities()).Where(iter => iter.ZoneID == zoneID).ToList();

            ZoneMap map = new ZoneMap() {
                Hexes = hexes,
                Facilities = facs,
                Links = links
            };

            return Ok(map);
        }

        [HttpGet("hexes")]
        public async Task<ActionResult<List<PsMapHex>>> GetHexes() {
            List<PsMapHex> hexes = await _MapRepository.GetHexes();
            return Ok(hexes);
        }

        [HttpGet("links")]
        public async Task<ActionResult<List<PsFacilityLink>>> GetLinks() {
            List<PsFacilityLink> links = await _MapRepository.GetFacilityLinks();
            return Ok(links);
        }

        [HttpGet("facilities")]
        public async Task<ActionResult<List<PsFacility>>> GetFacilities() {
            List<PsFacility> facs = await _MapRepository.GetFacilities();
            return Ok(facs);
        }

    }
}
