using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Api;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("/api/session")]
    public class SessionApiController : ControllerBase {

        private readonly ILogger<SessionApiController> _Logger;

        private readonly ISessionDbStore _SessionDb;
        private readonly ICharacterRepository _CharacterRepository;
        private readonly IOutfitRepository _OutfitRepository;

        public SessionApiController(ILogger<SessionApiController> logger,
            ISessionDbStore sessionDb, ICharacterRepository charRepo,
            IOutfitRepository outfitRepo) {

            _Logger = logger;

            _SessionDb = sessionDb;
            _CharacterRepository = charRepo;
            _OutfitRepository = outfitRepo;
        }

        [HttpGet("{sessionID}")]
        public async Task<ActionResult<Session>> GetByID(long sessionID) {
            Session? session = await _SessionDb.GetByID(sessionID);
            if (session == null) {
                return NoContent();
            }

            return Ok(session);
        }

        [HttpGet("mass")]
        public async Task<ActionResult<List<ExpandedSession>>> GetByOutfitIDAndTime([FromQuery] List<string> outfitID, [FromQuery] DateTime start, [FromQuery] DateTime end) {
            List<ExpandedSession> all = new List<ExpandedSession>();

            _Logger.LogDebug($"RANGE: {start} - {end}");

            Dictionary<string, PsCharacter?> chars = new Dictionary<string, PsCharacter?>();

            foreach (string id in outfitID) {
                PsOutfit? outfit = await _OutfitRepository.GetByID(id);

                List<Session> sessions = await _SessionDb.GetByRangeAndOutfit(id, start, end);

                foreach (Session s in sessions) {
                    if (chars.ContainsKey(s.CharacterID) == false) {
                        chars.Add(s.CharacterID, await _CharacterRepository.GetByID(s.CharacterID));
                    }

                    all.Add(new ExpandedSession() {
                        Session = s,
                        Outfit = outfit,
                        Character = chars[s.CharacterID]
                    });
                }
            }

            return Ok(all);
        }

    }
}
