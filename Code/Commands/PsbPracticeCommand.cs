using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Commands;
using watchtower.Models.PSB;
using watchtower.Services.Repositories.PSB;

namespace watchtower.Code.Commands {

    [Command]
    public class PsbPracticeCommand {

        private readonly ILogger<PsbPracticeCommand> _Logger;
        private readonly PsbDriveRepository _PsbDrive;

        public PsbPracticeCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<PsbPracticeCommand>>();
            _PsbDrive = services.GetRequiredService<PsbDriveRepository>();
        }

        public async Task List() {
            List<PsbDriveFile>? files = await _PsbDrive.GetPracticeSheets();
            if (files == null) {
                _Logger.LogError($"Failed to get practice sheets: {_PsbDrive.GetInitializeFailureReason()}");
                return;
            }

            foreach (PsbDriveFile file in files) {
                _Logger.LogInformation($"{file.Name} => {file.ID}");
            }
        }

    }
}
