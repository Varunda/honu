using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Commands;
using watchtower.Services.Hosted;

namespace watchtower.Code.Commands {

    [Command]
    public class DiscordCommand {

        private readonly ILogger<DiscordCommand> _Logger;
        private readonly DiscordService _Discord;

        public DiscordCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<DiscordCommand>>();
            _Discord = services.GetRequiredService<DiscordService>();
        }

        public async Task Reconnect() {
            _Logger.LogInformation($"starting reconnect on Discord client");

            _Logger.LogInformation($"disconnect and reconnect");
            await _Discord.GetClient().DisconnectAsync();
            await _Discord.GetClient().ConnectAsync();

            _Logger.LogInformation($"reconnect method itself");
            await _Discord.GetClient().ReconnectAsync(startNewSession: true);

            _Logger.LogInformation($"reconnect done");
        }

        public async Task ListServers() {
            _Logger.LogInformation($"Listing all servers");

            List<string> strs = new();

            IReadOnlyDictionary<ulong, DiscordGuild?> guilds = _Discord.GetClient().Guilds;

            foreach (KeyValuePair<ulong, DiscordGuild?> iter in guilds) {
                DiscordGuild? guild = iter.Value;
                if (guild == null) {
                    guild = await _Discord.GetClient().TryGetGuild(iter.Key);
                }

                strs.Add($"${iter.Key}/{(guild?.Name ?? $"<unloaded Guild {iter.Key}>")}");
            }

            _Logger.LogInformation($"In {guilds.Count} guilds: [{string.Join(", ", strs)}]");
        }

    }
}
