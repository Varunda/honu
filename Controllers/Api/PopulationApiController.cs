using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Models;
using watchtower.Models.Api;
using watchtower.Models.Db;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    /// <summary>
    ///     Endpoints for getting world populations
    /// </summary>
    [ApiController]
    [Route("/api/population")]
    public class PopulationApiController : ApiControllerBase {

        private readonly ILogger<PopulationApiController> _Logger;
        private readonly OutfitDbStore _OutfitDb;
        private readonly WorldPopulationRepository _PopulationRepository;
        private readonly PopulationDbStore _PopulationDb;

        public PopulationApiController(ILogger<PopulationApiController> logger,
            OutfitDbStore outfitRepo, WorldPopulationRepository popRepo,
            PopulationDbStore populationDb) {

            _Logger = logger;

            _OutfitDb = outfitRepo;
            _PopulationRepository = popRepo ?? throw new ArgumentNullException(nameof(popRepo));
            _PopulationDb = populationDb;
        }

        /// <summary>
        ///     Get how many members outfits have online at a specific time
        /// </summary>
        /// <remarks>
        ///     This data is based on session data, which includes the outfit ID of a character when created. This means
        ///     the data will stay the same even if a character changes outfits, or if a character is deleted
        /// </remarks>
        /// <param name="worldID">ID of the world to get the outfits of</param>
        /// <param name="time">When to do the lookup. If left null, it will default to the current time</param>
        /// <response code="200">
        ///     The response will contain a list of <see cref="OutfitPopulation"/>s
        /// </response>
        [HttpGet("{worldID}/outfits")]
        public async Task<ApiResponse<List<OutfitPopulation>>> GetOutfits(short worldID, [FromQuery] DateTime? time) {
            DateTime when = time ?? DateTime.UtcNow;

            List<OutfitPopulation> pops = await _OutfitDb.GetPopulation(when, worldID);
            return ApiOk(pops);
        }

        /// <summary>
        ///     Get the current world population
        /// </summary>
        /// <param name="worldID">ID of the world</param>
        /// <response code="200">
        ///     The response will contain the <see cref="WorldPopulation"/> for the 
        ///     world passed in <paramref name="worldID"/>
        /// </response>
        [HttpGet("{worldID}")]
        public ApiResponse<WorldPopulation> Get(short worldID) {
            WorldPopulation pop = _PopulationRepository.GetByWorldID(worldID);
            return ApiOk(pop);
        }

        /// <summary>
        ///     Get the current zone population of a world
        /// </summary>
        /// <param name="worldID">ID of the world</param>
        /// <response code="200">
        ///     The response will contain a list of <see cref="ZonePopulation"/>s 
        ///     for the world passed in <paramref name="worldID"/>
        /// </response>
        [HttpGet("{worldID}/zones")]
        public ApiResponse<List<ZonePopulation>> GetZoneByWorldID(short worldID) {
            List<ZonePopulation> pops = _PopulationRepository.GetZonesByWorldID(worldID);
            return ApiOk(pops);
        }

        /// <summary>
        ///     Get the current world population of multiple worlds
        /// </summary>
        /// <param name="worldID">List of world IDs to include. If an invalid world ID is passed, it is not included in the response</param>
        /// <response code="200">
        ///     The response will contain a list of <see cref="WorldPopulation"/>s, one for each world ID passed that is a valid world
        ///     Entries that are not a valid world ID (as defined at <see cref="World.All"/>), will not be given an entry.
        ///     If only invalid world IDs were passed (such as 30), the list will empty
        /// </response>
        [HttpGet("multiple")]
        public ApiResponse<List<WorldPopulation>> GetMultiple([FromQuery] List<short> worldID) {
            List<WorldPopulation> pops = new List<WorldPopulation>();

            foreach (short id in worldID) {
                if (World.All.Contains(id) == false) {
                    continue;
                }

                pops.Add(_PopulationRepository.GetByWorldID(id));
            }

            return ApiOk(pops);
        }

        /// <summary>
        ///     Get historical population data between two time periods, optionally filtering by world and faction
        /// </summary>
        /// <remarks>
        ///     If <paramref name="worlds"/> and <paramref name="factions"/> are not provided, 
        ///     they have default behaviors described in the parameter documentation
        ///     <br/><br/>
        ///     Example queries:
        ///     <br/>
        ///     Loading the month of August, all worlds combined into one value, all factions combined into one:<br/>
        ///     <code>&amp;start=2022-08-01T00:00&amp;end=2022-08-31T23:59</code>
        ///     <br/><br/>
        ///     Loading the month of August, filtered for only Connery NC<br/>
        ///     <code>&amp;start=2022-08-01T00:00&amp;end=2022-08-31T23:59&amp;worlds=1&amp;factions=2</code>
        ///     <br/><br/>
        ///     Loading the month of August, filtered for only Emerald and Cobalt, all factions broken out<br/>
        ///     <code>&amp;start=2022-08-01T00:00&amp;end=2022-08-31T23:59&amp;worlds=19&amp;worlds=13&amp;factions=1&amp;factions=2&amp;factions=3&amp;factions=4</code>
        /// </remarks>
        /// <param name="start">Start of the range where historical population data will be loaded</param>
        /// <param name="end">End of the range (exclusive) where historical population data will be loaded</param>
        /// <param name="worlds">
        ///     Which worlds to include in the response. If no values are provided, it is assumed you want
        ///     population data across all worlds combined into one value (world_id = 0).
        ///     If you want to filter for specific worlds, you must set this parameter.
        ///     <br/>
        ///     For example, if you wanted to compare Connery (world 1) to Emerald (world 17), you'd have
        ///     <code>&amp;worlds=1&amp;worlds=17</code>
        /// </param>
        /// <param name="factions">
        ///     Which factions to include in the response. If no values are provided, it is assumed you want
        ///     population data across all factions combined into one value (faction_id = 0).
        ///     If you want to filter for specific factions, you must set this parameter.
        ///     <br/>
        ///     For example, if you want to load only VS and NC data, you'd have
        ///     <code>&amp;factions=1&amp;factions=2</code>
        /// </param>
        /// <response code="200">
        ///     The response will contain a list of <see cref="PopulationEntry"/>s that match the 
        ///     filters given. See the parameter documentation about the filters
        /// </response>
        /// <response code="400">
        ///     One of the parameters passed failed validation:
        ///     <ul>
        ///         <li><paramref name="start"/> was the default value</li>
        ///         <li><paramref name="end"/> was the default value</li>
        ///         <li><paramref name="start"/> came after <paramref name="end"/></li>
        ///     </ul>
        /// </response>
        [HttpGet("historical")]
        public async Task<ApiResponse<List<PopulationEntry>>> GetByRange(
            [FromQuery] DateTime start,
            [FromQuery] DateTime end,
            [FromQuery] List<short> worlds,
            [FromQuery] List<short> factions) {

            if (start == default) {
                return ApiBadRequest<List<PopulationEntry>>($"{nameof(start)} must be provided");
            }

            if (end == default) {
                return ApiBadRequest<List<PopulationEntry>>($"{nameof(end)} must be provided");
            }

            if (start >= end) {
                return ApiBadRequest<List<PopulationEntry>>($"{nameof(start)} must come before {nameof(end)}");
            }

            List<PopulationEntry> entries = await _PopulationDb.GetByTimestampRange(start, end);

            if (worlds.Count > 0) {
                entries = entries.Where(iter => worlds.Contains(iter.WorldID)).ToList();
            } else {
                entries = entries.Where(iter => iter.WorldID == 0).ToList();
            }

            if (factions.Count > 0) {
                entries = entries.Where(iter => factions.Contains(iter.FactionID)).ToList();
            } else {
                entries = entries.Where(iter => iter.FactionID == 0).ToList();
            }

            return ApiOk(entries);
        }

    }
}
