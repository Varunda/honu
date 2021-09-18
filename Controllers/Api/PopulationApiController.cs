using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Db;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("/api/population")]
    public class PopulationApiController : ControllerBase {

        private readonly ILogger<PopulationApiController> _Logger;
        private readonly IOutfitDbStore _OutfitDb;

        public PopulationApiController(ILogger<PopulationApiController> logger,
            IOutfitDbStore outfitRepo) {

            _Logger = logger;
            _OutfitDb = outfitRepo;
        }

        [HttpGet("{worldID}")]
        public async Task<ActionResult<List<OutfitPopulation>>> Get(short worldID, [FromQuery] DateTime? start, [FromQuery] DateTime? end) {
            DateTime startPeriod = start ?? DateTime.UtcNow;
            DateTime endPeriod = end ?? DateTime.UtcNow.AddHours(-2);

            _Logger.LogInformation($"{startPeriod} {endPeriod}");

            List<OutfitPopulation> pops = await _OutfitDb.GetPopulation(startPeriod, endPeriod, worldID);
            return Ok(pops);
        }

    }
}
