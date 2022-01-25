using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.CharacterViewer;
using watchtower.Models.CharacterViewer.CharacterStats;

namespace watchtower.Services.CharacterViewer {

    public interface ICharacterStatGeneratorStore {

        bool Add(ICharacterStatGenerator generator);

        List<ICharacterStatGenerator> GetAll();

        Task<List<ExtraStatSet>> GenerateAll(string charID);

    }
}
