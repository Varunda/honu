using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Models.Events;
using watchtower.Services.Db;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("api/achievment-earned")]
    public class AchievementEarnedApiController : ApiControllerBase {

        private readonly ILogger<AchievementEarnedApiController> _Logger;
        private readonly AchievementEarnedDbStore _AchievementEarnedDb;

        public AchievementEarnedApiController(ILogger<AchievementEarnedApiController> logger,
            AchievementEarnedDbStore achievementEarnedDb) {

            _Logger = logger;
            _AchievementEarnedDb = achievementEarnedDb;
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

    }
}
