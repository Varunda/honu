using System.Threading;
using System.Threading.Tasks;

namespace watchtower.Services.Repositories.Static {

    /// <summary>
    ///     Interface that lets a repository refresh its data
    /// </summary>
    public interface IRefreshableRepository {

        /// <summary>
        ///     Perform a refresh of the data within this repository
        /// </summary>
        /// <param name="cancel"></param>
        /// <returns></returns>
        public Task Refresh(CancellationToken cancel);

    }
}
