using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models;

namespace watchtower.Services.Repositories {

    public interface IDataBuilderRepository {

        Task<WorldData> Build(short worldID, CancellationToken? stoppingToken);

    }
}
