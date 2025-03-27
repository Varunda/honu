using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Models.Api;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("/api/name-fight")]
    public class NameFightApiController : ApiControllerBase {

        private readonly ILogger<NameFightApiController> _Logger;
        private readonly NameFightRepository _NameFightRepository;

        public NameFightApiController(ILogger<NameFightApiController> logger,
            NameFightRepository nameFightRepository) {

            _Logger = logger;
            _NameFightRepository = nameFightRepository;
        }

        [HttpGet]
        public async Task<ApiResponse<List<NameFightEntry>>> Get() {
            List<NameFightEntry> entry = await _NameFightRepository.Get();
            return ApiOk(entry);
        }

    }
}
