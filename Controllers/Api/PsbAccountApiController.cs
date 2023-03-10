using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code;
using watchtower.Code.Constants;
using watchtower.Models;
using watchtower.Models.Api.PSB;
using watchtower.Models.Census;
using watchtower.Models.Internal;
using watchtower.Models.PSB;
using watchtower.Services;
using watchtower.Services.Repositories;
using watchtower.Services.Repositories.PSB;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("/api/psb-account/")]
    public class PsbAccountApiController : ApiControllerBase {

        private readonly ILogger<PsbAccountApiController> _Logger;
        private readonly CurrentHonuAccount _CurrentUser;

        private readonly PsbAccountRepository _NamedRepository;
        private readonly CharacterRepository _CharacterRepository;
        private readonly HonuAccountPermissionRepository _AccountPermissions;

        public PsbAccountApiController(ILogger<PsbAccountApiController> logger,
            PsbAccountRepository namedRepo, CharacterRepository charRepo,
            CurrentHonuAccount current, HonuAccountPermissionRepository accountPermissions) {

            _Logger = logger;
            _CurrentUser = current;

            _NamedRepository = namedRepo;
            _CharacterRepository = charRepo;
            _AccountPermissions = accountPermissions;
        }

        /// <summary>
        ///     Get all PSB accounts
        /// </summary>
        /// <response code="200">
        ///     The response will contain a list of all PSB accounts
        /// </response>
        [HttpGet]
        [PermissionNeeded(HonuPermission.PSB_NAMED_GET, HonuPermission.PSB_PRACTICE_GET)]
        [Authorize]
        public async Task<ApiResponse<List<ExpandedPsbAccount>>> GetAll() {
            List<PsbAccount> named = await _NamedRepository.GetAll();

            List<ExpandedPsbAccount> expanded = await MakeExpanded(named);

            return ApiOk(expanded);
        }

        /// <summary>
        ///     Get all PSB accounts of a specific account type
        /// </summary>
        /// <param name="typeID">ID of the account type</param>
        /// <response code="200">
        ///     The response will contain a list of <see cref="PsbAccount"/>s
        ///     with <see cref="PsbAccount.AccountType"/> of <paramref name="typeID"/>
        /// </response>
        [HttpGet("type/{typeID}")]
        [PermissionNeeded(HonuPermission.PSB_NAMED_GET, HonuPermission.PSB_PRACTICE_GET)]
        [Authorize]
        public async Task<ApiResponse<List<ExpandedPsbAccount>>> GetByTypeID(long typeID) {
            List<PsbAccount> accounts = await _NamedRepository.GetByTypeID(typeID);

            List<ExpandedPsbAccount> ex = await MakeExpanded(accounts);

            return ApiOk(ex);
        }

        /// <summary>
        ///     Get a specific PSB account
        /// </summary>
        /// <param name="ID">ID of the account</param>
        /// <response code="200">
        ///     The response will contain the <see cref="PsbAccount"/>
        ///     with <see cref="PsbAccount.ID"/> of <paramref name="ID"/>
        /// </response>
        /// <response code="204">
        ///     No <see cref="PsbAccount"/> with <see cref="PsbAccount.ID"/> of <paramref name="ID"/> exists
        /// </response>
        [HttpGet("{ID}")]
        [PermissionNeeded(HonuPermission.PSB_NAMED_GET)]
        [Authorize]
        public async Task<ApiResponse<ExpandedPsbAccount>> GetByID(long ID) {
            PsbAccount? acc = await _NamedRepository.GetByID(ID);
            if (acc == null) {
                return ApiNoContent<ExpandedPsbAccount>();
            }

            ExpandedPsbAccount ex = await MakeExpanded(acc);

            return ApiOk(ex);
        }

        /// <summary>
        ///     Recheck an account, updating the status fields
        /// </summary>
        /// <param name="accountID">Account ID to recheck</param>
        /// <response code="200">
        ///     Returned after the recheck is complete
        /// </response>
        [HttpGet("recheck/{accountID}")]
        [PermissionNeeded(HonuPermission.PSB_NAMED_GET, HonuPermission.PSB_PRACTICE_GET)]
        [Authorize]
        public async Task<ApiResponse<PsbAccount>> Recheck(long accountID) {
            PsbAccount? acc = await _NamedRepository.GetByID(accountID);
            if (acc == null) {
                return ApiNotFound<PsbAccount>($"{nameof(PsbAccount)} {accountID}");
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
        ///     The <see cref="PsbAccount"/> with <see cref="PsbAccount.ID"/> of <paramref name="ID"/>
        ///     was successfully marked as deleted
        /// </response>
        /// <response code="400">
        ///     The <see cref="PsbAccount"/> with <see cref="PsbAccount.ID"/> of <paramref name="ID"/>
        ///     was already deleted
        /// </response>
        /// <response code="404">
        ///     No <see cref="PsbAccount"/> with <see cref="PsbAccount.ID"/> of <paramref name="ID"/> exists
        /// </response>
        [HttpDelete("{ID}")]
        [PermissionNeeded(HonuPermission.PSB_NAMED_MANAGE, HonuPermission.PSB_PRACTICE_MANAGE)]
        [Authorize]
        public async Task<ApiResponse> Delete(long ID) {
            PsbAccount? acc = await _NamedRepository.GetByID(ID);
            if (acc == null) {
                return ApiNotFound($"{nameof(PsbAccount)} {ID}");
            }

            if (acc.DeletedAt != null || acc.DeletedBy != null) {
                return ApiBadRequest($"{nameof(PsbAccount)} {ID} has already been deleted");
            }

            ApiResponse? errorResponse = await CheckManagePermission(acc.AccountType);
            if (errorResponse != null) {
                return errorResponse;
            }

            HonuAccount? account = await _CurrentUser.Get();
            if (account == null) {
                return ApiInternalError(new Exception("Failed to get current user"));
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
        [PermissionNeeded(HonuPermission.PSB_NAMED_GET, HonuPermission.PSB_PRACTICE_GET)]
        [Authorize]
        public async Task<ApiResponse<PsbCharacterSet>> GetCharacterSet([FromQuery] string? tag, [FromQuery] string name) {
            PsbCharacterSet set = await _NamedRepository.GetCharacterSet(tag, name);

            return ApiOk(set);
        }

        /// <summary>
        ///     Create a new account to be tracked
        /// </summary>
        /// <param name="tag">Tag of the account to be tracked</param>
        /// <param name="name">Name of the account to be tracked</param>
        /// <param name="accountTypeID">ID of the account type</param>
        /// <param name="allowMissing">If an account can be made with missing characters</param>
        /// <response code="200">
        ///     The response will contain the <see cref="PsbAccount"/> that was created
        /// </response>
        /// <response code="400">
        ///     One of the following validation errors occured:
        ///     <ul>
        ///         <li>
        ///             One of the 4 faction characters could not be found
        ///         </li>
        ///         <li>
        ///             An account using the tag and name already exists
        ///         </li>
        ///         <li>
        ///             <paramref name="accountTypeID"/> is for an unknown account type
        ///         </li>
        ///     </ul>
        /// </response>
        /// <response code="403">
        ///     The user making the request lacks the permission to manage the account type
        ///     from <paramref name="accountTypeID"/>
        /// </response>
        [HttpPost]
        [PermissionNeeded(HonuPermission.PSB_NAMED_MANAGE, HonuPermission.PSB_PRACTICE_MANAGE)]
        [Authorize]
        public async Task<ApiResponse<PsbAccount>> Create([FromQuery] string? tag, [FromQuery] string name,
            [FromQuery] short accountTypeID, [FromQuery] bool allowMissing = false) {

            ApiResponse<PsbAccount>? errorResponse = await CheckManagePermission<PsbAccount>(accountTypeID);
            if (errorResponse != null) {
                return errorResponse;
            }

            PsbCharacterSet set = await _NamedRepository.GetCharacterSet(tag, name);
            if (allowMissing == false) {
                if (set.VS == null || set.NC == null || set.TR == null || set.NS == null) {
                    return ApiBadRequest<PsbAccount>($"One of the faction characters does not exist");
                }
            }

            PsbAccount? acc = await _NamedRepository.GetByTagAndName(tag, name);
            if (acc != null && acc.DeletedAt == null) {
                return ApiBadRequest<PsbAccount>($"A named account for {tag}x{name} already exists");
            }

            acc = await _NamedRepository.Create(tag, name, accountTypeID);

            return ApiOk(acc);
        }

        /// <summary>
        ///     Rename an existing psb account with a new name. Used when the player who owns the account has not changed,
        ///     but they have renamed for whatever reason
        /// </summary>
        /// <param name="accountID">ID of the account</param>
        /// <param name="tag">New tag of the account</param>
        /// <param name="name">New name of the account</param>
        /// <response code="200">
        ///     The response will contain the <see cref="PsbAccount"/> that was successfully renamed using the parameters
        /// </response>
        /// <response code="400">
        ///     One of the following errors occured. The response data will have a string describing the error
        ///     <ul>
        ///         <li>
        ///             One of the 4 characters (one for each faction) was missing
        ///         </li>
        ///         <li>
        ///             A psb account using the tag and name already exist
        ///         </li>
        ///     </ul>
        /// </response>
        /// <response code="404">
        ///     No <see cref="PsbAccount"/> with <see cref="PsbAccount.ID"/> of <paramref name="accountID"/> exists
        /// </response>
        [HttpPost("{accountID}")]
        [PermissionNeeded(HonuPermission.PSB_NAMED_MANAGE)]
        [Authorize]
        public async Task<ApiResponse<PsbAccount>> Rename(long accountID, [FromQuery] string? tag, [FromQuery] string name) {
            if (string.IsNullOrEmpty(name) == true) {
                return ApiBadRequest<PsbAccount>($"Missing {nameof(name)} parameter");
            }

            PsbAccount? acc = await _NamedRepository.GetByID(accountID);
            if (acc == null) {
                return ApiNotFound<PsbAccount>($"{nameof(PsbAccount)} {accountID}");
            }

            PsbAccount? existing = await _NamedRepository.GetByTagAndName(tag, name);
            if (existing != null && existing.ID != accountID) {
                return ApiBadRequest<PsbAccount>($"A {nameof(PsbAccount)} already exist with name {tag}x{name}");
            }

            PsbCharacterSet set = await _NamedRepository.GetCharacterSet(tag, name);
            List<string> errors = new List<string>();
            if (set.VS == null) { errors.Add($"Missing VS character"); }
            if (set.NC == null) { errors.Add($"Missing NC character"); }
            if (set.TR == null) { errors.Add($"Missing TR character"); }
            if (set.NS == null) { errors.Add($"Missing NS character"); }
            if (errors.Count > 0) {
                return ApiBadRequest<PsbAccount>($"One of the faction is missing a character: {string.Join(", ", errors)}");
            }

            acc = await _NamedRepository.Rename(accountID, tag, name);
            if (acc == null) {
                return ApiBadRequest<PsbAccount>($"failed to rename, generic error");
            }

            return ApiOk(acc);
        }

        /// <summary>
        ///     Take a list of <see cref="PsbAccount"/>s and make <see cref="ExpandedPsbAccount"/>s for them
        /// </summary>
        private async Task<List<ExpandedPsbAccount>> MakeExpanded(List<PsbAccount> named) {
            List<ExpandedPsbAccount> ex = new List<ExpandedPsbAccount>(named.Count);

            List<string> IDs = new List<string>();

            foreach (PsbAccount acc in named) {
                if (acc.VsID != null) { IDs.Add(acc.VsID);  }
                if (acc.NcID != null) { IDs.Add(acc.NcID);  }
                if (acc.TrID != null) { IDs.Add(acc.TrID);  }
                if (acc.NsID != null) { IDs.Add(acc.NsID);  }
            }

            List<PsCharacter> characters = await _CharacterRepository.GetByIDs(IDs, CensusEnvironment.PC, fast: true);

            foreach (PsbAccount acc in named) {
                PsCharacter? vs = (acc.VsID != null) ? characters.FirstOrDefault(iter => iter.ID == acc.VsID) : null;
                PsCharacter? nc = (acc.NcID != null) ? characters.FirstOrDefault(iter => iter.ID == acc.NcID) : null;
                PsCharacter? tr = (acc.TrID != null) ? characters.FirstOrDefault(iter => iter.ID == acc.TrID) : null;
                PsCharacter? ns = (acc.NsID != null) ? characters.FirstOrDefault(iter => iter.ID == acc.NsID) : null;

                ExpandedPsbAccount expanded = new ExpandedPsbAccount() {
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

        private async Task<ExpandedPsbAccount> MakeExpanded(PsbAccount acc) {
            List<ExpandedPsbAccount> ex = await MakeExpanded(new List<PsbAccount>() { acc });
            return ex.ElementAt(0);
        }

        /// <summary>
        ///     Check the current user has the manage permission to a type of account
        /// </summary>
        /// <returns>
        ///     An <see cref="ApiResponse"/> if the current user is not allowed to make the request
        /// </returns>
        private async Task<ApiResponse<T>?> CheckManagePermission<T>(short accountTypeID) {
            HonuAccount? currentUser = await _CurrentUser.Get();
            if (currentUser == null) {
                return ApiAuthorize<T>();
            }

            List<HonuAccountPermission> permissions = await _AccountPermissions.GetByAccountID(currentUser.ID);
            if (accountTypeID == PsbAccountType.NAMED) {
                if (permissions.FirstOrDefault(iter => iter.Permission == HonuPermission.PSB_NAMED_MANAGE) == null) {
                    return ApiForbidden<T>(HonuPermission.PSB_NAMED_MANAGE);
                }
            } else if (accountTypeID == PsbAccountType.PRACTICE) {
                if (permissions.FirstOrDefault(iter => iter.Permission == HonuPermission.PSB_PRACTICE_MANAGE) == null) {
                    return ApiForbidden<T>(HonuPermission.PSB_PRACTICE_MANAGE);
                }
            } else {
                return ApiBadRequest<T>($"Unchecked {nameof(accountTypeID)} {accountTypeID}");
            }

            return null;
        }

        private async Task<ApiResponse?> CheckManagePermission(short accountTypeID) {
            HonuAccount? currentUser = await _CurrentUser.Get();
            if (currentUser == null) {
                return ApiAuthorize();
            }

            List<HonuAccountPermission> permissions = await _AccountPermissions.GetByAccountID(currentUser.ID);
            if (accountTypeID == PsbAccountType.NAMED) {
                if (permissions.FirstOrDefault(iter => iter.Permission == HonuPermission.PSB_NAMED_MANAGE) == null) {
                    return ApiForbidden(HonuPermission.PSB_NAMED_MANAGE);
                }
            } else if (accountTypeID == PsbAccountType.PRACTICE) {
                if (permissions.FirstOrDefault(iter => iter.Permission == HonuPermission.PSB_PRACTICE_MANAGE) == null) {
                    return ApiForbidden(HonuPermission.PSB_PRACTICE_MANAGE);
                }
            } else {
                return ApiBadRequest($"Unchecked {nameof(accountTypeID)} {accountTypeID}");
            }

            return null;

        }

    }
}
