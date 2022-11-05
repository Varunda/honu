using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly WeaponStatBucketDbStore _BucketDb;
        private readonly WeaponStatTopDbStore _StatTopDb;
        private readonly WeaponStatSnapshotDbStore _SnapshotDb;

        public ItemApiController(ILogger<ItemApiController> logger,
            ItemRepository itemRepo, IWeaponStatPercentileCacheDbStore percDb,
            CharacterWeaponStatDbStore statDb, CharacterRepository charRepo,
            WeaponStatBucketDbStore bucketDb, WeaponStatTopDbStore statTopDb,
            WeaponStatSnapshotDbStore snapshotDb) {

            _Logger = logger;

            _ItemRepository = itemRepo;
            _PercentileDb = percDb ?? throw new ArgumentNullException(nameof(percDb));
            _StatDb = statDb ?? throw new ArgumentNullException(nameof(statDb));
            _CharacterRepository = charRepo;
            _BucketDb = bucketDb;
            _StatTopDb = statTopDb;
            _SnapshotDb = snapshotDb;
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
        ///     Get many items at once
        /// </summary>
        /// <param name="IDs">List of IDs to get</param>
        /// <response code="200">
        ///     The response will contain a list of <see cref="PsItem"/>, each with an <see cref="PsItem.ID"/>
        ///     that was passed in <paramref name="IDs"/>
        /// </response>
        [HttpGet("many")]
        public async Task<ApiResponse<List<PsItem>>> GetByIDs([FromQuery] List<int> IDs) {
            List<PsItem> allItems = await _ItemRepository.GetAll();
            Dictionary<int, PsItem> map = allItems.ToDictionary(iter => iter.ID);

            List<PsItem> items = new(IDs.Count);
            foreach (int itemID in IDs) {
                if (map.TryGetValue(itemID, out PsItem? i) == true) {
                    items.Add(i);
                }
            }

            return ApiOk(items);
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
        ///     Get all the top stats of an item
        /// </summary>
        /// <remarks>
        ///     There is no check to ensure the item actually exists. Because Census doesn't have all the items
        ///     in the game, it is possible to have weapon stats for a weapon that isn't in Census 
        /// </remarks>
        /// <param name="itemID">ID of the item</param>
        /// <response code="200">
        ///     The response will contain a list of <see cref="ExpandedWeaponStatTop"/> entries
        ///     that represent the top characters in different stats. You can use <see cref="WeaponStatTop.TypeID"/>
        ///     to filter to specific stats, such as KD, KPM or HSR. See <see cref="PercentileCacheType"/> for which is which
        /// </response>
        [HttpGet("{itemID}/top/all")]
        public async Task<ApiResponse<List<ExpandedWeaponStatTop>>> GetTopAll(int itemID) {
            Stopwatch timer = Stopwatch.StartNew();
            Stopwatch stepTimer = Stopwatch.StartNew();
            List<WeaponStatTop> tops = await _StatTopDb.GetByItemID(itemID);
            long loadTopMs = stepTimer.ElapsedMilliseconds; stepTimer.Restart();

            List<ExpandedWeaponStatTop> expandedTops = new(tops.Count);

            List<string> charIDs = tops.Select(iter => iter.CharacterID).Distinct().ToList();
            Dictionary<string, PsCharacter> characters = (await _CharacterRepository.GetByIDs(charIDs, CensusEnvironment.PC, fast: true))
                .ToDictionary(iter => iter.ID);
            long loadCharsMs = stepTimer.ElapsedMilliseconds; stepTimer.Restart();

            foreach (WeaponStatTop top in tops) {
                Stopwatch exTimer = Stopwatch.StartNew();
                _ = characters.TryGetValue(top.CharacterID, out PsCharacter? c);

                ExpandedWeaponStatTop exTop = new();
                exTop.Character = c;
                exTop.Entry = top;

                expandedTops.Add(exTop);
            }

            long expandMs = stepTimer.ElapsedMilliseconds; stepTimer.Restart();

            _Logger.LogDebug($"Took {timer.ElapsedMilliseconds}ms to load top for item ID {itemID}, load top: {loadTopMs}ms, load chars: {loadCharsMs}ms, expand: {expandMs}ms");

            return ApiOk(expandedTops);
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
            if (int.TryParse(itemID, out int id) == false) {
                return ApiBadRequest<WeaponStatPercentileAll>($"{itemID} is not a valid Int32");
            }

            List<WeaponStatBucket> allBuckets = await _BucketDb.GetByItemID(id);

            WeaponStatPercentileAll all = new WeaponStatPercentileAll();
            all.ItemID = itemID;
            all.KD = allBuckets.Where(iter => iter.TypeID == PercentileCacheType.KD).ToList();
            all.KPM = allBuckets.Where(iter => iter.TypeID == PercentileCacheType.KPM).ToList();
            all.Accuracy = allBuckets.Where(iter => iter.TypeID == PercentileCacheType.ACC).ToList();
            all.HeadshotRatio = allBuckets.Where(iter => iter.TypeID == PercentileCacheType.HSR).ToList();
            all.VKPM = allBuckets.Where(iter => iter.TypeID == PercentileCacheType.VKPM).ToList();

            return ApiOk(all);
        }

        /// <summary>
        ///     Get the weapon stat snapshots for a specific item
        /// </summary>
        /// <param name="itemID">ID of the item</param>
        /// <response code="200">
        ///     The response will contain a list of <see cref="WeaponStatSnapshot"/>s
        /// </response>
        [HttpGet("{itemID}/snapshots")]
        public async Task<ApiResponse<List<WeaponStatSnapshot>>> GetSnapshots(int itemID) {
            List<WeaponStatSnapshot> snapshots = await _SnapshotDb.GetByItemID(itemID);
            return ApiOk(snapshots);
        }

        private async Task<List<ExpandedWeaponStatEntry>> GetExpanded(List<WeaponStatEntry> entries) {
            List<ExpandedWeaponStatEntry> expanded = new List<ExpandedWeaponStatEntry>(entries.Count);

            List<string> charIDs = entries.Select(iter => iter.CharacterID).Distinct().ToList();
            Dictionary<string, PsCharacter> chars = (await _CharacterRepository.GetByIDs(charIDs, CensusEnvironment.PC)).ToDictionary(iter => iter.ID);

            foreach (WeaponStatEntry entry in entries) {
                ExpandedWeaponStatEntry ex = new ExpandedWeaponStatEntry() {
                    Entry = entry
                };

                _ = chars.TryGetValue(entry.CharacterID, out PsCharacter? c);
                ex.Character = c;

                expanded.Add(ex);
            }

            return expanded;
        }

    }
}
