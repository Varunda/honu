
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
        ///     Get all sessions of a charater between a time period
        /// </summary>
        /// <param name="charID"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        Task<List<Session>> GetByRangeAndCharacterID(string charID, DateTime start, DateTime end);

        /// <summary>
        ///     Get all sessions that occured between a time period
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        Task<List<Session>> GetByRange(DateTime start, DateTime end);

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
        ///     Get all sessions between two ranges within an outfit
        /// </summary>
        /// <param name="outfitID">Optional ID of the outfit to provide</param>
        /// <param name="start">Start range</param>
        /// <param name="end">End range</param>
        /// <returns>
        ///     A list of all <see cref="Session"/>s that occured between <paramref name="start"/> and <paramref name="end"/>,
        ///     with <see cref="Session.OutfitID"/> of <paramref name="outfitID"/>
        /// </returns>
        Task<List<Session>> GetByRangeAndOutfit(string? outfitID, DateTime start, DateTime end);

        /// <summary>
        ///     Start a new session of a tracked player
        /// </summary>
        /// <param name="player">Player that will have a new session</param>
        /// <param name="when">When the session started</param>
        /// <returns>
        ///     A task for when the task is completed
        /// </returns>
        Task Start(TrackedPlayer player, DateTime when);

        /// <summary>
        ///     End an existing session of a tracked player
        /// </summary>
        /// <param name="player">Player who's session is endign</param>
        /// <param name="when">When the session ended</param>
        /// <returns>
        ///     A task for when the task is complete
        /// </returns>
        Task End(TrackedPlayer player, DateTime when);

        /// <summary>
        ///     End all sessions currently opened in the DB
        /// </summary>
        /// <returns>
        ///     A task for when the operation is complete
        /// </returns>
        Task EndAll();

    }

}