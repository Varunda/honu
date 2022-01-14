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

namespace watchtower.Code {

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class PsbAdminAttribute : TypeFilterAttribute {

        public PsbAdminAttribute()
            : base(typeof(PsbAdminFilter)) { }

    }

    /// <summary>
    ///     Auth filter that only allowed users with a <see cref="HonuAccount"/> to use the action
    /// </summary>
    public class PsbAdminFilter : IAsyncAuthorizationFilter {

        private readonly ILogger<PsbAdminFilter> _Logger;
        private readonly IHttpContextAccessor _Context;
        private readonly HonuAccountDbStore _HonuAccountDb;
        private readonly HonuAccountAccessLogDbStore _HonuAccountLogsDb;

        public PsbAdminFilter(ILogger<PsbAdminFilter> logger,
            IHttpContextAccessor context, HonuAccountDbStore honuAccountDb,
            HonuAccountAccessLogDbStore logDb) {

            _Logger = logger;
            _Context = context;
            _HonuAccountDb = honuAccountDb;
            _HonuAccountLogsDb = logDb;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context) {
            HttpContext? httpContext = _Context.HttpContext;
            if (httpContext == null) {
                throw new ArgumentNullException($"_Context.HttpContext cannot be null");
            }

            HonuAccount? account;

            if (httpContext.User.Identity == null) {
                _Logger.LogWarning($"httpContext.User.Identity is null");
                return;
            }

            if (httpContext.User.Identity.IsAuthenticated == false) {
                _Logger.LogWarning($"User is not authed, return them to the sign in");
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
                    throw new ArgumentException($"email was null or empty");
                }

                string email = emailClaim.Value;
                account = await _HonuAccountDb.GetByEmail(email, CancellationToken.None);

                if (account == null) {
                    _Logger.LogWarning($"Blocked access from {email}");

                    if (context.HttpContext.Response.HasStarted == true) {
                        _Logger.LogError($"Response started, cannot set 403Forbidden");
                        throw new ApplicationException($"Cannot forbid access to psb admin action, response has started.");
                    }

                    string method = httpContext.Request.Method;
                    string url = httpContext.Request.Path;

                    if (httpContext.Request.Path.StartsWithSegments("/api") == true) {
                        //_Logger.LogDebug($"{method} {url}\n[PsbAdmin] protected action is an API action, returning an ApiResponse");
                        context.Result = new ApiResponse(403, "{ \"error\": \"User is not a psb admin\" }");
                    } else {
                        //_Logger.LogDebug($"{method} {url}\n[PsbAdmin] protected action IS NOT api action");
                        context.Result = new RedirectToActionResult("PsbUnauthorized", "Home", null);
                    }

                    HonuAccountAccessLog log = new HonuAccountAccessLog();
                    log.Success = false;
                    log.Email = email;

                    await _HonuAccountLogsDb.Insert(log);
                }
            }
        }

    }

}
