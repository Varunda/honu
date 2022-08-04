using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using watchtower.Models;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("/api/world")]
    public class WorldApiController : ApiControllerBase {

        private readonly ILogger<WorldApiController> _Logger;

        private readonly WorldOverviewRepository _WorldOverviewRepository;

        public WorldApiController(ILogger<WorldApiController> logger,
            WorldOverviewRepository worldOverviewRepository) {

            _Logger = logger;
            _WorldOverviewRepository = worldOverviewRepository;
        }

        /// <summary>
        ///     Get an overview of all worlds and each zone
        /// </summary>
        [HttpGet("overview")]
        public ApiResponse<List<WorldOverview>> GetOverview() {
            return ApiOk(_WorldOverviewRepository.Build());
        }

    }
}
