using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Models.Internal;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("/api/permission")]
    public class HonuPermissionApiController : ApiControllerBase {

        private readonly ILogger<HonuPermissionApiController> _Logger;

        public HonuPermissionApiController(ILogger<HonuPermissionApiController> logger) {
            _Logger = logger;
        }

        /// <summary>
        ///     Get all permissions available to users
        /// </summary>
        /// <response code="200">
        ///     A list of <see cref="HonuPermission"/>s
        /// </response>
        [HttpGet]
        public ApiResponse<List<HonuPermission>> GetAll() {
            return ApiOk(HonuPermission.All);
        }

    }
}
