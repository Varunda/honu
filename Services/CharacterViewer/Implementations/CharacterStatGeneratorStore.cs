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

        public void ClearGenerators() {
            _Generators.Clear();
        }

        public List<ICharacterStatGenerator> GetAll() {
            return _Generators;
        }

        public async Task<List<ExtraStatSet>> GenerateAll(string charID) {
            List<ExtraStatSet> stats = new List<ExtraStatSet>();

            foreach (ICharacterStatGenerator gen in _Generators) {
                ExtraStatSet? set = await gen.Generate(charID);
                if (set != null) {
                    stats.Add(set);
                }
            }

            return stats;
        }

    }
}
