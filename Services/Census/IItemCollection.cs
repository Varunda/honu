using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public interface IItemCollection {

        Task<PsItem?> GetByID(string itemID);

    }
}
