using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace watchtower.Services.Census {

    public interface IStaticCollection<T> {

        Task<List<T>> GetAll();

        Task<List<T>> GetAll(CancellationToken cancel);

    }

}