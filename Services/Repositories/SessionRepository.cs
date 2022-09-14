using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Services.Db;
using watchtower.Services.Queues;

namespace watchtower.Services.Repositories {

    public class SessionRepository {

        private readonly ILogger<SessionRepository> _Logger;
        private readonly SessionDbStore _SessionDb;
        private readonly CharacterRepository _CharacterRepository;

        public SessionRepository(ILogger<SessionRepository> logger,
            SessionDbStore sessionDb, CharacterRepository characterRepository) {

            _Logger = logger;

            _SessionDb = sessionDb;
            _CharacterRepository = characterRepository;
        }

        /// <summary>
        ///     Get all sessions of a character
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <returns>
        ///     All <see cref="Session"/> with <see cref="Session.CharacterID"/> of <paramref name="charID"/>
        /// </returns>
        public Task<List<Session>> GetAllByCharacterID(string charID) {
            return _SessionDb.GetAllByCharacterID(charID);
        }

        /// <summary>
        ///     Get a specific session
        /// </summary>
        /// <param name="sessionID">ID of the session to get</param>
        /// <returns>
        ///     The <see cref="Session"/> with <see cref="Session.ID"/> of <paramref name="sessionID"/>,
        ///     or <c>null</c> if it does not exist
        /// </returns>
        public Task<Session?> GetByID(long sessionID) {
            return _SessionDb.GetByID(sessionID);
        }
        
        /// <summary>
        ///     Get all sessions of a charater between a time period
        /// </summary>
        /// <param name="charID">Character ID</param>
        /// <param name="start">Range of sessions to look for</param>
        /// <param name="end">End period of the range</param>
        /// <returns></returns>
        public Task<List<Session>> GetByRangeAndCharacterID(string charID, DateTime start, DateTime end) {
            if (start > end) {
                throw new ArgumentException($"{nameof(start)} cannot come after {nameof(end)}");
            }

            return _SessionDb.GetByRangeAndCharacterID(charID, start, end);
        }

        /// <summary>
        ///     Get all sessions that occured between a time period
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public Task<List<Session>> GetByRange(DateTime start, DateTime end) {
            return _SessionDb.GetByRange(start, end);
        }

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
        public Task<List<Session>> GetByRangeAndOutfit(string? outfitID, DateTime start, DateTime end) {
            return _SessionDb.GetByRangeAndOutfit(outfitID, start, end);
        }

        /// <summary>
        ///     Start a new session of a tracked player
        /// </summary>
        /// <param name="charID">ID of the character that is starting the session</param>
        /// <param name="when">When the session started</param>
        /// <param name="outfitID">ID of the outfit the character currently is</param>
        /// <param name="teamID">ID of the team the character is currently on</param>
        /// <returns>
        ///     A task for when the task is completed
        /// </returns>
        public async Task Start(string charID, DateTime when, string? outfitID, short teamID) {
            TrackedPlayer? player = CharacterStore.Get().GetByCharacterID(charID);
            if (player == null) {
                _Logger.LogError($"Cannot start session for {charID}, does not exist in CharacterStore");
                return;
            }

            if (player.Online == true) {
                //_Logger.LogWarning($"Not starting session for {player.ID}, logged in at {player.LastLogin}");
                return;
            }

            Session s = new Session();
            s.CharacterID = charID;
            s.Start = when;
            s.OutfitID = outfitID;
            s.TeamID = teamID;

            long ID = await _SessionDb.Insert(s);

            player.SessionID = ID;
            player.Online = true;

            CharacterStore.Get().SetByCharacterID(charID, player);
        }

        /// <summary>
        ///     End an existing session of a tracked player
        /// </summary>
        /// <param name="charID">ID of the character who's session is ending</param>
        /// <param name="when">When the session ended</param>
        /// <returns>
        ///     A task for when the task is complete
        /// </returns>
        public async Task End(string charID, DateTime when) {
            TrackedPlayer? player = CharacterStore.Get().GetByCharacterID(charID);
            if (player == null) {
                _Logger.LogError($"Cannot start session for {charID}, does not exist in CharacterStore");
                return;
            }

            if (player.Online == false) {
                //_Logger.LogWarning($"Player {player.ID} is already offline, might not have a session to end");
                return;
            }

            if (player.SessionID == null) {
                _Logger.LogWarning($"player {player.ID} does not have a session?");
                return;
            }

            await _SessionDb.SetSessionEndByID(player.SessionID.Value, when);

            player.Online = false;
            player.ZoneID = 0;
            player.SessionID = null;

            CharacterStore.Get().SetByCharacterID(charID, player);
        }

        /// <summary>
        ///     End all sessions currently opened in the DB
        /// </summary>
        /// <param name="when">What the finish value will be set to</param>
        /// <param name="cancel">Cancelation token</param>
        public Task EndAll(DateTime when, CancellationToken cancel) {
            return _SessionDb.EndAll(when, cancel);
        }

    }
}
