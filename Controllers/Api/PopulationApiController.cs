using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Models.Api;
using watchtower.Models.Db;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    /// <summary>
    ///     Endpoints for getting world populations
    /// </summary>
    [ApiController]
    [Route("/api/population")]
    public class PopulationApiController : ApiControllerBase {

        private readonly ILogger<PopulationApiController> _Logger;
        private readonly OutfitDbStore _OutfitDb;
        private readonly WorldPopulationRepository _PopulationRepository;

        public PopulationApiController(ILogger<PopulationApiController> logger,
            OutfitDbStore outfitRepo, WorldPopulationRepository popRepo) {

            _Logger = logger;

            _OutfitDb = outfitRepo;
            _PopulationRepository = popRepo ?? throw new ArgumentNullException(nameof(popRepo));
        }

        /// <summary>
        ///     Get how many members outfits have online at a specific time
        /// </summary>
        /// <remarks>
        ///     This data is based on session data, which includes the outfit ID of a character when created. This means
        ///     the data will stay the same even if a character changes outfits, or if a character is deleted
        /// </remarks>
        /// <param name="worldID">ID of the world to get the outfits of</param>
        /// <param name="time">When to do the lookup. If left null, it will default to the current time</param>
        /// <response code="200">
        ///     The response will contain a list of <see cref="OutfitPopulation"/>s
        /// </response>
        [HttpGet("{worldID}/outfits")]
        public async Task<ApiResponse<List<OutfitPopulation>>> GetOutfits(short worldID, [FromQuery] DateTime? time) {
            DateTime when = time ?? DateTime.UtcNow;

            List<OutfitPopulation> pops = await _OutfitDb.GetPopulation(when, worldID);
            return ApiOk(pops);
        }

        /// <summary>
        ///     Get the current world population
        /// </summary>
        /// <param name="worldID">ID of the world</param>
        /// <response code="200">
        ///     The response will contain the <see cref="WorldPopulation"/> for the 
        ///     world passed in <paramref name="worldID"/>
        /// </response>
        [HttpGet("{worldID}")]
        public ApiResponse<WorldPopulation> Get(short worldID) {
            WorldPopulation pop = _PopulationRepository.GetByWorldID(worldID);
            return ApiOk(pop);
        }

    }
}
