using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code;
using watchtower.Models;
using watchtower.Models.Internal;
using watchtower.Services;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {
    
    [ApiController]
    [Route("/api/account-permission")]
    public class HonuAccountPermissionApiController : ApiControllerBase {

        private readonly ILogger<HonuAccountPermissionApiController> _Logger;
        private readonly CurrentHonuAccount _CurrentAccount;

        private readonly HonuAccountDbStore _AccountDb;
        private readonly HonuAccountPermissionRepository _PermissionRepository;

        public HonuAccountPermissionApiController(ILogger<HonuAccountPermissionApiController> logger,
            HonuAccountDbStore accountDb, HonuAccountPermissionRepository permissionRepository, CurrentHonuAccount currentAccount) {

            _Logger = logger;
            _CurrentAccount = currentAccount;

            _AccountDb = accountDb;
            _PermissionRepository = permissionRepository;
        }

        /// <summary>
        ///     Get the permissions an account has
        /// </summary>
        /// <param name="accountID">ID of the account</param>
        /// <response code="200">
        ///     The response will contain a list of permissions an account has
        /// </response>
        /// <response code="404">
        ///     No <see cref="HonuAccount"/> with <see cref="HonuAccount.ID"/> of <paramref name="accountID"/> exists
        /// </response>
        [HttpGet("{accountID}")]
        [PermissionNeeded(HonuPermission.HONU_ACCOUNT_ADMIN)]
        [Authorize]
        public async Task<ApiResponse<List<HonuAccountPermission>>> GetByAccountID(long accountID) {
            HonuAccount? account = await _AccountDb.GetByID(accountID, CancellationToken.None);
            if (account == null) {
                return ApiNotFound<List<HonuAccountPermission>>($"{nameof(HonuAccount)} {accountID}");
            }

            List<HonuAccountPermission> perms = await _PermissionRepository.GetByAccountID(accountID);

            return ApiOk(perms);
        }

        /// <summary>
        ///     Remove a permission from an account
        /// </summary>
        /// <param name="accPermID">ID of the <see cref="HonuAccountPermission"/> to remove</param>
        /// <response code="200">
        ///     The <see cref="HonuAccountPermission"/> with <see cref="HonuAccountPermission.ID"/>
        ///     of <paramref name="accPermID"/> was successfully deleted
        /// </response>
        /// <response code="404">
        ///     No <see cref="HonuAccountPermission"/> with <see cref="HonuAccountPermission.ID"/>
        ///     of <paramref name="accPermID"/> exists
        /// </response>
        [HttpDelete("{accPermID}")]
        [PermissionNeeded(HonuPermission.HONU_ACCOUNT_ADMIN)]
        [Authorize]
        public async Task<ApiResponse> RemoveByID(long accPermID) {
            HonuAccountPermission? perm = await _PermissionRepository.GetByID(accPermID);
            if (perm == null) {
                return ApiNotFound($"{nameof(HonuAccountPermission)} {accPermID}");
            }

            await _PermissionRepository.DeleteByID(accPermID);

            return ApiOk();
        }

        /// <summary>
        ///     Insert a new permission for an account
        /// </summary>
        /// <param name="accountID">ID of the account to add the permission to</param>
        /// <param name="permission">Permission to be added to the account</param>
        /// <response code="200">
        ///     The reponse will contain the ID of the <see cref="HonuAccountPermission"/> that was just created
        ///     using the parameters passed
        /// </response>
        /// <response code="400">
        ///     The account already has a <see cref="HonuAccountPermission"/> for <paramref name="permission"/>
        /// </response>
        /// <response code="404">
        ///     One of the following objects count not be found:
        ///     <ul>
        ///         <li><see cref="HonuAccount"/> with <see cref="HonuAccount.ID"/> of <paramref name="accountID"/></li>
        ///         <li><see cref="HonuPermission"/> with <see cref="HonuPermission.ID"/> of <paramref name="permission"/></li>
        ///     </ul>
        /// </response>
        /// <exception cref="SystemException"></exception>
        [HttpPost("{accountID}")]
        [PermissionNeeded(HonuPermission.HONU_ACCOUNT_ADMIN)]
        [Authorize]
        public async Task<ApiResponse<long>> AddPermission(long accountID, [FromQuery] string permission) {
            HonuAccount? account = await _AccountDb.GetByID(accountID, CancellationToken.None);
            if (account == null) {
                return ApiNotFound<long>($"{nameof(HonuAccount)} {accountID}");
            }

            HonuPermission? p = HonuPermission.All.FirstOrDefault(iter => iter.ID.ToLower() == permission.ToLower());
            if (p == null) {
                return ApiNotFound<long>($"{nameof(HonuPermission)} {permission}");
            }

            List<HonuAccountPermission> perms = await _PermissionRepository.GetByAccountID(accountID);
            if (perms.FirstOrDefault(iter => iter.Permission.ToLower() == permission.ToLower()) != null) {
                return ApiBadRequest<long>($"{nameof(HonuAccount)} {accountID} already has permssion {permission}");
            }

            HonuAccount currentUser = await _CurrentAccount.Get() ?? throw new SystemException("no current user");

            HonuAccountPermission perm = new();
            perm.AccountID = accountID;
            perm.Permission = permission;
            perm.GrantedByID = currentUser.ID;

            long ID = await _PermissionRepository.Insert(perm);

            return ApiOk(ID);
        }

    }
}
