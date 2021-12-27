using System.Collections.Generic;
using System.Threading.Tasks;

namespace watchtower.Services.Census {

    public interface IStaticCollection<T> {

        Task<List<T>> GetAll();

    }

}