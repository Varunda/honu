using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Models;
using watchtower.Models.Api;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Queues;
using watchtower.Services.Census;
using watchtower.Services.Db;
using watchtower.Services.Queues;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    /// <summary>
    ///     Endpoints for outfits
    /// </summary>
    [ApiController]
    [Route("/api/outfit/")]
    public class OutfitApiController : ApiControllerBase {

        private readonly ILogger<OutfitApiController> _Logger;

        private readonly OutfitRepository _OutfitRepository;
        private readonly OutfitCollection _OutfitCollection;
        private readonly CharacterHistoryStatDbStore _CharacterHistoryStatDb;
        private readonly OutfitDbStore _OutfitDb;
        private readonly CharacterRepository _CharacterRepository;

        private readonly CharacterUpdateQueue _CacheQueue;
        private readonly CharacterCacheQueue _CharacterQueue;

        public OutfitApiController(ILogger<OutfitApiController> logger,
            OutfitRepository outfitRepo, OutfitCollection outfitCollection,
            CharacterHistoryStatDbStore histDb,
            CharacterUpdateQueue cacheQueue, OutfitDbStore outfitDb,
            CharacterCacheQueue charQueue, CharacterRepository characterRepository) {

            _Logger = logger;

            _OutfitRepository = outfitRepo ?? throw new ArgumentNullException(nameof(outfitRepo));
            _OutfitCollection = outfitCollection ?? throw new ArgumentNullException(nameof(outfitCollection));
            _CharacterHistoryStatDb = histDb;
            _OutfitDb = outfitDb;

            _CacheQueue = cacheQueue;
            _CharacterQueue = charQueue;
            _CharacterRepository = characterRepository;
        }

        /// <summary>
        ///     Get an outfit by its ID
        /// </summary>
        /// <remarks>
        ///     Basically a wrapper around the outfit collection, but also gets data from the local DB
        ///     first, allowing for quicker data, and getting deleted outfits
        /// </remarks>
        /// <param name="outfitID">ID of the outfit to get</param>
        /// <param name="includeWorldID">
        ///     Will the world ID of the leader of the faction be included? If false, then <see cref="PsOutfit.WorldID"/>
        ///     will always be null
        /// </param>
        /// <response code="200">
        ///     The response will contain the <see cref="PsOutfit"/>
        ///     with <see cref="PsOutfit.ID"/> of <paramref name="outfitID"/>
        /// </response>
        [HttpGet("{outfitID}")]
        public async Task<ApiResponse<PsOutfit>> GetByID(string outfitID, [FromQuery] bool includeWorldID = false) {
            PsOutfit? outfit = await _OutfitRepository.GetByID(outfitID);
            if (outfit == null) {
                return ApiNoContent<PsOutfit>();
            }

            if (includeWorldID == true) {
                PsCharacter? leader = await _CharacterRepository.GetByID(outfit.LeaderID, CensusEnvironment.PC, fast: true);
                outfit.WorldID = leader?.WorldID;
            }

            return ApiOk(outfit);
        }

        /// <summary>
        ///     Get multiple outfits in one API call
        /// </summary>
        /// <param name="IDs">List of outfit IDs to get the outfits of</param>
        [HttpGet("many")]
        public async Task<ApiResponse<List<PsOutfit>>> GetByIDs([FromQuery] List<string> IDs) {
            if (IDs.Count > 200) {
                return ApiBadRequest<List<PsOutfit>>($"cannot request more than 200 outfits in this API method");
            }

            List<PsOutfit> outfits = await _OutfitRepository.GetByIDs(IDs);

            return ApiOk(outfits);
        }

        /// <summary>
        ///     Get all outfits that use the given tag (case insensitive)
        /// </summary>
        /// <remarks>
        ///     Since outfits can change their tag whenever, tags may not always be unique if an outfit is deleted
        /// </remarks>
        /// <param name="outfitTag">Tag of the outfits to get</param>
        /// <param name="includeWorldID">
        ///     Will the world ID of the leader of the faction be included? If false, then <see cref="PsOutfit.WorldID"/>
        ///     will always be null
        /// </param>
        /// <response code="200">
        ///     The response will contain a list of all <see cref="PsOutfit"/>
        ///     with <see cref="PsOutfit.Tag"/> of <paramref name="outfitTag"/>
        /// </response>
        [HttpGet("tag/{outfitTag}")]
        public async Task<ApiResponse<List<PsOutfit>>> GetByTag(string outfitTag, [FromQuery] bool includeWorldID = false) {
            List<PsOutfit> outfits = await _OutfitRepository.GetByTag(outfitTag);

            if (includeWorldID == true) {
                List<string> leaderIDs = outfits.Select(iter => iter.LeaderID).ToList();
                List<PsCharacter> chars = await _CharacterRepository.GetByIDs(leaderIDs, CensusEnvironment.PC, fast: true);

                foreach (PsOutfit o in outfits) {
                    PsCharacter? leader = chars.FirstOrDefault(iter => iter.ID == o.LeaderID);
                    o.WorldID = leader?.WorldID;
                }
            }

            return ApiOk(outfits);
        }

        /// <summary>
        ///     Search an outfit by name
        /// </summary>
        /// <param name="tagOrName">Name to search by</param>
        /// <response code="200">
        ///     The response will contain a list of all <see cref="PsOutfit"/> with <see cref="PsOutfit.Name"/>
        ///     contained in <paramref name="tagOrName"/>, or the tag exactly matches 
        /// </response>
        [HttpGet("search/{tagOrName}")]
        public async Task<ApiResponse<List<PsOutfit>>> SearchByName(string tagOrName) {
            List<PsOutfit> outfits = await _OutfitRepository.Search(tagOrName);
            return ApiOk(outfits);
        }

        /// <summary>
        ///     Get the members of a PC outfit
        /// </summary>
        /// <remarks>
        ///     The character data includes the characters stats, if present in the DB. If the stats are not present
        ///     in the local DB, the character is put into a queue of characters to cache
        ///     <br/><br/>
        ///     This acts as a wrapper around the /outfit_member collection, and will not show historical data,
        ///     and will not include deleted characters. If an outfit is deleted, this will return an empty list
        /// </remarks>
        /// <param name="outfitID">ID of the outfit</param>
        /// <param name="includeStats">If the characters stats will be included, defaults to true</param>
        /// <response code="200">
        ///     The response will contain a list of outfit members that 
        /// </response>
        /// <response code="404">
        ///     No <see cref="PsOutfit"/> with <see cref="PsOutfit.ID"/> of <paramref name="outfitID"/> exists
        /// </response>
        [HttpGet("{outfitID}/members")]
        public async Task<ApiResponse<List<ExpandedOutfitMember>>> GetMembers(string outfitID, [FromQuery] bool includeStats = true) {
            PsOutfit? outfit = await _OutfitRepository.GetByID(outfitID);
            if (outfit == null) {
                return ApiNotFound<List<ExpandedOutfitMember>>($"{nameof(PsOutfit)} {outfitID}");
            }

            Stopwatch timer = Stopwatch.StartNew();

            List<OutfitMember> members = await _OutfitCollection.GetMembers(outfitID);
            List<ExpandedOutfitMember> expanded = new List<ExpandedOutfitMember>(members.Count);
            long getMembersMs = timer.ElapsedMilliseconds; timer.Restart();

            List<string> characterIDs = members.Select(iter => iter.CharacterID).ToList();

            // For large outfits, like SKL, it's much much quicker to load all the characters in one DB call,
            //      instead of making thousands of small calls. The characters are then put into a map, cause trying
            //      to iterate thru that list for each character is n^2, and when you have 5k members,
            //      that's 25 million iterations, so instead make a lookup table
            //List<PsCharacter> listCharacters = await _CharacterDb.GetByIDs(characterIDs);
            List<PsCharacter> listCharacters = await _CharacterRepository.GetByIDs(characterIDs, CensusEnvironment.PC, fast: true);
            long loadCharsMs = timer.ElapsedMilliseconds; timer.Restart();

            Dictionary<string, PsCharacter> charMap = new Dictionary<string, PsCharacter>(members.Count); // lookup table
            foreach (PsCharacter c in listCharacters) {
                if (c.DateLastLogin == DateTime.MinValue) {
                    _CacheQueue.Queue(c.ID);
                }

                charMap.Add(c.ID, c);
            }

            long processCharMs = timer.ElapsedMilliseconds; timer.Restart();


            foreach (OutfitMember member in members) {
                ExpandedOutfitMember ex = new();
                ex.Member = member;

                charMap.TryGetValue(member.CharacterID, out PsCharacter? c);
                ex.Character = c;

                expanded.Add(ex);
            }

            if (includeStats == true) {
                // Same thing but even more important. If you have 5k members, each with 10 stats, iterating thru them to find just the ones
                //      for the current character iterations =
                //      5k members * 10 stats = 50k
                //      5k members * 50k iterations = 250'000'000 iterations, no good
                List<PsCharacterHistoryStat> listStats = new();
                try {
                    listStats = await _CharacterHistoryStatDb.GetByCharacterIDs(characterIDs);
                } catch (NpgsqlException ex) {
                    if (ex.InnerException is TimeoutException) {
                        _Logger.LogWarning($"Timeout getting character history stats for {outfitID}");
                    } else {
                        throw;
                    }
                }
                long loadHistMs = timer.ElapsedMilliseconds; timer.Restart();

                Dictionary<string, List<PsCharacterHistoryStat>> statMap = new(members.Count);
                foreach (PsCharacterHistoryStat stat in listStats) {
                    if (statMap.ContainsKey(stat.CharacterID) == false) {
                        statMap.Add(stat.CharacterID, new List<PsCharacterHistoryStat>());
                    }

                    statMap[stat.CharacterID].Add(stat);
                }
                long processHistMs = timer.ElapsedMilliseconds; timer.Restart();

                foreach (ExpandedOutfitMember ex in expanded) {
                    string charID = ex.Member.CharacterID;

                    lock (CharacterStore.Get().Players) {
                        CharacterStore.Get().Players.TryGetValue(charID, out TrackedPlayer? player);
                        ex.Online = player?.Online ?? false;
                    }

                    bool hasCached = false;

                    // Load the character from the lookup table
                    _ = charMap.TryGetValue(ex.Member.CharacterID, out PsCharacter? c);
                    ex.Character = c;

                    // Character was not in the local DB, add to the queue to be cached
                    if (ex.Character == null) {
                        _CharacterQueue.Queue(new CharacterFetchQueueEntry() { CharacterID = charID, Store = false });
                        hasCached = true;
                    }

                    // Only send the kills, deaths, time and score stats. Don't bother sending the ones not displayed
                    _ = statMap.TryGetValue(ex.Member.CharacterID, out List<PsCharacterHistoryStat>? stats);
                    if (stats != null) {
                        ex.Stats = stats.Where(iter => iter.Type == "kills" || iter.Type == "deaths" || iter.Type == "time" || iter.Type == "score").ToList();
                    }

                    // If they have no stats (cause we load from DB not census), assume they haven't been pulled, so do so
                    if ((stats == null || stats.Count == 0) && hasCached == false) {
                        _CacheQueue.Queue(charID);
                        _CharacterQueue.Queue(new CharacterFetchQueueEntry() { CharacterID = charID, Store = false });
                    }
                }

                long processExMs = timer.ElapsedMilliseconds; timer.Restart();

                _Logger.LogInformation($"{outfitID}/{outfit.Name} get members: {getMembersMs} ({characterIDs.Count}), "
                    + $"load char db: {loadCharsMs}, process char ms: {processCharMs}, "
                    + $"load hist ms: {loadHistMs}, process hist ms: {processHistMs}, "
                    + $"process ex: {processExMs}");
            }

            return ApiOk(expanded);
        }

        /// <summary>
        ///     Get the activity statistics for an outfit, broken into 1 hour intervals
        /// </summary>
        /// <remarks>
        ///     <paramref name="start"/> is truncated down to the current hour.
        ///         For example, 2022-07-01T23:59 would be truncated to 2022-07-01T23:00
        ///         
        ///     <paramref name="finish"/> is truncted up to the next hour.
        ///         For example, 2022-07-02T00:01 would be truncated to 2022-07-03T01:00
        ///         
        ///     Because the outfit_id of a character is stored at the time of a session, this will include
        ///         characters that have changed outfits, but at the time of the session were in the outfit
        /// </remarks>
        /// <param name="outfitID">ID of the outfit</param>
        /// <param name="start">When the range to look at actvity starts</param>
        /// <param name="finish">When the range to look at activity ends</param>
        /// <response code="200">
        ///     The response will return a list of <see cref="OutfitActivity"/>s that contains how
        ///     many members in this outfit were online at a time
        /// </response>
        /// <response code="400">
        ///     An invalid request was made. An invalid request can be caused by:
        ///     <ul>
        ///         <li>
        ///             <paramref name="finish"/> came before <paramref name="start"/>
        ///         </li>
        ///         <li>
        ///             <paramref name="start"/> and <paramref name="finish"/> were more than 30 days apart
        ///         </li>
        ///     </ul>
        /// </response>
        /// <response code="404">
        ///     No <see cref="PsOutfit"/> with ID of <paramref name="outfitID"/> exists
        /// </response>
        [HttpGet("{outfitID}/activity")]
        public async Task<ApiResponse<List<OutfitActivity>>> GetActivity(string outfitID,
            [FromQuery] DateTime start, [FromQuery] DateTime finish) {

            if (finish <= start) {
                return ApiBadRequest<List<OutfitActivity>>($"{nameof(start)} must come before {nameof(finish)}");
            }
            if (finish - start >= TimeSpan.FromDays(31)) {
                return ApiBadRequest<List<OutfitActivity>>($"{nameof(start)} and {nameof(finish)} cannot be more than 30 days apart. Are {finish - start} apart");
            }

            PsOutfit? outfit = await _OutfitRepository.GetByID(outfitID);
            if (outfit == null) {
                return ApiNotFound<List<OutfitActivity>>($"{nameof(PsOutfit)} {outfitID}");
            }

            start = new DateTime(start.Ticks - start.Ticks % TimeSpan.TicksPerHour);
            finish = new DateTime(finish.Ticks - finish.Ticks % TimeSpan.TicksPerHour).AddHours(1);

            _Logger.LogInformation($"getting activity for {outfitID}/{outfit.Name} between {start:u} and {finish:u}");

            List<OutfitActivityDbEntry> dbEntries = await _OutfitDb.GetActivity(outfitID, start, finish);

            List<OutfitActivity> ret = new();

            for (DateTime i = start; i < finish; i = i.AddHours(1)) {
                OutfitActivity act = new();
                act.OutfitID = outfitID;
                act.Timestamp = i;
                act.IntervalSeconds = 3600;
                act.Count = dbEntries.FirstOrDefault(iter => iter.Timestamp == i)?.Count ?? 0;

                //_Logger.LogDebug($"{i:u} {act.Count}");

                ret.Add(act);
            }

            return ApiOk(ret);
        }

    }
}
