using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Code;
using watchtower.Models;
using watchtower.Models.Internal;
using watchtower.Models.PSB;
using watchtower.Services.Repositories.PSB;

namespace watchtower.Controllers.Api.Psb {

    [ApiController]
    [Route("/api/psb/contact")]
    public class PsbContactApiController : ApiControllerBase {

        private readonly ILogger<PsbOvOUsageApiController> _Logger;

        private readonly PsbOvOSheetRepository _SheetRepository;
        private readonly PsbOvOAccountUsageRepository _UsageRepository;
        private readonly PsbOvOAccountRepository _AccountRepository;
        private readonly PsbContactSheetRepository _ContactRepository;

        public PsbContactApiController(ILogger<PsbOvOUsageApiController> logger,
            PsbOvOSheetRepository sheetRepository, PsbOvOAccountUsageRepository usageRepository,
            PsbOvOAccountRepository accountRepository, PsbContactSheetRepository contactRepository) {

            _Logger = logger;

            _SheetRepository = sheetRepository;
            _UsageRepository = usageRepository;
            _AccountRepository = accountRepository;
            _ContactRepository = contactRepository;
        }

        [HttpGet]
        [PermissionNeeded(HonuPermission.PSB_OVO_GET)]
        [Authorize]
        public async Task<ApiResponse<List<PsbOvOContact>>> GetGroups() {
            List<PsbOvOContact> contacts = await _ContactRepository.GetOvOContacts();

            return ApiOk(contacts);
        }

    }
}
