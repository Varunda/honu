using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IKillEventDbStore _KillDbStore;

        public KillApiController(ILogger<KillApiController> logger,
            ICharacterRepository charRepo, IKillEventDbStore killDb,
            IItemRepository itemRepo) {

            _Logger = logger;

            _CharacterRepository = charRepo ?? throw new ArgumentNullException(nameof(charRepo));
            _ItemRepository = itemRepo ?? throw new ArgumentNullException(nameof(itemRepo));
            _KillDbStore = killDb ?? throw new ArgumentNullException(nameof(killDb));
        }

        [HttpGet("character/{charID}")]
        public async Task<ActionResult<List<CharacterWeaponKillEntry>>> CharacterKills(string charID) {
            PsCharacter? c = await _CharacterRepository.GetByID(charID);

            if (c == null) {
                return NoContent();
            }

            List<KillEvent> kills = await _KillDbStore.GetByCharacterID(charID, 120);

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
                        WeaponName = item?.Name ?? $"<Missing {ev.WeaponID}>"
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

    }
}
