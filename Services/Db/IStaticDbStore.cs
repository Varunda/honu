using System.Collections.Generic;
using System.Threading.Tasks;

namespace watchtower.Services.Db {

    public interface IStaticDbStore<T> {

        Task<List<T>> GetAll();

        Task Upsert(T param);

    }

}