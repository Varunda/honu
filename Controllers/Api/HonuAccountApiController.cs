using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
    [Route("/api/account")]
    public class HonuAccountApiController : ApiControllerBase {

        private readonly ILogger<HonuAccountApiController> _Logger;
        private readonly CurrentHonuAccount _CurrentUser;

        private readonly HonuAccountDbStore _AccountDb;
        private readonly HonuAccountPermissionRepository _PermissionRepository;

        public HonuAccountApiController(ILogger<HonuAccountApiController> logger, CurrentHonuAccount currentUser,
            HonuAccountPermissionRepository permissionRepository, HonuAccountDbStore accountDb) {

            _Logger = logger;
            _CurrentUser = currentUser;

            _PermissionRepository = permissionRepository;
            _AccountDb = accountDb;
        }

        /// <summary>
        ///     Get the current user who is making the API call
        /// </summary>
        /// <response code="200">
        ///     The response will contain the <see cref="HonuAccount"/> of the user who made the API call
        /// </response>
        /// <response code="204">
        ///     The user making the API call is either not signed in, or no has no account
        /// </response>
        [HttpGet("whoami")]
        public async Task<ApiResponse<HonuAccount>> WhoAmI() {
            HonuAccount? currentUser = await _CurrentUser.Get();

            if (currentUser == null) {
                return ApiNoContent<HonuAccount>();
            }

            return ApiOk(currentUser);
        }

        /// <summary>
        ///     Get all honu accounts
        /// </summary>
        /// <response code="200">
        ///     A list of all <see cref="HonuAccount"/>s
        /// </response>
        [HttpGet]
        [PermissionNeeded(HonuPermission.HONU_ACCOUNT_ADMIN)]
        [Authorize]
        public async Task<ApiResponse<List<HonuAccount>>> GetAll() {
            List<HonuAccount> accounts = await _AccountDb.GetAll(CancellationToken.None);

            return ApiOk(accounts);
        }

        /// <summary>
        ///     Create a Honu account
        /// </summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="discord"></param>
        /// <param name="discordID"></param>
        /// <response code="200">
        ///     The <see cref="HonuAccount.ID"/> of the <see cref="HonuAccount"/> that was created using the parameters passed
        /// </response>
        /// <response code="400">
        ///     One of the following validation errors occured:
        ///     <ul>
        ///         <li><paramref name="name"/> was empty or whitespace</li>
        ///         <li><paramref name="email"/> was empty or whitespace</li>
        ///         <li><paramref name="discord"/> was empty or whitespace</li>
        ///         <li><paramref name="discordID"/> was 0</li>
        ///         <li><paramref name="email"/> is already in use</li>
        ///     </ul>
        /// </response>
        [HttpPost("create")]
        [PermissionNeeded(HonuPermission.HONU_ACCOUNT_ADMIN)]
        [Authorize]
        public async Task<ApiResponse<long>> CreateAccount([FromQuery] string name, [FromQuery] string email, [FromQuery] string discord, [FromQuery] ulong discordID) {
            List<string> errors = new();

            if (string.IsNullOrWhiteSpace(name)) { errors.Add($"Missing {nameof(name)}"); }
            if (string.IsNullOrWhiteSpace(email)) { errors.Add($"Missing {nameof(email)}"); }
            if (string.IsNullOrWhiteSpace(discord)) { errors.Add($"Missing {nameof(discord)}"); }
            /*
            if (ulong.TryParse(discordID, out ulong dID) == false) {
                errors.Add($"Failed to parse {nameof(discordID)} to a valid ulong");
            }
            */

            if (discordID == 0) { errors.Add($"Missing {nameof(discordID)}"); }

            if (errors.Count > 0) {
                return ApiBadRequest<long>($"Validation errors: {string.Join("\n", errors)}");
            }

            HonuAccount? existingAccount = await _AccountDb.GetByEmail(email, CancellationToken.None);
            if (existingAccount != null) {
                return ApiBadRequest<long>($"Account for email {email} already exists");
            }

            HonuAccount acc = new();
            acc.Name = name;
            acc.Email = email;
            acc.Discord = discord;
            acc.DiscordID = discordID;
            acc.Timestamp = DateTime.UtcNow;

            long ID = await _AccountDb.Insert(acc, CancellationToken.None);

            return ApiOk(ID);
        }

        [HttpDelete("{accountID}")]
        [PermissionNeeded(HonuPermission.HONU_ACCOUNT_ADMIN)]
        [Authorize]
        public async Task<ApiResponse> DeactiviateAccount(long accountID) {
            if (accountID == 1) {
                return ApiBadRequest($"Cannot deactivate account ID 1, which is the system account");
            }

            HonuAccount? currentUser = await _CurrentUser.Get();
            if (currentUser == null) {
                return ApiInternalError($"current account is null?");
            }

            await _AccountDb.Delete(accountID, currentUser.ID, CancellationToken.None);

            return ApiOk();
        }


    }
}
