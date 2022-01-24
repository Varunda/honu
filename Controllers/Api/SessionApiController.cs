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

        private readonly ISessionDbStore _SessionDb;
        private readonly CharacterRepository _CharacterRepository;
        private readonly OutfitRepository _OutfitRepository;

        public SessionApiController(ILogger<SessionApiController> logger,
            ISessionDbStore sessionDb, CharacterRepository charRepo,
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
        [HttpGet("{sessionID}")]
        public async Task<ApiResponse<Session>> GetByID(long sessionID) {
            Session? session = await _SessionDb.GetByID(sessionID);
            if (session == null) {
                return ApiNoContent<Session>();
            }

            return ApiOk(session);
        }

    }
}
