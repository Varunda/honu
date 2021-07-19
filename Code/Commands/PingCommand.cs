using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models;

namespace watchtower.Commands {

    [Command]
    public class PingCommand {

        private readonly ILogger<PingCommand> _Logger;

        public PingCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<PingCommand>>();
        }

        public void Ping() {
            _Logger.LogInformation($"Pong");
            Console.WriteLine($"Pong");
        }

        public void TestAdd(int i) {
            Console.WriteLine($"{i + 5}");
        }

    }
}
