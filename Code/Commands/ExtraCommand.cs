using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using watchtower.Commands;
using watchtower.Services.CharacterViewer;

namespace watchtower.Code.Commands {

    [Command]
    public class ExtraCommand {

        private readonly ILogger<ExtraCommand> _Logger;
        private readonly ExtraStatHoster _Hosted;

        public ExtraCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<ExtraCommand>>();
            _Hosted = services.GetRequiredService<ExtraStatHoster>();
        }

        public void Refresh() {
            _Hosted.Clear();
            _Hosted.Discover();
        }


    }
}
