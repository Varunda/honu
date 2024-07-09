using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace watchtower.Services.Db {

    public interface IStaticDbStore<T> {

        Task<List<T>> GetAll(CancellationToken cancel = default);

        Task Upsert(T param);

    }

}