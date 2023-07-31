using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Commands;
using watchtower.Services;
using watchtower.Services.Hosted;

namespace watchtower.Code.Commands {

    [Command]
    public class DiscordCommand {

        private readonly ILogger<DiscordCommand> _Logger;
        private readonly DiscordWrapper _Discord;

        public DiscordCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<DiscordCommand>>();
            _Discord = services.GetRequiredService<DiscordWrapper>();
        }

        public async Task Reconnect() {
            _Logger.LogInformation($"starting reconnect on Discord client");

            _Logger.LogInformation($"disconnect and reconnect");
            await _Discord.Get().DisconnectAsync();
            await _Discord.Get().ConnectAsync();

            _Logger.LogInformation($"reconnect method itself");
            await _Discord.Get().ReconnectAsync(startNewSession: true);

            _Logger.LogInformation($"reconnect done");
        }

        public async Task ListServers() {
            _Logger.LogInformation($"Listing all servers");

            IReadOnlyDictionary<ulong, DiscordGuild?> guilds = _Discord.Get().Guilds;

            _Logger.LogInformation($"In {guilds.Count} guilds:");
            foreach (KeyValuePair<ulong, DiscordGuild?> iter in guilds) {
                DiscordGuild? guild = iter.Value;
                if (guild == null) {
                    guild = await _Discord.Get().TryGetGuild(iter.Key);
                }
                _Logger.LogDebug($"{iter.Key}/{(guild?.Name ?? $"<unloaded Guild {iter.Key}>")}");
            }
        }

        public async Task DebugServer(ulong serverId) {
            _Logger.LogInformation($"Printing debug for guild {serverId}");

            DiscordGuild? guild = await _Discord.Get().TryGetGuild(serverId);
            if (guild == null) {
                _Logger.LogWarning($"Failed to find guild {serverId}. Make sure the bot has connected");
                return;
            }

            _Logger.LogInformation($"Server {serverId}:");
            _Logger.LogInformation($"\tName: {guild.Name}");
            _Logger.LogInformation($"\tDescription: {guild.Description}");
            _Logger.LogInformation($"\tOwner: {guild.Owner?.DisplayName} / {guild.OwnerId}");
            _Logger.LogInformation($"\tChannels: {guild.Channels.Count}");
            _Logger.LogInformation($"\tMembers: {guild.MemberCount}");
        }

        public async Task DebugChannel(ulong serverId, ulong channelId) {
            _Logger.LogInformation($"Printing debug for channel {channelId} in guild {serverId}");

            DiscordChannel? channel = await _Discord.Get().GetChannelAsync(channelId);
            if (channel == null) {
                _Logger.LogWarning($"failed to find channel {channelId}");
                return;
            }

            _Logger.LogInformation($"Channel {channelId}:");
            _Logger.LogInformation($"\tName: {channel.Name}");
            _Logger.LogInformation($"\tTopic: {channel.Topic}");
        }

        public async Task DebugMemberOfServer(ulong serverId, ulong userId) {
            _Logger.LogInformation($"Checking if user {userId} is in server {serverId}");

            DiscordGuild? guild = await _Discord.Get().TryGetGuild(serverId);
            if (guild == null) {
                _Logger.LogWarning($"failed to find guild {serverId}");
                return;
            }

            DiscordMember? member = await guild.TryGetMember(userId);
            if (member == null) {
                _Logger.LogWarning($"user {userId} is not a member of {serverId}");
                return;
            }

            _Logger.LogInformation($"User {userId}/{member.GetDisplay()} is a member of {serverId}/{guild.Name}");
        }

        public async Task FindUser(ulong userId) {
            _Logger.LogInformation($"Looking for {userId}");

            DiscordMember? member = await _Discord.GetDiscordMember(userId);
            if (member == null) {
                _Logger.LogWarning($"failed to find member {userId}");
            } else {
                _Logger.LogInformation($"found {userId}/{member.GetDisplay()}");
            }
        }

    }
}
