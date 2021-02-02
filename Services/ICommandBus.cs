using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services {

    public interface ICommandBus {

        Task Execute(string command);

    }
}
