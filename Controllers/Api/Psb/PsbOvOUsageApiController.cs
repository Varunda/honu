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
    [Route("/api/psb/usage")]
    public class PsbOvOUsageApiController : ApiControllerBase {

        private readonly ILogger<PsbOvOUsageApiController> _Logger;

        private readonly PsbOvOSheetRepository _SheetRepository;
        private readonly PsbOvOAccountUsageRepository _UsageRepository;
        private readonly PsbOvOAccountRepository _AccountRepository;

        public PsbOvOUsageApiController(ILogger<PsbOvOUsageApiController> logger,
            PsbOvOSheetRepository sheetRepository, PsbOvOAccountUsageRepository usageRepository,
            PsbOvOAccountRepository accountRepository) {

            _Logger = logger;

            _SheetRepository = sheetRepository;
            _UsageRepository = usageRepository;
            _AccountRepository = accountRepository;
        }

        [HttpGet]
        [PermissionNeeded(HonuPermission.PSB_OVO_GET)]
        [Authorize]
        public async Task<ApiResponse<List<PsbDriveFile>>> GetGroups([FromQuery] string name) {
            List<PsbDriveFile> files = await _SheetRepository.GetOutfitUsage(name);

            return ApiOk(files);
        }

        [HttpGet("{fileID}")]
        [PermissionNeeded(HonuPermission.PSB_OVO_GET)]
        [Authorize]
        public async Task<ApiResponse<PsbOvOAccountSheet>> GetSheet(string fileID) {
            PsbOvOAccountSheet? usage = await _SheetRepository.GetByID(fileID);

            if (usage == null) {
                return ApiNoContent<PsbOvOAccountSheet>();
            }

            return ApiOk(usage);
        }

        [HttpGet("{fileID}/usage")]
        [PermissionNeeded(HonuPermission.PSB_OVO_GET)]
        [Authorize]
        public async Task<ApiResponse<List<PsbOvOAccountUsage>>> GetSheetUsage(string fileID) {
            PsbOvOAccountSheet? sheet = await _SheetRepository.GetByID(fileID);

            if (sheet == null) {
                return ApiNotFound<List<PsbOvOAccountUsage>>($"{nameof(PsbOvOAccountSheet)} {fileID}");
            }

            List<PsbOvOAccountUsage> usage = await _UsageRepository.CheckUsage(sheet);

            return ApiOk(usage);
        }

    }
}
