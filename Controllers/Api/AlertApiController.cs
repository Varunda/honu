using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Models.Alert;
using watchtower.Models.Census;
using watchtower.Models.Db;
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

        public AlertApiController(ILogger<AlertApiController> logger,
                AlertPlayerDataRepository participantDataRepository, AlertDbStore alertDb,
                CharacterRepository characterRepository,
                OutfitRepository outfitRepository, SessionDbStore sessionDb,
                AlertPlayerProfileDataDbStore profileDataDb) {

            _Logger = logger;

            _ParticipantDataRepository = participantDataRepository;
            _AlertDb = alertDb;
            _CharacterRepository = characterRepository;
            _OutfitRepository = outfitRepository;
            _SessionDb = sessionDb;
            _ProfileDataDb = profileDataDb;
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

            PsAlert? alert = await _AlertDb.GetByID(alertID);
            if (alert == null) {
                return ApiNotFound<ExpandedAlertPlayers>($"{nameof(PsAlert)} {alertID}");
            }

            DateTime alertEnd = alert.Timestamp + TimeSpan.FromSeconds(alert.Duration);
            if (DateTime.UtcNow < alertEnd) {
                return ApiBadRequest<ExpandedAlertPlayers>($"{nameof(PsAlert)} {alertID} has not finished ({alertEnd:u})");
            }

            ExpandedAlertPlayers block = new ExpandedAlertPlayers();

            List<AlertPlayerDataEntry> entries = await _ParticipantDataRepository.GetByAlert(alert, CancellationToken.None);
            block.Entries = entries;

            block.ProfileData = await _ProfileDataDb.GetByAlertID(alert.ID);

            if (excludeCharacters == false) {
                List<string> charIDs = entries.Select(iter => iter.CharacterID).Distinct().ToList();
                List<PsCharacter> characters = await _CharacterRepository.GetByIDs(charIDs);
                block.Characters = characters.Select(iter => new MinimalCharacter() {
                    ID = iter.ID, OutfitID = iter.OutfitID, OutfitTag = iter.OutfitTag, Name = iter.Name, FactionID = iter.FactionID
                }).ToList();
            }

            if (excludeOutfits == false) {
                List<string> outfitIDs = entries.Where(iter => iter.OutfitID != null).Select(iter => iter.OutfitID!).Distinct().ToList(); // force is safe
                List<PsOutfit> outfits = await _OutfitRepository.GetByIDs(outfitIDs);
                block.Outfits = outfits;
            }

            return ApiOk(block);
        }

    }
}
