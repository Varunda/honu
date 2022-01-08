using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

    }
}
