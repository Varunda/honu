using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Models.Api;
using watchtower.Models.Census;
using watchtower.Models.CharacterViewer.WeaponStats;
using watchtower.Models.Db;
using watchtower.Services;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("/api/character")]
    public class CharacterWeaponStatsApiController : ControllerBase {

        private readonly ILogger<CharacterWeaponStatsApiController> _Logger;

        private readonly ICharacterWeaponStatRepository _CharacterWeaponStatRepository;
        private readonly ICharacterRepository _CharacterRepository;
        private readonly IItemRepository _ItemRepository;
        private readonly IWeaponStatPercentileCacheDbStore _PercentileDb;

        private readonly IBackgroundWeaponPercentileCacheQueue _PercentileQueue;

        public CharacterWeaponStatsApiController(ILogger<CharacterWeaponStatsApiController> logger,
            ICharacterWeaponStatRepository charWeaponRepo, ICharacterRepository charRepo,
            IItemRepository itemRepo, IWeaponStatPercentileCacheDbStore percentDb,
            IBackgroundWeaponPercentileCacheQueue percentQueue) {

            _Logger = logger;

            _CharacterWeaponStatRepository = charWeaponRepo;
            _CharacterRepository = charRepo;
            _ItemRepository = itemRepo;
            _PercentileDb = percentDb;

            _PercentileQueue = percentQueue ?? throw new ArgumentNullException(nameof(percentQueue));
        }

        [HttpGet("{charID}/weapon_stats")]
        public async Task<ActionResult<List<CharacterWeaponStatEntry>>> GetWeaponStats(string charID) {
            PsCharacter? character = await _CharacterRepository.GetByID(charID);
            if (character == null) {
                return NotFound($"{nameof(PsCharacter)} {charID}");
            }

            List<WeaponStatEntry> statEntries = await _CharacterWeaponStatRepository.GetByCharacterID(charID);

            List<CharacterWeaponStatEntry> entries = new List<CharacterWeaponStatEntry>(statEntries.Count);

            foreach (WeaponStatEntry entry in statEntries) {
                CharacterWeaponStatEntry stat = new CharacterWeaponStatEntry();
                stat.CharacterID = charID;
                stat.Stat = entry;
                stat.ItemID = entry.WeaponID;
                stat.Item = await _ItemRepository.GetByID(stat.ItemID);

                if (stat.Stat.Kills > 100 && stat.ItemID != "0") {
                    stat = await _GetPercentileData(stat);
                }

                entries.Add(stat);
            }

            return Ok(entries);
        }

        private async Task<CharacterWeaponStatEntry> _GetPercentileData(CharacterWeaponStatEntry entry) {
            bool needsRegen = false;

            WeaponStatPercentileCache? kdCache = await _PercentileDb.GetByItemID(entry.ItemID, PercentileCacheType.KD);
            if (kdCache != null) {
                if (needsRegen == false && kdCache.Timestamp - DateTime.UtcNow > TimeSpan.FromDays(1)) {
                    //_Logger.LogDebug($"percentile cache for {entry.ItemID} is {hsrCache.Timestamp - DateTime.UtcNow} old, will regen");
                    needsRegen = true;
                }

                entry.KillDeathRatioPercentile = _InterpolatePercentile(kdCache, entry.Stat.KillDeathRatio);
            } else {
                needsRegen = true;
            }

            WeaponStatPercentileCache? kpmCache = await _PercentileDb.GetByItemID(entry.ItemID, PercentileCacheType.KPM);
            if (kpmCache != null) {
                if (needsRegen == false && kpmCache.Timestamp - DateTime.UtcNow > TimeSpan.FromDays(1)) {
                    needsRegen = true;
                }

                entry.KillsPerMinutePercentile = _InterpolatePercentile(kpmCache, entry.Stat.KillsPerMinute);
            } else {
                needsRegen = true;
            }

            WeaponStatPercentileCache? accCache = await _PercentileDb.GetByItemID(entry.ItemID, PercentileCacheType.ACC);
            if (accCache != null) {
                if (needsRegen == false && accCache.Timestamp - DateTime.UtcNow > TimeSpan.FromDays(1)) {
                    needsRegen = true;
                }

                entry.AccuracyPercentile = _InterpolatePercentile(accCache, entry.Stat.Accuracy);
            } else {
                needsRegen = true;
            }

            WeaponStatPercentileCache? hsrCache = await _PercentileDb.GetByItemID(entry.ItemID, PercentileCacheType.HSR);
            if (hsrCache != null) {
                if (needsRegen == false && hsrCache.Timestamp - DateTime.UtcNow > TimeSpan.FromDays(1)) {
                    needsRegen = true;
                }

                entry.HeadshotRatioPercentile = _InterpolatePercentile(hsrCache, entry.Stat.HeadshotRatio);
            } else {
                needsRegen = true;
            }

            if (needsRegen == true) {
                _PercentileQueue.Queue(entry.ItemID);
            }

            return entry;
        }

        private double _InterpolatePercentile(WeaponStatPercentileCache percentiles, double value) {
            if (value >= percentiles.Q100) {
                return 100.0d;
            }
            if (value <= percentiles.Q0) {
                return 0.0d;
            }

            double min = 0;
            double max = percentiles.Q100;

            List<double> values = new List<double>() {
                percentiles.Q0, percentiles.Q5, percentiles.Q10, percentiles.Q15, percentiles.Q20, percentiles.Q25,
                percentiles.Q30, percentiles.Q35, percentiles.Q40, percentiles.Q45, percentiles.Q50,
                percentiles.Q55, percentiles.Q60, percentiles.Q65, percentiles.Q70, percentiles.Q75,
                percentiles.Q80, percentiles.Q85, percentiles.Q90, percentiles.Q95, percentiles.Q100
            };

            // The percentiles are broken into 5% chunks. Find what chunk this value fits between
            int i;
            for (i = 1; i <= 20; ++i) {
                double iter = values[i];
                if (value < iter) {
                    min = values[i - 1];
                    max = values[i];
                    break;
                }
            }

            // Get what percent the value is between the min and max bounds
            double range = max - min;
            double offset = range - value + min;
            //_Logger.LogDebug($"MIN - MAX = {min} - {max} = {value} {i * 5}% - {(i + 1) * 5}%");
            double percent = 1d - (offset / range);

            //_Logger.LogDebug($"{percentiles.ItemID} MIN - MAX = {min} - {max} = {value} {i * 5}% - {(i + 1) * 5}%, {percent} {5d * percent} ANS = {(i * 5d) + (5d * percent)}%");

            // Divide by 5d cause each chunk is 5%
            double percentile = (i * 5d) + (5d * percent);

            return percentile;
        }

    }
}
