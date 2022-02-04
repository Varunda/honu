using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code;
using watchtower.Models;
using watchtower.Models.Api.PSB;
using watchtower.Models.Census;
using watchtower.Models.PSB;
using watchtower.Services;
using watchtower.Services.Repositories;
using watchtower.Services.Repositories.PSB;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("/api/psb-named/")]
    [Authorize]
    [PsbAdmin]
    public class PsbNamedApiController : ApiControllerBase {

        private readonly ILogger<PsbNamedApiController> _Logger;
        private readonly CurrentHonuAccount _CurrentUser;

        private readonly PsbAccountRepository _NamedRepository;
        private readonly CharacterRepository _CharacterRepository;

        public PsbNamedApiController(ILogger<PsbNamedApiController> logger,
            PsbAccountRepository namedRepo, CharacterRepository charRepo,
            CurrentHonuAccount current) {

            _Logger = logger;
            _CurrentUser = current;

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
            List<PsbNamedAccount> named = await _NamedRepository.GetAll();

            List<ExpandedPsbNamedAccount> expanded = await MakeExpanded(named);

            return ApiOk(expanded);
        }

        /// <summary>
        ///     Get a specific PSB account
        /// </summary>
        /// <param name="ID">ID of the account</param>
        /// <response code="200">
        ///     The response will contain the <see cref="PsbNamedAccount"/>
        ///     with <see cref="PsbNamedAccount.ID"/> of <paramref name="ID"/>
        /// </response>
        /// <response code="204">
        ///     No <see cref="PsbNamedAccount"/> with <see cref="PsbNamedAccount.ID"/> of <paramref name="ID"/> exists
        /// </response>
        [HttpGet("{ID}")]
        public async Task<ApiResponse<ExpandedPsbNamedAccount>> GetByID(long ID) {
            PsbNamedAccount? acc = await _NamedRepository.GetByID(ID);
            if (acc == null) {
                return ApiNoContent<ExpandedPsbNamedAccount>();
            }

            ExpandedPsbNamedAccount ex = await MakeExpanded(acc);

            return ApiOk(ex);
        }

        /// <summary>
        ///     Recheck an account
        /// </summary>
        /// <param name="accountID">Account ID to recheck</param>
        /// <response code="200">
        ///     Returned after the recheck is complete
        /// </response>
        [HttpGet("recheck/{accountID}")]
        public async Task<ApiResponse<PsbNamedAccount>> Recheck(long accountID) {
            PsbNamedAccount? acc = await _NamedRepository.GetByID(accountID);
            if (acc == null) {
                return ApiNotFound<PsbNamedAccount>($"{nameof(PsbNamedAccount)} {accountID}");
            }

            acc = await _NamedRepository.RecheckByID(accountID);
            if (acc == null) {
                throw new Exception($"acc was null after checking if it exists");
            }

            return ApiOk(acc);
        }

        /// <summary>
        ///     Mark an account as deleted
        /// </summary>
        /// <param name="ID">ID of the account</param>
        /// <response code="200">
        ///     The <see cref="PsbNamedAccount"/> with <see cref="PsbNamedAccount.ID"/> of <paramref name="ID"/>
        ///     was successfully marked as deleted
        /// </response>
        /// <response code="400">
        ///     The <see cref="PsbNamedAccount"/> with <see cref="PsbNamedAccount.ID"/> of <paramref name="ID"/>
        ///     was already deleted
        /// </response>
        /// <response code="404">
        ///     No <see cref="PsbNamedAccount"/> with <see cref="PsbNamedAccount.ID"/> of <paramref name="ID"/> exists
        /// </response>
        [HttpDelete("{ID}")]
        public async Task<ApiResponse> Delete(long ID) {
            PsbNamedAccount? acc = await _NamedRepository.GetByID(ID);
            if (acc == null) {
                return ApiNotFound($"{nameof(PsbNamedAccount)} {ID}");
            }

            if (acc.DeletedAt != null || acc.DeletedBy != null) {
                return ApiBadRequest($"{nameof(PsbNamedAccount)} {ID} has already been deleted");
            }

            HonuAccount? account = await _CurrentUser.Get();
            if (account == null) {
                return ApiInternalError("Failed to get current user");
            }

            await _NamedRepository.DeleteByID(ID, account.ID);

            return ApiOk();
        }

        /// <summary>
        ///     Get the characters that would exist for a set
        /// </summary>
        /// <param name="tag">Tag to use</param>
        /// <param name="name">Name of the characters</param>
        /// <response code="200">
        ///     The response will contain the <see cref="PsbCharacterSet"/> that matches the tag and name passed
        /// </response>
        [HttpGet("character-set")]
        public async Task<ApiResponse<PsbCharacterSet>> GetCharacterSet([FromQuery] string? tag, [FromQuery] string name) {
            PsbCharacterSet set = await _NamedRepository.GetCharacterSet(tag, name);

            return ApiOk(set);
        }

        /// <summary>
        ///     Create a new account to be tracked
        /// </summary>
        /// <param name="tag">Tag of the account to be tracked</param>
        /// <param name="name">Name of the account to be tracked</param>
        /// <response code="200">
        ///     The response will contain the <see cref="PsbNamedAccount"/> that was created
        /// </response>
        /// <resposne code="400">
        ///     One of the following validation errors occured:
        ///     <ul>
        ///         <li>
        ///             One of the 4 faction characters could not be found
        ///         </li>
        ///         <li>
        ///             An account using the tag and name already exists
        ///         </li>
        ///     </ul>
        /// </resposne>
        [HttpPost]
        public async Task<ApiResponse<PsbNamedAccount>> Create([FromQuery] string? tag, [FromQuery] string name) {
            PsbCharacterSet set = await _NamedRepository.GetCharacterSet(tag, name);
            if (set.VS == null || set.NC == null || set.TR == null || set.NS == null) {
                return ApiBadRequest<PsbNamedAccount>($"One of the faction characters does not exist");
            }

            PsbNamedAccount? acc = await _NamedRepository.GetByTagAndName(tag, name);
            if (acc != null && acc.DeletedAt == null) {
                return ApiBadRequest<PsbNamedAccount>($"A named account for {tag}x{name} already exists");
            }

            acc = await _NamedRepository.Create(tag, name);

            return ApiOk(acc);
        }

        /// <summary>
        ///     Rename an existing named account with a new name. Used when the player who owns the account has not changed,
        ///     but they have renamed for whatever reason
        /// </summary>
        /// <param name="accountID">ID of the account</param>
        /// <param name="tag">New tag of the account</param>
        /// <param name="name">New name of the account</param>
        /// <response code="200">
        ///     The response will contain the <see cref="PsbNamedAccount"/> that was successfully renamed using the parameters
        /// </response>
        /// <response code="400">
        ///     One of the following errors occured. The response data will have a string describing the error
        ///     <ul>
        ///         <li>
        ///             One of the 4 characters (one for each faction) was missing
        ///         </li>
        ///         <li>
        ///             A named account using the tag and name already exist
        ///         </li>
        ///     </ul>
        /// </response>
        /// <response code="404">
        ///     No <see cref="PsbNamedAccount"/>  with <see cref="PsbNamedAccount.ID"/> of <paramref name="accountID"/> exists
        /// </response>
        [HttpPost("{accountID}")]
        public async Task<ApiResponse<PsbNamedAccount>> Rename(long accountID, [FromQuery] string? tag, [FromQuery] string name) {
            if (string.IsNullOrEmpty(name) == true) {
                return ApiBadRequest<PsbNamedAccount>($"Missing {nameof(name)} parameter");
            }

            PsbNamedAccount? acc = await _NamedRepository.GetByID(accountID);
            if (acc == null) {
                return ApiNotFound<PsbNamedAccount>($"{nameof(PsbNamedAccount)} {accountID}");
            }

            PsbNamedAccount? existing = await _NamedRepository.GetByTagAndName(tag, name);
            if (existing != null && existing.ID != accountID) {
                return ApiBadRequest<PsbNamedAccount>($"A {nameof(PsbNamedAccount)} already exist with name {tag}x{name}");
            }

            PsbCharacterSet set = await _NamedRepository.GetCharacterSet(tag, name);
            List<string> errors = new List<string>();
            if (set.VS == null) { errors.Add($"Missing VS character"); }
            if (set.NC == null) { errors.Add($"Missing NC character"); }
            if (set.TR == null) { errors.Add($"Missing TR character"); }
            if (set.NS == null) { errors.Add($"Missing NS character"); }
            if (errors.Count > 0) {
                return ApiBadRequest<PsbNamedAccount>($"One of the faction is missing a character: {string.Join(", ", errors)}");
            }

            acc = await _NamedRepository.Rename(accountID, tag, name);
            if (acc == null) {
                return ApiBadRequest<PsbNamedAccount>($"failed to rename, generic error");
            }

            return ApiOk(acc);
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

        private async Task<ExpandedPsbNamedAccount> MakeExpanded(PsbNamedAccount acc) {
            List<ExpandedPsbNamedAccount> ex = await MakeExpanded(new List<PsbNamedAccount>() { acc });
            return ex.ElementAt(0);
        }

    }
}
