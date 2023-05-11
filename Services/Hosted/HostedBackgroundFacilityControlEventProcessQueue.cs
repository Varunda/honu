using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Events;
using watchtower.Models.Queues;
using watchtower.Services.Db;
using watchtower.Services.Queues;

namespace watchtower.Services.Hosted {

    public class HostedBackgroundFacilityControlEventProcessQueue : BackgroundService {

        private const string SERVICE_NAME = "facility_control_queue";

        private readonly ILogger<HostedBackgroundFacilityControlEventProcessQueue> _Logger;
        private readonly FacilityControlEventProcessQueue _Queue;

        private readonly FacilityControlDbStore _ControlDb;
        private readonly FacilityPlayerControlDbStore _FacilityPlayerDb;

        public HostedBackgroundFacilityControlEventProcessQueue(ILogger<HostedBackgroundFacilityControlEventProcessQueue> logger,
            FacilityControlEventProcessQueue queue, FacilityControlDbStore controlDb,
            FacilityPlayerControlDbStore facilityPlayerDb) {

            _Logger = logger;
            _Queue = queue;

            _ControlDb = controlDb;
            _FacilityPlayerDb = facilityPlayerDb;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"started {SERVICE_NAME}");

            Stopwatch timer = Stopwatch.StartNew();
            while (stoppingToken.IsCancellationRequested == false) {
                // get the queue entry
                FacilityControlEventQueueEntry? entry = null;
                try {
                    entry = await _Queue.Dequeue(stoppingToken);
                } catch (Exception ex) {
                    _Logger.LogError(ex, $"error loading {nameof(FacilityControlEventQueueEntry)}");
                    continue;
                }

                if (entry == null) {
                    _Logger.LogWarning($"missing entry?");
                    continue;
                }

                timer.Restart();
                // process the queue entry
                try {
                    if (entry.Event.Players != entry.Participants.Count) {
                        _Logger.LogWarning($"Mismatch between event players ({entry.Event.Players}) and participant count ({entry.Participants.Count})");
                    }

                    long ID = await _ControlDb.Insert(entry.Event);
                    entry.Event.ID = ID;

                    foreach (PlayerControlEvent playerControl in entry.Participants) {
                        await _FacilityPlayerDb.Insert(ID, playerControl);
                    }
                    
                    _Logger.LogDebug($"processed facility control event {ID}, "
                        + $"[Players={entry.Event.Players}] [Timestamp={entry.Event.Timestamp:u}] [OutfitID={entry.Event.OutfitID}] [FacilityID={entry.Event.FacilityID}]");
                } catch (Exception ex) {
                    _Logger.LogError(ex, $"failed to perform DB operations for {entry.Event.ID}");
                }
                _Queue.AddProcessTime(timer.ElapsedMilliseconds);
            }
        }

    }
}
