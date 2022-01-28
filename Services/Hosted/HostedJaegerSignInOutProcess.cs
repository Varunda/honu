using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.Queues;
using watchtower.Services.Queues;
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted {

    public class HostedJaegerSignInOutProcess : BackgroundService {

        private readonly ILogger<HostedJaegerSignInOutProcess> _Logger;
        private readonly JaegerSignInOutQueue _Queue;
        private readonly CharacterRepository _CharacterRepository;
        private readonly IDiscordMessageQueue _DiscordQueue;
        private readonly IServiceHealthMonitor _ServiceHealthMonitor;

        private const string SERVICE_NAME = "jaeger_signinout_process";
        private const int RUN_DELAY = 1000 * 60 * 5;
        //private const int RUN_DELAY = 1000 * 10 * 1;

        public HostedJaegerSignInOutProcess(ILogger<HostedJaegerSignInOutProcess> logger,
            JaegerSignInOutQueue queue, CharacterRepository charRepo,
            IDiscordMessageQueue discordQueue, IServiceHealthMonitor healthMon) {

            _Logger = logger;
            _Queue = queue;
            _CharacterRepository = charRepo;
            _DiscordQueue = discordQueue;
            _ServiceHealthMonitor = healthMon;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"{SERVICE_NAME}> started");

            while (stoppingToken.IsCancellationRequested == false) {
                try {
                    ServiceHealthEntry entry = _ServiceHealthMonitor.Get(SERVICE_NAME) ?? new ServiceHealthEntry() { Name = SERVICE_NAME };
                    if (entry.Enabled == false) {
                        await Task.Delay(5000, stoppingToken);
                        continue;
                    }

                    Stopwatch timer = Stopwatch.StartNew();

                    List<JaegerSigninoutEntry> signin = _Queue.GetSignIn();
                    List<JaegerSigninoutEntry> signout = _Queue.GetSignOut();

                    if (signin.Count + signout.Count > 0) {
                        HashSet<string> both = new(signin.Count + signout.Count);
                        foreach (JaegerSigninoutEntry s in signin) { both.Add(s.CharacterID); }
                        foreach (JaegerSigninoutEntry s in signout) { both.Add(s.CharacterID); }

                        List<PsCharacter> chars = await _CharacterRepository.GetByIDs(both.ToList(), fast: false);

                        string msg = "```diff\n";

                        int onlineCount = 0;
                        lock (CharacterStore.Get().Players) {
                            onlineCount = CharacterStore.Get().Players.Count(iter => iter.Value.WorldID == World.Jaeger && iter.Value.Online == true);
                        }

                        msg += $"Currently {onlineCount} characters are online on Jaeger\n";

                        msg += $"Logins ({signin.Count}):\n";

                        foreach (JaegerSigninoutEntry s in signin) {
                            PsCharacter? c = chars.FirstOrDefault(iter => iter.ID == s.CharacterID);
                            msg += $"+[{s.Timestamp:u}] {c?.GetDisplayName() ?? $"<missing {s.CharacterID}>"}\n";
                        }

                        msg += $"\nLogouts ({signout.Count}):\n";
                        foreach (JaegerSigninoutEntry s in signout) {
                            PsCharacter? c = chars.FirstOrDefault(iter => iter.ID == s.CharacterID);
                            msg += $"-[{s.Timestamp:u}] {c?.GetDisplayName() ?? $"<missing {s.CharacterID}>"}\n";
                        }

                        msg += "```\n";

                        _DiscordQueue.Queue(msg);
                        _Queue.Clear();
                    }

                    entry.RunDuration = timer.ElapsedMilliseconds;
                    entry.LastRan = DateTime.UtcNow;
                    entry.Message = $"{signin.Count} have signed in, ${signout.Count} have signed out";
                    _ServiceHealthMonitor.Set(SERVICE_NAME, entry);

                    await Task.Delay(RUN_DELAY, stoppingToken);
                } catch (Exception) when (stoppingToken.IsCancellationRequested == true) {
                    _Logger.LogInformation($"{SERVICE_NAME}> stop requested");
                } catch (Exception ex) when (stoppingToken.IsCancellationRequested == false) {
                    _Logger.LogError(ex, $"error in {SERVICE_NAME}");
                }
            }
        }

    }
}
