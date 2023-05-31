using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Services;

namespace watchtower.Code {

    /// <summary>
    ///     Attribute to prevent search engines (more specifically, user agents that match a search engine) from calling an action
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SearchBotBlockAttribute : TypeFilterAttribute {

        public SearchBotBlockAttribute() : base(typeof(SearchBotBlockFilter)) { }

    }

    /// <summary>
    ///     Implementation filter of the block
    /// </summary>
    public class SearchBotBlockFilter : IAsyncAuthorizationFilter {

        private readonly ILogger<SearchBotBlockFilter> _Logger;
        private readonly HttpUtilService _HttpUtil;

        public SearchBotBlockFilter(ILogger<SearchBotBlockFilter> logger, HttpUtilService httpUtil) {
            _Logger = logger;
            _HttpUtil = httpUtil;
        }

        public Task OnAuthorizationAsync(AuthorizationFilterContext context) {
            if (_HttpUtil.IsSearchEngineBot(context.HttpContext) == false) {
                return Task.CompletedTask;
            }

            if (context.HttpContext.Response.HasStarted == true) {
                _Logger.LogError($"Response started, cannot set 403Forbidden");
                throw new ApplicationException($"failed to block search bot request, response has started.");
            }

            _Logger.LogTrace($"blocked request based on search bot filter [Url='{context.HttpContext.Request.Path.Value}'] [UserAgent='{_HttpUtil.GetUserAgent(context.HttpContext)}']");

            if (context.HttpContext.Request.Path.StartsWithSegments("/api") == true) {
                context.Result = new ApiResponse(403, new Dictionary<string, string>() {
                    { "error", "request blocked by user agent (is a search engine)" }
                });
            } else {
                context.Result = new StatusCodeResult(403);
            }

            return Task.CompletedTask;
        }

    }

}
