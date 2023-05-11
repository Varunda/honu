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
        private readonly WeaponUpdateQueue _WeaponUpdateQueue;
        private readonly SessionEndQueue _SessionEndQueue;
        private readonly WrappedGenerationQueue _WrappedQueue;
        private readonly FacilityControlEventProcessQueue _FacilityControlQueue;

        public HealthApiController(ILogger<HealthApiController> logger, IMemoryCache cache,
            CensusRealtimeHealthRepository realtimeHealthRepository, CharacterCacheQueue characterCache,
            SessionStarterQueue sessionQueue, CharacterUpdateQueue weaponQueue,
            CensusRealtimeEventQueue taskQueue, WeaponPercentileCacheQueue percentileQueue,
            DiscordMessageQueue discordQueue, BadHealthRepository badHealthRepository,
            RealtimeReconnectDbStore reconnectDb, WeaponUpdateQueue weaponUpdateQueue,
            SessionEndQueue sessionEndQueue, WrappedGenerationQueue wrappedQueue,
            FacilityControlEventProcessQueue facilityControlQueue) {

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
            _WeaponUpdateQueue = weaponUpdateQueue;
            _SessionEndQueue = sessionEndQueue;
            _WrappedQueue = wrappedQueue;
            _FacilityControlQueue = facilityControlQueue;
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
                health.Timestamp = DateTime.UtcNow;
                health.Death = _RealtimeHealthRepository.GetDeathHealth().OrderBy(iter => iter.WorldID).ToList();
                health.Exp = _RealtimeHealthRepository.GetExpHealth().OrderBy(iter => iter.WorldID).ToList();

                health.Reconnects = (await _ReconnectDb.GetAllByInterval(DateTime.UtcNow - TimeSpan.FromDays(1), DateTime.UtcNow))
                    .OrderByDescending(iter => iter.Timestamp).ToList();

                ServiceQueueCount c = _MakeCount("character_cache_queue", _CharacterCache);
                ServiceQueueCount session = _MakeCount("session_start_queue", _SessionQueue);
                ServiceQueueCount weapon = _MakeCount("character_weapon_stat_queue", _WeaponQueue);
                ServiceQueueCount task = _MakeCount("task_queue", _TaskQueue);
                ServiceQueueCount percentile = _MakeCount("weapon_percentile_cache_queue", _PercentileQueue);
                ServiceQueueCount discord = _MakeCount("discord_message_queue", _DiscordQueue);
                ServiceQueueCount weaponUpdate = _MakeCount("weapon_update_queue", _WeaponUpdateQueue);
                ServiceQueueCount sessionEnd = _MakeCount("session_end_queue", _SessionEndQueue);
                ServiceQueueCount wrapped = _MakeCount("wrapped_generation", _WrappedQueue);
                ServiceQueueCount facility = _MakeCount("facility_control", _FacilityControlQueue);

                health.Queues = new List<ServiceQueueCount>() {
                    task, session, c, weapon, weaponUpdate, percentile,
                    discord, sessionEnd, wrapped, facility
                };

                _Cache.Set("Honu.Health", health, new MemoryCacheEntryOptions() {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(800)
                });
            }

            return ApiOk(health);
        }

        private ServiceQueueCount _MakeCount(string name, IProcessQueue queue) {
            ServiceQueueCount c = new() {
                QueueName = name,
                Count = queue.Count() ,
                Processed = queue.Processed()
            };

            List<long> times = queue.GetProcessTime();
            if (times.Count > 0) {
                c.Average = times.Average();
                c.Min = times.Min();
                c.Max = times.Max();

                List<long> sorted = times.OrderBy(i => i).ToList();
                int mid = sorted.Count / 2;
                if (sorted.Count % 2 == 0) {
                    c.Median = (sorted.ElementAt(mid - 1) + sorted.ElementAt(mid)) / 2;
                } else {
                    c.Median = sorted.ElementAt(mid);
                }
            }

            return c;
        }

        /// <summary>
        ///     Get historical reconnect data, optionally filtering by world ID
        /// </summary>
        /// <remarks>
        ///     The time span between <paramref name="start"/> and <paramref name="end"/> cannot be more
        ///     than 31 days
        /// </remarks>
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
