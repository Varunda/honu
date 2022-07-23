using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Models;
using watchtower.Models.Api;
using watchtower.Models.Census;
using watchtower.Models.CharacterViewer.WeaponStats;
using watchtower.Models.Db;
using watchtower.Services.Db;
using watchtower.Services.Queues;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    /// <summary>
    ///     Endpoints for getting the weapon stats of a character
    /// </summary>
    [ApiController]
    [Route("/api/character")]
    public class CharacterWeaponStatsApiController : ApiControllerBase {

        private readonly ILogger<CharacterWeaponStatsApiController> _Logger;

        private readonly CharacterWeaponStatRepository _CharacterWeaponStatRepository;
        private readonly CharacterRepository _CharacterRepository;
        private readonly ItemRepository _ItemRepository;
        private readonly IWeaponStatPercentileCacheDbStore _PercentileDb;
        private readonly CharacterWeaponStatDbStore _StatDb;
        private readonly VehicleRepository _VehicleRepository;

        private readonly WeaponPercentileCacheQueue _PercentileQueue;

        public CharacterWeaponStatsApiController(ILogger<CharacterWeaponStatsApiController> logger,
            CharacterWeaponStatRepository charWeaponRepo, CharacterRepository charRepo,
            ItemRepository itemRepo, IWeaponStatPercentileCacheDbStore percentDb,
            WeaponPercentileCacheQueue percentQueue, CharacterWeaponStatDbStore statDb,
            VehicleRepository vehicleRepository) {

            _Logger = logger;

            _CharacterWeaponStatRepository = charWeaponRepo;
            _CharacterRepository = charRepo;
            _ItemRepository = itemRepo;
            _PercentileDb = percentDb;
            _StatDb = statDb;

            _PercentileQueue = percentQueue ?? throw new ArgumentNullException(nameof(percentQueue));
            _VehicleRepository = vehicleRepository;
        }

        /// <summary>
        ///     Get the weapon stats of a character
        /// </summary>
        /// <remarks>
        ///     If a character has more than 100 kills with an item, and the item isn't ID 0, 
        ///     the <see cref="CharacterWeaponStatEntry.Stat"/> field is filled in
        /// </remarks>
        /// <param name="charID">ID of the character</param>
        /// <response code="200">
        ///     The response will contain all the weapon stats for the <see cref="PsCharacter"/>
        ///     with <see cref="PsCharacter.ID"/> of <paramref name="charID"/>
        /// </response>
        /// <response code="404">
        ///     No <see cref="PsCharacter"/> with <see cref="PsCharacter.ID"/> of <paramref name="charID"/> exists
        /// </response>
        [HttpGet("{charID}/weapon_stats")]
        public async Task<ApiResponse<List<CharacterWeaponStatEntry>>> GetWeaponStats(string charID) {
            PsCharacter? character = await _CharacterRepository.GetByID(charID);
            if (character == null) {
                return ApiNotFound<List<CharacterWeaponStatEntry>>($"{nameof(PsCharacter)} {charID}");
            }

            List<WeaponStatEntry> statEntries = await _CharacterWeaponStatRepository.GetByCharacterID(charID);

            List<CharacterWeaponStatEntry> entries = new List<CharacterWeaponStatEntry>(statEntries.Count);

            foreach (WeaponStatEntry entry in statEntries) {
                CharacterWeaponStatEntry stat = new CharacterWeaponStatEntry();
                stat.CharacterID = charID;
                stat.Stat = entry;
                stat.ItemID = entry.WeaponID;
                stat.Item = await _ItemRepository.GetByID(int.Parse(stat.ItemID));
                stat.Vehicle = await _VehicleRepository.GetByID(entry.VehicleID);

                // Ignore boring stuff like a helmet
                if (stat.Item != null && stat.Item.TypeID != 26) {
                    continue;
                }

                if (stat.Stat.Kills > 100 && stat.ItemID != "0") {
                    stat = await _GetPercentileData(stat);
                }

                entries.Add(stat);
            }

            entries = entries.OrderBy(iter => iter.ItemID).ToList();

            return ApiOk(entries);
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

            double min = percentiles.Q95;
            double max = percentiles.Q100;

            List<double> values = new List<double>() {
                percentiles.Q0, percentiles.Q5, percentiles.Q10, percentiles.Q15, percentiles.Q20, percentiles.Q25,
                percentiles.Q30, percentiles.Q35, percentiles.Q40, percentiles.Q45, percentiles.Q50,
                percentiles.Q55, percentiles.Q60, percentiles.Q65, percentiles.Q70, percentiles.Q75,
                percentiles.Q80, percentiles.Q85, percentiles.Q90, percentiles.Q95, percentiles.Q100
            };

            // The percentiles are broken into 5% chunks. Find what chunk this value fits between
            int i;
            for (i = 1; i <= 19; ++i) {
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
            double percent = 1d - (offset / range);

            //_Logger.LogDebug($"{percentiles.ItemID} MIN - MAX = {min} - {max} = {value} {(i - 1) * 5}% - {i * 5}%, {percent} {5d * percent} ANS = {((i - 1) * 5d) + (5d * percent)}%");

            // Divide by 5d cause each chunk is 5%
            double percentile = ((i - 1) * 5d) + (5d * percent);

            return percentile;
        }

    }
}
