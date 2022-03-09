using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Db;
using watchtower.Models.Events;
using watchtower.Services.Db;

namespace watchtower.Services.Hosted.Startup {

    public class KilledTeamIDFixerService : BackgroundService {

        private readonly ILogger<KilledTeamIDFixerService> _Logger;
        private readonly SessionDbStore _SessionDb;
        private readonly KillEventDbStore _KillDb;

        private const string SERVICE_NAME = "killed_team_id_fixer";

        public KilledTeamIDFixerService(ILogger<KilledTeamIDFixerService> logger,
            SessionDbStore sessionDb, KillEventDbStore killDb) {

            _Logger = logger;
            _SessionDb = sessionDb;
            _KillDb = killDb;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"{SERVICE_NAME}> started");

            long count = await _SessionDb.GetUnfixedCount();
            long completed = 0;

            _Logger.LogInformation($"{SERVICE_NAME}> have {count} sessions to go");

            Stopwatch timer = Stopwatch.StartNew();

            while (stoppingToken.IsCancellationRequested == false) {
                try {
                    List<Session> sessions = await _SessionDb.GetUnfixed(stoppingToken);

                    foreach (Session s in sessions) {
                        //_Logger.LogDebug($"Fixing session {s.ID}, from {s.Start} to {s.End}");

                        List<KillEvent> events = await _KillDb.GetKillsByCharacterID(s.CharacterID, s.Start, s.End ?? DateTime.UtcNow);

                        List<KillEvent> kills = events.Where(iter => iter.AttackerCharacterID == s.CharacterID).ToList();
                        List<KillEvent> deaths = events.Where(iter => iter.KilledCharacterID == s.CharacterID).ToList();

                        if (kills.Count == 0) {
                            //_Logger.LogDebug($"{SERVICE_NAME}> no kill events to use, have {deaths.Count} deaths");
                        } else {
                            short lastTeamID = kills[0].AttackerTeamID;

                            foreach (KillEvent ev in events) {
                                if (ev.AttackerCharacterID == s.CharacterID && lastTeamID != ev.AttackerTeamID) {
                                    lastTeamID = ev.AttackerTeamID;
                                    //_Logger.LogInformation($"{SERVICE_NAME}> {s.ID} new team_id {lastTeamID}");
                                }

                                if (ev.KilledCharacterID == s.CharacterID && lastTeamID != -1) {
                                    ev.KilledTeamID = lastTeamID;
                                    await _KillDb.UpdateKilledTeamID(ev.ID, ev.KilledTeamID);
                                }
                            }
                        }

                        await _SessionDb.SetFixed(s.ID, stoppingToken);
                        ++completed;
                    }

                    if (completed % 1000 == 0) {
                        _Logger.LogDebug($"{SERVICE_NAME}> completed {completed}/{count} in {timer.ElapsedMilliseconds / 1000}s");
                        timer.Restart();
                    }
                } catch (Exception ex) when (stoppingToken.IsCancellationRequested == false) {
                    _Logger.LogError(ex, $"error in {SERVICE_NAME}");
                } catch (Exception) when (stoppingToken.IsCancellationRequested == true) {
                    _Logger.LogInformation($"{SERVICE_NAME}> stopped");
                }
            }
        }

    }
}
