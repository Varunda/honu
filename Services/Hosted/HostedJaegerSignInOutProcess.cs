using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.Discord;
using watchtower.Models.PSB;
using watchtower.Models.Queues;
using watchtower.Services.Queues;
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted {

    public class HostedJaegerSignInOutProcess : BackgroundService {

        private readonly ILogger<HostedJaegerSignInOutProcess> _Logger;
        private readonly JaegerSignInOutQueue _Queue;
        private readonly CharacterRepository _CharacterRepository;
        private readonly DiscordMessageQueue _DiscordQueue;
        private readonly IServiceHealthMonitor _ServiceHealthMonitor;

        private readonly IOptions<JaegerNsaOptions> _Options;

        private const string SERVICE_NAME = "jaeger_signinout_process";
        private const int RUN_DELAY = 1000 * 60 * 5;
        //private const int RUN_DELAY = 1000 * 10 * 1;

        private readonly List<string> _DevAccounts = new List<string>() {
            "5428861140076767121", // njLive
            "5428662532303167729", // PlaysWithSteeringWheel
            "5429063001623372113"  // BlueOwl122
        };

        public HostedJaegerSignInOutProcess(ILogger<HostedJaegerSignInOutProcess> logger,
            JaegerSignInOutQueue queue, CharacterRepository charRepo,
            DiscordMessageQueue discordQueue, IServiceHealthMonitor healthMon,
            IOptions<JaegerNsaOptions> options) {

            _Logger = logger;
            _Queue = queue;
            _CharacterRepository = charRepo;
            _DiscordQueue = discordQueue;
            _ServiceHealthMonitor = healthMon;

            _Options = options;
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

                    int messageLimit = 2000;
                    bool devSeen = false;

                    if (signin.Count + signout.Count > 0) {
                        HashSet<string> both = new(signin.Count + signout.Count);
                        foreach (JaegerSigninoutEntry s in signin) { both.Add(s.CharacterID); }
                        foreach (JaegerSigninoutEntry s in signout) { both.Add(s.CharacterID); }

                        List<PsCharacter> chars = await _CharacterRepository.GetByIDs(both.ToList(), fast: false);

                        string msg = $"<https://wt.honu.pw/jaegernsa/{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}>\n";

                        msg += "```diff\n";

                        int onlineCount = 0;
                        lock (CharacterStore.Get().Players) {
                            onlineCount = CharacterStore.Get().Players.Count(iter => iter.Value.WorldID == World.Jaeger && iter.Value.Online == true);
                        }

                        msg += $"As of {DateTime.UtcNow:u} UTC, there are {onlineCount} characters online\n";

                        msg += $"\nLogins ({signin.Count}):\n";
                        foreach (JaegerSigninoutEntry s in signin) {
                            PsCharacter? c = chars.FirstOrDefault(iter => iter.ID == s.CharacterID);
                            string part = $"+[{s.Timestamp:u}] {c?.GetDisplayName() ?? $"<missing {s.CharacterID}>"}\n";

                            if (msg.Length + part.Length + "```".Length >= messageLimit) {
                                _DiscordQueue.Queue(msg + "```");
                                msg = "```diff\n";
                            }

                            msg += part;

                            if (devSeen == false && _DevAccounts.Find(iter => iter == s.CharacterID) != null) {
                                devSeen = true;
                            }
                        }

                        msg += $"\nLogouts ({signout.Count}):\n";
                        foreach (JaegerSigninoutEntry s in signout) {
                            PsCharacter? c = chars.FirstOrDefault(iter => iter.ID == s.CharacterID);
                            string part = $"-[{s.Timestamp:u}] {c?.GetDisplayName() ?? $"<missing {s.CharacterID}>"}\n";

                            if (msg.Length + part.Length + "```".Length >= messageLimit) {
                                _DiscordQueue.Queue(msg + "```");
                                msg = "```diff\n";
                            }

                            msg += part;
                        }

                        msg += "```\n";

                        _DiscordQueue.Queue(msg);
                        _Queue.Clear();
                    }

                    if (devSeen == true && _Options.Value.AlertRoleID != null) {
                        _DiscordQueue.Queue($"<@&{_Options.Value.AlertRoleID}> dev account has signed in");
                    }

                    entry.RunDuration = timer.ElapsedMilliseconds;
                    entry.LastRan = DateTime.UtcNow;
                    entry.Message = $"{signin.Count} have signed in, {signout.Count} have signed out";
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
