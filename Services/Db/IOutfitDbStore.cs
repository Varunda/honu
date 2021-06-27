using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Services.Db {

    public interface IOutfitDbStore {

        Task<PsOutfit?> GetByID(string outfitID);

        Task Upsert(PsOutfit outfit);

    }
}
