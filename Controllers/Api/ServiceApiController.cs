using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Models.Api;
using watchtower.Services;
using watchtower.Services.Queues;

namespace watchtower.Controllers.Api {

    /// <summary>
    ///     Endpoints about services hosted in Honu
    /// </summary>
    [ApiController]
    [Route("/api/services")]
    public class ServiceApiController : ApiControllerBase {

        private readonly ILogger<ServiceApiController> _Logger;

        private readonly IServiceHealthMonitor _ServiceHealthMonitor;

        private readonly CharacterCacheQueue _CharacterCache;
        private readonly SessionStarterQueue _SessionQueue;
        private readonly CharacterUpdateQueue _WeaponQueue;
        private readonly CensusRealtimeEventQueue _TaskQueue;
        private readonly WeaponPercentileCacheQueue _PercentileQueue;
        private readonly DiscordMessageQueue _DiscordQueue;
        private readonly LogoutUpdateBuffer _LogoutQueue;

        public ServiceApiController(ILogger<ServiceApiController> logger,
            IServiceHealthMonitor mon,
            CharacterCacheQueue charQueue, SessionStarterQueue session,
            CharacterUpdateQueue weapon, CensusRealtimeEventQueue task,
            WeaponPercentileCacheQueue percentile, DiscordMessageQueue discord,
            LogoutUpdateBuffer logoutQueue) {

            _Logger = logger;

            _ServiceHealthMonitor = mon;

            _CharacterCache = charQueue;
            _SessionQueue = session;
            _WeaponQueue = weapon;
            _TaskQueue = task;
            _PercentileQueue = percentile;
            _DiscordQueue = discord;
            _LogoutQueue = logoutQueue;
        }

        /// <summary>
        ///     Get the queue length for the background queues hosted in Honu
        /// </summary>
        /// <remarks>
        ///     Honu has several background queues that perform DB updates in the background. This endpoint will get
        ///     how many things are queued in each of these background queues
        /// </remarks>
        [HttpGet("queue_count")]
        public ApiResponse<List<ServiceQueueCount>> GetQueueCounts() {
            ServiceQueueCount c = new() { QueueName = "character_cache_queue", Count = _CharacterCache.Count() };
            ServiceQueueCount session = new() { QueueName = "session_start_queue", Count = _SessionQueue.Count() };
            ServiceQueueCount weapon = new() { QueueName = "character_weapon_stat_queue", Count = _WeaponQueue.Count() };
            ServiceQueueCount task = new() { QueueName = "task_queue", Count = _TaskQueue.Count() };
            ServiceQueueCount percentile = new() { QueueName = "weapon_percentile_cache_queue", Count = _PercentileQueue.Count() };
            ServiceQueueCount discord = new() { QueueName = "discord_message_queue", Count = _DiscordQueue.Count() };
            //ServiceQueueCount logout = new() { QueueName = "logout_buffer_queue", Count = 0 };

            List<ServiceQueueCount> counts = new() {
                c, session, weapon,
                task, percentile, discord
            };

            return ApiOk(counts);
        }

        /// <summary>
        ///     Get a summary of the services hosted in Honu
        /// </summary>
        [HttpGet]
        public ApiResponse<List<ServiceHealthEntry>> GetServices() {
            List<string> services = _ServiceHealthMonitor.GetServices();

            List<ServiceHealthEntry> entries = new List<ServiceHealthEntry>(services.Count);
            
            foreach (string service in services) {
                ServiceHealthEntry? entry = _ServiceHealthMonitor.Get(service);
                if (entry != null) {
                    entries.Add(entry);
                }
            }

            return ApiOk(entries);
        }

    }
}
