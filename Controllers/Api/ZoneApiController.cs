using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {


    [ApiController]
    [Route("/api/zone")]
    public class ZoneApiController : ApiControllerBase {

        private readonly ILogger<ZoneApiController> _Logger;

        private readonly MapRepository _MapRepository;


    }
}
