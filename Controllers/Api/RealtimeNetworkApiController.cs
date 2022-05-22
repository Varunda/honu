using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        public async Task<ApiResponse<RealtimeNetwork>> GetByWorldID(short worldID) {
            RealtimeNetwork network = await _Builder.Build(worldID);

            return ApiOk(network);
        }

    }
}
