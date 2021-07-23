
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Models.Db;

namespace watchtower.Services.Db {

    public interface ISessionDbStore {

        Task<List<Session>> GetByCharacterID(string charID, int interval);

        Task Start(TrackedPlayer player);

        Task End(TrackedPlayer player);

        Task EndAll();

    }

}