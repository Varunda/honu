using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Services.Census;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Commands {

    [Command]
    public class CharCommand {

        private readonly ILogger<CharCommand> _Logger;

        private readonly ICharacterRepository _CharacterRepository;
        private readonly IOutfitRepository _Outfitrepository;
        private readonly ISessionDbStore _SessionDb;

        public CharCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<CharCommand>>();

            _CharacterRepository = services.GetRequiredService<ICharacterRepository>();
            _Outfitrepository = services.GetRequiredService<IOutfitRepository>();
            _SessionDb = services.GetRequiredService<ISessionDbStore>();
        }

        public async Task Get(string name) {
            PsCharacter? c = await _CharacterRepository.GetByName(name);
            if (c != null) {
                _Logger.LogInformation($"{name} => {JToken.FromObject(c)}");

                if (c.OutfitID != null) {
                    PsOutfit? outfit = await _Outfitrepository.GetByID(c.OutfitID);

                    if (outfit != null) {
                        _Logger.LogInformation($"{JToken.FromObject(outfit)}");
                    }
                }
            } else {
                _Logger.LogInformation($"{name} => null");
            }
        }

    }
}
