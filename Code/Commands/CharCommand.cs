using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Census;
using watchtower.Models;

namespace watchtower.Commands {

    [Command]
    public class CharCommand {

        private readonly ILogger<CharCommand> _Logger;
        private readonly ICharacterCollection _Characters;

        public CharCommand(IServiceProvider services) {
            _Logger = (ILogger<CharCommand>)services.GetService(typeof(ILogger<CharCommand>));

            _Characters = (ICharacterCollection)services.GetService(typeof(ICharacterCollection));
        }

        public async Task Get(string name) {
            Character? c = await _Characters.GetByNameAsync(name);
            if (c != null) {
                _Logger.LogInformation($"{name} => {JToken.FromObject(c)}");
            } else {
                _Logger.LogInformation($"{name} => null");
            }
        }

    }
}
