using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Models.Api;
using watchtower.Models.Health;
using watchtower.Services.Db;
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
        private readonly RealtimeReconnectDbStore _ReconnectDb;

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
            DiscordMessageQueue discordQueue, BadHealthRepository badHealthRepository,
            RealtimeReconnectDbStore reconnectDb) {

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
            _ReconnectDb = reconnectDb;
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
        public async Task<ApiResponse<HonuHealth>> GetRealtimeHealth() {
            if (_Cache.TryGetValue("Honu.Health", out HonuHealth health) == false) {
                health = new HonuHealth();
                health.Death = _RealtimeHealthRepository.GetDeathHealth();
                health.Exp = _RealtimeHealthRepository.GetExpHealth();

                health.Reconnects = (await _ReconnectDb.GetAllByInterval(DateTime.UtcNow - TimeSpan.FromDays(1), DateTime.UtcNow))
                    .OrderByDescending(iter => iter.Timestamp).ToList();

                ServiceQueueCount c = new() { QueueName = "character_cache_queue", Count = _CharacterCache.Count() };
                ServiceQueueCount session = new() { QueueName = "session_start_queue", Count = _SessionQueue.Count() };
                ServiceQueueCount weapon = new() { QueueName = "character_weapon_stat_queue", Count = _WeaponQueue.Count() };
                ServiceQueueCount task = new() { QueueName = "task_queue", Count = _TaskQueue.Count(), Average = _TaskQueue.GetProcessTime().Average() };
                ServiceQueueCount percentile = new() { QueueName = "weapon_percentile_cache_queue", Count = _PercentileQueue.Count() };
                ServiceQueueCount discord = new() { QueueName = "discord_message_queue", Count = _DiscordQueue.Count() };

                health.Queues = new List<ServiceQueueCount>() {
                    c, session, weapon,
                    task, percentile, discord
                };

                _Cache.Set("Honu.Health", health, new MemoryCacheEntryOptions() {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(800)
                });
            }

            return ApiOk(health);
        }

        /// <summary>
        ///     Get historical reconnect data, optionally filtering by world ID
        /// </summary>
        /// <param name="start">Start period</param>
        /// <param name="end">End period</param>
        /// <param name="worldID">Optional ID of the world to filter the results to</param>
        /// <response code="200">
        ///     The response will contain all <see cref="RealtimeReconnectEntry"/>s that occured during the time period given
        ///     from <paramref name="start"/> to <paramref name="end"/>
        /// </response>
        /// <response code="400">
        ///     One of the following validation errors occured:
        ///     <ul>
        ///         <li>
        ///             <paramref name="start"/> came after or at <paramref name="end"/>
        ///         </li>
        ///         <li>
        ///             The time period between <paramref name="start"/> and <paramref name="end"/> was over 30 days
        ///         </li>
        ///     </ul>
        /// </response>
        [HttpGet("reconnects")]
        public async Task<ApiResponse<List<RealtimeReconnectEntry>>> GetReconnects([FromQuery] DateTime start, [FromQuery] DateTime end, [FromQuery] short? worldID) {
            if (start >= end) {
                return ApiBadRequest<List<RealtimeReconnectEntry>>($"{nameof(start)} cannot come after or at {nameof(end)}");
            }

            if (end - start >= TimeSpan.FromDays(31)) {
                return ApiBadRequest<List<RealtimeReconnectEntry>>($"The time period can be at most 30 days, 23 hours, 59 minutes and 59 seconds");
            }

            List<RealtimeReconnectEntry> reconnects;
            if (worldID == null) {
                reconnects = await _ReconnectDb.GetAllByInterval(start, end);
            } else {
                reconnects = await _ReconnectDb.GetByInterval(worldID.Value, start, end);
            }

            return ApiOk(reconnects);
        }

    }
}
