using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Models.Watchtower;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("/api/realtime-network")]
    public class RealtimeNetworkApiController : ApiControllerBase {

        private readonly ILogger<RealtimeNetworkApiController> _Logger;

        private readonly RealtimeNetworkBuilder _Builder;

        public RealtimeNetworkApiController(ILogger<RealtimeNetworkApiController> logger,
            RealtimeNetworkBuilder builder) {

            _Logger = logger;
            _Builder = builder;
        }

        [HttpGet("{worldID}")]
        public async Task<ApiResponse<RealtimeNetwork>> GetByWorldID(short worldID,
            [FromQuery] uint? zoneID, [FromQuery] DateTime? start, [FromQuery] DateTime? end) {

            if (start != null && end != null) {
                if (end <= start) {
                    return ApiBadRequest<RealtimeNetwork>($"{nameof(end)} {end:u} cannot come before {nameof(start)} {start:u}");
                }
            }

            RealtimeNetwork network = await _Builder.GetByRange((start ?? DateTime.UtcNow) - TimeSpan.FromMinutes(3), end ?? DateTime.UtcNow, zoneID, worldID);

            return ApiOk(network);
        }

    }
}
