using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.CharacterViewer;
using watchtower.Services.CharacterViewer;

namespace watchtower.Services.Hosted.Startup {

    public class CharacterStatGeneratorPopulator : IHostedService {

        private readonly ILogger<CharacterStatGeneratorPopulator> _Logger;
        private readonly ExtraStatHoster _ExtraStatHoster;

        public CharacterStatGeneratorPopulator(ILogger<CharacterStatGeneratorPopulator> logger,
            ExtraStatHoster hoster) {

            _Logger = logger;
            _ExtraStatHoster = hoster;
        }

        public Task StartAsync(CancellationToken cancellationToken) {
            _ExtraStatHoster.Discover();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    }
}
