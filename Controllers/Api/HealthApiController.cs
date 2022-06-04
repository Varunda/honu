using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
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
        private readonly IMemoryCache _Cache;

        private readonly CensusRealtimeHealthRepository _RealtimeHealthRepository;
        private readonly BadHealthRepository _BadHealthRepository;

        private readonly CharacterCacheQueue _CharacterCache;
        private readonly SessionStarterQueue _SessionQueue;
        private readonly CharacterUpdateQueue _WeaponQueue;
        private readonly CensusRealtimeEventQueue _TaskQueue;
        private readonly WeaponPercentileCacheQueue _PercentileQueue;
        private readonly DiscordMessageQueue _DiscordQueue;

        public HealthApiController(ILogger<HealthApiController> logger, IMemoryCache cache,
            CensusRealtimeHealthRepository realtimeHealthRepository, CharacterCacheQueue characterCache,
            SessionStarterQueue sessionQueue, CharacterUpdateQueue weaponQueue,
            CensusRealtimeEventQueue taskQueue, WeaponPercentileCacheQueue percentileQueue,
            DiscordMessageQueue discordQueue, BadHealthRepository badHealthRepository) {

            _Logger = logger;
            _Cache = cache;

            _RealtimeHealthRepository = realtimeHealthRepository;
            _BadHealthRepository = badHealthRepository;

            _CharacterCache = characterCache;
            _SessionQueue = sessionQueue;
            _WeaponQueue = weaponQueue;
            _TaskQueue = taskQueue;
            _PercentileQueue = percentileQueue;
            _DiscordQueue = discordQueue;
        }

        /// <summary>
        ///     Get an object that indicates how healthy Honu is in various metrics
        /// </summary>
        /// <remarks>
        ///     Feel free to hammer this endpoint as much as you'd like. The results are cached for 800ms, and it only takes like 2ms to
        ///     get all the data, so hitting this endpoint is not a burden
        /// </remarks>
        /// <response code="200">
        ///     The response will contain a <see cref="HonuHealth"/> that represents the health of Honu at the time of being called
        /// </response>
        [HttpGet]
        public ApiResponse<HonuHealth> GetRealtimeHealth() {
            if (_Cache.TryGetValue("Honu.Health", out HonuHealth health) == false) {
                health = new HonuHealth();
                health.Death = _RealtimeHealthRepository.GetDeathHealth();
                health.Exp = _RealtimeHealthRepository.GetExpHealth();
                health.RealtimeHealthFailures = _BadHealthRepository.GetRecent();

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

                _Cache.Set("Honu.Health", health, new MemoryCacheEntryOptions() {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(800)
                });
            }

            return ApiOk(health);
        }

    }
}
