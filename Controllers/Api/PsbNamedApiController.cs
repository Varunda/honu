using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code;
using watchtower.Models;
using watchtower.Models.Api.PSB;
using watchtower.Models.Census;
using watchtower.Models.PSB;
using watchtower.Services.Repositories;
using watchtower.Services.Repositories.PSB;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("/api/psb-named/")]
    //[Authorize]
    //[PsbAdmin]
    public class PsbNamedApiController : ApiControllerBase {

        private readonly ILogger<PsbNamedApiController> _Logger;
        private readonly PsbAccountRepository _NamedRepository;
        private readonly ICharacterRepository _CharacterRepository;

        public PsbNamedApiController(ILogger<PsbNamedApiController> logger,
            PsbAccountRepository namedRepo, ICharacterRepository charRepo) {

            _Logger = logger;
            _NamedRepository = namedRepo;
            _CharacterRepository = charRepo;
        }

        /// <summary>
        ///     Get all PSB named accounts
        /// </summary>
        /// <response code="200">
        ///     The response will contain a list of all PSB named accounts
        /// </response>
        [HttpGet]
        public async Task<ApiResponse<List<ExpandedPsbNamedAccount>>> GetAll() {
            List<PsbNamedAccount> named = (await _NamedRepository.GetAll()).Where(iter => iter.DeletedBy == null).ToList();

            List<ExpandedPsbNamedAccount> expanded = await MakeExpanded(named);

            return ApiOk(expanded);
        }

        /// <summary>
        ///     Take a list of <see cref="PsbNamedAccount"/>s and make <see cref="ExpandedPsbNamedAccount"/>s for them
        /// </summary>
        /// <param name="named"></param>
        /// <returns></returns>
        private async Task<List<ExpandedPsbNamedAccount>> MakeExpanded(List<PsbNamedAccount> named) {
            List<ExpandedPsbNamedAccount> ex = new List<ExpandedPsbNamedAccount>(named.Count);

            List<string> IDs = new List<string>();

            foreach (PsbNamedAccount acc in named) {
                if (acc.VsID != null) { IDs.Add(acc.VsID);  }
                if (acc.NcID != null) { IDs.Add(acc.NcID);  }
                if (acc.TrID != null) { IDs.Add(acc.TrID);  }
                if (acc.NsID != null) { IDs.Add(acc.NsID);  }
            }

            List<PsCharacter> characters = await _CharacterRepository.GetByIDs(IDs, true);

            foreach (PsbNamedAccount acc in named) {
                PsCharacter? vs = (acc.VsID != null) ? characters.FirstOrDefault(iter => iter.ID == acc.VsID) : null;
                PsCharacter? nc = (acc.NcID != null) ? characters.FirstOrDefault(iter => iter.ID == acc.NcID) : null;
                PsCharacter? tr = (acc.TrID != null) ? characters.FirstOrDefault(iter => iter.ID == acc.TrID) : null;
                PsCharacter? ns = (acc.NsID != null) ? characters.FirstOrDefault(iter => iter.ID == acc.NsID) : null;

                ExpandedPsbNamedAccount expanded = new ExpandedPsbNamedAccount() {
                    Account = acc,
                    VsCharacter = vs,
                    NcCharacter = nc,
                    TrCharacter = tr,
                    NsCharacter = ns
                };

                ex.Add(expanded);
            }

            return ex;
        }

    }
}
