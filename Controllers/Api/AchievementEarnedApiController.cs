using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Models.Api;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Events;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("api/achievement-earned")]
    public class AchievementEarnedApiController : ApiControllerBase {

        private readonly ILogger<AchievementEarnedApiController> _Logger;

        private readonly AchievementEarnedDbStore _AchievementEarnedDb;
        private readonly SessionDbStore _SessionDb;
        private readonly AchievementRepository _AchievementRepository;

        public AchievementEarnedApiController(ILogger<AchievementEarnedApiController> logger,
            AchievementEarnedDbStore achievementEarnedDb, SessionDbStore sessionDb,
            AchievementRepository achievementRepository) {

            _Logger = logger;

            _AchievementEarnedDb = achievementEarnedDb;
            _SessionDb = sessionDb;
            _AchievementRepository = achievementRepository;
        }

        [HttpGet("{charID}")]
        public async Task<ApiResponse<List<AchievementEarnedEvent>>> GetByCharacterIDAndRange(string charID,
            [FromQuery] DateTime start, [FromQuery] DateTime end) {

            if (start >= end) {
                return ApiBadRequest<List<AchievementEarnedEvent>>($"{nameof(start)} cannot come after {nameof(end)}");
            }

            List<AchievementEarnedEvent> events = await _AchievementEarnedDb.GetByCharacterIDAndRange(charID, start, end);

            return ApiOk(events);
        }

        [HttpGet("session/{sessionID}")]
        public async Task<ApiResponse<List<AchievementEarnedEvent>>> GetBySessionID(long sessionID) {
            Session? session = await _SessionDb.GetByID(sessionID);
            if (session == null) {
                return ApiNotFound<List<AchievementEarnedEvent>>($"{nameof(Session)} {sessionID}");
            }

            List<AchievementEarnedEvent> events = await _AchievementEarnedDb.GetByCharacterIDAndRange(session.CharacterID, session.Start, session.End ?? DateTime.UtcNow);

            return ApiOk(events);
        }

        /// <summary>
        ///     Get the achievements earned by within a session as a block.
        ///     The block will contain the list of <see cref="AchievementEarnedEvent"/>s, as well as
        ///     a list of <see cref="Achievement"/>s for the ones that were earned during a session
        /// </summary>
        /// <param name="sessionID">ID of the session</param>
        /// <response code="200">
        ///     The response will contain a <see cref="AchievementEarnedBlock"/> which contains the events
        ///     within a session, as well as what those achievements were
        /// </response>
        /// <response code="404">
        ///     No <see cref="Session"/> with <see cref="Session.ID"/> of <paramref name="sessionID"/> exists
        /// </response>
        [HttpGet("session/{sessionID}/block")]
        public async Task<ApiResponse<AchievementEarnedBlock>> GetBlockBySessionID(long sessionID) {
            Session? session = await _SessionDb.GetByID(sessionID);
            if (session == null) {
                return ApiNotFound<AchievementEarnedBlock>($"{nameof(Session)} {sessionID}");
            }

            List<AchievementEarnedEvent> events = await _AchievementEarnedDb.GetByCharacterIDAndRange(session.CharacterID, session.Start, session.End ?? DateTime.UtcNow);

            AchievementEarnedBlock block = await _MakeBlock(events);

            return ApiOk(block);
        }

        /// <summary>
        ///     Convert a list of <see cref="AchievementEarnedEvent"/>s into a block
        /// </summary>
        /// <param name="events">Events to convert</param>
        private async Task<AchievementEarnedBlock> _MakeBlock(List<AchievementEarnedEvent> events) {
            List<int> ids = events.Select(iter => iter.AchievementID).Distinct().ToList();

            Dictionary<int, Achievement> achvs = (await _AchievementRepository.GetAll()).Where(iter => ids.Contains(iter.ID))
                .ToDictionary(iter => iter.ID);

            AchievementEarnedBlock block = new();
            Dictionary<int, Achievement?> found = new();

            foreach (AchievementEarnedEvent ev in events) {
                if (found.ContainsKey(ev.AchievementID) == true) {
                    continue;
                }

                _ = achvs.TryGetValue(ev.AchievementID, out Achievement? acvh);

                found.Add(ev.AchievementID, acvh);
            }

            block.Events = events;
            block.Achievements = found.Values.Where(iter => iter != null).Select(iter => (iter!)).ToList();

            return block;
        }

    }
}
