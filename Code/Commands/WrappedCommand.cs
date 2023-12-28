using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using watchtower.Commands;
using watchtower.Services.Repositories;

namespace watchtower.Code.Commands {

    [Command]
    public class WrappedCommand {

        private readonly ILogger<WrappedCommand> _Logger;
        private readonly HonuMetadataRepository _MetadataRepository;

        public WrappedCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<WrappedCommand>>();
            _MetadataRepository = services.GetRequiredService<HonuMetadataRepository>();
        }

        public async Task Enable() {
            _Logger.LogInformation($"enabling wrapped...");
            await _MetadataRepository.Upsert("wrapped.enabled", true);
            _Logger.LogInformation($"enabled wrapped!");
        }

        public async Task Disable() {
            _Logger.LogInformation($"disabling wrapped...");
            await _MetadataRepository.Upsert("wrapped.enabled", false);
            _Logger.LogInformation($"disabled wrapped!");
        }

    }
}
