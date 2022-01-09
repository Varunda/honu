using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using watchtower.Commands;
using watchtower.Models.PSB;
using watchtower.Services.Repositories.PSB;

namespace watchtower.Code.Commands {

    [Command]
    public class PsbNamedCommand {

        private readonly ILogger<PsbNamedCommand> _Logger;
        private readonly PsbNamedRepository _NamedRepository;

        public PsbNamedCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<PsbNamedCommand>>();
            _NamedRepository = services.GetRequiredService<PsbNamedRepository>();
        }

        public async Task Create(string tagg, string name) {
            string? tag = tagg;
            if (tag == ".") {
                tag = null;
            }

            PsbNamedAccount acc = await _NamedRepository.Create(tag, name);

            _Logger.LogInformation($"Created new named account {acc.VsID} {acc.NcID} {acc.TrID} {acc.NsID}");
        }

        public async Task Get(string tagg, string name) {
            string? tag = tagg == "." ? null : tagg;

            PsbNamedAccount? acc = await _NamedRepository.GetByTagAndName(tag, name);

            if (acc == null) {
                _Logger.LogWarning($"Failed to find {tag}x{name}");
                return;
            }

            _Logger.LogInformation($"{tag}x{name} => \n{JToken.FromObject(acc)}");
        }

        public async Task Rename(long ID, string tagg, string name) {
            string? tag = tagg == "." ? null : tagg;

            bool success = await _NamedRepository.Rename(ID, tag, name);
            if (success == true) {
                _Logger.LogInformation($"Successfully renamed {ID} to {tag}x{name}");
            } else {
                _Logger.LogWarning($"Failed to update {ID} to {tag}x{name}");
            }
        }

    }
}
