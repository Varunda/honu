using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Hosted.Startup {

    public class BackCreateAlertStartupService : BackgroundService {

        private readonly ILogger<BackCreateAlertStartupService> _Logger;
        private readonly AlertDbStore _AlertDb;

        private const string FILE_PATH = "./ps2alerts.json";
        private const string SERVICE_NAME = "back_create_alerts";

        public BackCreateAlertStartupService(ILogger<BackCreateAlertStartupService> logger,
            AlertDbStore alertDb) {

            _Logger = logger;
            _AlertDb = alertDb;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            try {
                string input = await File.ReadAllTextAsync(FILE_PATH, stoppingToken);

                JToken token = JToken.Parse(input);

                JEnumerable<JToken> children = token.Children();
                _Logger.LogInformation($"{SERVICE_NAME}> Creating {children.Count()} alerts");

                DateTime minimum = new DateTime(2021, 7, 9, 5, 40, 0); //"2021-07-09T05:40");
                _Logger.LogDebug($"{SERVICE_NAME}> Date minimum: {minimum:u}");

                int total = 0;
                int skipped = 0;
                int done = 0;

                foreach (JToken iter in token.Children()) {
                    ++total;

                    short worldID = iter.GetRequiredInt16("world_id");
                    if (worldID == 1000 || worldID == 2000) {
                        ++skipped;
                        continue;
                    }

                    DateTime timestamp = iter.GetRequiredDateTime("timestamp");
                    if (timestamp < minimum) {
                        ++skipped;
                        continue;
                    }
                    int instanceID = iter.GetRequiredInt32("instance_id");

                    if (await _AlertDb.GetByInstanceID(instanceID, worldID) != null) {
                        _Logger.LogTrace($"{SERVICE_NAME}> Skipping alert already added {worldID}-{instanceID}");
                        ++skipped;
                        continue;
                    }

                    uint zoneID = iter.GetUInt32("zone_id");
                    int duration = iter.GetRequiredInt32("duration");
                    int alertID = iter.GetRequiredInt32("alert_id");

                    PsAlert alert = new PsAlert();
                    alert.WorldID = worldID;
                    alert.ZoneID = zoneID;
                    alert.Duration = duration;
                    alert.AlertID = alertID;
                    alert.InstanceID = instanceID;
                    alert.Timestamp = timestamp;

                    await _AlertDb.Insert(alert);

                    ++done;
                }

                _Logger.LogInformation($"{SERVICE_NAME}> Done. Skipped {skipped}/{total}, did {done}/{total}");
            } catch (Exception ex) {
                _Logger.LogError(ex, "failed to back create alerts");
            }
        }

    }
}
