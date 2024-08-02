using DaybreakGames.Census.Exceptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.Tracking;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.Queues;
using watchtower.Services.Db;
using watchtower.Services.Queues;
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted {

    /// <summary>
    ///     Hosted queue that starts sessions for characters
    /// </summary>
    public class HostedSessionStarterQueue : BackgroundService {

        private const string SERVICE_NAME = "background_session_queue";

        private readonly ILogger<HostedSessionStarterQueue> _Logger;
        private readonly SessionStarterQueue _Queue;

        private readonly SessionRepository _SessionRepository;
        private readonly CharacterRepository _CharacterRepository;
        private readonly CharacterCacheQueue _CharacterCacheQueue;

        // Used to prevent a single character stuck in the queue from causing it to run a ton of times
        private string _LastCharacterId = "";

        public HostedSessionStarterQueue(ILogger<HostedSessionStarterQueue> logger,
            SessionStarterQueue queue, CharacterRepository characterRepository,
            CharacterCacheQueue characterCacheQueue, SessionRepository sessionRepository) {

            _Logger = logger;

            _Queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _CharacterRepository = characterRepository;
            _CharacterCacheQueue = characterCacheQueue;
            _SessionRepository = sessionRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"Started {SERVICE_NAME}");

            Stopwatch timer = Stopwatch.StartNew();

            while (stoppingToken.IsCancellationRequested == false) {
                try {
                    CharacterSessionStartQueueEntry entry = await _Queue.Dequeue(stoppingToken);
                    timer.Restart();

                    // if the character is already online, no need to start the session (which will set Online true)
                    // while the queue doesn't allow duplicate character IDs, there is a period of time where the entry is queued,
                    //      that character performs and event while not in the queue, and gets queued again
                    // HOWEVER: we don't want to skip this if SessionID is null, as we need to update the session once we try again
                    //      this is fine, as honu doesn't create a new session
                    TrackedPlayer? p = CharacterStore.Get().GetByCharacterID(entry.CharacterID);
                    if (p != null && p.Online == true && entry.SessionID == null) {
                        //_Logger.LogTrace($"character {entry.CharacterID} is already online, dropping from queue. Session ID {p.SessionID}");
                        continue;
                    }

                    // If Honu has a single entry in the queue, don't constantly loop thru it, take a breather
                    if (_LastCharacterId == entry.CharacterID) {
                        await Task.Delay(100, stoppingToken);
                    }

                    if (entry.FailCount > 0 && entry.Backoff >= DateTime.UtcNow) {
                        //_Logger.LogDebug($"Backoff for {entry.CharacterID} is {entry.Backoff:u}, currently {DateTime.UtcNow:u}, requeueing");
                        _Queue.Queue(entry);
                        continue;
                    }

                    _LastCharacterId = entry.CharacterID;

                    using (Activity? start = HonuActivitySource.Root.StartActivity("session start")) {
                        // if honu cannot find the character for some reason (census is down, character service is down, etc.)
                        //      we want to start the session right away. if the session is not started, then it will stay
                        //      in this loop until the FailCount is 10, which then the session will be inserted into the DB
                        // this is bad, this can take like 30 minutes to timeout, so we want to show the session asap,
                        //      and update once Census returns the data (or we never get it anyways)
                        if (entry.FailCount > 0 && entry.SessionID == null) {
                            using (Activity? repoCall = HonuActivitySource.Root.StartActivity("repo start")) {
                                entry.SessionID = await _SessionRepository.Start(entry.CharacterID, entry.LastEvent, "0", 0);
                            }
                            _Logger.LogDebug($"character could not be found from Census, starting session anyways [characterID={entry.CharacterID}] [sessionID={entry.SessionID}]");

                            if (entry.SessionID == null) {
                                _Logger.LogWarning($"expected a session ID, why is this here? [charID={entry.CharacterID}] [sessionID={entry.SessionID}]");
                            }
                        }


                        // load the character from our repo
                        PsCharacter? c = null;
                        try {
                            using (Activity? getCharacter = HonuActivitySource.Root.StartActivity("get char")) {
                                c = await _CharacterRepository.GetByID(entry.CharacterID, entry.Environment);
                            }
                            // if no character, queue it for a cache and retry after 5 minutes
                            if (c == null) {
                                ++entry.FailCount;
                                entry.Backoff = DateTime.UtcNow + TimeSpan.FromSeconds(10 * Math.Min(1, entry.FailCount));

                                // only re-queue for 10 tries
                                if (entry.FailCount <= 10) {
                                    _Logger.LogTrace($"requeuing due to missing character [charID={entry.CharacterID}] [sessionID={entry.SessionID}] [failCount={entry.FailCount}] [backoff={entry.Backoff:u}]");
                                    _Queue.Queue(entry);
                                    _CharacterCacheQueue.Queue(entry.CharacterID, entry.Environment);
                                    continue;
                                }
                            }

                            // at this point, we either have the character, or we failed too many times
                            if (entry.FailCount > 0) {
                                _Logger.LogDebug($"found character after failed attempts [character={entry.CharacterID}/{c?.Name}] [failCount={entry.FailCount}] [sessionID={entry.SessionID}]");
                            }
                        } catch (Exception ex) {
                            // if any exception occurs (such as as a CensusServiceUnavailable), then we also want to inc the FailCount
                            ++entry.FailCount;
                            entry.Backoff = DateTime.UtcNow + TimeSpan.FromMinutes(Math.Min(5, entry.FailCount));

                            string parms = $"[charID={entry.CharacterID}] [failCount={entry.FailCount}] [backoff={entry.Backoff:u}] [Exception={ex.Message}]";
                            // after 10 tries, don't queue again
                            if (entry.FailCount <= 10) {
                                _Logger.LogWarning($"failed to get character due to exception, re-queueing " + parms);
                                _Queue.Queue(entry);
                                continue;
                            }

                            _Logger.LogError($"failed to find character due to exception, this causes wrong outfit ID and faction ID " + parms);
                        }

                        // at this point, honu has either has the character data, or failed to get it (after 10 tries)
                        //      so honu will either create the new session (if the session was found right away)
                        //      or update the existing one (if honu failed to get the info the first time)
                        using (Activity? repoCall = HonuActivitySource.Root.StartActivity("repo start")) {
                            if (entry.SessionID == null) {
                                repoCall?.AddTag("updated", false);
                                _Logger.LogTrace($"started new session [charID={entry.CharacterID}] [delay={DateTime.UtcNow - entry.LastEvent}]");
                                await _SessionRepository.Start(entry.CharacterID, entry.LastEvent, c?.OutfitID ?? "0", c?.FactionID ?? 0);
                            } else {
                                repoCall?.AddTag("updated", true);
                                TimeSpan delay = DateTime.UtcNow - entry.LastEvent;
                                _Logger.LogDebug($"updating session already started [charID={entry.CharacterID}] [sessionID={entry.SessionID}] [failCount={entry.FailCount}] [delay={delay}]");
                                await _SessionRepository.Update(entry.SessionID.Value, c?.OutfitID ?? "0", c?.FactionID ?? 0);
                            }
                        }
                    }

                    _Queue.AddProcessTime(timer.ElapsedMilliseconds);
                } catch (Exception ex) when (stoppingToken.IsCancellationRequested == false) {
                    _Logger.LogError(ex, "error starting session in the background");
                } catch (Exception) when (stoppingToken.IsCancellationRequested == true) {
                    _Logger.LogInformation($"Stopping {SERVICE_NAME} with {_Queue.Count()} left");
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"Ending all current sessions");
            Stopwatch timer = Stopwatch.StartNew();
            await _SessionRepository.EndAll(DateTime.UtcNow, stoppingToken);

            _Logger.LogDebug($"Took {timer.ElapsedMilliseconds}ms to close all sessions");
        }

    }
}
