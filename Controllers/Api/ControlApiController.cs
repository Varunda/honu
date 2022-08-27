using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Models;
using watchtower.Models.Api;
using watchtower.Models.Census;
using watchtower.Models.Events;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("/api/control")]
    public class ControlApiController : ApiControllerBase {

        private readonly ILogger<ControlApiController> _Logger;
        private readonly FacilityControlDbStore _ControlDb;
        private readonly FacilityPlayerControlDbStore _PlayerControlDb;
        private readonly CharacterRepository _CharacterRepository;

        public ControlApiController(ILogger<ControlApiController> logger,
            FacilityControlDbStore controlDb, FacilityPlayerControlDbStore playerControlDb,
            CharacterRepository characterRepository) {

            _Logger = logger;
            _ControlDb = controlDb;
            _PlayerControlDb = playerControlDb;
            _CharacterRepository = characterRepository;
        }

        /// <summary>
        ///     Get the <see cref="PlayerControlEvent"/>s of a <see cref="FacilityControlEvent"/>
        /// </summary>
        /// <param name="controlID">ID of the <see cref="PlayerControlEvent"/></param>
        /// <response code="200">
        ///     The response will contain a list of <see cref="PlayerControlEvent"/> 
        ///     with <see cref="PlayerControlEvent.ControlID"/> of <paramref name="controlID"/>
        /// </response>
        /// <response code="404">
        ///     No <see cref="FacilityControlEvent"/> with <see cref="FacilityControlEvent.ID"/> of <paramref name="controlID"/> exists
        /// </response>
        [HttpGet("{controlID}/players")]
        public async Task<ApiResponse<List<ExpandedPlayerControlEvent>>> GetByControlID(long controlID) {
            FacilityControlEvent? ev = await _ControlDb.GetByID(controlID);
            if (ev == null) {
                return ApiNotFound<List<ExpandedPlayerControlEvent>>($"{nameof(FacilityControlEvent)} {controlID}");
            }

            List<PlayerControlEvent> players = await _PlayerControlDb.GetByEventID(controlID);

            List<ExpandedPlayerControlEvent> expanded = new List<ExpandedPlayerControlEvent>(players.Count);

            List<PsCharacter> chars = await _CharacterRepository.GetByIDs(players.Select(iter => iter.CharacterID).ToList(), CensusEnvironment.PC);

            foreach (PlayerControlEvent i in players) {
                expanded.Add(new ExpandedPlayerControlEvent() {
                    Event = i,
                    Character = chars.FirstOrDefault(iter => iter.ID == i.CharacterID)
                });
            }

            return ApiOk(expanded);
        }

    }
}
