using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models;

namespace watchtower.Services.Repositories {

    public interface IStaticRepository<T> {

        Task<List<T>> GetAll();

        Task<T?> GetByID(int ID);

    }

}