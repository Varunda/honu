using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Tracking;
using watchtower.Models;
using watchtower.Models.Api;
using watchtower.Models.Db;
using watchtower.Models.Queues;
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
        private readonly DiscordMessageQueue _DiscordQueue;
        private readonly LogoutUpdateBuffer _LogoutQueue;
        private readonly WeaponUpdateQueue _WeaponUpdateQueue;
        private readonly PriorityCharacterUpdateQueue _CharacterPriorityQueue;

        public ServiceApiController(ILogger<ServiceApiController> logger,
            IServiceHealthMonitor mon,
            CharacterCacheQueue charQueue, SessionStarterQueue session,
            CharacterUpdateQueue weapon, CensusRealtimeEventQueue task,
            DiscordMessageQueue discord, LogoutUpdateBuffer logoutQueue,
            WeaponUpdateQueue weaponUpdateQueue, PriorityCharacterUpdateQueue characterPriorityQueue) {

            _Logger = logger;

            _ServiceHealthMonitor = mon;

            _CharacterCache = charQueue;
            _SessionQueue = session;
            _WeaponQueue = weapon;
            _TaskQueue = task;
            _DiscordQueue = discord;
            _LogoutQueue = logoutQueue;
            _WeaponUpdateQueue = weaponUpdateQueue;
            _CharacterPriorityQueue = characterPriorityQueue;
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
            ServiceQueueCount discord = new() { QueueName = "discord_message_queue", Count = _DiscordQueue.Count() };
            //ServiceQueueCount logout = new() { QueueName = "logout_buffer_queue", Count = 0 };

            List<ServiceQueueCount> counts = new() {
                c, session, weapon,
                task, discord
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

        /// <summary>
        ///     Get a list of weapon IDs that represent the order the stats will be updated in
        /// </summary>
        /// <response code="200">
        ///     The response will contain a list of longs, with the order representing what place in queue
        ///     that weapon is to be updated. The weapon currently being updated will be at index 0, with
        ///     all weapons in queue at index 1 and beyond
        /// </response>
        [HttpGet("weapon_update_queue")]
        public ApiResponse<List<long>> GetWeaponUpdateQueue() {
            List<long> queued = _WeaponUpdateQueue.ToList();

            long? mostRecent = _WeaponUpdateQueue.GetMostRecentDequeued();
            if (mostRecent != null) {
                queued.Insert(0, mostRecent.Value);
            }

            return ApiOk(queued);
        }

        /// <summary>
        ///     get a list of character IDs that are pending an update in the priority queue
        /// </summary>
        /// <response code="200">
        ///     The response will contain a list of strings, which represent a character ID.
        ///     The order of the list matches the order in which these characters will be updated,
        /// </response>
        [HttpGet("character_priority_queue")]
        public ApiResponse<List<string>> GetPriorityCharacterUpdateQueue() {
            List<CharacterUpdateQueueEntry> queued = _CharacterPriorityQueue.ToList();

            return ApiOk(queued.Select(iter => iter.CharacterID).ToList());
        }

        /// <summary>
        ///     get a list of sessions that are being started
        /// </summary>
        /// <response code="200">
        ///     the response will contain a list of <see cref="CharacterSessionStartQueueEntry"/>s
        /// </response>
        [HttpGet("session_start")]
        public ApiResponse<List<CharacterSessionStartQueueEntry>> GetSessionStartQueue() {
            List<CharacterSessionStartQueueEntry> queued = _SessionQueue.ToList();

            return ApiOk(queued);
        }

    }
}
