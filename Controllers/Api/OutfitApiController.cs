using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Api;
using watchtower.Models.Census;
using watchtower.Services;
using watchtower.Services.Census;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("/api/outfit/")]
    public class OutfitApiController : ControllerBase {

        private readonly ILogger<OutfitApiController> _Logger;

        private readonly IOutfitRepository _OutfitRepository;
        private readonly IOutfitCollection _OutfitCollection;
        private readonly ICharacterHistoryStatDbStore _CharacterHistoryStatDb;
        private readonly ICharacterDbStore _CharacterDb;

        private readonly IBackgroundCharacterWeaponStatQueue _CacheQueue;

        public OutfitApiController(ILogger<OutfitApiController> logger,
            IOutfitRepository outfitRepo, IOutfitCollection outfitCollection,
            ICharacterDbStore charDb, ICharacterHistoryStatDbStore histDb,
            IBackgroundCharacterWeaponStatQueue cacheQueue) {

            _Logger = logger;

            _OutfitRepository = outfitRepo ?? throw new ArgumentNullException(nameof(outfitRepo));
            _OutfitCollection = outfitCollection ?? throw new ArgumentNullException(nameof(outfitCollection));
            _CharacterDb = charDb;
            _CharacterHistoryStatDb = histDb;

            _CacheQueue = cacheQueue;
        }

        [HttpGet("{outfitID}")]
        public async Task<ActionResult<PsOutfit>> GetByID(string outfitID) {
            PsOutfit? outfit = await _OutfitRepository.GetByID(outfitID);
            if (outfit == null) {
                return NoContent();
            }

            return Ok(outfit);
        }

        [HttpGet("{outfitID}/members")]
        public async Task<ActionResult<List<ExpandedOutfitMember>>> GetMembers(string outfitID) {
            PsOutfit? outfit = await _OutfitRepository.GetByID(outfitID);
            if (outfit == null) {
                return NotFound($"{nameof(PsOutfit)} {outfitID}");
            }

            List<OutfitMember> members = await _OutfitCollection.GetMembers(outfitID);

            List<string> characterIDs = members.Select(iter => iter.CharacterID).ToList();

            // For large outfits, like SKL, it's much much quicker to load all the characters in one DB call,
            //      instead of making thousands of small calls. The characters are then put into a map, cause trying
            //      to iterate thru that list for each character is n^2, and when you have 5k members,
            //      that's 25 million iterations, so instead make a lookup table
            List<PsCharacter> listCharacters = await _CharacterDb.GetByIDs(characterIDs);
            Dictionary<string, PsCharacter> charMap = new Dictionary<string, PsCharacter>(members.Count); // lookup table
            foreach (PsCharacter c in listCharacters) {
                charMap.Add(c.ID, c);
            }

            // Same thing but even more important. If you have 5k members, each with 10 stats, iterating thru them to find just the ones
            //      for the current character iterations =
            //      5k members * 10 stats = 50k
            //      5k members * 50k iterations = 250'000'000 iterations, no good
            List<PsCharacterHistoryStat> listStats = await _CharacterHistoryStatDb.GetByCharacterIDs(characterIDs);
            Dictionary<string, List<PsCharacterHistoryStat>> statMap = new Dictionary<string, List<PsCharacterHistoryStat>>(members.Count);
            foreach (PsCharacterHistoryStat stat in listStats) {
                if (statMap.ContainsKey(stat.CharacterID) == false) {
                    statMap.Add(stat.CharacterID, new List<PsCharacterHistoryStat>());
                }

                statMap[stat.CharacterID].Add(stat);
            }

            List<ExpandedOutfitMember> expanded = new List<ExpandedOutfitMember>(members.Count);

            foreach (OutfitMember member in members) {
                ExpandedOutfitMember ex = new ExpandedOutfitMember() {
                    Member = member
                };

                bool hasCached = false;

                // Load the character from the lookup table
                _ = charMap.TryGetValue(ex.Member.CharacterID, out PsCharacter? c);
                ex.Character = c;

                if (ex.Character == null) {
                    _CacheQueue.Queue(member.CharacterID);
                    hasCached = true;
                }

                // Only send the kills, deaths, time and score stats. Don't bother sending the ones not displayed
                _ = statMap.TryGetValue(ex.Member.CharacterID, out List<PsCharacterHistoryStat>? stats);
                if (stats != null) {
                    ex.Stats = stats.Where(iter => iter.Type == "kills" || iter.Type == "deaths" || iter.Type == "time" || iter.Type == "score").ToList();
                }

                // If they have no stats (cause we load from DB not census), assume they haven't been pulled, so do so
                if ((stats == null || stats.Count == 0) && hasCached == false) {
                    _CacheQueue.Queue(member.CharacterID);
                }

                expanded.Add(ex);
            }

            return Ok(expanded);
        }

    }
}
