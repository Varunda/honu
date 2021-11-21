using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace watchtower.Services {

    public interface IBackgroundCharacterWeaponStatQueue {

        void Queue(string charID);

        Task<string> Dequeue(CancellationToken cancel);

        int Count();

    }
}
