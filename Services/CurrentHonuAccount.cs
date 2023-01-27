using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Services.Db;

namespace watchtower.Services {

    /// <summary>
    ///     Service to get the current user making an HTTP request
    /// </summary>
    public class CurrentHonuAccount {

        private readonly ILogger<CurrentHonuAccount> _Logger;
        private readonly IHttpContextAccessor _Context;
        private readonly HonuAccountDbStore _HonuAccountDb;

        public CurrentHonuAccount(ILogger<CurrentHonuAccount> logger,
            IHttpContextAccessor context, HonuAccountDbStore accountDb) {

            _Logger = logger;
            _Context = context;
            _HonuAccountDb = accountDb;
        }

        /// <summary>
        ///     Get the current user based who a <see cref="BaseContext"/>
        /// </summary>
        /// <param name="ctx">Context of the application command</param>
        /// <returns>
        ///     Null if the field <see cref="BaseContext.Member"/> is null, or null if the user doesn't have an account
        /// </returns>
        public async Task<HonuAccount?> GetDiscord(BaseContext ctx) {
            DiscordMember? caller = ctx.Member;
            if (caller == null) {
                return null;
            }

            HonuAccount? account = await _HonuAccountDb.GetByDiscordID(caller.Id, CancellationToken.None);

            return account;
        }

        /// <summary>
        ///     Get the current user, null if the user is not signed in
        /// </summary>
        /// <returns></returns>
        public async Task<HonuAccount?> Get() {
            if (_Context.HttpContext == null) {
                _Logger.LogWarning($"_Context.HttpContext is null, cannot get claims");
                return null;
            }

            HttpContext httpContext = _Context.HttpContext;

            if (httpContext.User.Identity == null) {
                _Logger.LogWarning($"httpContext.User.Identity is null");
                return null;
            }

            if (httpContext.User.Identity.IsAuthenticated == false) {
                _Logger.LogWarning($"User is not authed, return them to the sign in");
                return null;
            } else if (httpContext.User is ClaimsPrincipal claims) {
                /*
                string s = "";
                foreach (Claim claim in claims.Claims) {
                    s += $"{claim.Type} = {claim.Value};";
                }
                _Logger.LogDebug($"{s}");
                */

                // Get the email claim of the authed user
                Claim? emailClaim = claims.FindFirst(ClaimTypes.Email);
                if (emailClaim == null || string.IsNullOrEmpty(emailClaim.Value)) {
                    return null;
                }

                string email = emailClaim.Value;
                return await _HonuAccountDb.GetByEmail(email, CancellationToken.None);
            } else {
                _Logger.LogWarning($"Unchecked stat of httpContext.User");
            }

            return null;
        }

    }
}
