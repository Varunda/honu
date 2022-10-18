using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Models;
using watchtower.Models.CharacterViewer.WeaponStats;
using watchtower.Models.Db;
using watchtower.Services.Db;
using watchtower.Services.Queues;

namespace watchtower.Services.Hosted {

    public class HostedBackgroundWeaponStatQueue : BackgroundService {

        private const string SERVICE_NAME = "background_weapon_update";
        private const int BUCKET_COUNT = 100;

        private readonly ILogger<HostedBackgroundWeaponStatQueue> _Logger;
        private readonly CharacterWeaponStatDbStore _WeaponStatDb;
        private readonly WeaponStatSnapshotDbStore _WeaponStatSnapshotDb;
        private readonly IWeaponStatPercentileCacheDbStore _WeaponPercentileDb;
        private readonly WeaponStatBucketDbStore _BucketDb;

        private readonly WeaponUpdateQueue _Queue;

        private readonly Dictionary<long, DateTime> _LastUpdated = new();

        public HostedBackgroundWeaponStatQueue(ILogger<HostedBackgroundWeaponStatQueue> logger,
            CharacterWeaponStatDbStore weaponStatDb, WeaponUpdateQueue queue,
            WeaponStatSnapshotDbStore weaponStatSnapshotDb, IWeaponStatPercentileCacheDbStore weaponPercentileDb,
            WeaponStatBucketDbStore bucketDb) {

            _Logger = logger;
            _WeaponStatDb = weaponStatDb;
            _Queue = queue;
            _WeaponStatSnapshotDb = weaponStatSnapshotDb;
            _WeaponPercentileDb = weaponPercentileDb;
            _BucketDb = bucketDb;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"{SERVICE_NAME}> started");

            while (stoppingToken.IsCancellationRequested == false) {
                try {
                    long itemID = await _Queue.Dequeue(stoppingToken);

                    if (_LastUpdated.TryGetValue(itemID, out DateTime lastUpdatedAt) == true) {
                        if ((lastUpdatedAt - DateTime.UtcNow) <= TimeSpan.FromHours(2)) {
                            _Logger.LogInformation($"{SERVICE_NAME}> Last updated {itemID} at {lastUpdatedAt:u}, skipping");
                            continue;
                        }
                    }

                    Stopwatch timer = Stopwatch.StartNew();
                    bool errored = false;
                    DateTime timestamp = DateTime.UtcNow;

                    _Logger.LogDebug($"{SERVICE_NAME}> Updating stats for {itemID}");
                    List<WeaponStatEntry> stats = await _WeaponStatDb.GetByItemID($"{itemID}", 0);

                    // Only include stats from people who have auaraxed the gun
                    List<WeaponStatEntry> filtered = stats.Where(iter => iter.Kills > 1159).ToList();
                    if (filtered.Count < 100) { // But if there's not enough, expand the sample
                        filtered = stats.Where(iter => iter.Kills > 50).ToList();
                    }

                    _Logger.LogTrace($"{SERVICE_NAME}> Loaded {stats.Count} entries for {itemID}");

                    if (stats.Count == 0) {
                        continue;
                    }

                    try {
                        await AddSnapshot((int)itemID, stats, stoppingToken);
                    } catch (Exception ex) {
                        _Logger.LogError(ex, $"error updating snapshot for weapon id {itemID}");
                        errored = true;
                    }

                    try {
                        await GeneratePercentiles((int)itemID, filtered, stoppingToken);
                    } catch (Exception ex) {
                        _Logger.LogError(ex, $"error generating percentiles for weapon id {itemID}");
                        errored = true;
                    }

                    try {
                        await GenerateBuckets((int)itemID, filtered, timestamp, stoppingToken);
                    } catch (Exception ex) {
                        _Logger.LogError(ex, $"error generating buckets for weapon id {itemID}");
                        errored = true;
                    }

                    if (errored == false) {
                        _LastUpdated[itemID] = DateTime.UtcNow;
                    }

                    string msg = $"Updated weapon stats for {itemID} in {timer.ElapsedMilliseconds}ms, used {stats.Count} entries";

                    if (errored == true) {
                        msg = "Completed with errors: " + msg;
                        _Logger.LogWarning(msg);
                    } else {
                        _Logger.LogInformation(msg);
                    }

                    _Queue.AddProcessTime(timer.ElapsedMilliseconds);
                } catch (Exception ex) {
                    _Logger.LogError(ex, "error updating weapon stats");
                }
            }
        }

        private async Task GenerateTop(int itemID, List<WeaponStatEntry> stats, CancellationToken stoppingToken) {

            Dictionary<string, WeaponStatEntry> map = stats.ToDictionary(iter => iter.CharacterID);

            List<WeaponStatEntry> topKD = stats.OrderByDescending(iter => iter.KillDeathRatio).ToList();




        }

        private async Task GenerateBuckets(int itemID, List<WeaponStatEntry> stats, DateTime timestamp, CancellationToken stoppingToken) {
            List<WeaponStatBucket> kd = GetBuckets(stats.Select(iter => iter.KillDeathRatio).ToList(), BUCKET_COUNT);
            foreach (WeaponStatBucket b in kd) { b.TypeID = PercentileCacheType.KD; b.Timestamp = timestamp; }
            await _BucketDb.SetByItemID(itemID, PercentileCacheType.KD, kd);

            List<WeaponStatBucket> kpm = GetBuckets(stats.Select(iter => iter.KillsPerMinute).ToList(), BUCKET_COUNT);
            foreach (WeaponStatBucket b in kpm) { b.TypeID = PercentileCacheType.KPM; b.Timestamp = timestamp; }
            await _BucketDb.SetByItemID(itemID, PercentileCacheType.KPM, kpm);

            List<WeaponStatBucket> acc = GetBuckets(stats.Select(iter => iter.Accuracy * 100).ToList(), BUCKET_COUNT);
            foreach (WeaponStatBucket b in acc) { b.TypeID = PercentileCacheType.ACC; b.Timestamp = timestamp; }
            await _BucketDb.SetByItemID(itemID, PercentileCacheType.ACC, acc);

            List<WeaponStatBucket> hsr = GetBuckets(stats.Select(iter => iter.HeadshotRatio * 100).ToList(), BUCKET_COUNT);
            foreach (WeaponStatBucket b in hsr) { b.TypeID = PercentileCacheType.HSR; b.Timestamp = timestamp; }
            await _BucketDb.SetByItemID(itemID, PercentileCacheType.HSR, hsr);

            List<WeaponStatBucket> vkpm = GetBuckets(stats.Select(iter => iter.VehicleKillsPerMinute).ToList(), BUCKET_COUNT);
            foreach (WeaponStatBucket b in vkpm) { b.TypeID = PercentileCacheType.VKPM; b.Timestamp = timestamp; }
            await _BucketDb.SetByItemID(itemID, PercentileCacheType.VKPM, vkpm);
        }

        private List<WeaponStatBucket> GetBuckets(List<double> values, int bucketCount) {
            if (values.Count == 0) {
                return new List<WeaponStatBucket>();
            }

            List<double> sorted = values.OrderBy(iter => iter).ToList();

            int mi = MedianIndex(0, values.Count);
            double median = Median(sorted, 0, values.Count);

            //int q1i = MedianIndex(0, m2i);
            double q1 = Median(sorted, 0, mi);
            //int q3i = m2i + MedianIndex(m2i, values.Count);
            double q3 = Median(sorted, mi, values.Count);

            double iqr = q3 - q1;

            // Exclude data outside 8 times the inter-quartile range (iqr)
            sorted = sorted.Where(iter => iter < q3 + (iqr * 8)).ToList();

            if (sorted.Count == 0) {
                return new List<WeaponStatBucket>();
            }

            double max = sorted.Last();
            double min = sorted.First();

            //_Logger.LogInformation($"{min}/0 {q1}/{q1i} {median2}/{m2i} {q3}/{q3i} {max}/{values.Count - 1}, iqr = {iqr}");

            double bucketWidth = (max - min) / bucketCount;

            List<WeaponStatBucket> buckets = new();

            WeaponStatBucket iter = new() {
                Start = min,
                Width = bucketWidth,
                Count = 0
            };

            for (double i = min; i <= max; i += bucketWidth) {
                int count = sorted.Where(iter => (iter >= i && iter < i + bucketWidth)).Count();

                WeaponStatBucket b = new() {
                    Start = i,
                    Width = bucketWidth,
                    Count = count
                };

                buckets.Add(b);

                //_Logger.LogInformation($"Bucket {i} - {i + bucketWidth} = {count}");
            }

            return buckets;
        }

        private async Task AddSnapshot(int itemID, List<WeaponStatEntry> stats, CancellationToken stoppingToken) {
            if (stats.Count == 0) {
                return;
            }

            WeaponStatSnapshot snapshot = new();
            snapshot.ItemID = itemID;
            snapshot.Timestamp = DateTime.UtcNow;

            foreach (WeaponStatEntry entry in stats) {
                if (entry.Kills <= 100) {
                    continue;
                }

                snapshot.Kills += entry.Kills;
                snapshot.Deaths += entry.Deaths;
                snapshot.Shots += entry.Shots;
                snapshot.ShotsHit += entry.ShotsHit;
                snapshot.VehicleKills += entry.VehicleKills;
                snapshot.SecondsWith += entry.SecondsWith;
                snapshot.Headshots += entry.Headshots;
                snapshot.Users += 1;
            }

            await _WeaponStatSnapshotDb.Insert(snapshot, stoppingToken);
        }

        private async Task GeneratePercentiles(int itemID, List<WeaponStatEntry> stats, CancellationToken stoppingToken) {
            WeaponStatPercentileCache kd = new();
            kd.ItemID = $"{itemID}";
            kd.TypeID = PercentileCacheType.KD;
            CopyPercentiles(kd, stats.Select(iter => iter.KillDeathRatio).ToList());
            await _WeaponPercentileDb.Upsert(kd.ItemID, kd);

            WeaponStatPercentileCache kpm = new();
            kpm.ItemID = $"{itemID}";
            kpm.TypeID = PercentileCacheType.KPM;
            CopyPercentiles(kpm, stats.Select(iter => iter.KillsPerMinute).ToList());
            await _WeaponPercentileDb.Upsert(kpm.ItemID, kpm);

            WeaponStatPercentileCache acc = new();
            acc.ItemID = $"{itemID}";
            acc.TypeID = PercentileCacheType.ACC;
            CopyPercentiles(acc, stats.Select(iter => iter.Accuracy).ToList());
            await _WeaponPercentileDb.Upsert(acc.ItemID, acc);

            WeaponStatPercentileCache hsr = new();
            hsr.ItemID = $"{itemID}";
            hsr.TypeID = PercentileCacheType.HSR;
            CopyPercentiles(hsr, stats.Select(iter => iter.HeadshotRatio).ToList());
            await _WeaponPercentileDb.Upsert(hsr.ItemID, hsr);

            WeaponStatPercentileCache vkpm = new();
            vkpm.ItemID = $"{itemID}";
            vkpm.TypeID = PercentileCacheType.VKPM;
            CopyPercentiles(vkpm, stats.Select(iter => iter.VehicleKillsPerMinute).ToList());
            await _WeaponPercentileDb.Upsert(vkpm.ItemID, vkpm);
        }

        private static double Median(List<double> list, int lowerBound, int upperBound) {
            int median = MedianIndex(lowerBound, upperBound) + lowerBound;
            if (median % 2 == 0) {
                return list[median];
            }
            return (list[median + 1] + list[median]) / 2;
        }

        private static int MedianIndex(int lowerBound, int upperBound) {
            int n = upperBound - lowerBound + 1;
            n = (n + 1) / 2 - 1;
            return n;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        ///     Used https://stackoverflow.com/questions/8137391/percentile-calculation
        /// </remarks>
        /// <param name="values"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        private double Percentile(List<double> values, double percent) {
            values.Sort();

            int count = values.Count;
            double index = (count - 1) * percent + 1;

            if (index == 1d) {
                return values[0];
            }

            if (count == index) {
                return values[count - 1];
            }

            int offset = (int)index;
            double difference = index - offset;

            return values[offset - 1] + difference * (values[offset] - values[offset - 1]);
        }

        private void CopyPercentiles(WeaponStatPercentileCache entry, List<double> values) {
            entry.Q0 = Percentile(values, 0d);
            entry.Q5 = Percentile(values, 0.05d);
            entry.Q10 = Percentile(values, 0.10d);
            entry.Q15 = Percentile(values, 0.15d);
            entry.Q20 = Percentile(values, 0.20d);
            entry.Q25 = Percentile(values, 0.25d);
            entry.Q30 = Percentile(values, 0.30d);
            entry.Q35 = Percentile(values, 0.35d);
            entry.Q40 = Percentile(values, 0.40d);
            entry.Q45 = Percentile(values, 0.45d);
            entry.Q50 = Percentile(values, 0.50d);
            entry.Q55 = Percentile(values, 0.55d);
            entry.Q60 = Percentile(values, 0.60d);
            entry.Q65 = Percentile(values, 0.65d);
            entry.Q70 = Percentile(values, 0.70d);
            entry.Q75 = Percentile(values, 0.75d);
            entry.Q80 = Percentile(values, 0.80d);
            entry.Q85 = Percentile(values, 0.85d);
            entry.Q90 = Percentile(values, 0.90d);
            entry.Q95 = Percentile(values, 0.95d);
            entry.Q100 = Percentile(values, 1.00d);
        }

    }
}
