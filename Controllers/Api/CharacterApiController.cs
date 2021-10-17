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
    [Route("/api/")]
    public class CharacterApiController : ControllerBase {

        private readonly ILogger<CharacterApiController> _Logger;

        private readonly ICharacterRepository _CharacterRepository;

        public CharacterApiController(ILogger<CharacterApiController> logger,
            ICharacterRepository charRepo) {

            _Logger = logger;
            _CharacterRepository = charRepo;
        }

        [HttpGet("character/{charID}")]
        public async Task<ActionResult<PsCharacter?>> GetByID(string charID) {
            if (charID.All(char.IsDigit) == false) {
                return BadRequest($"{nameof(charID)} was not all digits: '{charID}'");
            }

            PsCharacter? c = await _CharacterRepository.GetByID(charID);
            if (c == null) {
                return NoContent();
            }

            return Ok(c);
        }

        [HttpGet("characters/name/{name}")]
        public async Task<ActionResult<List<PsCharacter>>> GetByName(string name) {
            List<PsCharacter> chars = await _CharacterRepository.GetByName(name);

            return Ok(chars);
        }

    }
}
