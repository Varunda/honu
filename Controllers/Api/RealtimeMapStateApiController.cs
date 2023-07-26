using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Models;
using watchtower.Models.Db;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("/api/realtime-map-state")]
    public class RealtimeMapStateApiController : ApiControllerBase {

        private readonly ILogger<RealtimeMapStateApiController> _Logger;
        private readonly RealtimeMapStateRepository _RealtimeMapStateRepository;

        public RealtimeMapStateApiController(ILogger<RealtimeMapStateApiController> logger, RealtimeMapStateRepository realtimeMapStateRepository) {
            _Logger = logger;
            _RealtimeMapStateRepository = realtimeMapStateRepository;
        }

        /// <summary>
        ///     Get 60 minutes of history for a region in a world
        /// </summary>
        /// <param name="worldID">ID of the world</param>
        /// <param name="regionID">ID of the region</param>
        /// <response code="200">
        ///     Contains a list of <see cref="RealtimeMapState"/>s for the last hour that have occured
        ///     on world <paramref name="worldID"/> and in the region <paramref name="regionID"/>
        /// </response>
        [HttpGet("history/{worldID}/{regionID}")]
        public async Task<ApiResponse<List<RealtimeMapState>>> GetRecent(short worldID, int regionID) {
            if (World.IsValidID(worldID) == false) {
                return ApiBadRequest<List<RealtimeMapState>>($"{worldID} is not a valid WorldID");
            }

            DateTime periodEnd = DateTime.UtcNow;
            DateTime periodStart = periodEnd - TimeSpan.FromMinutes(60);

            List<RealtimeMapState> state = await _RealtimeMapStateRepository.GetHistoricalByWorldAndRegion(worldID, regionID, periodStart, periodEnd);

            return ApiOk(state);
        }

    }
}
