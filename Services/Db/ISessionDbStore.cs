
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Models.Db;

namespace watchtower.Services.Db {

    public interface ISessionDbStore {

        /// <summary>
        ///     Get the sessions of a character that are within the interval from now
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <param name="interval">How many minutes back to go</param>
        /// <returns>
        ///     All <see cref="Session"/>s within the period given
        /// </returns>
        Task<List<Session>> GetByCharacterID(string charID, int interval);

        /// <summary>
        ///     Get all sessions of a character
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <returns>
        ///     All <see cref="Session"/> with <see cref="Session.CharacterID"/> of <paramref name="charID"/>
        /// </returns>
        Task<List<Session>> GetAllByCharacterID(string charID);

        /// <summary>
        ///     Get a specific session
        /// </summary>
        /// <param name="sessionID">ID of the session to get</param>
        /// <returns>
        ///     The <see cref="Session"/> with <see cref="Session.ID"/> of <paramref name="sessionID"/>,
        ///     or <c>null</c> if it does not exist
        /// </returns>
        Task<Session?> GetByID(long sessionID);

        /// <summary>
        ///     Start a new session of a tracked player
        /// </summary>
        /// <param name="player">Player that will have a new session</param>
        /// <returns>
        ///     A task for when the task is completed
        /// </returns>
        Task Start(TrackedPlayer player);

        Task End(TrackedPlayer player);

        Task EndAll();

    }

}