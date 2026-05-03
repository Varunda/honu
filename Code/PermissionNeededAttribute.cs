using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Services;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Code {

    /// <summary>
    ///     Attribute to add to actions to require a user to have a <see cref="HonuAccount"/>,
    ///     and that account has the necessary permissions
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class PermissionNeededAttribute : TypeFilterAttribute {

        public PermissionNeededAttribute(params string[] perms) : base(typeof(PermissionNeededFilter)) {
            Arguments = new object[] { perms };
        }

    }

    public class PermissionNeededFilter : IAsyncAuthorizationFilter {

        private readonly ILogger<PermissionNeededFilter> _Logger;
        private readonly IHttpContextAccessor _Context;
        private readonly HonuAccountDbStore _HonuAccountDb;
        private readonly HonuAccountPermissionRepository _PermissionRepository;
        private readonly CurrentHonuAccount _CurrentUser;

        public readonly List<string> Permissions;

        public PermissionNeededFilter(ILogger<PermissionNeededFilter> logger,
            IHttpContextAccessor context, HonuAccountDbStore honuAccountDb,
            HonuAccountPermissionRepository permissionRepository, CurrentHonuAccount currentUser,
            string[] perms) {

            Permissions = perms.ToList();

            _Logger = logger;
            _Context = context;
            _HonuAccountDb = honuAccountDb;
            _PermissionRepository = permissionRepository;
            _CurrentUser = currentUser;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context) {
            HttpContext httpContext = context.HttpContext
                ?? throw new ArgumentNullException($"_Context.HttpContext cannot be null");

            HonuAccount? account = await _CurrentUser.Get();

            if (account == null) {
                if (context.HttpContext.Response.HasStarted == true) {
                    _Logger.LogError($"Response started, cannot set 403Forbidden");
                    throw new ApplicationException($"Cannot forbid access to psb admin action, response has started.");
                }

                if (httpContext.Request.Path.StartsWithSegments("/api") == true) {
                    context.Result = new ApiResponse(403, new Dictionary<string, string>() {
                        { "error", "user does not have an account" }
                    });
                } else {
                    context.Result = new RedirectToActionResult("PsbUnauthorized", "Home", null);
                }

                return;
            }

            HashSet<string> accountPerms = new();

            List<HonuAccountPermission> perms = await _PermissionRepository.GetByAccountID(account.ID);
            foreach (HonuAccountPermission perm in perms) {
                accountPerms.Add(perm.Permission.ToLower());
            }

            bool hasPerm = false;

            foreach (string perm in Permissions) {
                if (accountPerms.Contains(perm.ToLower())) {
                    hasPerm = true;
                    break;
                }
            }

            if (hasPerm == false) {
                if (context.HttpContext.Response.HasStarted == true) {
                    _Logger.LogError($"Response started, cannot set 403Forbidden");
                    throw new ApplicationException($"Cannot forbid access to psb admin action, response has started.");
                }

                if (httpContext.Request.Path.StartsWithSegments("/api") == true) {
                    context.Result = new ApiResponse(403, new Dictionary<string, string>() {
                        { "error", "no permission" }
                    });
                } else {
                    context.Result = new RedirectToActionResult("PsbUnauthorized", "Home", null);
                }
            }

        }

    }

}
