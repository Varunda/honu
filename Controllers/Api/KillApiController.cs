using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using watchtower.Models;
using watchtower.Models.Api;
using watchtower.Models.Census;
using watchtower.Models.Events;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Controllers {

    [ApiController]
    [Route("/api/kills")]
    public class KillApiController : ControllerBase {

        private readonly ILogger<KillApiController> _Logger;

        private readonly ICharacterRepository _CharacterRepository;
        private readonly IItemRepository _ItemRepository;
        private readonly IOutfitRepository _OutfitRepository;

        private readonly IKillEventDbStore _KillDbStore;

        public KillApiController(ILogger<KillApiController> logger,
            ICharacterRepository charRepo, IKillEventDbStore killDb,
            IItemRepository itemRepo, IOutfitRepository outfitRepo) {

            _Logger = logger;

            _CharacterRepository = charRepo ?? throw new ArgumentNullException(nameof(charRepo));
            _ItemRepository = itemRepo ?? throw new ArgumentNullException(nameof(itemRepo));
            _OutfitRepository = outfitRepo ?? throw new ArgumentNullException(nameof(outfitRepo));

            _KillDbStore = killDb ?? throw new ArgumentNullException(nameof(killDb));
        }

        [HttpGet("character/{charID}")]
        public async Task<ActionResult<List<CharacterWeaponKillEntry>>> CharacterKills(string charID) {
            PsCharacter? c = await _CharacterRepository.GetByID(charID);

            if (c == null) {
                return NotFound($"{nameof(PsCharacter)} {charID}");
            }

            List<KillEvent> kills = await _KillDbStore.GetKillsByCharacterID(charID, 120);

            Dictionary<string, CharacterWeaponKillEntry> entries = new Dictionary<string, CharacterWeaponKillEntry>();

            foreach (KillEvent ev in kills) {
                // Skip character deaths or TKs
                if (ev.KilledCharacterID == charID || ev.AttackerTeamID == ev.KilledTeamID) {
                    continue;
                }

                if (entries.TryGetValue(ev.WeaponID, out CharacterWeaponKillEntry? entry) == false) {
                    PsItem? item = await _ItemRepository.GetByID(ev.WeaponID);

                    entry = new CharacterWeaponKillEntry() {
                        WeaponID = ev.WeaponID,
                        WeaponName = item?.Name ?? $"<missing {ev.WeaponID}>"
                    };
                }

                ++entry.Kills;
                if (ev.IsHeadshot == true) {
                    ++entry.HeadshotKills;
                }

                entries[ev.WeaponID] = entry;
            }

            List<CharacterWeaponKillEntry> list = entries.Values
                .OrderByDescending(iter => iter.Kills)
                .ThenByDescending(iter => iter.HeadshotKills)
                .ToList();

            return Ok(list);
        }

        [HttpGet("outfit/{outfitID}")]
        public async Task<ActionResult<List<OutfitKillerEntry>>> OutfitKills(string outfitID) {
            PsOutfit? outfit = await _OutfitRepository.GetByID(outfitID);

            if (outfit == null) {
                return NotFound($"outfit {outfitID}");
            }

            List<KillEvent> kills = await _KillDbStore.GetKillsByOutfitID(outfitID, 120);

            Dictionary<string, OutfitKillerEntry> entries = new Dictionary<string, OutfitKillerEntry>();

            foreach (KillEvent ev in kills) {
                // Skip character deaths or TKs
                if (ev.KilledCharacterID == ev.AttackerCharacterID || ev.AttackerTeamID == ev.KilledTeamID) {
                    continue;
                }

                if (entries.TryGetValue(ev.AttackerCharacterID, out OutfitKillerEntry? entry) == false) {
                    PsCharacter? character = await _CharacterRepository.GetByID(ev.AttackerCharacterID);
                    entry = new OutfitKillerEntry() {
                        CharacterID = ev.AttackerCharacterID,
                        CharacterName = character?.Name ?? $"Missing {ev.AttackerCharacterID}"
                    };
                }

                ++entry.Kills;

                entries[ev.AttackerCharacterID] = entry;
            }

            List<OutfitKillerEntry> list = entries.Values
                .OrderByDescending(iter => iter.Kills)
                .ToList();

            return Ok(list);
        }

    }
}
