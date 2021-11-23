using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Report;

namespace watchtower.Services.Db {

    public interface IReportDbStore {

        Task Insert(OutfitReport report);

    }
}
