using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Db;
using watchtower.Services.Db;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("/api/session")]
    public class SessionApiController : ControllerBase {

        private readonly ILogger<SessionApiController> _Logger;
        private readonly ISessionDbStore _SessionDb;

        public SessionApiController(ILogger<SessionApiController> logger,
            ISessionDbStore sessionDb) {

            _Logger = logger;
            _SessionDb = sessionDb;
        }

        [HttpGet("{sessionID}")]
        public async Task<ActionResult<Session>> GetByID(long sessionID) {
            Session? session = await _SessionDb.GetByID(sessionID);
            if (session == null) {
                return NoContent();
            }

            return Ok(session);
        }

    }
}
