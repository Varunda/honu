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
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    /// <summary>
    ///     Endpoint for getting items and weapon stats
    /// </summary>
    [ApiController]
    [Route("/api/item")]
    public class ItemApiController : ApiControllerBase {

        private readonly ILogger<ItemApiController> _Logger;

        private readonly ItemRepository _ItemRepository;
        private readonly IWeaponStatPercentileCacheDbStore _PercentileDb;
        private readonly CharacterWeaponStatDbStore _StatDb;
        private readonly CharacterRepository _CharacterRepository;

        public ItemApiController(ILogger<ItemApiController> logger,
            ItemRepository itemRepo, IWeaponStatPercentileCacheDbStore percDb,
            CharacterWeaponStatDbStore statDb, CharacterRepository charRepo) {

            _Logger = logger;

            _ItemRepository = itemRepo;
            _PercentileDb = percDb ?? throw new ArgumentNullException(nameof(percDb));
            _StatDb = statDb ?? throw new ArgumentNullException(nameof(statDb));
            _CharacterRepository = charRepo;
        }

        /// <summary>
        ///     Get a specific item
        /// </summary>
        /// <param name="itemID">ID of the item</param>
        /// <response code="200">
        ///     The response will contain the <see cref="PsItem"/> with <see cref="PsItem.ID"/> of <paramref name="itemID"/>
        /// </response>
        /// <response code="204">
        ///     No <see cref="PsItem"/> with <see cref="PsItem.ID"/> of <paramref name="itemID"/> exists
        /// </response>
        [HttpGet("{itemID}")]
        public async Task<ApiResponse<PsItem>> GetByID(int itemID) {
            PsItem? item = await _ItemRepository.GetByID(itemID);
            if (item == null) {
                return ApiNoContent<PsItem>();
            }
            return ApiOk(item);
        }

        /// <summary>
        ///     Get all items
        /// </summary>
        /// <response code="200">
        ///     The response will contain a list of all <see cref="PsItem"/>s
        /// </response>
        [HttpGet]
        [Route("/api/items/weapons")]
        public async Task<ApiResponse<List<PsItem>>> GetWeapons() {
            List<PsItem> items = (await _ItemRepository.GetAll()).Where(iter => iter.TypeID == 26 || iter.TypeID == 36).ToList();

            return ApiOk(items);
        }

        /// <summary>
        ///     Get the characters (and their weapon stats) with the top KD of an item
        /// </summary>
        /// <remarks>
        ///     In order for a character to count, they must have auraxed (gotten at least 1160 kills) the weapon.
        ///     If there are less than 50 characters that have auraxed a weapon, the kill limit is lowered
        ///     instead to 50. This is useful when a weapon is just released, and people are racing to aurax
        ///     <br/><br/>
        ///     There is no check to ensure the item actually exists. Because Census doesn't have all the items
        ///     in the game, it is possible to have weapon stats for a weapon that isn't in Census 
        /// </remarks>
        /// <param name="itemID">ID of the item</param>
        /// <param name="worldIDs">Array of what servers the top will be limited to</param>
        /// <param name="factionIDs">Array of what factions the top will be limited to</param>
        /// <response code="200">
        ///     The response will contain the <see cref="ExpandedWeaponStatEntry"/>s for the characters
        ///     with the top KD of the item passed
        /// </response>
        [HttpGet("{itemID}/top/kd")]
        public async Task<ApiResponse<List<ExpandedWeaponStatEntry>>> GetTopKD(string itemID,
            [FromQuery] List<short> worldIDs,
            [FromQuery] List<short> factionIDs) {

            List<WeaponStatEntry> entries = await _StatDb.GetTopKD(itemID, worldIDs, factionIDs);
            _Logger.LogTrace($"Got {entries.Count} entries for {itemID}");
            if (entries.Count < 50) {
                entries = await _StatDb.GetTopKD(itemID, worldIDs, factionIDs, 50);
            }
            List<ExpandedWeaponStatEntry> expanded = await GetExpanded(entries);

            return ApiOk(expanded);
        }

        /// <summary>
        ///     Get the characters (and their weapon stats) with the top KPM of an item
        /// </summary>
        /// <remarks>
        ///     In order for a character to count, they must have auraxed (gotten at least 1160 kills) the weapon.
        ///     If there are less than 50 characters that have auraxed a weapon, the kill limit is lowered
        ///     instead to 50. This is useful when a weapon is just released, and people are racing to aurax
        ///     <br/><br/>
        ///     There is no check to ensure the item actually exists. Because Census doesn't have all the items
        ///     in the game, it is possible to have weapon stats for a weapon that isn't in Census 
        /// </remarks>
        /// <param name="itemID">ID of the item</param>
        /// <param name="worldIDs">Array of what servers the top will be limited to</param>
        /// <param name="factionIDs">Array of what factions the top will be limited to</param>
        /// <response code="200">
        ///     The response will contain the <see cref="ExpandedWeaponStatEntry"/>s for the characters
        ///     with the top KPM of the item passed
        /// </response>
        [HttpGet("{itemID}/top/kpm")]
        public async Task<ApiResponse<List<ExpandedWeaponStatEntry>>> GetTopKpm(string itemID,
            [FromQuery] List<short> worldIDs,
            [FromQuery] List<short> factionIDs) {

            List<WeaponStatEntry> entries = await _StatDb.GetTopKPM(itemID, worldIDs, factionIDs);
            if (entries.Count < 50) {
                entries = await _StatDb.GetTopKPM(itemID, worldIDs, factionIDs, 50);
            }
            List<ExpandedWeaponStatEntry> expanded = await GetExpanded(entries);

            return ApiOk(expanded);
        }

        /// <summary>
        ///     Get the characters (and their weapon stats) with the top accuracy of an item
        /// </summary>
        /// <remarks>
        ///     In order for a character to count, they must have auraxed (gotten at least 1160 kills) the weapon.
        ///     If there are less than 50 characters that have auraxed a weapon, the kill limit is lowered
        ///     instead to 50. This is useful when a weapon is just released, and people are racing to aurax
        ///     <br/><br/>
        ///     There is no check to ensure the item actually exists. Because Census doesn't have all the items
        ///     in the game, it is possible to have weapon stats for a weapon that isn't in Census 
        /// </remarks>
        /// <param name="itemID">ID of the item</param>
        /// <param name="worldIDs">Array of what servers the top will be limited to</param>
        /// <param name="factionIDs">Array of what factions the top will be limited to</param>
        /// <response code="200">
        ///     The response will contain the <see cref="ExpandedWeaponStatEntry"/>s for the characters
        ///     with the top accuracy of the item passed
        /// </response>
        [HttpGet("{itemID}/top/accuracy")]
        public async Task<ApiResponse<List<ExpandedWeaponStatEntry>>> GetTopAcc(string itemID,
            [FromQuery] List<short> worldIDs,
            [FromQuery] List<short> factionIDs) {
            List<WeaponStatEntry> entries = await _StatDb.GetTopAccuracy(itemID, new List<short>(), new List<short>());
            if (entries.Count < 50) {
                entries = await _StatDb.GetTopAccuracy(itemID, worldIDs, factionIDs, 50);
            }
            List<ExpandedWeaponStatEntry> expanded = await GetExpanded(entries);

            return ApiOk(expanded);
        }

        /// <summary>
        ///     Get the characters (and their weapon stats) with the top headshot ratio of an item
        /// </summary>
        /// <remarks>
        ///     In order for a character to count, they must have auraxed (gotten at least 1160 kills) the weapon.
        ///     If there are less than 50 characters that have auraxed a weapon, the kill limit is lowered
        ///     instead to 50. This is useful when a weapon is just released, and people are racing to aurax
        ///     <br/><br/>
        ///     There is no check to ensure the item actually exists. Because Census doesn't have all the items
        ///     in the game, it is possible to have weapon stats for a weapon that isn't in Census 
        /// </remarks>
        /// <param name="itemID">ID of the item</param>
        /// <param name="worldIDs">Array of what servers the top will be limited to</param>
        /// <param name="factionIDs">Array of what factions the top will be limited to</param>
        /// <response code="200">
        ///     The response will contain the <see cref="ExpandedWeaponStatEntry"/>s for the characters
        ///     with the top headshot ratio of the item passed
        /// </response>
        [HttpGet("{itemID}/top/hsr")]
        public async Task<ApiResponse<List<ExpandedWeaponStatEntry>>> GetTopHsr(string itemID,
            [FromQuery] List<short> worldIDs,
            [FromQuery] List<short> factionIDs) {

            List<WeaponStatEntry> entries = await _StatDb.GetTopHeadshotRatio(itemID, new List<short>(), new List<short>());
            if (entries.Count < 50) {
                entries = await _StatDb.GetTopHeadshotRatio(itemID, worldIDs, factionIDs, 50);
            }
            List<ExpandedWeaponStatEntry> expanded = await GetExpanded(entries);

            return ApiOk(expanded);
        }

        /// <summary>
        ///     Get the characters (and their weapon stats) with the most amount of kills
        /// </summary>
        /// <remarks>
        ///     There is no check to ensure the item actually exists. Because Census doesn't have all the items
        ///     in the game, it is possible to have weapon stats for a weapon that isn't in Census 
        /// </remarks>
        /// <param name="itemID">ID of the item</param>
        /// <param name="worldIDs">Array of what servers the top will be limited to</param>
        /// <param name="factionIDs">Array of what factions the top will be limited to</param>
        /// <response code="200">
        ///     The response will contain the <see cref="ExpandedWeaponStatEntry"/>s for the characters
        ///     with the most amount of kills with the item
        /// </response>
        [HttpGet("{itemID}/top/kills")]
        public async Task<ApiResponse<List<ExpandedWeaponStatEntry>>> GetTopKills(string itemID,
            [FromQuery] List<short> worldIDs,
            [FromQuery] List<short> factionIDs) {

            List<WeaponStatEntry> entries = await _StatDb.GetTopKills(itemID, worldIDs, factionIDs);
            List<ExpandedWeaponStatEntry> expanded = await GetExpanded(entries);

            return ApiOk(expanded);
        }

        /// <summary>
        ///     Get the percentile stats for an item
        /// </summary>
        /// <remarks>
        ///     The percentile included in the response are:
        ///     <ul>
        ///         <li>KPM</li>
        ///         <li>KD</li>
        ///         <li>Accuracy</li>
        ///         <li>Headshot ratio</li>
        ///     </ul>
        ///     <br/><br/>
        ///     No check is done to ensure the item actually exists, as Census does not have all items
        ///     in the game in the API. It is possible for weapons stats for an item that doesn't exist,
        ///     to exist
        /// </remarks>
        /// <param name="itemID">ID of the item</param>
        /// <response code="200">
        ///     The response will contain the <see cref="WeaponStatPercentileAll"/> for the item
        /// </response>
        [HttpGet("{itemID}/percentile_stats")]
        public async Task<ApiResponse<WeaponStatPercentileAll>> GetPercentileStats(string itemID) {
            List<WeaponStatEntry> entries = await _StatDb.GetByItemID(itemID, 1159);
            if (entries.Count < 50) {
                entries = await _StatDb.GetByItemID(itemID, 50);
            }

            WeaponStatPercentileAll all = new WeaponStatPercentileAll();
            all.ItemID = itemID;
            all.KD = GetBuckets(entries.Select(iter => iter.KillDeathRatio).ToList(), 100);
            all.KPM = GetBuckets(entries.Select(iter => iter.KillsPerMinute).ToList(), 100);
            all.Accuracy = GetBuckets(entries.Select(iter => iter.Accuracy * 100).ToList(), 100);
            all.HeadshotRatio = GetBuckets(entries.Select(iter => iter.HeadshotRatio * 100).ToList(), 100);

            return ApiOk(all);
        }

        private async Task<List<ExpandedWeaponStatEntry>> GetExpanded(List<WeaponStatEntry> entries) {
            List<ExpandedWeaponStatEntry> expanded = new List<ExpandedWeaponStatEntry>(entries.Count);

            foreach (WeaponStatEntry entry in entries) {
                ExpandedWeaponStatEntry ex = new ExpandedWeaponStatEntry() {
                    Entry = entry
                };

                PsCharacter? c = await _CharacterRepository.GetByID(entry.CharacterID, CensusEnvironment.PC);
                ex.Character = c;

                expanded.Add(ex);
            }

            return expanded;
        }

        private List<Bucket> GetBuckets(List<double> values, int bucketCount) {
            if (values.Count == 0) {
                return new List<Bucket>();
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
                return new List<Bucket>();
            }

            double max = sorted.Last();
            double min = sorted.First();

            //_Logger.LogInformation($"{min}/0 {q1}/{q1i} {median2}/{m2i} {q3}/{q3i} {max}/{values.Count - 1}, iqr = {iqr}");

            double bucketWidth = (max - min) / bucketCount;

            List<Bucket> buckets = new List<Bucket>();

            Bucket iter = new Bucket() {
                Start = min,
                Width = bucketWidth,
                Count = 0
            };

            for (double i = min; i <= max; i += bucketWidth) {
                int count = sorted.Where(iter => (iter >= i && iter < i + bucketWidth)).Count();

                Bucket b = new Bucket() {
                    Start = i,
                    Width = bucketWidth,
                    Count = count
                };

                buckets.Add(b);

                //_Logger.LogInformation($"Bucket {i} - {i + bucketWidth} = {count}");
            }

            /*
            for (int i = 0; i < sorted.Count; ++i) {
                double entry = sorted[i];

                if (entry > iqr * 8) {
                    if (iter.Count > 0) {
                        buckets.Add(iter);
                    }
                    break;
                }

                if (entry > (iter.Start + iter.Width)) {
                    buckets.Add(iter);
                    iter = new Bucket() {
                        Start = entry,
                        Width = bucketWidth,
                        Count = 0
                    };
                }

                ++iter.Count;
            }
            */

            return buckets;
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

    }
}
