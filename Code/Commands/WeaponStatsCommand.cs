using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Commands;
using watchtower.Models.Census;
using watchtower.Models.CharacterViewer.WeaponStats;
using watchtower.Models.Db;
using watchtower.Services.Census;
using watchtower.Services.Db;
using watchtower.Services.Repositories;
using watchtower.Services.Queues;

namespace watchtower.Code.Commands {

    [Command]
    public class WeaponStatsCommand {

        private readonly ILogger<WeaponStatsCommand> _Logger;

        private readonly CharacterWeaponStatCollection _StatCensus;
        private readonly CharacterRepository _CharacterRepository;
        private readonly ItemRepository _ItemRepository;
        private readonly IWeaponStatPercentileCacheDbStore _PercentileDb;
        private readonly CharacterWeaponStatDbStore _StatDb;

        public WeaponStatsCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<WeaponStatsCommand>>();

            _StatCensus = services.GetRequiredService<CharacterWeaponStatCollection>();
            _CharacterRepository = services.GetRequiredService<CharacterRepository>();
            _ItemRepository = services.GetRequiredService<ItemRepository>();
            _PercentileDb = services.GetRequiredService<IWeaponStatPercentileCacheDbStore>();
            _StatDb = services.GetRequiredService<CharacterWeaponStatDbStore>();
        }

        public async Task Char(string charName) {
            PsCharacter? c = await _CharacterRepository.GetFirstByName(charName);
            if (c == null) {
                _Logger.LogError($"No character {charName} exists");
                return;
            }

            List<WeaponStatEntry> entries = await _StatCensus.GetByCharacterID(c.ID);

            foreach (WeaponStatEntry entry in entries) {
                if (entry.Kills == 0 || entry.SecondsWith < 10) {
                    continue;
                }
                PsItem? weapon = await _ItemRepository.GetByID(int.Parse(entry.WeaponID));

                _Logger.LogInformation($"{entry.WeaponID}/{weapon?.Name}: KD = {entry.Kills}/{entry.Deaths}:{entry.KillDeathRatio} KPM = {entry.Kills}/{entry.SecondsWith}:{entry.KillsPerMinute} S{entry.Shots} H{entry.ShotsHit} HS{entry.Headshots}");
            }

            _Logger.LogInformation($"done");
        }

        class Bucket {
            public double Start { get; set; } = 0d;
            public double Width { get; set; } = 0d;
            public int Count { get; set; }
        }

        public async Task edf(int itemID) {
            List<WeaponStatEntry> entries = await _StatDb.GetByItemID(itemID.ToString(), 1159);

            if (entries.Count < 1) {
                _Logger.LogWarning($"Have {entries.Count} stats for {itemID}");
                return;
            }

            List<WeaponStatEntry> sorted = entries.OrderByDescending(iter => iter.KillsPerMinute).ToList();

            double maxKpm = sorted.First().KillsPerMinute;
            double minKpm = sorted.Last().KillsPerMinute;

            double bucketWidth = (maxKpm - minKpm) / 100;

            List<Bucket> buckets = new List<Bucket>();

            Bucket iter = new Bucket() {
                Start = minKpm,
                Width = bucketWidth,
                Count = 0
            };

            for (int i = sorted.Count - 1; i >= 0; --i) {
                WeaponStatEntry entry = sorted[i];

                if (entry.KillsPerMinute > (iter.Start + iter.Width)) {
                    buckets.Add(iter);
                    iter = new Bucket() {
                        Start = entry.KillsPerMinute,
                        Width = bucketWidth,
                        Count = 0
                    };
                }

                ++iter.Count;
            }

            foreach (Bucket b in buckets) {
                _Logger.LogInformation($"{b.Start} - {b.Start + b.Width} = {b.Count}");
            }
        }

        public async Task Regen(int itemID) {
            PsItem? item = await _ItemRepository.GetByID(itemID);

            _Logger.LogInformation($"Regening all cached percentile stats for {item?.Name}/{itemID}");

            WeaponStatPercentileCache? kd = await _PercentileDb.GenerateKd(itemID.ToString());
            if (kd != null) { await _PercentileDb.Upsert(itemID.ToString(), kd); }

            WeaponStatPercentileCache? kpm = await _PercentileDb.GenerateKpm(itemID.ToString());
            if (kpm != null) { await _PercentileDb.Upsert(itemID.ToString(), kpm); }

            WeaponStatPercentileCache? acc = await _PercentileDb.GenerateAcc(itemID.ToString());
            if (acc != null) { await _PercentileDb.Upsert(itemID.ToString(), acc); }

            WeaponStatPercentileCache? hsr = await _PercentileDb.GenerateHsr(itemID.ToString());
            if (hsr != null) { await _PercentileDb.Upsert(itemID.ToString(), hsr); }

            _Logger.LogInformation($"Percentile weapon stats for {item?.Name}/{itemID} remade");
        }

        public async Task RegenKPM(int itemID) {
            PsItem? item = await _ItemRepository.GetByID(itemID);
            WeaponStatPercentileCache? entry = await _PercentileDb.GenerateKpm(itemID.ToString());

            _Logger.LogInformation($"KPM percentiles for {item?.Name}/{itemID}");
            await PrintPercentile(itemID, item, entry, PercentileCacheType.KPM);
        }

        public async Task RegenKD(int itemID) {
            PsItem? item = await _ItemRepository.GetByID(itemID);
            WeaponStatPercentileCache? entry = await _PercentileDb.GenerateKd(itemID.ToString());

            _Logger.LogInformation($"KD percentiles for {item?.Name}/{itemID}");
            await PrintPercentile(itemID, item, entry, PercentileCacheType.KD);
        }

        public async Task RegenAcc(int itemID) {
            PsItem? item = await _ItemRepository.GetByID(itemID);
            WeaponStatPercentileCache? entry = await _PercentileDb.GenerateAcc(itemID.ToString());

            _Logger.LogInformation($"ACC percentiles for {item?.Name}/{itemID}");
            await PrintPercentile(itemID, item, entry, PercentileCacheType.ACC);
        }

        public async Task RegenHsr(int itemID) {
            PsItem? item = await _ItemRepository.GetByID(itemID);
            WeaponStatPercentileCache? entry = await _PercentileDb.GenerateHsr(itemID.ToString());

            _Logger.LogInformation($"HSR percentiles for {item?.Name}/{itemID}");
            await PrintPercentile(itemID, item, entry, PercentileCacheType.HSR);
        }

        private async Task PrintPercentile(int itemID, PsItem? item, WeaponStatPercentileCache? entry, short typeID) {
            if (entry != null) {
                entry.TypeID = typeID;
                _Logger.LogDebug($"Generated KD percentile, saving in cache");
                await _PercentileDb.Upsert(itemID.ToString(), entry);
            }

            if (entry == null) {
                _Logger.LogWarning($"Failed to get percentile data for {item?.Name}/{itemID}, does the item exist?");
                return;
            }

            _Logger.LogInformation($"{entry.ItemID}:{entry.TypeID}, generated at {entry.Timestamp}"
                + $"\n00% - 24%> {entry.Q0} {entry.Q5} {entry.Q10} {entry.Q15} {entry.Q20}"
                + $"\n25% - 49%> {entry.Q25} {entry.Q30} {entry.Q35} {entry.Q40} {entry.Q45}"
                + $"\n50% - 74%> {entry.Q50} {entry.Q55} {entry.Q60} {entry.Q65} {entry.Q70}"
                + $"\n75% - MAX> {entry.Q75} {entry.Q85} {entry.Q90} {entry.Q95} {entry.Q100}"
            );
        }

    }
}
