﻿using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Models.Watchtower;

namespace watchtower.Code.Hubs {

    public interface IRealtimeNetworkHub {

        /// <summary>
        ///     Send an update of a realtime network
        /// </summary>
        /// <param name="network"></param>
        /// <returns></returns>
        Task UpdateNetwork(RealtimeNetwork network);

        Task SendCharacters(List<PsCharacter> chars);

        Task SendOutfits(List<PsOutfit> outfits);

    }
}
