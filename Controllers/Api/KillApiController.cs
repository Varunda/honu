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

    /// <summary>
    ///     Endpoints for kill events
    /// </summary>
    [ApiController]
    [Route("/api/kills")]
    public class KillApiController : ApiControllerBase {

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

        /// <summary>
        ///     Get the kills and deaths that occured in a session
        /// </summary>
        /// <remarks>
        ///     The character ID used to find the kills and deaths is based on the <see cref="Session.CharacterID"/>
        ///     of the <see cref="Session"/> with <see cref="Session.ID"/> of <paramref name="sessionID"/>.
        ///     This is the session owner
        ///     <br/><br/>
        ///     The response will contain a list of all <see cref="ExpandedKillEvent"/>s that have the session owner (see above)
        ///     as the character killed, or the character attacker. To get the amount of kills or deaths, you must check
        ///     <see cref="ExpandedKillEvent.Event"/> and see what the value of <see cref="KillEvent.AttackerCharacterID"/>
        ///     and <see cref="KillEvent.KilledCharacterID"/> is
        /// </remarks>
        /// <param name="sessionID">ID of the session</param>
        /// <response code="200">
        ///     The response will contain all <see cref="ExpandedKillEvent"/>s that occured in the <see cref="Session"/>
        ///     with <see cref="Session.ID"/> of <paramref name="sessionID"/>, and had the killed character or attacking character
        ///     as the character who the session is for (based on <see cref="Session.CharacterID"/>)
        /// </response>
        /// <response code="404">
        ///     No <see cref="Session"/> with <see cref="Session.ID"/> of <paramref name="sessionID"/> exists
        /// </response>
        [HttpGet("session/{sessionID}")]
        public async Task<ApiResponse<List<ExpandedKillEvent>>> GetBySessionID(long sessionID) {
            Session? session = await _SessionDb.GetByID(sessionID);
            if (session == null) {
                return ApiNotFound<List<ExpandedKillEvent>>($"{nameof(Session)} {sessionID}");
            }

            List<KillEvent> events = await _KillDbStore.GetKillsByCharacterID(session.CharacterID, session.Start, session.End ?? DateTime.UtcNow);

            List<ExpandedKillEvent> expanded = new List<ExpandedKillEvent>(events.Count);

            Dictionary<string, PsCharacter?> chars = new Dictionary<string, PsCharacter?>();
            Dictionary<string, PsItem?> items = new Dictionary<string, PsItem?>();

            foreach (KillEvent ev in events) {
                ExpandedKillEvent ex = new ExpandedKillEvent();

                ex.Event = ev;

                if (chars.ContainsKey(ev.AttackerCharacterID) == false) {
                    chars.Add(ev.AttackerCharacterID, await _CharacterRepository.GetByID(ev.AttackerCharacterID));
                }
                if (chars.ContainsKey(ev.KilledCharacterID) == false) {
                    chars.Add(ev.KilledCharacterID, await _CharacterRepository.GetByID(ev.KilledCharacterID));
                }
                if (items.ContainsKey(ev.WeaponID) == false) {
                    items.Add(ev.WeaponID, await _ItemRepository.GetByID(ev.WeaponID));
                }

                ex.Attacker = chars[ev.AttackerCharacterID];
                ex.Killed = chars[ev.KilledCharacterID];
                ex.Item = items[ev.WeaponID];

                expanded.Add(ex);
            }

            return ApiOk(expanded);
        }

        /// <summary>
        ///     Get the weapons a character has used in the last 2 hours
        /// </summary>
        /// <remarks>
        ///     Used for the realtime server view, when you click on the character in the top killers list
        ///     <br/><br/>
        ///     Get the weapons a character used to get kills in the last 2 hours. This excludes TKs
        /// </remarks>
        /// <param name="charID">ID of the character</param>
        /// <response code="200">
        ///     The response will contain the <see cref="CharacterWeaponKillEntry"/>s for the character
        ///     passed in <paramref name="charID"/> and within the last 2 hours
        /// </response>
        /// <response code="404">
        ///     No <see cref="PsCharacter"/> with <see cref="PsCharacter.ID"/> of <paramref name="charID"/> exists
        /// </response>
        [HttpGet("character/{charID}")]
        public async Task<ApiResponse<List<CharacterWeaponKillEntry>>> CharacterKills(string charID) {
            PsCharacter? c = await _CharacterRepository.GetByID(charID);

            if (c == null) {
                return ApiNotFound<List<CharacterWeaponKillEntry>>($"{nameof(PsCharacter)} {charID}");
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

            return ApiOk(list);
        }

        /// <summary>
        ///     Get the characters who have gotten the most kills in an outfit in the last 2 hours
        /// </summary>
        /// <remarks>
        ///      Used for the realtime server view, when you click on the amount of kills an outfit has gotten
        ///      <br/><br/>
        ///      Get the top killers in an outfit in the last 2 hours
        /// </remarks>
        /// <param name="outfitID"></param>
        /// <response code="200">
        ///     The response will contain a list of <see cref="OutfitKillerEntry"/>s for the
        ///     <see cref="PsOutfit"/> with <see cref="PsOutfit.ID"/> of <paramref name="outfitID"/>
        /// </response>
        /// <response code="404">
        ///     No <see cref="PsOutfit"/> with <see cref="PsOutfit.ID"/> of <paramref name="outfitID"/> exists
        /// </response>
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
