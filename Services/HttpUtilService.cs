using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Net;
using watchtower.Models;

namespace watchtower.Services {

    public class HttpUtilService {

        private readonly ILogger<HttpUtilService> _Logger;
        private readonly IOptionsSnapshot<HttpConfig> _Config;

        public HttpUtilService(ILogger<HttpUtilService> logger,
            IOptionsSnapshot<HttpConfig> config) {

            _Logger = logger;
            _Config = config;
        }

        public string? GetHttpRemoteIp(HttpContext? context) {
            if (context == null) {
                return null;
            }

            IPAddress? addr = context.Connection.RemoteIpAddress;
            if (addr == null) {
                return null;
            }

            return addr.ToString();
        }

        /// <summary>
        ///     Get the user agent of request of an HttpContext, returning null if it was not provided
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string? GetUserAgent(HttpContext context) {
            StringValues userAgent = context.Request.Headers["User-Agent"];

            foreach (string? v in userAgent) {
                if (v == null) {
                    continue;
                }
                return v;
            }

            return null;
        }

        /// <summary>
        ///     Check if a request comes from a search engine bot (based on user agent)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool IsSearchEngineBot(HttpContext context) {
            string? userAgent = GetUserAgent(context);

            if (userAgent == null) {
                return false;
            }

            foreach (string iter in _Config.Value.SearchEngineUserAgents) {
                if (userAgent.ToLower().Contains(iter.ToLower())) {
                    return true;
                }
            }

            return false;
        }

    }
}
