using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Models.Api;
using watchtower.Models.Health;
using watchtower.Services.Queues;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("api/health")]
    public class HealthApiController : ApiControllerBase {

        private readonly ILogger<HealthApiController> _Logger;
        private readonly CensusRealtimeHealthRepository _RealtimeHealthRepository;
        private readonly CharacterCacheQueue _CharacterCache;
        private readonly SessionStarterQueue _SessionQueue;
        private readonly CharacterUpdateQueue _WeaponQueue;
        private readonly CensusRealtimeEventQueue _TaskQueue;
        private readonly WeaponPercentileCacheQueue _PercentileQueue;
        private readonly DiscordMessageQueue _DiscordQueue;

        public HealthApiController(ILogger<HealthApiController> logger,
            CensusRealtimeHealthRepository realtimeHealthRepository, CharacterCacheQueue characterCache,
            SessionStarterQueue sessionQueue, CharacterUpdateQueue weaponQueue,
            CensusRealtimeEventQueue taskQueue, WeaponPercentileCacheQueue percentileQueue,
            DiscordMessageQueue discordQueue) {

            _Logger = logger;
            _RealtimeHealthRepository = realtimeHealthRepository;
            _CharacterCache = characterCache;
            _SessionQueue = sessionQueue;
            _WeaponQueue = weaponQueue;
            _TaskQueue = taskQueue;
            _PercentileQueue = percentileQueue;
            _DiscordQueue = discordQueue;
        }

        [HttpGet]
        public ApiResponse<HonuHealth> GetRealtimeHealth() {
            HonuHealth health = new HonuHealth();

            health.Death = _RealtimeHealthRepository.GetDeathHealth();
            health.Exp = _RealtimeHealthRepository.GetExpHealth();

            ServiceQueueCount c = new() { QueueName = "character_cache_queue", Count = _CharacterCache.Count() };
            ServiceQueueCount session = new() { QueueName = "session_start_queue", Count = _SessionQueue.Count() };
            ServiceQueueCount weapon = new() { QueueName = "character_weapon_stat_queue", Count = _WeaponQueue.Count() };
            ServiceQueueCount task = new() { QueueName = "task_queue", Count = _TaskQueue.Count(), Average = _TaskQueue.GetProcessTime().Average() };
            ServiceQueueCount percentile = new() { QueueName = "weapon_percentile_cache_queue", Count = _PercentileQueue.Count() };
            ServiceQueueCount discord = new() { QueueName = "discord_message_queue", Count = _DiscordQueue.Count() };

            health.Queues = new() {
                c, session, weapon,
                task, percentile, discord
            };

            return ApiOk(health);
        }

    }
}
