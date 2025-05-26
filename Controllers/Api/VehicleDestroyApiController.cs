using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Models;
using watchtower.Models.Api;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Events;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    [Route("/api/vehicle-destroy")]
    [ApiController]
    public class VehicleDestroyApiController : ApiControllerBase {

        private readonly ILogger<VehicleDestroyApiController> _Logger;

        private readonly VehicleDestroyDbStore _VehicleDestroyDb;
        private readonly SessionDbStore _SessionDb;

        private readonly CharacterRepository _CharacterRepository;
        private readonly VehicleRepository _VehicleRepository;
        private readonly ItemRepository _ItemRepository;
        private readonly ItemCategoryRepository _ItemCategoryRepository;

        public VehicleDestroyApiController(ILogger<VehicleDestroyApiController> logger,
            VehicleDestroyDbStore vehicleDestroyDb, SessionDbStore sessionDb,
            CharacterRepository charRepo, VehicleRepository vehRepo,
            ItemRepository itemRepo, ItemCategoryRepository itemCategoryRepository) {

            _Logger = logger;

            _VehicleDestroyDb = vehicleDestroyDb;
            _SessionDb = sessionDb;
            _CharacterRepository = charRepo;
            _VehicleRepository = vehRepo;
            _ItemRepository = itemRepo;
            _ItemCategoryRepository = itemCategoryRepository;
        }

        /// <summary>
        ///     Get the vehicle destory events that occured during a session
        /// </summary>
        /// <param name="sessionID">ID of the session</param>
        /// <response code="200">
        ///     The response will contain a list of <see cref="ExpandedVehicleDestroyEvent"/>s
        ///     that happening during the <see cref="Session"/> with <see cref="Session.ID"/> of <paramref name="sessionID"/>
        /// </response>
        /// <response code="404">
        ///     No <see cref="Session"/> with <see cref="Session.ID"/> of <paramref name="sessionID"/> exists
        /// </response>
        [HttpGet("session/{sessionID}")]
        public async Task<ApiResponse<List<ExpandedVehicleDestroyEvent>>> GetBySessionID(long sessionID) {
            Session? session = await _SessionDb.GetByID(sessionID);
            if (session == null) {
                return ApiNotFound<List<ExpandedVehicleDestroyEvent>>($"{nameof(Session)} {sessionID}");
            }

            List<VehicleDestroyEvent> events = await _VehicleDestroyDb.GetByCharacterID(session.CharacterID, session.Start, session.End ?? DateTime.UtcNow);

            List<string> charIDs = events.Select(iter => iter.AttackerCharacterID).Union(events.Select(iter => iter.KilledCharacterID)).ToList();

            Dictionary<string, PsCharacter> chars;
            try {
                chars = (await _CharacterRepository.GetByIDs(charIDs, CensusEnvironment.PC, true))
                    .ToDictionary(iter => iter.ID);
            } catch (Exception ex) {
                chars = new Dictionary<string, PsCharacter>();
                _Logger.LogWarning($"failed to get characters for vehicle destroy by session [sessionID={sessionID}] [Exception={ex.Message}]");
            }

            List<ExpandedVehicleDestroyEvent> exs = new(events.Count);
            foreach (VehicleDestroyEvent ev in events) {
                ExpandedVehicleDestroyEvent ex = new();
                ex.Event = ev;

                ex.Attacker = chars.GetValueOrDefault(ev.AttackerCharacterID);
                ex.AttackerVehicle = await _VehicleRepository.GetByID(int.Parse(ev.AttackerVehicleID));

                ex.Killed = chars.GetValueOrDefault(ev.KilledCharacterID);
                ex.KilledVehicle = await _VehicleRepository.GetByID(int.Parse(ev.KilledVehicleID));

                ex.Item = await _ItemRepository.GetByID(ev.AttackerWeaponID);

                exs.Add(ex);
            }

            return ApiOk(exs);
        }

        [HttpGet("session/{sessionID}/block")]
        public async Task<ApiResponse<VehicleKillDeathBlock>> GetBySessionIDBlock(long sessionID) {
            Session? session = await _SessionDb.GetByID(sessionID);
            if (session == null) {
                return ApiNotFound<VehicleKillDeathBlock>($"{nameof(Session)} {sessionID}");
            }

            List<VehicleDestroyEvent> events = await _VehicleDestroyDb.GetByCharacterID(session.CharacterID, session.Start, session.End ?? DateTime.UtcNow);

            VehicleKillDeathBlock block = new();
            block.Kills = events.Where(iter => iter.AttackerCharacterID == session.CharacterID && iter.KilledCharacterID != session.CharacterID).ToList();
            block.Deaths = events.Where(iter => iter.KilledCharacterID == session.CharacterID).ToList();

            // load characters
            List<string> IDs = events.Select(iter => iter.AttackerCharacterID).Distinct().ToList();
            IDs.AddRange(events.Select(iter => iter.KilledCharacterID).Distinct());
            block.Characters = await _CharacterRepository.GetByIDs(IDs, CensusEnvironment.PC);

            // load items
            IEnumerable<int> itemIDs = events.Select(iter => iter.AttackerWeaponID).Distinct();
            block.Weapons = await _ItemRepository.GetByIDs(itemIDs);

            // load item categories
            IEnumerable<int> categoryIDs = block.Weapons.Select(iter => iter.CategoryID).Distinct();
            block.ItemCategories = await _ItemCategoryRepository.GetByIDs(categoryIDs);

            // load vehicles
            List<int> vehicleIDs = events.Select(iter => int.Parse(iter.AttackerVehicleID)).Distinct().ToList();
            vehicleIDs.AddRange(events.Select(iter => int.Parse(iter.KilledVehicleID)).Distinct());
            block.Vehicles = await _VehicleRepository.GetByIDs(vehicleIDs);

            return ApiOk(block);
        }

    }
}
