using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using watchtower.Code.Constants;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Models.Api;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Events;
using watchtower.Services.Db;
using watchtower.Services.Repositories;
using watchtower.Services.Repositories.Static;

namespace watchtower.Controllers {

    /// <summary>
    ///     Endpoint for getting information about exp events a character has done
    /// </summary>
    [ApiController]
    [Route("/api/exp")]
    public class ExpApiController : ApiControllerBase {

        private readonly ILogger<ExpApiController> _Logger;

        private readonly CharacterRepository _CharacterRepository;
        private readonly OutfitRepository _OutfitRepository;
        private readonly ExperienceTypeRepository _ExperienceTypeRepository;
        private readonly ExperienceAwardTypeRepository _ExperienceAwardTypeRepository;

        private readonly ExpEventDbStore _ExpDbStore;
        private readonly SessionDbStore _SessionDb;

        public ExpApiController(ILogger<ExpApiController> logger,
            CharacterRepository charRepo, ExpEventDbStore killDb,
            OutfitRepository outfitRepo, SessionDbStore sessionDb,
            ExperienceTypeRepository experienceTypeRepository, ExperienceAwardTypeRepository experienceAwardTypeRepository) {

            _Logger = logger;

            _CharacterRepository = charRepo ?? throw new ArgumentNullException(nameof(charRepo));
            _OutfitRepository = outfitRepo ?? throw new ArgumentNullException(nameof(outfitRepo));

            _ExpDbStore = killDb ?? throw new ArgumentNullException(nameof(killDb));
            _SessionDb = sessionDb ?? throw new ArgumentNullException(nameof(sessionDb));
            _ExperienceTypeRepository = experienceTypeRepository;
            _ExperienceAwardTypeRepository = experienceAwardTypeRepository;
        }

        /// <summary>
        ///     Get the experience events a PC player got during a time period.
        ///     Please use <see cref="GetByCharacterIDAndRange2(string, DateTime, DateTime, bool?, bool?, List{int}?)"/>
        /// </summary>
        /// <param name="charID">ID of the character to get the events of</param>
        /// <param name="start">When the time period to load started</param>
        /// <param name="end">When the time period to load will end</param>
        /// <param name="interestedEvents">List of IDs to filter for in the response. If left null, all exp events are included</param>
        /// <response code="200">
        ///     The response will contain a list of <see cref="ExpandedExpEvent"/>s for each exp event that occured
        ///     between <paramref name="start"/> and <paramref name="end"/> with a <see cref="ExpEvent.SourceID"/> of <paramref name="charID"/>
        /// </response>
        /// <resposne code="400">
        ///     One of the following errors occured:
        ///     <ul>
        ///         <li>
        ///             <paramref name="start"/> came before <paramref name="end"/>
        ///         </li>
        ///         <li>
        ///             <paramref name="start"/> and <paramref name="end"/> have more than a 24 hour difference
        ///         </li>
        ///     </ul>
        /// </resposne>
        [HttpGet("{charID}/period")]
        public async Task<ApiResponse<List<ExpandedExpEvent>>> GetByCharacterIDAndRange(string charID,
            [FromQuery] DateTime start, [FromQuery] DateTime end,
            [FromQuery] List<int>? interestedEvents = null) {

            if (end - start > TimeSpan.FromDays(1)) {
                return ApiBadRequest<List<ExpandedExpEvent>>($"{nameof(start)} and {nameof(end)} cannot have more than a 24 hour difference");
            }
            if (start >= end) {
                return ApiBadRequest<List<ExpandedExpEvent>>($"{nameof(start)} must come before ${nameof(end)}");
            }

            List<ExpEvent> expEvents = await _ExpDbStore.GetByCharacterID(charID, start, end);
            if (interestedEvents != null && interestedEvents.Count > 0) {
                _Logger.LogDebug($"Filtering exp events for {charID} between {start:u} - {end:u}: {string.Join(", ", interestedEvents)}");
                expEvents = expEvents.Where(iter => interestedEvents.IndexOf(iter.ExperienceID) > -1).ToList();
            }

            List<ExpandedExpEvent> expanded = new List<ExpandedExpEvent>(expEvents.Count);

            // 19 characters is a character ID, other ones are NPC IDs
            List<string> characterIDs = expEvents.Select(iter => iter.SourceID).ToList();
            characterIDs.AddRange(expEvents.Where(iter => iter.OtherID.Length == 19).Select(iter => iter.OtherID).ToList());
            characterIDs = characterIDs.Distinct().ToList();

            List<PsCharacter> chars = await _CharacterRepository.GetByIDs(characterIDs, CensusEnvironment.PC);

            foreach (ExpEvent ev in expEvents) {
                ExpandedExpEvent ex = new ExpandedExpEvent();
                ex.Source = chars.FirstOrDefault(iter => iter.ID == ev.SourceID);
                ex.Other = (ev.OtherID.Length == 19) ? chars.FirstOrDefault(iter => iter.ID == ev.OtherID) : null;
                ex.Event = ev;

                expanded.Add(ex);
            }

            return ApiOk(expanded);
        }

        /// <summary>
        ///     Get the <see cref="ExpEvent"/>s a character earned between a period.
        ///     See remarks for more information
        /// </summary>
        /// <remarks>
        ///     This response is in a "block" form, where all the possible joins/resolves one would want to do is included in the response.
        ///     For example, if you wanted to match the <see cref="ExpEvent.ExperienceID"/> to the <see cref="ExperienceType"/>,
        ///     you would use <see cref="ExperienceBlock.ExperienceTypes"/>, which is a list of all <see cref="ExperienceType"/>s
        ///     that occured for all events. 
        ///     <br/>
        ///     The following is available in the block:
        ///     <ul>
        ///         <li><see cref="ExperienceBlock.Characters"/>: All the characters that were the source or other id of the exp event</li>
        ///         <li><see cref="ExperienceBlock.ExperienceTypes"/>: All the experience definitions that occured</li>
        ///     </ul>
        ///     This is different from <see cref="GetByCharacterIDAndRange(string, DateTime, DateTime, List{int}?)"/>, as it expands all events,
        ///     which can lead to duplicate information. An expended events puts the resolved character alongside every event, so if someone healed
        ///     someone else 10 times, that single character would be included 10 times. Instead, in block form, all those characters are put
        ///     into a list, and are joined on the recievers end
        /// </remarks>
        /// <param name="charID">ID of the character to get the events of</param>
        /// <param name="start">When the interested period starts</param>
        /// <param name="end">When the interested period ends</param>
        /// <param name="includeCharacters">If the <see cref="ExperienceBlock.Characters"/> will be populated. Defaults to true</param>
        /// <param name="includeExpTypes">If the <see cref="ExperienceBlock.ExperienceTypes"/> will be populated. Defaults to true</param>
        /// <param name="interestedEvents">If provided, a filter of what events are included in the response. Leave empty for all events</param>
        /// <response code="200">
        ///     The response will contain a <see cref="ExperienceBlock"/>, that contains all the events requested as well as the information
        ///     that would be useful for resolving IDs (such as characters)
        /// </response>
        /// <response code="400">
        ///     One of the following validation errors occured:
        ///     <ul>
        ///         <li><paramref name="end"/> comes before <paramref name="start"/></li>
        ///         <li><paramref name="start"/> and <paramref name="end"/> are more than 24 hours apart</li>
        ///     </ul>
        /// </response>
        [HttpGet("{charID}/period2")]
        public async Task<ApiResponse<ExperienceBlock>> GetByCharacterIDAndRange2(string charID,
            [FromQuery] DateTime start, [FromQuery] DateTime end,
            [FromQuery] bool? includeCharacters = true,
            [FromQuery] bool? includeExpTypes = true,
            [FromQuery] List<int>? interestedEvents = null) {

            if (end - start > TimeSpan.FromDays(1)) {
                return ApiBadRequest<ExperienceBlock>($"{nameof(start)} and {nameof(end)} cannot have more than a 24 hour difference");
            }
            if (start >= end) {
                return ApiBadRequest<ExperienceBlock>($"{nameof(start)} must come before ${nameof(end)}");
            }

            ExperienceBlock block = await _GetExpBlock(charID, start, end, includeCharacters, includeExpTypes, interestedEvents, false);
            return ApiOk(block);
        }

        private async Task<ExperienceBlock> _GetExpBlock(string charID,
            DateTime start, DateTime end,
            bool? includeCharacters = true, bool? includeExpTypes = true,
            List<int>? interestedEvents = null, bool? useOther = false) {

            if (end - start > TimeSpan.FromDays(1)) {
                throw new Exception($"{nameof(start)} and {nameof(end)} cannot have more than a 24 hour difference");
            }
            if (start >= end) {
                throw new Exception($"{nameof(start)} must come before ${nameof(end)}");
            }

            // 
            List<ExpEvent> events = (useOther == true)
                ? await _ExpDbStore.GetOtherByCharacterIDs([charID], start, end)
                : await _ExpDbStore.GetByCharacterID(charID, start, end);

            if (interestedEvents != null && interestedEvents.Count > 0) {
                _Logger.LogDebug($"filtering exp events [charID={charID}] [start={start:u}] [end={end:u}] [events={string.Join(", ", interestedEvents)}]");
                events = events.Where(iter => interestedEvents.IndexOf(iter.ExperienceID) > -1).ToList();
            }

            ExperienceBlock block = new ExperienceBlock();
            block.Events = events;
            block.InputCharacters = new() { charID };
            block.PeriodStart = start;
            block.PeriodEnd = end;

            HashSet<string> charIDs = new();
            HashSet<int> expTypeIDs = new();

            if (includeCharacters == true || includeExpTypes == true) {
                foreach (ExpEvent ev in events) {
                    charIDs.Add(ev.SourceID);
                    if (ev.OtherID.Length == 19) {
                        charIDs.Add(ev.OtherID);
                    }

                    expTypeIDs.Add(ev.ExperienceID);
                }
            }

            if (includeCharacters == true) {
                List<PsCharacter> chars = await _CharacterRepository.GetByIDs(charIDs, CensusEnvironment.PC, fast: true);
                block.Characters = chars;
            }

            if (includeExpTypes == true) {
                List<ExperienceType> types = await _ExperienceTypeRepository.GetByIDs(expTypeIDs);
                block.ExperienceTypes = types;
            }

            return block;
        }

        /// <summary>
        ///     Get the exp events done in a session
        /// </summary>
        /// <remarks>
        ///     These exp events are expanded, meaning they contain additional information about the exp event.
        ///     For example, if the exp event is a revive event, the <see cref="ExpandedExpEvent.Other"/>
        ///     will contain the <see cref="PsCharacter"/> that was revived
        /// </remarks>
        /// <param name="sessionID">ID of the session to get the exp events of</param>
        /// <response code="200">
        ///     The response will contain a list of <see cref="ExpEvent"/> for the character 
        ///     the <see cref="Session"/> with <see cref="Session.ID"/> of <paramref name="sessionID"/>
        ///     is for, in the time range the same session is for
        /// </response>
        /// <response code="404">
        ///     No <see cref="Session"/> with <see cref="Session.ID"/> of <paramref name="sessionID"/> exists
        /// </response>
        [HttpGet("session/{sessionID}")]
        public async Task<ApiResponse<List<ExpandedExpEvent>>> GetBySessionID(long sessionID) {
            Session? session = await _SessionDb.GetByID(sessionID);
            if (session == null) {
                return ApiNotFound<List<ExpandedExpEvent>>($"{nameof(Session)} {sessionID}");
            }

            return await GetByCharacterIDAndRange(session.CharacterID, session.Start, session.End ?? DateTime.UtcNow);
        }

        /// <summary>
        ///     get the exp events in a session where the <see cref="ExpEvent.OtherID"/> is the
        ///     <see cref="Session.CharacterID"/> of the <see cref="Session"/> with <see cref="Session.ID"/>
        ///     of <paramref name="sessionID"/>
        /// </summary>
        /// <param name="sessionID">ID of the session to get the <see cref="ExpEvent"/>s of</param>
        /// <response code="200">
        ///     the response will contain a <see cref="ExperienceBlock"/> for the <see cref="ExpEvent"/>s
        ///     that occured within the <see cref="Session"/> with <see cref="Session.ID"/> of <paramref name="sessionID"/>
        ///     with a <see cref="ExpEvent.OtherID"/> of the <see cref="Session"/>
        /// </response>
        /// <response code="404">
        ///     no <see cref="Session"/> with <see cref="Session.ID"/> of <paramref name="sessionID"/> exists
        /// </response>
        [HttpGet("session/{sessionID}/target")]
        public async Task<ApiResponse<ExperienceBlock>> GetTargetBySessionID(long sessionID) {
            Session? session = await _SessionDb.GetByID(sessionID);
            if (session == null) {
                return ApiNotFound<ExperienceBlock>($"{nameof(Session)} {sessionID}");
            }

            ExperienceBlock block = await _GetExpBlock(session.CharacterID, session.Start, session.End ?? DateTime.UtcNow, true, true, useOther: true);
            return ApiOk(block);
        }

        /// <summary>
        ///     Get the events that took place during a session in block form. See remarks for more details.
        /// </summary>
        /// <remarks>
        ///     Block form does not expanded the list of <see cref="ExpEvent"/> into <see cref="ExpandedExpEvent"/>s.
        ///     Instead, it will create a Dictionary of characters (<see cref="ExperienceBlock.Characters"/>) populated
        ///     by the <see cref="ExpEvent.SourceID"/> and <see cref="ExpEvent.OtherID"/>
        /// </remarks>
        /// <param name="sessionID">ID of the session to get the events of</param>
        /// <response code="200">
        ///     The response will contain a <see cref="ExperienceBlock"/>, which contains all the events that took
        ///     place during a session, as well as information used to perform joins (such as the characters)
        /// </response>
        /// <response code="404">
        ///     No <see cref="Session"/> with <see cref="Session.ID"/> of <paramref name="sessionID"/> exists
        /// </response>
        [HttpGet("sessions/{sessionID}")]
        public async Task<ApiResponse<ExperienceBlock>> GetBySessionID2(long sessionID) {
            Session? session = await _SessionDb.GetByID(sessionID);
            if (session == null) {
                return ApiNotFound<ExperienceBlock>($"{nameof(Session)} {sessionID}");
            }

            return await GetByCharacterIDAndRange2(session.CharacterID, session.Start, session.End ?? DateTime.UtcNow);
        }

        /// <summary>
        ///     Get the entities another PC character is supporting in the last 2 hours
        /// </summary>
        /// <remarks>
        ///     A support event is a specific type of exp event, where the other_id is another character that benefitted
        ///     from the action taken. For example, a revive is a support event. This will get the characters that a
        ///     character has supported, such as how many times a character has been revived by the character
        ///     passed in <paramref name="charID"/>
        ///     <br/><br/>
        ///     If <paramref name="type"/> is heals, revives, resupplies or shield_repairs, the resulting <see cref="CharacterExpSupportEntry"/>s
        ///     contain the characters that received the benefit of the action
        ///     <br/><br/>
        ///     In addition to support events, two other exp event types are supported, spawns and vehicle kills.
        ///     <br/><br/>
        ///     In the case spawns, the <see cref="CharacterExpSupportEntry.CharacterName"/> will instead be the source of the spawn,
        ///     such as beacon, sunderer or router
        ///     <br/><br/>
        ///     In the case of vehicle kills, the <see cref="CharacterExpSupportEntry.CharacterName"/> will instead be the name of 
        ///     the vehicle that was killed, such as harasser, reaver or magrider. Sunderers are not counted as vehicle kills
        /// </remarks>
        /// <param name="charID">ID of the character</param>
        /// <param name="type">
        ///     What type of exp event the supported entries will be for. Expected values are:
        ///     <ul>
        ///         <li>spawns</li>
        ///         <li>vehicleKills</li>
        ///         <li>heals</li>
        ///         <li>revives</li>
        ///         <li>resupplies</li>
        ///         <li>shield_repair</li>
        ///     </ul>
        ///     All other values will produced a 404 Bad Request response
        /// </param>
        /// <param name="useShort">
        ///     True if only getting the data from the last hour, otherwise false for 2 hours
        /// </param>
        /// <response code="200">
        ///     The response will contain a list of <see cref="CharacterExpSupportEntry"/>s the character has produced
        ///     in the last 1 or 2 hours (depending on <paramref name="useShort"/>. See remarks for more info
        /// </response>
        /// <response code="400">
        ///     <paramref name="type"/> was an invalid value. See the parameter documentation on what is a valid value
        /// </response>
        /// <response code="404">
        ///     No <see cref="PsCharacter"/> with <see cref="PsCharacter.ID"/> of <paramref name="charID"/> exists
        /// </response>
        [HttpGet("character/{charID}/{type}")]
        public async Task<ApiResponse<List<CharacterExpSupportEntry>>> CharacterEntries(string charID, string type, [FromQuery] bool useShort = false) {
            PsCharacter? c = await _CharacterRepository.GetByID(charID, CensusEnvironment.PC);
            if (c == null) {
                return ApiNotFound<List<CharacterExpSupportEntry>>($"{nameof(PsCharacter)} {charID}");
            }

            if (type == "spawns") {
                List<CharacterExpSupportEntry> spawns = await CharacterSpawns(charID, useShort);
                return ApiOk(spawns);
            }

            if (type == "vehicleKills") {
                List<CharacterExpSupportEntry> kills = await CharacterVehicleKills(charID, useShort);
                return ApiOk(kills);
            }

            List<int> expTypes = new();

            if (type == "heals") {
                expTypes = new List<int> { Experience.HEAL, Experience.SQUAD_HEAL };
            } else if (type == "revives") {
                expTypes = new List<int> { Experience.REVIVE, Experience.SQUAD_REVIVE };
            } else if (type == "resupplies") {
                expTypes = new List<int> { Experience.RESUPPLY, Experience.SQUAD_RESUPPLY };
            } else if (type == "shield_repair") {
                expTypes = new List<int> { Experience.SHIELD_REPAIR, Experience.SQUAD_SHIELD_REPAIR };
            } else {
                return ApiBadRequest<List<CharacterExpSupportEntry>>($"Unknown type '{type}'");
            }

            List<CharacterExpSupportEntry> list = await GetByCharacterAndExpIDs(charID, expTypes, useShort);

            return ApiOk(list);
        }

        /// <summary>
        ///     Get the characters in an outfit that have performed the exp event in <paramref name="type"/>
        /// </summary>
        /// 
        /// <remarks>
        ///     Get a list of characters in an outfit that have performed the exp event passed in <paramref name="type"/>.
        ///     For example, getting the top healers of an outfit in the last 2 hours. See <see cref="CharacterEntries(string, string, bool)"/>
        ///     for more remarks on what a "support event" is
        /// </remarks>
        /// 
        /// <param name="outfitID">
        ///     ID of the outfit
        /// </param>
        /// <param name="type">
        ///     What type of exp event the supported entries will be for. Expected values are:
        ///     <ul>
        ///         <li>spawns</li>
        ///         <li>vehicleKills</li>
        ///         <li>heals</li>
        ///         <li>revives</li>
        ///         <li>resupplies</li>
        ///         <li>shield_repair</li>
        ///     </ul>
        ///     All other values will produced a 404 Bad Request response
        /// </param>
        /// <param name="worldID">
        ///     ID of the world to restrict the data to. Needed as outfit members may be on multiple servers
        /// </param>
        /// <param name="teamID">
        ///     Team ID to restrict the data to. Needed for NSO characters currently on different teams, but are in the outfit
        /// </param>
        /// <param name="useShort">
        ///     True if only getting the data from the last hour, otherwise false for 2 hours
        /// </param>
        /// 
        /// <response code="200">
        ///     The response will contain a list of <see cref="OutfitExpEntry"/>s for the parameters given in the last 2 hours.
        ///     See remarks for more info
        /// </response>
        /// <response code="400">
        ///     <paramref name="type"/> was an invalid value. See the parameter documentation on what is a valid value
        /// </response>
        /// <response code="404">
        ///     No <see cref="PsOutfit"/> with <see cref="PsOutfit.ID"/> of <paramref name="outfitID"/> exists
        /// </response>
        [HttpGet("outfit/{outfitID}/{type}/{worldID}/{teamID}")]
        public async Task<ApiResponse<List<OutfitExpEntry>>> OutfitEntries(string outfitID, string type, short worldID, short teamID, [FromQuery] bool useShort = false) {
            PsOutfit? outfit = await _OutfitRepository.GetByID(outfitID);
            if (outfitID != "0" && outfit == null) {
                return ApiNotFound<List<OutfitExpEntry>>($"{nameof(PsOutfit)} {outfitID}");
            }

            List<int> expTypes = new List<int>();

            if (type == "heals") {
                expTypes = new List<int> { Experience.HEAL, Experience.SQUAD_HEAL };
            } else if (type == "revives") {
                expTypes = new List<int> { Experience.REVIVE, Experience.SQUAD_REVIVE };
            } else if (type == "resupplies") {
                expTypes = new List<int> { Experience.RESUPPLY, Experience.SQUAD_RESUPPLY };
            } else if (type == "shield_repair") {
                expTypes = new List<int> { Experience.SHIELD_REPAIR, Experience.SQUAD_SHIELD_REPAIR };
            } else if (type == "spawns") {
                expTypes = new List<int> {
                    Experience.SQUAD_SPAWN, Experience.GALAXY_SPAWN_BONUS,
                    Experience.SUNDERER_SPAWN_BONUS, Experience.GENERIC_NPC_SPAWN,
                    Experience.SQUAD_VEHICLE_SPAWN_BONUS
                };
            } else if (type == "vehicleKills") {
                expTypes = Experience.VehicleKillEvents;
            } else {
                return ApiBadRequest<List<OutfitExpEntry>>($"Unknown type '{type}'");
            }

            List<OutfitExpEntry> list = await GetByOutfitAndExpIDs(outfitID, expTypes, worldID, teamID, useShort);

            return ApiOk(list);
        }

        /// <summary>
        ///     Get all the <see cref="ExperienceType"/>s stored
        /// </summary>
        /// <response code="200">
        ///     The response will contain a list of <see cref="ExperienceType"/>s
        /// </response>
        [HttpGet("types")]
        public async Task<ApiResponse<List<ExperienceType>>> GetExperienceTypes() {
            List<ExperienceType> types = await _ExperienceTypeRepository.GetAll();

            return ApiOk(types);
        }

        /// <summary>
        ///     get all <see cref="ExperienceAwardType"/>s stored
        /// </summary>
        /// <response code="200">
        ///     the response will contain a list of all <see cref="ExperienceAwardType"/>s
        /// </response>
        [HttpGet("award-types")]
        public async Task<ApiResponse<List<ExperienceAwardType>>> GetExperienceAwardTypes() {
            List<ExperienceAwardType> types = await _ExperienceAwardTypeRepository.GetAll();

            return ApiOk(types);
        }

        private async Task<List<CharacterExpSupportEntry>> CharacterSpawns(string charID, bool useShort) {
            List<ExpEvent> events = await _ExpDbStore.GetRecentByCharacterID(charID, useShort == true ? 60 : 120);

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

            return list;
        }

        private async Task<List<CharacterExpSupportEntry>> CharacterVehicleKills(string charID, bool useShort) {
            List<ExpEvent> events = await _ExpDbStore.GetRecentByCharacterID(charID, useShort == true ? 60 : 120);

            // please stop adding more vehicles ;-;
            CharacterExpSupportEntry flashKills = new CharacterExpSupportEntry() { CharacterName = "Flashes" };
            CharacterExpSupportEntry galaxyKills = new CharacterExpSupportEntry() { CharacterName = "Galaxies" };
            CharacterExpSupportEntry libKills = new CharacterExpSupportEntry() { CharacterName = "Liberators" };
            CharacterExpSupportEntry lightningKills = new CharacterExpSupportEntry() { CharacterName = "Lightnings" };
            CharacterExpSupportEntry magriderKills = new CharacterExpSupportEntry() { CharacterName = "Magriders" };
            CharacterExpSupportEntry mosquitoKills = new CharacterExpSupportEntry() { CharacterName = "Mosquitos" };
            CharacterExpSupportEntry prowlerKills = new CharacterExpSupportEntry() { CharacterName = "Prowlers" };
            CharacterExpSupportEntry reaverKills = new CharacterExpSupportEntry() { CharacterName = "Reavers" };
            CharacterExpSupportEntry scytheKills = new CharacterExpSupportEntry() { CharacterName = "Scythes" };
            CharacterExpSupportEntry vanguardKills = new CharacterExpSupportEntry() { CharacterName = "Vanguards" };
            CharacterExpSupportEntry harasserKills = new CharacterExpSupportEntry() { CharacterName = "Harassers" };
            CharacterExpSupportEntry valkKills = new CharacterExpSupportEntry() { CharacterName = "Valkyries" };
            CharacterExpSupportEntry antKills = new CharacterExpSupportEntry() { CharacterName = "ANTs" };
            CharacterExpSupportEntry colossusKills = new CharacterExpSupportEntry() { CharacterName = "Colossuses" };
            CharacterExpSupportEntry javelinKills = new CharacterExpSupportEntry() { CharacterName = "Javelins" };
            CharacterExpSupportEntry chimeraKills = new CharacterExpSupportEntry() { CharacterName = "Chimeras" };
            CharacterExpSupportEntry dervishKills = new CharacterExpSupportEntry() { CharacterName = "Dervishes" };

            foreach (ExpEvent ev in events) {
                if (ev.ExperienceID == Experience.VKILL_FLASH) {
                    ++flashKills.Amount;
                } else if (ev.ExperienceID == Experience.VKILL_GALAXY) {
                    ++galaxyKills.Amount;
                } else if (ev.ExperienceID == Experience.VKILL_LIBERATOR) {
                    ++libKills.Amount;
                } else if (ev.ExperienceID == Experience.VKILL_LIGHTNING) {
                    ++lightningKills.Amount;
                } else if (ev.ExperienceID == Experience.VKILL_MAGRIDER) {
                    ++magriderKills.Amount;
                } else if (ev.ExperienceID == Experience.VKILL_MOSQUITO) {
                    ++mosquitoKills.Amount;
                } else if (ev.ExperienceID == Experience.VKILL_PROWLER) {
                    ++prowlerKills.Amount;
                } else if (ev.ExperienceID == Experience.VKILL_REAVER) {
                    ++reaverKills.Amount;
                } else if (ev.ExperienceID == Experience.VKILL_SCYTHE) {
                    ++scytheKills.Amount;
                } else if (ev.ExperienceID == Experience.VKILL_VANGUARD) {
                    ++vanguardKills.Amount;
                } else if (ev.ExperienceID == Experience.VKILL_HARASSER) {
                    ++harasserKills.Amount;
                } else if (ev.ExperienceID == Experience.VKILL_VALKYRIE) {
                    ++valkKills.Amount;
                } else if (ev.ExperienceID == Experience.VKILL_ANT) {
                    ++antKills.Amount;
                } else if (ev.ExperienceID == Experience.VKILL_COLOSSUS) {
                    ++colossusKills.Amount;
                } else if (ev.ExperienceID == Experience.VKILL_JAVELIN) {
                    ++javelinKills.Amount;
                } else if (ev.ExperienceID == Experience.VKILL_CHIMERA) {
                    ++chimeraKills.Amount;
                } else if (ev.ExperienceID == Experience.VKILL_DERVISH) {
                    ++dervishKills.Amount;
                }
            }

            List<CharacterExpSupportEntry> list = (new List<CharacterExpSupportEntry>() {
                    flashKills, galaxyKills, libKills,
                    lightningKills, magriderKills, mosquitoKills,
                    prowlerKills, reaverKills, scytheKills,
                    vanguardKills, harasserKills, valkKills,
                    antKills, colossusKills, javelinKills,
                    dervishKills, chimeraKills
            }).Where(iter => iter.Amount > 0)
                .OrderByDescending(iter => iter.Amount).ToList();

            return list;
        }

        private async Task<List<CharacterExpSupportEntry>> GetByCharacterAndExpIDs(string charID, List<int> events, bool useShort) {
            List<ExpEvent> exps = await _ExpDbStore.GetRecentByCharacterID(charID, useShort ? 60 : 120);

            Dictionary<string, CharacterExpSupportEntry> entries = new Dictionary<string, CharacterExpSupportEntry>();

            foreach (ExpEvent ev in exps) {
                if (events.Contains(ev.ExperienceID) == false) {
                    continue;
                }

                if (entries.TryGetValue(ev.OtherID, out CharacterExpSupportEntry? entry) == false) {
                    PsCharacter? character = await _CharacterRepository.GetByID(ev.OtherID, CensusEnvironment.PC);

                    entry = new CharacterExpSupportEntry() {
                        CharacterID = ev.OtherID,
                        CharacterName = character?.GetDisplayName() ?? $"missing {ev.OtherID}"
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

        private async Task<List<OutfitExpEntry>> GetByOutfitAndExpIDs(string outfitID, List<int> events, short worldID, short teamID, bool useShort) {
            List<ExpEvent> exp = await _ExpDbStore.GetByOutfitID(outfitID, worldID, teamID, useShort ? 60 : 120);

            Dictionary<string, OutfitExpEntry> entries = new Dictionary<string, OutfitExpEntry>();

            foreach (ExpEvent ev in exp) {
                if (events.Contains(ev.ExperienceID) == false) {
                    continue;
                }

                if (entries.TryGetValue(ev.SourceID, out OutfitExpEntry? entry) == false) {
                    PsCharacter? character = await _CharacterRepository.GetByID(ev.SourceID, CensusEnvironment.PC);
                    entry = new OutfitExpEntry() {
                        CharacterID = ev.SourceID,
                        CharacterName = character?.GetDisplayName() ?? $"missing {ev.SourceID}"
                    };
                }

                ++entry.Amount;

                entries[ev.SourceID] = entry;
            }

            List<OutfitExpEntry> list = entries.Values
                .OrderByDescending(iter => iter.Amount)
                .ToList();

            return list;
        }

    }
}
