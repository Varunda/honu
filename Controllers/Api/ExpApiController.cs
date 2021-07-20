using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Models.Api;
using watchtower.Models.Census;
using watchtower.Models.Events;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Controllers {

    [ApiController]
    [Route("/api/exp")]
    public class ExpApiController : ControllerBase {

        private readonly ILogger<ExpApiController> _Logger;

        private readonly ICharacterRepository _CharacterRepository;
        private readonly IItemRepository _ItemRepository;
        private readonly IOutfitRepository _OutfitRepository;

        private readonly IExpEventDbStore _ExpDbStore;

        public ExpApiController(ILogger<ExpApiController> logger,
            ICharacterRepository charRepo, IExpEventDbStore killDb,
            IItemRepository itemRepo, IOutfitRepository outfitRepo) {

            _Logger = logger;

            _CharacterRepository = charRepo ?? throw new ArgumentNullException(nameof(charRepo));
            _ItemRepository = itemRepo ?? throw new ArgumentNullException(nameof(itemRepo));
            _OutfitRepository = outfitRepo ?? throw new ArgumentNullException(nameof(outfitRepo));

            _ExpDbStore = killDb ?? throw new ArgumentNullException(nameof(killDb));
        }

        [HttpGet("character/{charID}/heals")]
        public async Task<ActionResult<List<CharacterExpSupportEntry>>> CharacterHeals(string charID) {
            PsCharacter? c = await _CharacterRepository.GetByID(charID);

            if (c == null) {
                return NotFound($"{nameof(PsCharacter)} {charID}");
            }

            List<CharacterExpSupportEntry> list = 
                await GetByCharacterAndExpIDs(charID, new List<int> { Experience.HEAL, Experience.SQUAD_HEAL });

            return Ok(list);
        }

        [HttpGet("character/{charID}/revives")]
        public async Task<ActionResult<List<CharacterExpSupportEntry>>> CharacterRevives(string charID) {
            PsCharacter? c = await _CharacterRepository.GetByID(charID);

            if (c == null) {
                return NotFound($"{nameof(PsCharacter)} {charID}");
            }

            List<CharacterExpSupportEntry> list = 
                await GetByCharacterAndExpIDs(charID, new List<int> { Experience.REVIVE, Experience.SQUAD_REVIVE });

            return Ok(list);
        }

        [HttpGet("character/{charID}/resupplies")]
        public async Task<ActionResult<List<CharacterExpSupportEntry>>> CharacterResupplies(string charID) {
            PsCharacter? c = await _CharacterRepository.GetByID(charID);

            if (c == null) {
                return NotFound($"{nameof(PsCharacter)} {charID}");
            }

            List<CharacterExpSupportEntry> list = 
                await GetByCharacterAndExpIDs(charID, new List<int> { Experience.RESUPPLY, Experience.SQUAD_RESUPPLY });

            return Ok(list);
        }

        [HttpGet("character/{charID}/spawns")]
        public async Task<ActionResult<List<CharacterExpSupportEntry>>> CharacterSpawns(string charID) {
            PsCharacter? c = await _CharacterRepository.GetByID(charID);

            if (c == null) {
                return NotFound($"{nameof(PsCharacter)} {charID}");
            }

            List<ExpEvent> events = await _ExpDbStore.GetByCharacterID(charID, 120);

            CharacterExpSupportEntry sundySpawns = new CharacterExpSupportEntry() { CharacterName = "Sunderers" };
            CharacterExpSupportEntry routerSpawns = new CharacterExpSupportEntry() { CharacterName = "Routers" };
            CharacterExpSupportEntry squadSpawns = new CharacterExpSupportEntry() { CharacterName = "Squad" };
            CharacterExpSupportEntry squadVehicleSpawns = new CharacterExpSupportEntry() { CharacterName = "Squad vehicle" };

            foreach (ExpEvent ev in events) {
                if (ev.ExperienceID == Experience.SUNDERER_SPAWN_BONUS) {
                    ++sundySpawns.Amount;
                } else if (ev.ExperienceID == Experience.GENERIC_NPC_SPAWN) {
                    ++routerSpawns.Amount;
                } else if (ev.ExperienceID == Experience.SQUAD_SPAWN) {
                    ++squadSpawns.Amount;
                } else if (ev.ExperienceID == Experience.SQUAD_VEHICLE_SPAWN_BONUS) {
                    ++squadVehicleSpawns.Amount;
                }
            }

            List<CharacterExpSupportEntry> list = (new List<CharacterExpSupportEntry>() {
                sundySpawns, routerSpawns, squadSpawns, squadVehicleSpawns
            }).OrderByDescending(iter => iter.Amount).ToList();

            return Ok(list);
        }

        private async Task<List<CharacterExpSupportEntry>> GetByCharacterAndExpIDs(string charID, List<int> events) {
            List<ExpEvent> exps = await _ExpDbStore.GetByCharacterID(charID, 120);

            Dictionary<string, CharacterExpSupportEntry> entries = new Dictionary<string, CharacterExpSupportEntry>();

            foreach (ExpEvent ev in exps) {
                if (events.Contains(ev.ExperienceID) == false) {
                    continue;
                }

                if (entries.TryGetValue(ev.OtherID, out CharacterExpSupportEntry? entry) == false) {
                    PsCharacter? character = await _CharacterRepository.GetByID(ev.OtherID);

                    entry = new CharacterExpSupportEntry() {
                        CharacterID = ev.OtherID,
                        CharacterName = character?.GetDisplayName() ?? $"Missing {ev.OtherID}"
                    };
                }

                ++entry.Amount;

                entries[ev.OtherID] = entry;
            }

            List<CharacterExpSupportEntry> list = entries.Values
                .OrderByDescending(iter => iter.Amount)
                .ToList();

            return list;
        }

    }
}
