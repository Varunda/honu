using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Commands;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Code.Commands {

    [Command]
    public class AlertCommand {

        private readonly ILogger<AlertCommand> _Logger;
        private readonly AlertPlayerDataRepository _DataRepository;
        private readonly AlertParticipantDataDbStore _DataDb;

        public AlertCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<AlertCommand>>();
            _DataRepository = services.GetRequiredService<AlertPlayerDataRepository>();
            _DataDb = services.GetRequiredService<AlertParticipantDataDbStore>();
        }

        public async Task Rebuild(long alertID) {
            _Logger.LogInformation($"Rebuilding participant data for alert {alertID}...");
            await _DataDb.DeleteByAlertID(alertID);
            await _DataRepository.GetByAlert(alertID, CancellationToken.None);
            _Logger.LogInformation($"Done rebuilding participant data for alert {alertID}");
        }

    }
}
