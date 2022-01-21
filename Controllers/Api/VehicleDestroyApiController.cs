using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Models.Api;
using watchtower.Models.Db;
using watchtower.Models.Events;
using watchtower.Services.Db;
using watchtower.Services.Repositories;
using watchtower.Services.Repositories.Implementations;

namespace watchtower.Controllers.Api {

    [Route("/api/vehicle-destroy")]
    [ApiController]
    public class VehicleDestroyApiController : ApiControllerBase {

        private readonly ILogger<VehicleDestroyApiController> _Logger;

        private VehicleDestroyDbStore _VehicleDestroyDb;
        private ISessionDbStore _SessionDb;

        private readonly ICharacterRepository _CharacterRepository;
        private readonly VehicleRepository _VehicleRepository;
        private readonly ItemRepository _ItemRepository;

        public VehicleDestroyApiController(ILogger<VehicleDestroyApiController> logger,
            VehicleDestroyDbStore vehicleDestroyDb, ISessionDbStore sessionDb,
            ICharacterRepository charRepo, VehicleRepository vehRepo,
            ItemRepository itemRepo) {

            _Logger = logger;

            _VehicleDestroyDb = vehicleDestroyDb;
            _SessionDb = sessionDb;
            _CharacterRepository = charRepo;
            _VehicleRepository = vehRepo;
            _ItemRepository = itemRepo;
        }

        [HttpGet("session/{sessionID}")]
        public async Task<ApiResponse<List<ExpandedVehicleDestroyEvent>>> GetBySessionID(long sessionID) {
            Session? session = await _SessionDb.GetByID(sessionID);
            if (session == null) {
                return ApiNotFound<List<ExpandedVehicleDestroyEvent>>($"{nameof(Session)} {sessionID}");
            }

            List<VehicleDestroyEvent> events = await _VehicleDestroyDb.GetByCharacterID(session.CharacterID, session.Start, session.End ?? DateTime.UtcNow);

            List<ExpandedVehicleDestroyEvent> exs = new List<ExpandedVehicleDestroyEvent>(events.Count);

            foreach (VehicleDestroyEvent ev in events) {
                ExpandedVehicleDestroyEvent ex = new ExpandedVehicleDestroyEvent();
                ex.Event = ev;
                ex.Attacker = await _CharacterRepository.GetByID(ev.AttackerCharacterID);
                ex.AttackerVehicle = await _VehicleRepository.GetByID(int.Parse(ev.AttackerVehicleID));
                ex.Killed = await _CharacterRepository.GetByID(ev.KilledCharacterID);
                ex.KilledVehicle = await _VehicleRepository.GetByID(int.Parse(ev.KilledVehicleID));
                ex.Item = await _ItemRepository.GetByID(ev.AttackerWeaponID);

                exs.Add(ex);
            }

            return ApiOk(exs);
        }

    }
}
