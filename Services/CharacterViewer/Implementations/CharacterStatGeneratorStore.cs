using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.CharacterViewer;
using watchtower.Models.CharacterViewer.CharacterStats;

namespace watchtower.Services.CharacterViewer.Implementations {

    public class CharacterStatGeneratorStore : ICharacterStatGeneratorStore {

        private readonly ILogger<CharacterStatGeneratorStore> _Logger;

        private List<ICharacterStatGenerator> _Generators = new List<ICharacterStatGenerator>();

        public CharacterStatGeneratorStore(ILogger<CharacterStatGeneratorStore> logger) {
            _Logger = logger;
        }

        public bool Add(ICharacterStatGenerator generator) {
            _Generators.Add(generator);
            return true;
        }

        public List<ICharacterStatGenerator> GetAll() {
            return _Generators;
        }

        public async Task<List<CharacterStatBase>> GenerateAll(string charID) {
            List<CharacterStatBase> stats = new List<CharacterStatBase>();

            foreach (ICharacterStatGenerator gen in _Generators) {
                CharacterStatBase? stat = await gen.Generate(charID);
                if (stat != null) {
                    stat.CharacterID = charID; // Some generators don't set this grrr
                    stats.Add(stat);
                }
            }

            return stats;
        }

    }
}
