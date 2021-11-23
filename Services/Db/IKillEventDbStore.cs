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
        ///     Insert a new <see cref="KillEvent"/>
        /// </summary>
        /// <param name="ev">Event to be inserted</param>
        Task Insert(KillEvent ev);

        /// <summary>
        ///     Update the revived column of a death to indicate a death was revived, and does not count towards K/D
        /// </summary>
        /// <param name="charID">ID of the character that was revived</param>
        /// <param name="revivedID">ID of the exp event for the revive</param>
        /// <returns>A task for when the operation is complete</returns>
        Task SetRevivedID(string charID, long revivedID);

        /// <summary>
        ///     Get the top 8 killers from the parameters given
        /// </summary>
        /// <param name="options">Options used to generate the data</param>
        /// <returns></returns>
        Task<List<KillDbEntry>> GetTopKillers(KillDbOptions options);

        /// <summary>
        ///     Get the top killers in an outfit
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        Task<List<KillDbOutfitEntry>> GetTopOutfitKillers(KillDbOptions options);

        /// <summary>
        ///     Get all the kills and deaths of a character between a period
        /// </summary>
        /// <param name="characterID"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        Task<List<KillEvent>> GetKillsByCharacterID(string characterID, DateTime start, DateTime end);

        /// <summary>
        ///     Get all kills and deaths of a set of characters
        /// </summary>
        /// <param name="IDs"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        Task<List<KillEvent>> GetKillsByCharacterIDs(List<string> IDs, DateTime start, DateTime end);

        /// <summary>
        ///     Get the 
        /// </summary>
        /// <param name="charID"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        Task<List<KillEvent>> GetRecentKillsByCharacterID(string charID, int interval);

        /// <summary>
        ///     Get all kills of an outfit
        /// </summary>
        /// <param name="outfitID"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        Task<List<KillEvent>> GetKillsByOutfitID(string outfitID, int interval);

        /// <summary>
        ///     Get the top weapons used 
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        Task<List<KillItemEntry>> GetTopWeapons(KillDbOptions options);

    }

    public static class IKillEventDbStoreExtensionMethods {

        /*
        public static Task<List<KillEvent>> GetRecentKillsByCharacterID(this IKillEventDbStore db, string characterID, int interval) {
            DateTime start = DateTime.UtcNow - TimeSpan.FromSeconds(interval * 60);
            return db.GetKillsByCharacterID(characterID, start, DateTime.UtcNow);

        }
        */

    }

}
