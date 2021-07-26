using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Realtime;
using watchtower.Services;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Commands {

    [Command]
    public class DbCommand {

        private readonly ILogger<DbCommand> _Logger;

        public DbCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<DbCommand>>();
        }

        public void FlushPool() {
            NpgsqlConnection.ClearAllPools();
        }

    }

}