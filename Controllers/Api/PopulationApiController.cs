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

    [ApiController]
    [Route("/api/population")]
    public class PopulationApiController : ControllerBase {

        private readonly ILogger<PopulationApiController> _Logger;
        private readonly IOutfitDbStore _OutfitDb;
        private readonly WorldPopulationRepository _PopulationRepository;

        public PopulationApiController(ILogger<PopulationApiController> logger,
            IOutfitDbStore outfitRepo, WorldPopulationRepository popRepo) {

            _Logger = logger;

            _OutfitDb = outfitRepo;
            _PopulationRepository = popRepo ?? throw new ArgumentNullException(nameof(popRepo));
        }

        [HttpGet("{worldID}/outfits")]
        public async Task<ActionResult<List<OutfitPopulation>>> GetOutfits(short worldID, [FromQuery] DateTime? time) {
            DateTime when = time ?? DateTime.UtcNow;

            _Logger.LogInformation($"{when}");

            List<OutfitPopulation> pops = await _OutfitDb.GetPopulation(when, worldID);
            return Ok(pops);
        }

        [HttpGet("{worldID}")]
        public ActionResult<WorldPopulation> Get(short worldID) {
            WorldPopulation pop = _PopulationRepository.GetByWorldID(worldID);
            return Ok(pop);
        }

    }
}
