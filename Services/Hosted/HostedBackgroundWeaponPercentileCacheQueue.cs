using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Models.Db;
using watchtower.Services.Db;
using watchtower.Services.Queues;

namespace watchtower.Services.Hosted {

    public class HostedBackgroundWeaponPercentileCacheQueue : BackgroundService {

        private const string SERVICE_NAME = "background_weapon_pcache";

        private readonly ILogger<HostedBackgroundWeaponPercentileCacheQueue> _Logger;
        private readonly WeaponPercentileCacheQueue _Queue;

        private readonly IWeaponStatPercentileCacheDbStore _PercentileDb;

        public HostedBackgroundWeaponPercentileCacheQueue(ILogger<HostedBackgroundWeaponPercentileCacheQueue> logger,
            WeaponPercentileCacheQueue queue, IWeaponStatPercentileCacheDbStore percentDb) { 

            _Logger = logger;

            _Queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _PercentileDb = percentDb;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"Started {SERVICE_NAME}");

            while (stoppingToken.IsCancellationRequested == false) {
                try {
                    string itemID = await _Queue.Dequeue(stoppingToken);
                    _Logger.LogTrace($"generating {itemID}");

                    Stopwatch timer = Stopwatch.StartNew();
                    Stopwatch stepTimer = Stopwatch.StartNew();

                    WeaponStatPercentileCache? kd = await _PercentileDb.GenerateKd(itemID);
                    if (kd != null) {
                        kd.TypeID = PercentileCacheType.KD;
                        if (kd.ItemID == "6006064") {
                            _Logger.LogDebug($"percentile stats for {kd.ItemID}: {kd}");
                        }

                        await _PercentileDb.Upsert(itemID, kd);
                    }
                    long kdMs = stepTimer.ElapsedMilliseconds; stepTimer.Restart();

                    WeaponStatPercentileCache? kpm = await _PercentileDb.GenerateKpm(itemID);
                    if (kpm != null) {
                        kpm.TypeID = PercentileCacheType.KPM;
                        await _PercentileDb.Upsert(itemID, kpm);
                    }
                    long kpmMs = stepTimer.ElapsedMilliseconds; stepTimer.Restart();

                    WeaponStatPercentileCache? acc = await _PercentileDb.GenerateAcc(itemID);
                    if (acc != null) {
                        acc.TypeID = PercentileCacheType.ACC;
                        await _PercentileDb.Upsert(itemID, acc);
                    }
                    long accMs = stepTimer.ElapsedMilliseconds; stepTimer.Restart();

                    WeaponStatPercentileCache? hsr = await _PercentileDb.GenerateHsr(itemID);
                    if (hsr != null) {
                        hsr.TypeID = PercentileCacheType.HSR;
                        await _PercentileDb.Upsert(itemID, hsr);
                    }
                    long hsrMs = stepTimer.ElapsedMilliseconds; stepTimer.Restart();

                    WeaponStatPercentileCache? vkpm = await _PercentileDb.GenerateVKpm(itemID);
                    if (vkpm != null) {
                        vkpm.TypeID = PercentileCacheType.VKPM;
                        await _PercentileDb.Upsert(itemID, vkpm);
                    }
                    long vkpmMs = stepTimer.ElapsedMilliseconds; stepTimer.Restart();

                    _Logger.LogInformation($"generated percentile data for {itemID} in {timer.ElapsedMilliseconds}ms "
                        + $"[KD={kdMs}ms] [KPM={kpmMs}ms] [ACC={accMs}ms] [HSR={hsrMs}ms] [VKPM={vkpmMs}ms]");

                    _Queue.AddProcessTime(timer.ElapsedMilliseconds);

                } catch (Exception ex) when (stoppingToken.IsCancellationRequested == false) {
                    _Logger.LogError(ex, $"{SERVICE_NAME}> Error while generated weapon percentiles");
                } catch (Exception) when (stoppingToken.IsCancellationRequested == true) {
                    _Logger.LogInformation($"Stopping {SERVICE_NAME} with {_Queue.Count()} left");
                }
            }
        }


    }
}
