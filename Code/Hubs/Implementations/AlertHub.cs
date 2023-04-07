using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace watchtower.Code.Hubs.Implementations {

    public class AlertHub : Hub<IAlertHub> {

        private readonly ILogger<AlertHub> _Logger;

        public AlertHub(ILogger<AlertHub> logger) {
            _Logger = logger;
        }

        public async Task LoadAlert(long alertID) {

        }

    }
}
