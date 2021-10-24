using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("/api/item")]
    public class ItemApiController : ControllerBase {

        private readonly ILogger<ItemApiController> _Logger;
        private readonly IItemRepository _ItemRepository;

        public ItemApiController(ILogger<ItemApiController> logger,
            IItemRepository itemRepo) {

            _Logger = logger;
            _ItemRepository = itemRepo;
        }

        [HttpGet("{itemID}")]
        public async Task<ActionResult<PsItem>> GetByID(string itemID) {
            PsItem? item = await _ItemRepository.GetByID(itemID);
            if (item == null) {
                return NoContent();
            }
            return Ok(item);
        }

        [HttpGet]
        public async Task<ActionResult<List<PsItem>>> GetMultiple([FromQuery] List<string> IDs) {
            List<PsItem> items = new List<PsItem>();

            foreach (string ID in IDs) {
                PsItem? item = await _ItemRepository.GetByID(ID);
                if (item != null) {
                    items.Add(item);
                }
            }

            return Ok(items);
        }

    }
}
