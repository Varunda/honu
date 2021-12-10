using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models;

namespace watchtower.Services.Hosted {

    public class DiscordService : BackgroundService {

        private readonly ILogger<DiscordService> _Logger;

        private readonly IDiscordMessageQueue _MessageQueue;

        private readonly DiscordClient _Discord;
        private IOptions<DiscordOptions> _DiscordOptions;

        private bool _IsConnected = false;
        private const string SERVICE_NAME = "discord";

        public DiscordService(ILogger<DiscordService> logger,
            IDiscordMessageQueue msgQueue, IOptions<DiscordOptions> discordOptions) {

            _Logger = logger;
            _MessageQueue = msgQueue ?? throw new ArgumentNullException(nameof(msgQueue));

            _DiscordOptions = discordOptions;

            try {
                _Discord = new DiscordClient(new DiscordConfiguration() {
                    Token = _DiscordOptions.Value.Key,
                    TokenType = TokenType.Bot,
                });
            } catch (Exception) {
                throw;
            }

            _Discord.Ready += Client_Ready;
        }

        public async override Task StartAsync(CancellationToken cancellationToken) {
            try {
                await _Discord.ConnectAsync();

                await base.StartAsync(cancellationToken);
            } catch (Exception ex) {
                _Logger.LogError(ex, "Error in start up of DiscordService");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"Started {SERVICE_NAME}");

            while (stoppingToken.IsCancellationRequested == false) {
                try {
                    if (_IsConnected == false) {
                        await Task.Delay(1000, stoppingToken);
                        continue;
                    }

                    string msg = await _MessageQueue.DequeueAsync(stoppingToken);

                    DiscordChannel? channel = await _Discord.GetChannelAsync(_DiscordOptions.Value.ChannelId);
                    if (channel != null) {
                        await channel.SendMessageAsync(msg);
                    }
                } catch (Exception ex) when (stoppingToken.IsCancellationRequested == false) {
                    _Logger.LogError(ex, "Error while caching character");
                } catch (Exception) when (stoppingToken.IsCancellationRequested == true) {
                    _Logger.LogInformation($"Stopping {SERVICE_NAME} with {_MessageQueue.Count()} left");
                }
            }
        }

        private async Task Client_Ready(DiscordClient sender, ReadyEventArgs args) {
            _Logger.LogInformation($"Discord client connected");

            _IsConnected = true;

            DiscordChannel? channel = await sender.GetChannelAsync(_DiscordOptions.Value.ChannelId);
            if (channel == null) {
                _Logger.LogWarning($"Failed to find channel");
            }
        }

    }
}
