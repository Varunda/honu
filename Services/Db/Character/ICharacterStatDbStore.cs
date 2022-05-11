using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Services.Db {

    public interface ICharacterStatDbStore {

        Task<List<PsCharacterStat>> GetByID(string charID);

        Task Set(string charID, List<PsCharacterStat> stats);

    }
}
