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

namespace watchtower.Code.Commands {

    [Command]
    public class WeaponStatsCommand {

        private readonly ILogger<WeaponStatsCommand> _Logger;

        private readonly ICharacterWeaponStatCollection _StatCensus;
        private readonly ICharacterRepository _CharacterRepository;
        private readonly IItemRepository _ItemRepository;
        private readonly IWeaponStatPercentileCacheDbStore _PercentileDb;

        public WeaponStatsCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<WeaponStatsCommand>>();

            _StatCensus = services.GetRequiredService<ICharacterWeaponStatCollection>();
            _CharacterRepository = services.GetRequiredService<ICharacterRepository>();
            _ItemRepository = services.GetRequiredService<IItemRepository>();
            _PercentileDb = services.GetRequiredService<IWeaponStatPercentileCacheDbStore>();
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
                PsItem? weapon = await _ItemRepository.GetByID(entry.WeaponID);

                _Logger.LogInformation($"{entry.WeaponID}/{weapon?.Name}: KD = {entry.Kills}/{entry.Deaths}:{entry.KillDeathRatio} KPM = {entry.Kills}/{entry.SecondsWith}:{entry.KillsPerMinute} S{entry.Shots} H{entry.ShotsHit} HS{entry.Headshots}");
            }

            _Logger.LogInformation($"done");
        }

        public async Task Regen(string itemID) {
            PsItem? item = await _ItemRepository.GetByID(itemID);

            _Logger.LogInformation($"Regening all cached percentile stats for {item?.Name}/{itemID}");

            WeaponStatPercentileCache? kd = await _PercentileDb.GenerateKd(itemID);
            if (kd != null) { await _PercentileDb.Upsert(itemID, kd); }

            WeaponStatPercentileCache? kpm = await _PercentileDb.GenerateKpm(itemID);
            if (kpm != null) { await _PercentileDb.Upsert(itemID, kpm); }

            WeaponStatPercentileCache? acc = await _PercentileDb.GenerateAcc(itemID);
            if (acc != null) { await _PercentileDb.Upsert(itemID, acc); }

            WeaponStatPercentileCache? hsr = await _PercentileDb.GenerateHsr(itemID);
            if (hsr != null) { await _PercentileDb.Upsert(itemID, hsr); }

            _Logger.LogInformation($"Percentile weapon stats for {item?.Name}/{itemID} remade");
        }

        public async Task RegenKPM(string itemID) {
            PsItem? item = await _ItemRepository.GetByID(itemID);
            WeaponStatPercentileCache? entry = await _PercentileDb.GenerateKpm(itemID);

            _Logger.LogInformation($"KPM percentiles for {item?.Name}/{itemID}");
            await PrintPercentile(itemID, item, entry, PercentileCacheType.KPM);
        }

        public async Task RegenKD(string itemID) {
            PsItem? item = await _ItemRepository.GetByID(itemID);
            WeaponStatPercentileCache? entry = await _PercentileDb.GenerateKd(itemID);

            _Logger.LogInformation($"KD percentiles for {item?.Name}/{itemID}");
            await PrintPercentile(itemID, item, entry, PercentileCacheType.KD);
        }

        public async Task RegenAcc(string itemID) {
            PsItem? item = await _ItemRepository.GetByID(itemID);
            WeaponStatPercentileCache? entry = await _PercentileDb.GenerateAcc(itemID);

            _Logger.LogInformation($"ACC percentiles for {item?.Name}/{itemID}");
            await PrintPercentile(itemID, item, entry, PercentileCacheType.ACC);
        }

        public async Task RegenHsr(string itemID) {
            PsItem? item = await _ItemRepository.GetByID(itemID);
            WeaponStatPercentileCache? entry = await _PercentileDb.GenerateHsr(itemID);

            _Logger.LogInformation($"HSR percentiles for {item?.Name}/{itemID}");
            await PrintPercentile(itemID, item, entry, PercentileCacheType.HSR);
        }

        private async Task PrintPercentile(string itemID, PsItem? item, WeaponStatPercentileCache? entry, short typeID) {
            if (entry != null) {
                entry.TypeID = typeID;
                _Logger.LogDebug($"Generated KD percentile, saving in cache");
                await _PercentileDb.Upsert(itemID, entry);
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
