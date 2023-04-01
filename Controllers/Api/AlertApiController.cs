using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code;
using watchtower.Code.Constants;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Models.Alert;
using watchtower.Models.Api;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Events;
using watchtower.Models.Internal;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("/api/alerts")]
    public class AlertApiController : ApiControllerBase {

        private readonly ILogger<AlertApiController> _Logger;

        private readonly AlertDbStore _AlertDb;
        private readonly AlertPlayerDataRepository _ParticipantDataRepository;
        private readonly CharacterRepository _CharacterRepository;
        private readonly OutfitRepository _OutfitRepository;
        private readonly SessionDbStore _SessionDb;
        private readonly AlertPlayerProfileDataDbStore _ProfileDataDb;
        private readonly FacilityControlDbStore _ControlDb;
        private readonly FacilityRepository _FacilityRepository;
        private readonly AlertPopulationRepository _AlertPopulationRepository;

        public AlertApiController(ILogger<AlertApiController> logger,
                AlertPlayerDataRepository participantDataRepository, AlertDbStore alertDb,
                CharacterRepository characterRepository,
                OutfitRepository outfitRepository, SessionDbStore sessionDb,
                AlertPlayerProfileDataDbStore profileDataDb, FacilityControlDbStore controlDb,
                FacilityRepository facilityRepository, AlertPopulationRepository alertPopulationRepository) {

            _Logger = logger;

            _ParticipantDataRepository = participantDataRepository;
            _AlertDb = alertDb;
            _CharacterRepository = characterRepository;
            _OutfitRepository = outfitRepository;
            _SessionDb = sessionDb;
            _ProfileDataDb = profileDataDb;
            _ControlDb = controlDb;
            _FacilityRepository = facilityRepository;
            _AlertPopulationRepository = alertPopulationRepository;
        }

        /// <summary>
        ///     Get the alert with the corresponding ID
        /// </summary>
        /// <param name="alertID">ID of the alert</param>
        /// <response code="200">
        ///     The response will contain the <see cref="PsAlert"/> with <see cref="PsAlert.ID"/> of <paramref name="alertID"/>
        /// </response> 
        /// <response code="204">
        ///     No <see cref="PsAlert"/> with <see cref="PsAlert.ID"/> of <paramref name="alertID"/> exists
        /// </response>
        [HttpGet("{alertID}")]
        public async Task<ApiResponse<PsAlert>> GetAlertByID(long alertID) {
            PsAlert? alert = await _AlertDb.GetByID(alertID);

            if (alert == null) {
                return ApiNoContent<PsAlert>();
            }

            return ApiOk(alert);
        }

        /// <summary>
        ///     Get all alerts
        /// </summary>
        /// <response code="200">
        ///     Get all alerts Honu has tracked
        /// </response>
        [HttpGet]
        public async Task<ApiResponse<List<PsAlert>>> GetAll() {
            List<PsAlert> alerts = await _AlertDb.GetAll();

            return ApiOk(alerts);
        }

        /// <summary>
        ///     Get an alert by it's instance ID, which is unique per world
        /// </summary>
        /// <param name="instanceID">Instance ID</param>
        /// <param name="worldID">World the instance ID is from</param>
        /// <response code="200">
        ///     The response will contain the <see cref="PsAlert"/> with <see cref="PsAlert.InstanceID"/> of <paramref name="instanceID"/>,
        ///     and <see cref="PsAlert.WorldID"/> of <paramref name="worldID"/>
        /// </response>
        /// <response code="204">
        ///     No <see cref="PsAlert"/> exists with those parameters
        /// </response>
        [HttpGet("{worldID}/{instanceID}")]
        public async Task<ApiResponse<PsAlert>> GetByInstanceID(int instanceID, short worldID) {
            PsAlert? alert = await _AlertDb.GetByInstanceID(instanceID, worldID);

            if (alert == null) {
                return ApiNoContent<PsAlert>();
            }

            return ApiOk(alert);
        }

        /// <summary>
        ///     Get the participant data for an alert, as well as data that would be useful in displaying this data, such as the characters relevant
        /// </summary>
        /// <remarks>
        ///     See <see cref="GetParticipants(long, bool, bool)"/> for more information
        /// </remarks>
        [HttpGet("{worldID}/{instanceID}/participants")]
        public async Task<ApiResponse<ExpandedAlertPlayers>> GetParticipantsByInstanceID(int instanceID, short worldID,
                [FromQuery] bool excludeCharacters = false,
                [FromQuery] bool excludeOutfits = false
            ) {

            PsAlert? alert = await _AlertDb.GetByInstanceID(instanceID, worldID);

            if (alert == null) {
                return ApiNotFound<ExpandedAlertPlayers>($"{nameof(PsAlert)} {worldID}-{instanceID}");
            }

            return await GetParticipants(alert.ID, excludeCharacters, excludeOutfits);
        }

        /// <summary>
        ///     Get the participant data for an alert, as well as data that would be useful in displaying this data, such as the characters relevant
        /// </summary>
        /// <param name="alertID">ID of the alert</param>
        /// <param name="excludeCharacters">
        ///     If the resulting <see cref="ExpandedAlertPlayers"/> will NOT have <see cref="ExpandedAlertPlayers.Characters"/> fill in. 
        ///     Useful for services that use Honu API, but not the frontend
        /// </param>
        /// <param name="excludeOutfits">
        ///     If the resulting <see cref="ExpandedAlertPlayers"/> will NOT have <see cref="ExpandedAlertPlayers.Outfits"/> fill in. 
        ///     Useful for services that use Honu API, but not the frontend
        /// </param>
        /// <response code="200">
        ///     The response will contain the <see cref="ExpandedAlertPlayers"/> for the alert requested, 
        ///     optionally including data that may be useful to the requester. The requester can request to not
        ///     recieve this data, by using <paramref name="excludeCharacters"/> and co
        /// </response>
        /// <response code="400">
        ///     The alert exists, but has not yet finished
        /// </response>
        /// <response code="404">
        ///     No <see cref="PsAlert"/> with <see cref="PsAlert.ID"/> of <paramref name="alertID"/> exists
        /// </response>
        [HttpGet("{alertID}/participants")]
        public async Task<ApiResponse<ExpandedAlertPlayers>> GetParticipants(
                long alertID,
                [FromQuery] bool excludeCharacters = false,
                [FromQuery] bool excludeOutfits = false
            ) {

            Stopwatch timer = Stopwatch.StartNew();

            PsAlert? alert = await _AlertDb.GetByID(alertID);
            if (alert == null) {
                return ApiNotFound<ExpandedAlertPlayers>($"{nameof(PsAlert)} {alertID}");
            }

            long alertLoad = timer.ElapsedMilliseconds; timer.Restart();

            DateTime alertEnd = alert.Timestamp + TimeSpan.FromSeconds(alert.Duration);
            if (DateTime.UtcNow < alertEnd) {
                return ApiBadRequest<ExpandedAlertPlayers>($"{nameof(PsAlert)} {alertID} has not finished ({alertEnd:u})");
            }

            ExpandedAlertPlayers block = new ExpandedAlertPlayers();

            List<AlertPlayerDataEntry> entries = await _ParticipantDataRepository.GetByAlert(alert, CancellationToken.None);
            block.Entries = entries;

            block.ProfileData = await _ProfileDataDb.GetByAlertID(alert.ID);

            long dataLoad = timer.ElapsedMilliseconds; timer.Restart();

            if (excludeCharacters == false) {
                List<string> charIDs = entries.Select(iter => iter.CharacterID).Distinct().ToList();
                List<PsCharacter> characters = await _CharacterRepository.GetByIDs(charIDs, CensusEnvironment.PC, fast: true);
                block.Characters = characters.Select(iter => new MinimalCharacter() {
                    ID = iter.ID, OutfitID = iter.OutfitID, OutfitTag = iter.OutfitTag, Name = iter.Name, FactionID = iter.FactionID
                }).ToList();
            }

            long characterLoad = timer.ElapsedMilliseconds; timer.Restart();

            if (excludeOutfits == false) {
                List<string> outfitIDs = entries.Where(iter => iter.OutfitID != null).Select(iter => iter.OutfitID!).Distinct().ToList(); // force is safe
                List<PsOutfit> outfits = await _OutfitRepository.GetByIDs(outfitIDs);
                block.Outfits = outfits;
            }

            long outfitLoad = timer.ElapsedMilliseconds; timer.Restart();

            _Logger.LogDebug($"Loading alert {alert.ID}/{alert.WorldID}-{alert.InstanceID}: Alert: {alertLoad}ms, data: {dataLoad}ms, characters: {characterLoad}ms, outfits: {outfitLoad}ms");

            return ApiOk(block);
        }

        /// <summary>
        ///     Get the facility control events (capture and defend) of an alert
        /// </summary>
        /// <param name="alertID">ID of the alert</param>
        /// <response code="200">
        ///     The response will contain a list of <see cref="FacilityControlDbEntry"/>s that took place during the alert
        /// </response>
        /// <response code="404">
        ///     No <see cref="PsAlert"/> with <see cref="PsAlert.ID"/> of <paramref name="alertID"/> exists
        /// </response>
        [HttpGet("{alertID}/control")]
        public async Task<ApiResponse<List<ExpandedFacilityControlEvent>>> GetControlEvents(long alertID) {
            PsAlert? alert = await _AlertDb.GetByID(alertID);
            if (alert == null) {
                return ApiNotFound<List<ExpandedFacilityControlEvent>>($"{nameof(PsAlert)} {alertID}");
            }

            List<FacilityControlEvent> controls = await _ControlDb.GetEvents(new FacilityControlOptions() {
                PeriodStart = alert.Timestamp,
                PeriodEnd = alert.Timestamp + TimeSpan.FromSeconds(alert.Duration),
                WorldIDs = new List<short>() { alert.WorldID },
                PlayerThreshold = 0,
                UnstableState = null,
                ZoneID = alert.ZoneID
            });

            List<ExpandedFacilityControlEvent> ex = new List<ExpandedFacilityControlEvent>(controls.Count);

            foreach (FacilityControlEvent ev in controls) {
                ex.Add(new ExpandedFacilityControlEvent() {
                    Event = ev,
                    Outfit = (ev.NewFactionID != ev.OldFactionID && ev.OutfitID != "0" && ev.OutfitID != null) ? await _OutfitRepository.GetByID(ev.OutfitID) : null,
                    Facility = await _FacilityRepository.GetByID(ev.FacilityID)

                });
            }

            return ApiOk(ex);
        }

        /// <summary>
        ///     Get the population of an alert
        /// </summary>
        /// <param name="alertID">ID of the alert</param>
        /// <response code="200">
        ///     The response will contain a list of <see cref="AlertPopulation"/>s that
        ///     represent the changing population over the course of an alert
        /// </response>
        /// <response code="404">
        ///     No <see cref="PsAlert"/> with <see cref="PsAlert.ID"/> of <paramref name="alertID"/> exists
        /// </response>
        [HttpGet("{alertID}/population")]
        public async Task<ApiResponse<List<AlertPopulation>>> GetPopulation(long alertID) {
            PsAlert? alert = await _AlertDb.GetByID(alertID);
            if (alert == null) {
                return ApiNotFound<List<AlertPopulation>>($"{nameof(PsAlert)} {alertID}");
            }

            List<AlertPopulation> pops = await _AlertPopulationRepository.GetByAlertID(alertID, CancellationToken.None);

            return ApiOk(pops);
        }

        [HttpPost]
        [PermissionNeeded(HonuPermission.ALERT_CREATE)]
        [Authorize]
        public async Task<ApiResponse<long>> CreateCustom([FromBody] PsAlert parameters) {
            List<string> errors = new();

            if (string.IsNullOrEmpty(parameters.Name)) {
                errors.Add("Missing name");
            }
            if (World.All.IndexOf(parameters.WorldID) == -1) {
                errors.Add($"Invalid world {parameters.WorldID}");
            }
            if (parameters.ZoneID != 0 && Zone.StaticZones.IndexOf(parameters.ZoneID) == -1) {
                errors.Add($"Invalid zone {parameters.ZoneID}");
            }

            if (errors.Count > 0) {
                return ApiBadRequest<long>($"{string.Join(", ", errors)}");
            }

            long ID = await _AlertDb.Insert(parameters);

            return ApiOk(ID);
        }

    }
}
