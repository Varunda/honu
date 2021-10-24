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
using watchtower.Models.Db;
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
        private readonly ISessionDbStore _SessionDb;

        public KillApiController(ILogger<KillApiController> logger,
            ICharacterRepository charRepo, IKillEventDbStore killDb,
            IItemRepository itemRepo, IOutfitRepository outfitRepo,
            ISessionDbStore sessionDb) {

            _Logger = logger;

            _CharacterRepository = charRepo ?? throw new ArgumentNullException(nameof(charRepo));
            _ItemRepository = itemRepo ?? throw new ArgumentNullException(nameof(itemRepo));
            _OutfitRepository = outfitRepo ?? throw new ArgumentNullException(nameof(outfitRepo));

            _KillDbStore = killDb ?? throw new ArgumentNullException(nameof(killDb));
            _SessionDb = sessionDb ?? throw new ArgumentNullException(nameof(sessionDb));
        }

        [HttpGet("session/{sessionID}")]
        public async Task<ActionResult<List<ExpandedKillEvent>>> GetBySessionID(long sessionID) {
            Session? session = await _SessionDb.GetByID(sessionID);
            if (session == null) {
                return NotFound($"{nameof(Session)} {sessionID}");
            }

            List<KillEvent> events = await _KillDbStore.GetKillsByCharacterID(session.CharacterID, session.Start, session.End ?? DateTime.UtcNow);

            List<ExpandedKillEvent> expanded = new List<ExpandedKillEvent>(events.Count);

            foreach (KillEvent ev in events) {
                ExpandedKillEvent ex = new ExpandedKillEvent();

                ex.Event = ev;
                ex.Attacker = await _CharacterRepository.GetByID(ev.AttackerCharacterID);
                ex.Killed = await _CharacterRepository.GetByID(ev.KilledCharacterID);
                ex.Item = await _ItemRepository.GetByID(ev.WeaponID);

                expanded.Add(ex);
            }

            return Ok(expanded);
        }

        [HttpGet("character/{charID}")]
        public async Task<ActionResult<List<CharacterWeaponKillEntry>>> CharacterKills(string charID) {
            PsCharacter? c = await _CharacterRepository.GetByID(charID);

            if (c == null) {
                return NotFound($"{nameof(PsCharacter)} {charID}");
            }

            List<KillEvent> kills = await _KillDbStore.GetRecentKillsByCharacterID(charID, 120);

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
