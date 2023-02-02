using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Models.Api;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Events;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    [Route("/api/item-added")]
    [ApiController]
    public class ItemAddedEventApiController : ApiControllerBase {

        private readonly ILogger<ItemAddedEventApiController> _Logger;
        private readonly ItemAddedDbStore _ItemAddedDb;
        private readonly SessionRepository _SessionRepository;
        private readonly ItemRepository _ItemRepository;

        public ItemAddedEventApiController(ILogger<ItemAddedEventApiController> logger,
            ItemAddedDbStore itemAddedDb, SessionRepository sessionRepository,
            ItemRepository itemRepository) {

            _Logger = logger;

            _ItemAddedDb = itemAddedDb;
            _SessionRepository = sessionRepository;
            _ItemRepository = itemRepository;
        }

        [HttpGet("session/{sessionID}")]
        public async Task<ApiResponse<ItemAddedEventBlock>> GetBySessionID(long sessionID) {
            Session? session = await _SessionRepository.GetByID(sessionID);
            if (session == null) {
                return ApiNotFound<ItemAddedEventBlock>($"{nameof(Session)} {sessionID}");
            }

            List<ItemAddedEvent> evs = await _ItemAddedDb.GetByCharacterAndPeriod(session.CharacterID, session.Start, session.End ?? DateTime.UtcNow);

            ItemAddedEventBlock block = new();
            block.Events = evs;

            Dictionary<int, PsItem?> items = new();

            foreach (ItemAddedEvent ev in block.Events) {
                if (items.ContainsKey(ev.ItemID) == true) {
                    continue;
                }

                PsItem? item = await _ItemRepository.GetByID(ev.ItemID);
                items.Add(ev.ItemID, item);
            }

            block.Items = items.Values.Where(iter => iter != null).Select(iter => (PsItem)iter!).ToList();

            return ApiOk(block);
        }

    }
}
