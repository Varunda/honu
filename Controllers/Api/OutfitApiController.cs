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
        private readonly ICharacterRepository _CharacterRepository;
        private readonly ICharacterHistoryStatDbStore _CharacterHistoryStatDb;

        private readonly IBackgroundCharacterWeaponStatQueue _CacheQueue;

        public OutfitApiController(ILogger<OutfitApiController> logger,
            IOutfitRepository outfitRepo, IOutfitCollection outfitCollection,
            ICharacterRepository charRepo, ICharacterHistoryStatDbStore histDb,
            IBackgroundCharacterWeaponStatQueue cacheQueue) {

            _Logger = logger;

            _OutfitRepository = outfitRepo ?? throw new ArgumentNullException(nameof(outfitRepo));
            _OutfitCollection = outfitCollection ?? throw new ArgumentNullException(nameof(outfitCollection));
            _CharacterRepository = charRepo;
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

            List<ExpandedOutfitMember> expanded = new List<ExpandedOutfitMember>(members.Count);

            foreach (OutfitMember member in members) {
                ExpandedOutfitMember ex = new ExpandedOutfitMember() {
                    Member = member
                };

                ex.Character = await _CharacterRepository.GetByID(ex.Member.CharacterID);

                // Only send the kills, deaths, time and score stats. Don't bother sending the ones not displayed
                List<PsCharacterHistoryStat> stats = await _CharacterHistoryStatDb.GetByCharacterID(ex.Member.CharacterID);
                ex.Stats = stats.Where(iter => iter.Type == "kills" || iter.Type == "deaths" || iter.Type == "time" || iter.Type == "score").ToList();

                // If they have no stats (cause we load from DB not census), assume they haven't been pulled, so do so
                if (stats.Count == 0) {
                    _CacheQueue.Queue(member.CharacterID);
                }

                expanded.Add(ex);
            }

            return Ok(expanded);
        }

    }
}
