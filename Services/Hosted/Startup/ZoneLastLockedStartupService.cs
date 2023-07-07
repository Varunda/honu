using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code;
using watchtower.Models;
using watchtower.Models.Db;
using watchtower.Services.Db;

namespace watchtower.Services.Hosted.Startup {

    public class ZoneLastLockedStartupService : BackgroundService {

        private readonly ILogger<ZoneLastLockedStartupService> _Logger;
        private readonly ContinentLockDbStore _ContinentLockDb;

        public ZoneLastLockedStartupService(ILogger<ZoneLastLockedStartupService> logger,
            ContinentLockDbStore continentLockDb) {

            _Logger = logger;
            _ContinentLockDb = continentLockDb;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            try {
                Stopwatch timer = Stopwatch.StartNew();

                List<ContinentLockEntry> locks = await _ContinentLockDb.GetAll();

                foreach (ContinentLockEntry entry in locks) {
                    lock (ZoneStateStore.Get().Zones) {
                        ZoneState? state = ZoneStateStore.Get().GetZone(entry.WorldID, entry.ZoneID);

                        if (state == null) {
                            state = new ZoneState() {
                                WorldID = entry.WorldID,
                                ZoneID = entry.ZoneID
                            };
                        }

                        state.LastLocked = entry.Timestamp;

                        ZoneStateStore.Get().SetZone(entry.WorldID, entry.ZoneID, state);
                    }
                }

                _Logger.LogInformation($"Performed last lock update in {timer.ElapsedMilliseconds}ms");

            } catch (Exception ex) {
                _Logger.LogError(ex, $"uncaught error while executing zone last locked startup service");
            }
        }

    }
}
