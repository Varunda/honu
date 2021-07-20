using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Db;
using watchtower.Models.Events;

namespace watchtower.Services.Db {

    /// <summary>
    /// Interface to interact with the DB
    /// </summary>
    public interface IKillEventDbStore {

        /// <summary>
        /// Insert a new <see cref="KillEvent"/>
        /// </summary>
        /// <param name="ev">Event to be inserted</param>
        Task Insert(KillEvent ev);

        /// <summary>
        /// Update the revived column of a death to indicate a death was revived, and does not count towards K/D
        /// </summary>
        /// <param name="charID">ID of the character that was revived</param>
        /// <param name="revivedID">ID of the exp event for the revive</param>
        /// <returns>A task for when the operation is complete</returns>
        Task SetRevivedID(string charID, long revivedID);

        /// <summary>
        /// Get the top 8 killers from the parameters given
        /// </summary>
        /// <param name="options">Options used to generate the data</param>
        /// <returns></returns>
        Task<List<KillDbEntry>> GetTopKillers(KillDbOptions options);

        Task<List<KillDbOutfitEntry>> GetTopOutfitKillers(KillDbOptions options);

        Task<List<KillEvent>> GetKillsByCharacterID(string characterID, int interval);

        Task<List<KillEvent>> GetKillsByOutfitID(string outfitID, int interval);

    }
}
