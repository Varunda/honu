using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using watchtower.Models;

namespace watchtower.Services {

    /// <summary>
    ///     Get instance specific information about the running Honu instance
    /// </summary>
    public class InstanceInfo {

        private readonly ILogger<InstanceInfo> _Logger;

        private readonly IOptions<InstanceOptions> _Options;

        public InstanceInfo(ILogger<InstanceInfo> logger, IOptions<InstanceOptions> options) {
            _Logger = logger;
            _Options = options;
        }

        /// <summary>
        ///     Get the web host of this Honu instance. Defaults to wt.honu.pw if not given
        /// </summary>
        public string GetHost() {
            if (_Options.Value.Host.Length == 0) {
                _Logger.LogWarning($"Instance host not set! Defaulting to 'wt.honu.pw'. Use dotnet user-secrets set Instance:Host $HOST to set this");
                return "wt.honu.pw";
            }

            return _Options.Value.Host;
        }

    }
}
