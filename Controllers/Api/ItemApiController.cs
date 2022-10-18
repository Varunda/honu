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
        private readonly WeaponStatBucketDbStore _BucketDb;

        public ItemApiController(ILogger<ItemApiController> logger,
            ItemRepository itemRepo, IWeaponStatPercentileCacheDbStore percDb,
            CharacterWeaponStatDbStore statDb, CharacterRepository charRepo,
            WeaponStatBucketDbStore bucketDb) {

            _Logger = logger;

            _ItemRepository = itemRepo;
            _PercentileDb = percDb ?? throw new ArgumentNullException(nameof(percDb));
            _StatDb = statDb ?? throw new ArgumentNullException(nameof(statDb));
            _CharacterRepository = charRepo;
            _BucketDb = bucketDb;
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

    }
}
