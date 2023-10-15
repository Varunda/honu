using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Code.ExtensionMethods;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.CharacterViewer.WeaponStats;
using watchtower.Models.Db;
using watchtower.Services.Db;
using watchtower.Services.Queues;
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted {

    public class HostedBackgroundWeaponStatQueue : BackgroundService {

        private const string SERVICE_NAME = "background_weapon_update";
        private const int BUCKET_COUNT = 100;

        private readonly ILogger<HostedBackgroundWeaponStatQueue> _Logger;
        private readonly CharacterWeaponStatDbStore _WeaponStatDb;
        private readonly WeaponStatSnapshotDbStore _WeaponStatSnapshotDb;
        private readonly IWeaponStatPercentileCacheDbStore _WeaponPercentileDb;
        private readonly CharacterRepository _CharacterRepository;
        private readonly WeaponStatBucketDbStore _BucketDb;
        private readonly WeaponStatTopDbStore _WeaponTopDb;
        private readonly IServiceHealthMonitor _ServiceHealthMonitor;

        private readonly WeaponUpdateQueue _Queue;

        private readonly Dictionary<long, DateTime> _LastUpdated = new();

        public HostedBackgroundWeaponStatQueue(ILogger<HostedBackgroundWeaponStatQueue> logger,
            CharacterWeaponStatDbStore weaponStatDb, WeaponUpdateQueue queue,
            WeaponStatSnapshotDbStore weaponStatSnapshotDb, IWeaponStatPercentileCacheDbStore weaponPercentileDb,
            WeaponStatBucketDbStore bucketDb, CharacterRepository characterRepository,
            WeaponStatTopDbStore weaponTopDb, IServiceHealthMonitor serviceHealthMonitor)
        {

            _Logger = logger;
            _WeaponStatDb = weaponStatDb;
            _Queue = queue;
            _WeaponStatSnapshotDb = weaponStatSnapshotDb;
            _WeaponPercentileDb = weaponPercentileDb;
            _BucketDb = bucketDb;
            _CharacterRepository = characterRepository;
            _WeaponTopDb = weaponTopDb;
            _ServiceHealthMonitor = serviceHealthMonitor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"{SERVICE_NAME}> started");

            while (stoppingToken.IsCancellationRequested == false) {
                try {

                    ServiceHealthEntry? entry = _ServiceHealthMonitor.Get(SERVICE_NAME);
                    if (entry == null) {
                        entry = new ServiceHealthEntry() {
                            Name = SERVICE_NAME
                        };
                    }

                    // Useful for debugging on my laptop which can't handle running the queries and run vscode at the same time
                    if (entry.Enabled == false) {
                        await Task.Delay(1000, stoppingToken);
                        continue;
                    }

                    long itemID = await _Queue.Dequeue(stoppingToken);

                    if (itemID == 0) {
                        continue;
                    }

                    if (_LastUpdated.TryGetValue(itemID, out DateTime lastUpdatedAt) == true) {
                        TimeSpan diff = DateTime.UtcNow - lastUpdatedAt;
                        if (diff.Duration() <= TimeSpan.FromHours(2)) {
                            //_Logger.LogInformation($"{SERVICE_NAME}> Last updated {itemID} at {lastUpdatedAt:u}, diff {diff}, skipping");
                            continue;
                        }
                    }

                    Stopwatch timer = Stopwatch.StartNew();
                    Stopwatch stepTimer = Stopwatch.StartNew();
                    bool errored = false;
                    DateTime timestamp = DateTime.UtcNow;

                    _Logger.LogDebug($"{SERVICE_NAME}> Updating stats for {itemID}");
                    List<WeaponStatEntry> stats = await _WeaponStatDb.GetByItemID($"{itemID}", 0);

                    long loadStatsMs = stepTimer.ElapsedMilliseconds; stepTimer.Restart();

                    // Only include stats from people who have auaraxed the gun
                    List<WeaponStatEntry> filtered = stats.Where(iter => iter.Kills > 1159).ToList();
                    if (filtered.Count < 100) { // But if there's not enough, expand the sample
                        filtered = stats.Where(iter => iter.Kills > 50).ToList();
                    }
                    if (filtered.Count < 100) { // if less than 100 users have 50 kills, just include everyone
                        filtered = stats;
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
                    long snapshotMs = stepTimer.ElapsedMilliseconds; stepTimer.Restart();

                    try {
                        await GeneratePercentiles((int)itemID, filtered, stoppingToken);
                    } catch (Exception ex) {
                        _Logger.LogError(ex, $"error generating percentiles for weapon id {itemID}");
                        errored = true;
                    }
                    long percentileMs = stepTimer.ElapsedMilliseconds; stepTimer.Restart();

                    try {
                        await GenerateBuckets((int)itemID, filtered, timestamp, stoppingToken);
                    } catch (Exception ex) {
                        _Logger.LogError(ex, $"error generating buckets for weapon id {itemID}");
                        errored = true;
                    }
                    long bucketMs = stepTimer.ElapsedMilliseconds; stepTimer.Restart();

                    try {
                        await GenerateTop((int)itemID, stats, timestamp, stoppingToken);
                    } catch (Exception ex) {
                        _Logger.LogError(ex, $"error generating top for weapon id {itemID}");
                        errored = true;
                    }
                    long topMs = stepTimer.ElapsedMilliseconds; stepTimer.Restart();

                    if (errored == false) {
                        _LastUpdated[itemID] = DateTime.UtcNow;
                    }

                    string msg = $"Updated weapon stats for {itemID} in {timer.ElapsedMilliseconds}ms, used {stats.Count} entries; ";
                    msg += $"db load: {loadStatsMs}ms, snapshot: {snapshotMs}ms, percentile: {percentileMs}ms, bucket: {bucketMs}ms, top: {topMs}ms";

                    if (errored == true) {
                        msg = "Completed with errors: " + msg;
                        _Logger.LogWarning(msg);
                    } else {
                        _Logger.LogInformation(msg);
                    }

                    _Queue.AddProcessTime(timer.ElapsedMilliseconds);

                    _ServiceHealthMonitor.Set(SERVICE_NAME, entry);
                } catch (Exception ex) {
                    _Logger.LogError(ex, "error updating weapon stats");
                }
            }
        }

        /// <summary>
        ///     Generate the top characters in various stats
        /// </summary>
        /// <param name="itemID">ID of the item</param>
        /// <param name="stats">Stats to generate the top from</param>
        /// <param name="timestamp">When these stats were pulled from the DB</param>
        /// <param name="stoppingToken">Stopping token</param>
        private async Task GenerateTop(int itemID, List<WeaponStatEntry> stats, DateTime timestamp, CancellationToken stoppingToken) {
            List<string> charIDs = stats.Select(iter => iter.CharacterID).Distinct().ToList();
            Dictionary<string, PsCharacter> chars = (await _CharacterRepository.GetByIDs(charIDs, CensusEnvironment.PC, fast: true))
                .ToDictionaryDistinct(iter => iter.ID);

            List<WeaponStatTop> all = new();

            HashSet<string> missingCharacters = new();

            void AddToAll(Dictionary<string, WeaponStatEntry> data, short typeID) {
                foreach (KeyValuePair<string, WeaponStatEntry> kvp in data) {
                    WeaponStatEntry t = kvp.Value;

                    if (chars.TryGetValue(t.CharacterID, out PsCharacter? c) == false) {
                        _Logger.LogWarning($"Missing {nameof(PsCharacter)} {t.CharacterID} while generating the top?");
                        continue;
                    }

                    WeaponStatTop top = new();
                    top.TypeID = typeID;
                    top.WorldID = c.WorldID;
                    top.FactionID = c.FactionID;
                    top.Timestamp = timestamp;

                    top.ItemID = itemID;
                    top.CharacterID = t.CharacterID;
                    top.VehicleID = t.VehicleID;

                    top.Kills = t.Kills;
                    top.Deaths = t.Deaths;
                    top.Shots = t.Shots;
                    top.ShotsHit = t.ShotsHit;
                    top.Headshots = t.Headshots;
                    top.VehicleKills = t.VehicleKills;
                    top.SecondsWith = t.SecondsWith;

                    top.KillDeathRatio = top.Kills / Math.Max(1d, top.Deaths);
                    top.KillsPerMinute = top.Kills / (Math.Max(1d, top.SecondsWith) / 60d);
                    top.Accuracy = top.ShotsHit / Math.Max(1d, top.Shots);
                    top.HeadshotRatio = top.Headshots / Math.Max(1d, top.Kills);
                    top.VehicleKillsPerMinute = top.VehicleKills / (Math.Max(1d, top.SecondsWith) / 60d);

                    all.Add(top);
                }
            }

            Dictionary<string, WeaponStatEntry> GetTopWeaponStats(Func<WeaponStatEntry, double> selector, int minKills, int count) {
                List<WeaponStatEntry> ordered = stats.OrderByDescending(selector).ToList();
                Dictionary<string, WeaponStatEntry> topDict = new();

                HashSet<string> missingCharacterIds = new();
                
                foreach (short worldID in World.PcStreams) {
                    foreach (short factionID in Faction.All) {

                        List<WeaponStatEntry> list = new();
                        foreach (WeaponStatEntry entry in ordered) {
                            _ = chars.TryGetValue(entry.CharacterID, out PsCharacter? c);
                            if (c == null) {
                                if (missingCharacters.Contains(entry.CharacterID) == false) {
                                    if (false) {
                                        _Logger.LogWarning($"Failed to find character {entry.CharacterID}");
                                    }
                                }
                                missingCharacters.Add(entry.CharacterID);
                                continue;
                            }

                            if (entry.Kills < minKills) {
                                continue;
                            }

                            if (c.WorldID != worldID || c.FactionID != factionID) {
                                continue;
                            }

                            list.Add(entry);

                            if (list.Count >= count) {
                                break;
                            }
                        }

                        foreach (WeaponStatEntry entry in list) {
                            topDict[entry.CharacterID] = entry;
                        }
                    }
                }

                return topDict;
            }

            Dictionary<string, WeaponStatEntry> topKD = GetTopWeaponStats(iter => iter.KillDeathRatio, 1159, 50);
            if (topKD.Count < 20) {
                topKD = GetTopWeaponStats(iter => iter.KillDeathRatio, 50, 50);
            }
            AddToAll(topKD, PercentileCacheType.KD);

            Dictionary<string, WeaponStatEntry> topKPM = GetTopWeaponStats(iter => iter.KillsPerMinute, 1159, 50);
            if (topKPM.Count < 20) {
                topKPM = GetTopWeaponStats(iter => iter.KillsPerMinute, 50, 50);
            }
            AddToAll(topKPM, PercentileCacheType.KPM);

            Dictionary<string, WeaponStatEntry> topACC = GetTopWeaponStats(iter => iter.Accuracy, 1159, 50);
            if (topACC.Count < 20) {
                topACC = GetTopWeaponStats(iter => iter.Accuracy, 50, 50);
            }
            AddToAll(topACC, PercentileCacheType.ACC);

            Dictionary<string, WeaponStatEntry> topHSR = GetTopWeaponStats(iter => iter.HeadshotRatio, 1159, 50);
            if (topHSR.Count < 20) {
                topHSR = GetTopWeaponStats(iter => iter.HeadshotRatio, 50, 50);
            }
            AddToAll(topHSR, PercentileCacheType.HSR);

            Dictionary<string, WeaponStatEntry> topVKPM = GetTopWeaponStats(iter => iter.VehicleKillsPerMinute, 1159, 50);
            if (topVKPM.Count < 20) {
                topVKPM = GetTopWeaponStats(iter => iter.VehicleKillsPerMinute, 50, 50);
            }
            AddToAll(topVKPM, PercentileCacheType.VKPM);

            Dictionary<string, WeaponStatEntry> topKills = GetTopWeaponStats(iter => iter.Kills, 0, 200);
            AddToAll(topKills, PercentileCacheType.KILLS);

            Dictionary<string, WeaponStatEntry> topVKills = GetTopWeaponStats(iter => iter.VehicleKills, 0, 200);
            AddToAll(topVKills, PercentileCacheType.VKILLS);

            _Logger.LogDebug($"GenerateTop(itemID {itemID})> Setting {all.Count} entries");

            if (missingCharacters.Count > 0) {
                string missingStr = string.Join(", ", missingCharacters.Take(25)) + (missingCharacters.Count > 25 ? $"+{missingCharacters.Count - 25} more..." : "");
                _Logger.LogDebug($"Failed to find characters for weapons [ItemID={itemID}] [Count={missingCharacters.Count}]: [{missingStr}]");
            }

            await _WeaponTopDb.SetByItemID(itemID, all);
        }

        /// <summary>
        ///     Generate bucketed stats for a weapon
        /// </summary>
        /// <param name="itemID">ID of the item</param>
        /// <param name="stats">Stats to generate the buckets from</param>
        /// <param name="timestamp">Timestamp of when the data was pulled from the DB</param>
        /// <param name="stoppingToken">Stopping token</param>
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

        /// <summary>
        ///     Generate a list of buckets given a list of values and a target number of buckets
        /// </summary>
        /// <param name="values">Values to be bucketed. Can be unsorted</param>
        /// <param name="bucketCount">How many buckets to make</param>
        /// <returns>A list of bucketed stats generated from <paramref name="bucketCount"/></returns>
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

        /// <summary>
        ///     Generate a snapshot of a weapon's stats
        /// </summary>
        /// <param name="itemID">ID of the item to generate the snapshot of</param>
        /// <param name="stats">Stats used to generate the snapshot</param>
        /// <param name="stoppingToken">Stopping token</param>
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

        /// <summary>
        ///     Generate percentile stats for a weapon
        /// </summary>
        /// <param name="itemID">ID of the item</param>
        /// <param name="stats">Stats used to generate the percentile stats</param>
        /// <param name="stoppingToken">Stopping token</param>
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
        ///     Given a (potentially) unsorted list of doubles, find the value that exists at <paramref name="percent"/>
        ///     percent of the way thru the sorted list
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
