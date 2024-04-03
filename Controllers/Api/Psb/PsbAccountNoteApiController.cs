using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code;
using watchtower.Models;
using watchtower.Models.Internal;
using watchtower.Models.PSB;
using watchtower.Services.Db;
using watchtower.Services.Repositories.PSB;

namespace watchtower.Controllers.Api.Psb {

    [ApiController]
    [Route("/api/psb-notes")]
    public class PsbAccountNoteApiController : ApiControllerBase {

        private readonly ILogger<PsbAccountNoteApiController> _Logger;
        private readonly PsbAccountNoteDbStore _NoteDb;
        private readonly PsbAccountRepository _NamedRepository;

        public PsbAccountNoteApiController(ILogger<PsbAccountNoteApiController> logger,
            PsbAccountNoteDbStore db, PsbAccountRepository namedRepo) {

            _Logger = logger;
            _NoteDb = db;
            _NamedRepository = namedRepo;
        }

        /// <summary>
        ///     get the list of <see cref="PsbAccountNote"/>s on an account
        /// </summary>
        /// <param name="accountID"></param>
        /// <returns></returns>
        [HttpGet("account/{accountID}")]
        [PermissionNeeded(HonuPermission.PSB_NAMED_GET)]
        [Authorize]
        public async Task<ApiResponse<List<PsbAccountNote>>> GetByAccountID(long accountID) {
            PsbAccount? account = await _NamedRepository.GetByID(accountID);
            if (account == null) {
                return ApiNotFound<List<PsbAccountNote>>($"{nameof(PsbAccount)} {accountID}");
            }

            List<PsbAccountNote> notes = await _NoteDb.GetByAccountID(accountID, CancellationToken.None);
            return ApiOk(notes);
        }

    }
}
