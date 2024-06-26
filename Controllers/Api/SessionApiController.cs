﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Models;
using watchtower.Models.Api;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    /// <summary>
    ///     Endpoints about sessions
    /// </summary>
    [ApiController]
    [Route("/api/session")]
    public class SessionApiController : ApiControllerBase {

        private readonly ILogger<SessionApiController> _Logger;

        private readonly SessionDbStore _SessionDb;
        private readonly CharacterRepository _CharacterRepository;
        private readonly OutfitRepository _OutfitRepository;

        public SessionApiController(ILogger<SessionApiController> logger,
            SessionDbStore sessionDb, CharacterRepository charRepo,
            OutfitRepository outfitRepo) {

            _Logger = logger;

            _SessionDb = sessionDb;
            _CharacterRepository = charRepo;
            _OutfitRepository = outfitRepo;
        }

        /// <summary>
        ///     Get a specific session
        /// </summary>
        /// <param name="sessionID">ID of the session</param>
        /// <response code="200">
        ///     The response will contain the <see cref="Session"/> with <see cref="Session.ID"/> of <paramref name="sessionID"/>
        /// </response>
        /// <response code="204">
        ///     No <see cref="Session"/> with <see cref="Session.ID"/> of <paramref name="sessionID"/> exists
        /// </response>
        [HttpGet("{sessionID:long}")]
        public async Task<ApiResponse<Session>> GetByID(long sessionID) {
            Session? session = await _SessionDb.GetByID(sessionID);
            if (session == null) {
                return ApiNoContent<Session>();
            }

            return ApiOk(session);
        }

        /// <summary>
        ///     Get all PC sessions that were online at a given time
        /// </summary>
        /// <param name="whenEpoch">When to lookup the sessions, in unix epoch seconds</param>
        /// <param name="worldID">Optional world ID to limit the lookup by</param>
        /// <response code="200">
        ///     The response will contain a list of <see cref="ExpandedSession"/>s that represent characters online
        ///     at the <c>DateTime</c> from <paramref name="whenEpoch"/>, which is in units of Unix epoch seconds
        /// </response>
        [HttpGet("history/{whenEpoch}")]
        public async Task<ApiResponse<List<ExpandedSession>>> GetByDateTime(long whenEpoch, [FromQuery] short? worldID) {
            DateTime when = DateTimeOffset.FromUnixTimeSeconds(whenEpoch).DateTime;

            List<Session> sessions = await _SessionDb.GetByRange(when - TimeSpan.FromMinutes(1), when + TimeSpan.FromMinutes(1));

            _Logger.LogDebug($"Loaded {sessions.Count} sessions from {when:u}");

            List<string> charIDs = sessions.Select(iter => iter.CharacterID).Distinct().ToList();

            List<PsCharacter> chars = await _CharacterRepository.GetByIDs(charIDs, CensusEnvironment.PC);

            List<ExpandedSession> expanded = new List<ExpandedSession>(sessions.Count);

            foreach (Session session in sessions) {
                PsCharacter? c = chars.FirstOrDefault(iter => iter.ID == session.CharacterID);

                if (worldID != null && c != null && c.WorldID != worldID) {
                    continue;
                }

                ExpandedSession ex = new ExpandedSession() {
                    Session = session,
                    Character = chars.FirstOrDefault(iter => iter.ID == session.CharacterID)
                };

                expanded.Add(ex);
            }

            return ApiOk(expanded);
        }

        /// <summary>
        ///     Get the sessions of a character that where online during the period given
        /// </summary>
        /// <param name="characterID">ID of the character</param>
        /// <param name="start">When the period starts</param>
        /// <param name="end">When the period ends</param>
        /// <response code="200">
        ///     A list of <see cref="Session"/>s for the character that were online between <paramref name="start"/> and <paramref name="end"/>
        /// </response>
        [HttpGet("character/{characterID}/period")]
        public async Task<ApiResponse<List<Session>>> GetByCharacterIDAndPeriod(string characterID, [FromQuery] DateTime start, [FromQuery] DateTime end) {
            List<Session> sessions = await _SessionDb.GetByRangeAndCharacterID(characterID, start, end);

            return ApiOk(sessions);
        }

        [HttpGet("outfit/{outfitID}")]
        public async Task<ApiResponse<List<Session>>> GetByOutfitID(string outfitID) {
            List<Session> sessions = await _SessionDb.GetByRangeAndOutfit(outfitID, DateTime.MinValue, DateTime.UtcNow);

            return ApiOk(sessions);
        }

    }
}
