using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("/api/outfit/")]
    public class OutfitApiController : ControllerBase {

        private readonly ILogger<OutfitApiController> _Logger;

        private readonly IOutfitRepository _OutfitRepository;

        public OutfitApiController(ILogger<OutfitApiController> logger,
            IOutfitRepository outfitRepo) {

            _Logger = logger;
            _OutfitRepository = outfitRepo ?? throw new ArgumentNullException(nameof(outfitRepo));
        }

        [HttpGet("{outfitID}")]
        public async Task<ActionResult<PsOutfit>> GetByID(string outfitID) {
            PsOutfit? outfit = await _OutfitRepository.GetByID(outfitID);
            if (outfit == null) {
                return NoContent();
            }

            return Ok(outfit);
        }

    }
}
