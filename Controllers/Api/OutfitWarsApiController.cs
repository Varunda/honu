﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Models;
using watchtower.Models.Api;
using watchtower.Models.Census;
using watchtower.Services.Census;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("/api/outfit-wars")]
    public class OutfitWarsApiController : ApiControllerBase {

        private readonly ILogger<OutfitWarsApiController> _Logger;

        private readonly OutfitWarsOutfitCollection _OutfitRegistrationCensus;
        private readonly OutfitWarsMatchCollection _MatchCensus;

        private readonly CharacterRepository _CharacterRepository;
        private readonly OutfitRepository _OutfitRepository;

        public OutfitWarsApiController(ILogger<OutfitWarsApiController> logger,
            OutfitRepository outfitRepository, OutfitWarsOutfitCollection outfitCensus,
            CharacterRepository characterRepository, OutfitWarsMatchCollection matchCensus) {

            _Logger = logger;

            _OutfitRepository = outfitRepository;
            _OutfitRegistrationCensus = outfitCensus;
            _CharacterRepository = characterRepository;
            _MatchCensus = matchCensus;
        }

        /// <summary>
        ///     Get all outfits participating in outfit wars
        /// </summary>
        /// <response code="200">
        ///     A list of <see cref="ExpandedOutfitWarsOutfit"/>
        /// </response>
        [HttpGet("registration")]
        public async Task<ApiResponse<List<ExpandedOutfitWarsOutfit>>> GetParticipants() {
            List<OutfitWarsOutfit> entries = await _OutfitRegistrationCensus.GetAll(CancellationToken.None);

            Dictionary<string, PsOutfit> outfits = 
                (await _OutfitRepository.GetByIDs(entries.Select(iter => iter.OutfitID).Distinct().ToList(), fast: true))
                .ToDictionary(iter => iter.ID);

            Dictionary<string, PsCharacter> leaders = 
                (await _CharacterRepository.GetByIDs(outfits.Values.Select(iter => iter.LeaderID), CensusEnvironment.PC, fast: true))
                .ToDictionary(iter => iter.ID);

            List<ExpandedOutfitWarsOutfit> ex = new();

            foreach (OutfitWarsOutfit o in entries) {

                PsOutfit? oo = outfits.GetValueOrDefault(o.OutfitID);
                if (oo != null) {
                    PsCharacter? leader = leaders.GetValueOrDefault(oo.LeaderID);
                    oo.WorldID = leader?.WorldID;
                }

                ex.Add(new ExpandedOutfitWarsOutfit() {
                    Entry = o,
                    Outfit = oo
                });
            }

            return ApiOk(ex);
        }

        [HttpGet("match")]
        public async Task<ApiResponse<List<ExpandedOutfitWarsMatch>>> GetMatches() {
            List<OutfitWarsMatch> matches = await _MatchCensus.GetAll(CancellationToken.None);

            List<string> outfitIDs = matches.Select(iter => iter.OutfitAId).Union(matches.Select(iter => iter.OutfitBId)).Distinct().ToList();
            Dictionary<string, PsOutfit> outfits = (await _OutfitRepository.GetByIDs(outfitIDs, fast: true)).ToDictionary(iter => iter.ID);

            Dictionary<string, PsCharacter> leaders = 
                (await _CharacterRepository.GetByIDs(outfits.Values.Select(iter => iter.LeaderID), CensusEnvironment.PC, fast: true))
                .ToDictionary(iter => iter.ID);

            List<ExpandedOutfitWarsMatch> ex = new();

            foreach (OutfitWarsMatch match in matches) {
                ex.Add(new ExpandedOutfitWarsMatch() {
                    Match = match,
                    OutfitA = outfits.GetValueOrDefault(match.OutfitAId),
                    OutfitB = outfits.GetValueOrDefault(match.OutfitBId)
                });
            }

            return ApiOk(ex);
        }

    }
}
