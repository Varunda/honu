using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using watchtower.Code.Constants;
using watchtower.Code.Tracking;
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

        private readonly CharacterRepository _CharacterRepository;
        private readonly ItemRepository _ItemRepository;
        private readonly OutfitRepository _OutfitRepository;
        private readonly FireGroupToFireModeRepository _FireGroupToFireModeRepository;
        private readonly ItemCategoryRepository _ItemCategoryRepository;

        private readonly KillEventDbStore _KillDbStore;
        private readonly SessionDbStore _SessionDb;

        public KillApiController(ILogger<KillApiController> logger,
            CharacterRepository charRepo, KillEventDbStore killDb,
            ItemRepository itemRepo, OutfitRepository outfitRepo,
            SessionDbStore sessionDb, FireGroupToFireModeRepository fireGroupToFireModeRepository,
            ItemCategoryRepository itemCategoryRepository) {

            _Logger = logger;

            _CharacterRepository = charRepo ?? throw new ArgumentNullException(nameof(charRepo));
            _ItemRepository = itemRepo ?? throw new ArgumentNullException(nameof(itemRepo));
            _OutfitRepository = outfitRepo ?? throw new ArgumentNullException(nameof(outfitRepo));

            _KillDbStore = killDb ?? throw new ArgumentNullException(nameof(killDb));
            _SessionDb = sessionDb ?? throw new ArgumentNullException(nameof(sessionDb));
            _FireGroupToFireModeRepository = fireGroupToFireModeRepository;
            _ItemCategoryRepository = itemCategoryRepository;
        }

        /// <summary>
        ///     Get the kills and deaths that occured in a session of a PC player
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
            using Activity? trace = HonuActivitySource.Root.StartActivity("get kills by session");
            trace?.AddTag("honu.sessionID", sessionID);

            Session? session = await _SessionDb.GetByID(sessionID);
            if (session == null) {
                return ApiNotFound<List<ExpandedKillEvent>>($"{nameof(Session)} {sessionID}");
            }

            List<KillEvent> events = await _KillDbStore.GetKillsByCharacterID(session.CharacterID, session.Start, session.End ?? DateTime.UtcNow);

            List<ExpandedKillEvent> expanded = new List<ExpandedKillEvent>(events.Count);

            Dictionary<string, PsCharacter?> chars = new Dictionary<string, PsCharacter?>();

            List<string> IDs = events.Select(iter => iter.AttackerCharacterID).Distinct().ToList();
            IDs.AddRange(events.Select(iter => iter.KilledCharacterID).Distinct());

            List<PsCharacter> characters = await _CharacterRepository.GetByIDs(IDs, CensusEnvironment.PC);
            foreach (PsCharacter c in characters) {
                if (chars.ContainsKey(c.ID) == false) {
                    chars.Add(c.ID, c);
                }
            }

            Dictionary<int, PsItem?> items = new();

            foreach (KillEvent ev in events) {
                ExpandedKillEvent ex = new();
                ex.Event = ev;

                if (items.ContainsKey(ev.WeaponID) == false) {
                    items.Add(ev.WeaponID, await _ItemRepository.GetByID(ev.WeaponID));
                }

                chars.TryGetValue(ev.AttackerCharacterID, out PsCharacter? attacker);
                chars.TryGetValue(ev.KilledCharacterID, out PsCharacter? killed);

                ex.Attacker = attacker;
                ex.Killed = killed;
                ex.Item = items[ev.WeaponID];
                ex.FireGroupToFireMode = (await _FireGroupToFireModeRepository.GetByFireModeID(ev.AttackerFireModeID)).ElementAtOrDefault(0);

                expanded.Add(ex);
            }

            return ApiOk(expanded);
        }

        [HttpGet("session/{sessionID}/block")]
        public async Task<ApiResponse<KillDeathBlock>> GetBySessionBlock(long sessionID) {
            using Activity? trace = HonuActivitySource.Root.StartActivity("get kills by session");
            trace?.AddTag("honu.sessionID", sessionID);

            Session? session = await _SessionDb.GetByID(sessionID);
            if (session == null) {
                return ApiNotFound<KillDeathBlock>($"{nameof(Session)} {sessionID}");
            }

            List<KillEvent> events = await _KillDbStore.GetKillsByCharacterID(session.CharacterID, session.Start, session.End ?? DateTime.UtcNow);

            KillDeathBlock block = new();
            block.Kills = events.Where(iter => iter.AttackerCharacterID == session.CharacterID && iter.KilledCharacterID != session.CharacterID).ToList();
            block.Deaths = events.Where(iter => iter.KilledCharacterID == session.CharacterID).ToList();

            // load characters
            List<string> IDs = events.Select(iter => iter.AttackerCharacterID).Distinct().ToList();
            IDs.AddRange(events.Select(iter => iter.KilledCharacterID).Distinct());
            block.Characters = await _CharacterRepository.GetByIDs(IDs, CensusEnvironment.PC);

            // load items
            IEnumerable<int> itemIDs = events.Select(iter => iter.WeaponID).Distinct();
            block.Weapons = await _ItemRepository.GetByIDs(itemIDs);

            // load fire modes
            IEnumerable<int> fireModeIDs = events.Select(iter => iter.AttackerFireModeID).Distinct();
            block.FireModes = await _FireGroupToFireModeRepository.GetByFireModes(fireModeIDs);

            // load item categories
            IEnumerable<int> categoryIDs = block.Weapons.Select(iter => iter.CategoryID).Distinct();
            block.ItemCategories = await _ItemCategoryRepository.GetByIDs(categoryIDs);

            return ApiOk(block);
        }

        /// <summary>
        ///     Get the weapons a PC character has used in the last 2 hours
        /// </summary>
        /// <remarks>
        ///     Used for the realtime server view, when you click on the character in the top killers list
        ///     <br/><br/>
        ///     Get the weapons a character used to get kills in the last 2 hours. This excludes TKs
        /// </remarks>
        /// <param name="charID">ID of the character</param>
        /// <param name="useShort">Will only the last hour of data be usd instead of 2 hours?</param>
        /// <response code="200">
        ///     The response will contain the <see cref="CharacterWeaponKillEntry"/>s for the character
        ///     passed in <paramref name="charID"/> and within the last 2 hours
        /// </response>
        [HttpGet("character/{charID}")]
        public async Task<ApiResponse<List<CharacterWeaponKillEntry>>> CharacterKills(string charID, [FromQuery] bool useShort = false) {
            List<KillEvent> kills = await _KillDbStore.GetRecentKillsByCharacterID(charID, useShort ? 60 :120);

            Dictionary<int, CharacterWeaponKillEntry> entries = new Dictionary<int, CharacterWeaponKillEntry>();

            foreach (KillEvent ev in kills) {
                // Skip character deaths or TKs
                if (ev.KilledCharacterID == charID || ev.AttackerTeamID == ev.KilledTeamID) {
                    continue;
                }

                if (entries.TryGetValue(ev.WeaponID, out CharacterWeaponKillEntry? entry) == false) {
                    PsItem? item = await _ItemRepository.GetByID(ev.WeaponID);

                    entry = new CharacterWeaponKillEntry() {
                        WeaponID = ev.WeaponID,
                        WeaponName = (ev.WeaponID == 0) ? "No weapon" : (item?.Name ?? $"<missing {ev.WeaponID}>")
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
        ///     Get the PC characters who have gotten the most kills in an outfit in the last 2 hours
        /// </summary>
        /// <remarks>
        ///      Used for the realtime server view, when you click on the amount of kills an outfit has gotten
        ///      <br/><br/>
        ///      Get the top killers in an outfit in the last 2 hours
        /// </remarks>
        /// <param name="outfitID">ID of the outfit</param>
        /// <param name="useShort">Will only 1 hour of data be used instead of 2 hours?</param>
        /// <response code="200">
        ///     The response will contain a list of <see cref="OutfitKillerEntry"/>s for the
        ///     <see cref="PsOutfit"/> with <see cref="PsOutfit.ID"/> of <paramref name="outfitID"/>
        /// </response>
        [HttpGet("outfit/{outfitID}")]
        public async Task<ActionResult<List<OutfitKillerEntry>>> OutfitKills(string outfitID, [FromQuery] bool useShort = false) {
            List<KillEvent> kills = await _KillDbStore.GetKillsByOutfitID(outfitID, 120);

            Dictionary<string, OutfitKillerEntry> entries = new Dictionary<string, OutfitKillerEntry>();

            foreach (KillEvent ev in kills) {
                // Skip character deaths or TKs
                if (ev.KilledCharacterID == ev.AttackerCharacterID || ev.AttackerTeamID == ev.KilledTeamID) {
                    continue;
                }

                if (entries.TryGetValue(ev.AttackerCharacterID, out OutfitKillerEntry? entry) == false) {
                    PsCharacter? character = await _CharacterRepository.GetByID(ev.AttackerCharacterID, CensusEnvironment.PC);
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

        /// <summary>
        ///     Get the kills of a PC character between two times
        /// </summary>
        /// <param name="charID">ID of the character to get the kills of</param>
        /// <param name="start">When the time period starts</param>
        /// <param name="end">When the time period ends</param>
        /// <response code="200">
        ///     The response will contain a list of <see cref="ExpandedKillEvent"/>s that occured for the character between the range given
        /// </response>
        /// <response code="400">
        ///     One of the following validation errors occured:
        ///     <ul>
        ///         <li>
        ///             <paramref name="start"/> came after <paramref name="end"/>     
        ///         </li>
        ///         <li>
        ///             <paramref name="end"/> was more than 24 hours after <paramref name="start"/>
        ///         </li>
        ///     </ul>
        /// </response>
        [HttpGet("character/{charID}/period")]
        public async Task<ApiResponse<List<ExpandedKillEvent>>> GetByCharacterIDAndRange(string charID, [FromQuery] DateTime start, [FromQuery] DateTime end) {
            if (start >= end) {
                return ApiBadRequest<List<ExpandedKillEvent>>($"{nameof(start)} must come before {nameof(end)}");
            }

            if (end - start > TimeSpan.FromDays(1)) {
                return ApiBadRequest<List<ExpandedKillEvent>>($"{nameof(start)} and {nameof(end)} cannot have more than a 24 hour difference");
            }

            List<KillEvent> events = await _KillDbStore.GetKillsByCharacterID(charID, start, end);
            List<ExpandedKillEvent> ex = new List<ExpandedKillEvent>(events.Count);

            Dictionary<string, PsCharacter?> chars = new Dictionary<string, PsCharacter?>();

            List<string> IDs = events.Select(iter => iter.AttackerCharacterID).Distinct().ToList();
            IDs.AddRange(events.Select(iter => iter.KilledCharacterID).Distinct());

            List<PsCharacter> characters = await _CharacterRepository.GetByIDs(IDs, CensusEnvironment.PC);
            foreach (PsCharacter c in characters) {
                if (chars.ContainsKey(c.ID) == false) {
                    chars.Add(c.ID, c);
                }
            }

            Dictionary<int, PsItem?> items = new Dictionary<int, PsItem?>();

            foreach (KillEvent ev in events) {
                ExpandedKillEvent e = new();
                e.Event = ev;

                if (items.ContainsKey(ev.WeaponID) == false) {
                    items.Add(ev.WeaponID, await _ItemRepository.GetByID(ev.WeaponID));
                }

                chars.TryGetValue(ev.AttackerCharacterID, out PsCharacter? attacker);
                chars.TryGetValue(ev.KilledCharacterID, out PsCharacter? killed);

                e.Attacker = attacker;
                e.Killed = killed;
                e.Item = items[ev.WeaponID];

                ex.Add(e);
            }

            return ApiOk(ex);
        }

    }
}
