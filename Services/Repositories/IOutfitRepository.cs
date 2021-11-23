using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Services.Repositories {

    public interface IOutfitRepository {

        Task<PsOutfit?> GetByID(string outfitID);

        Task<List<PsOutfit>> GetByTag(string tag);

        Task Upsert(PsOutfit outfit);

    }
}
