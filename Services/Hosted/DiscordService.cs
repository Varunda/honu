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
using watchtower.Services.Queues;
using DSharpPlus.SlashCommands;

using HonuDiscord = watchtower.Models.Discord;
using watchtower.Code.DiscordInteractions;
using Newtonsoft.Json.Linq;
using DSharpPlus.SlashCommands.EventArgs;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Discord;
using static watchtower.Models.Discord.HonuDiscordMessage;
using DSharpPlus.Exceptions;

namespace watchtower.Services.Hosted {

    public class DiscordService : BackgroundService {

        private readonly ILogger<DiscordService> _Logger;

        private readonly DiscordMessageQueue _MessageQueue;

        private readonly DiscordClient _Discord;
        private readonly SlashCommandsExtension _SlashCommands;
        private IOptions<DiscordOptions> _DiscordOptions;

        private bool _IsConnected = false;
        private const string SERVICE_NAME = "discord";

        private Dictionary<ulong, ulong> _CachedMembership = new();

        public DiscordService(ILogger<DiscordService> logger, ILoggerFactory loggerFactory,
            DiscordMessageQueue msgQueue, IOptions<DiscordOptions> discordOptions, IServiceProvider services) {

            _Logger = logger;
            _MessageQueue = msgQueue ?? throw new ArgumentNullException(nameof(msgQueue));

            _DiscordOptions = discordOptions;

            if (_DiscordOptions.Value.GuildId == 0) {
                throw new ArgumentException($"GuildId is 0, must be set. Try running dotnet user-secrets set Discord:GuildId $VALUE");
            }

            if (_DiscordOptions.Value.ChannelId == 0) {
                throw new ArgumentException($"ChannelId is 0, must be set. Try running dotnet user-secrets set Discord:ChannelId $VALUE");
            }

            try {
                _Discord = new DiscordClient(new DiscordConfiguration() {
                    Token = _DiscordOptions.Value.Key,
                    TokenType = TokenType.Bot,
                    LoggerFactory = loggerFactory
                });
            } catch (Exception) {
                throw;
            }

            _Discord.Ready += Client_Ready;
            _Discord.InteractionCreated += Generic_Interaction_Created;
            _Discord.ContextMenuInteractionCreated += Generic_Interaction_Created;

            _SlashCommands = _Discord.UseSlashCommands(new SlashCommandsConfiguration() {
                Services = services
            });

            _SlashCommands.SlashCommandErrored += Slash_Command_Errored;
            _SlashCommands.ContextMenuErrored += Context_Menu_Errored;

            _SlashCommands.RegisterCommands<PingSlashCommand>(_DiscordOptions.Value.GuildId);
            _SlashCommands.RegisterCommands<AccessSlashCommands>(_DiscordOptions.Value.GuildId);
            _SlashCommands.RegisterCommands<HonuInternalSlashCommands>(_DiscordOptions.Value.GuildId);
            _SlashCommands.RegisterCommands<HonuAccountSlashCommand>(_DiscordOptions.Value.GuildId);
            _SlashCommands.RegisterCommands<PsbDiscordInteractions>(_DiscordOptions.Value.GuildId);
            _SlashCommands.RegisterCommands<SubscribeSlashCommand>(_DiscordOptions.Value.GuildId);
        }

        public async override Task StartAsync(CancellationToken cancellationToken) {
            try {
                await _Discord.ConnectAsync();

                IReadOnlyList<DiscordApplicationCommand> cmds = await _Discord.GetGuildApplicationCommandsAsync(_DiscordOptions.Value.GuildId);
                _Logger.LogDebug($"Have {cmds.Count} commands");
                foreach (DiscordApplicationCommand cmd in cmds) {
                    _Logger.LogDebug($"{cmd.Id} {cmd.Name}: {cmd.Description}");
                }

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

                    HonuDiscordMessage msg = await _MessageQueue.Dequeue(stoppingToken);
                    TargetType targetType = msg.Type;

                    if (targetType == TargetType.INVALID) {
                        _Logger.LogError($"Invalid TargetType [ChannelID {msg.ChannelID}] [GuildID {msg.GuildID}] [TargetUserID {msg.TargetUserID}]");
                        continue;
                    }

                    DiscordMessageBuilder builder = new DiscordMessageBuilder();

                    // the contents is ignored if there is any embeds
                    if (msg.Embeds.Count > 0) {
                        foreach (DSharpPlus.Entities.DiscordEmbed embed in msg.Embeds) {
                            builder.AddEmbed(embed);
                        }
                    } else {
                        builder.Content = msg.Message;
                    }

                    foreach (IMention mention in msg.Mentions) {
                        builder.WithAllowedMention(mention);
                    }

                    if (targetType == TargetType.CHANNEL) {
                        DiscordGuild? guild = null;
                        try {
                            guild = await _Discord.GetGuildAsync(msg.GuildID!.Value);
                        } catch (NotFoundException) {
                            _Logger.LogError($"Failed to find guild {msg.GuildID} (404)");
                            continue;
                        }

                        if (guild == null) {
                            _Logger.LogError($"Failed to get guild {msg.GuildID} (null)");
                            continue;
                        }

                        DiscordChannel? channel = guild.GetChannel(msg.ChannelID!.Value);
                        if (channel == null) {
                            _Logger.LogError($"Failed to get channel {msg.ChannelID} in guild {msg.GuildID}");
                            continue;
                        }

                        await channel.SendMessageAsync(builder);
                    } else if (targetType == TargetType.USER) {
                        DiscordMember? member = await GetDiscordMember(msg.TargetUserID!.Value);
                        if (member == null) {
                            _Logger.LogWarning($"Failed to find {msg.TargetUserID}");
                            continue;
                        }

                        await member.SendMessageAsync(builder);
                    }
                } catch (Exception ex) when (stoppingToken.IsCancellationRequested == false) {
                    _Logger.LogError(ex, "error sending message");
                } catch (Exception) when (stoppingToken.IsCancellationRequested == true) {
                    _Logger.LogInformation($"Stopping {SERVICE_NAME} with {_MessageQueue.Count()} left");
                }
            }
        }

        /// <summary>
        ///     Get a <see cref="DiscordMember"/> from an ID
        /// </summary>
        /// <param name="memberID">ID of the Discord member to get</param>
        /// <returns>
        ///     The <see cref="DiscordMember"/> with the corresponding ID, or <c>null</c>
        ///     if the user could not be found in any guild the bot is a part of
        /// </returns>
        private async Task<DiscordMember?> GetDiscordMember(ulong memberID) {
            if (_CachedMembership.TryGetValue(memberID, out ulong guildID) == true) {
                DiscordGuild? guild = await _Discord.GetGuildAsync(guildID);
                if (guild == null) {
                    _Logger.LogWarning($"Failed to get guild {guildID} from cached membership for member {memberID}");
                } else {
                    DiscordMember? member = await guild.GetMemberAsync(memberID);
                    if (member == null) {
                        _Logger.LogWarning($"Failed to get member {memberID} from guild {guildID}");
                        _CachedMembership.Remove(memberID);
                    } else {
                        _Logger.LogDebug($"Found member {memberID} from guild {guildID} (cached)");
                        return member;
                    }
                }
            }

            foreach (KeyValuePair<ulong, DiscordGuild> entry in _Discord.Guilds) {
                DiscordMember? member = await entry.Value.GetMemberAsync(memberID);
                if (member != null) {
                    _Logger.LogDebug($"Found member {memberID} from guild {entry.Value.Id}");
                    _CachedMembership[memberID] = entry.Value.Id;
                    return member;
                }
            }

            _Logger.LogWarning($"Cannot get member {memberID}, not cached and not in any guilds");

            return null;
        }

        /// <summary>
        ///     Event handler for when the client is ready
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task Client_Ready(DiscordClient sender, ReadyEventArgs args) {
            _Logger.LogInformation($"Discord client connected");

            _IsConnected = true;

            DiscordGuild? guild = await sender.GetGuildAsync(_DiscordOptions.Value.GuildId);
            if (guild == null) {
                _Logger.LogError($"Failed to get guild {_DiscordOptions.Value.GuildId} (what was passed in the options)");
            } else {
                _Logger.LogInformation($"Successfully found {guild.Name}/{guild.Id}");
            }

            DiscordChannel? channel = await sender.GetChannelAsync(_DiscordOptions.Value.ChannelId);
            if (channel == null) {
                _Logger.LogWarning($"Failed to find channel");
            }
        }

        /// <summary>
        ///     Event handler for both types of interaction (slash commands and context menu)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private Task Generic_Interaction_Created(DiscordClient sender, InteractionCreateEventArgs args) {
            DiscordInteraction interaction = args.Interaction;
            string user = interaction.User.GetDisplay();

            string interactionMethod = "slash";

            DiscordUser? targetMember = null;
            DSharpPlus.Entities.DiscordMessage? targetMessage = null;

            if (args is ContextMenuInteractionCreateEventArgs contextArgs) {
                targetMember = contextArgs.TargetUser;
                targetMessage = contextArgs.TargetMessage;
                interactionMethod = "context menu";
            }

            string feedback = $"{user} used '{interaction.Data.Name}' (a {interaction.Type}) as a {interactionMethod}: ";

            if (targetMember != null) {
                feedback += $"[target member: (user) {targetMember.GetDisplay()}]";
            }
            if (targetMessage != null) {
                feedback += $"[target message: (channel) {targetMessage.Id}] [author: (user) {targetMessage.Author.GetDisplay()}]";
            }

            if (targetMessage == null && targetMember == null) {
                feedback += $"{interaction.Data.Name} {GetCommandString(interaction.Data.Options)}";
            }

            _Logger.LogDebug(feedback);

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Event handler for when a slash command fails
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task Slash_Command_Errored(SlashCommandsExtension ext, SlashCommandErrorEventArgs args) {
            if (args.Exception is SlashExecutionChecksFailedException failedCheck) {
                string feedback = "Check failed:\n";

                foreach (SlashCheckBaseAttribute check in failedCheck.FailedChecks) {
                    if (check is RequiredRoleSlashAttribute role) {
                        _Logger.LogWarning($"{args.Context.User.GetDisplay()} attempted to use {args.Context.CommandName},"
                            + $" but lacks the Discord roles: {string.Join(", ", role.Roles)}");
                        feedback += $"You lack a required role: {string.Join(", ", role.Roles)}";
                    } else {
                        feedback += $"Unchecked check type: {check.GetType()}";
                        _Logger.LogError($"Unchecked check type: {check.GetType()}");
                    }
                }

                await args.Context.CreateImmediateText(feedback);

                return;
            }

            _Logger.LogError(args.Exception, $"error executing slash command: {args.Context.CommandName}");
            try {
                // if the response has already started, this won't be null, indicating to instead update the response
                DSharpPlus.Entities.DiscordMessage? msg = await args.Context.GetOriginalResponseAsync();

                if (msg == null) {
                    // if it is null, then no respons has been started, so one is created
                    // if you attempt to create a response for one that already exists, then a 400 is thrown
                    await args.Context.CreateImmediateText($"Error executing slash command: {args.Exception.Message}");
                } else {
                    await args.Context.EditResponseText($"Error executing slash command: {args.Exception.Message}");
                }
            } catch (Exception ex) {
                _Logger.LogError(ex, $"error sending error message to Discord");
            }
        }

        /// <summary>
        ///     Event handler for when a context menu command fails
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task Context_Menu_Errored(SlashCommandsExtension ext, ContextMenuErrorEventArgs args) {
            if (args.Exception is ContextMenuExecutionChecksFailedException failedCheck) {
                string feedback = "Check failed:\n";

                foreach (ContextMenuCheckBaseAttribute check in failedCheck.FailedChecks) {
                    if (check is RequiredRoleContextAttribute role) {
                        _Logger.LogWarning($"{args.Context.User.GetDisplay()} attempted to use {args.Context.CommandName},"
                            + $" but lacks the Discord roles: {string.Join(", ", role.Roles)}");

                        feedback += $"You lack a required role: {string.Join(", ", role.Roles)}";
                    } else {
                        feedback += $"Unchecked check type: {check.GetType()}";
                        _Logger.LogError($"Unchecked check type: {check.GetType()}");
                    }
                }

                await args.Context.CreateImmediateText(feedback);

                return;
            }

            _Logger.LogError(args.Exception, $"error executing context command: {args.Context.CommandName}");
            try {
                // if the response has already started, this won't be null, indicating to instead update the response
                DSharpPlus.Entities.DiscordMessage? msg = await args.Context.GetOriginalResponseAsync();

                if (msg == null) {
                    // if it is null, then no respons has been started, so one is created
                    // if you attempt to create a response for one that already exists, then a 400 is thrown
                    await args.Context.CreateImmediateText($"Error executing context command: {args.Exception.Message}");
                } else {
                    await args.Context.EditResponseText($"Error executing context command: {args.Exception.Message}");
                }
            } catch (Exception ex) {
                _Logger.LogError(ex, $"error sending error message to Discord");
            }
        }

        /// <summary>
        ///     Convert the Honu Wrapper for a discord mention into whatever library we're using
        /// </summary>
        /// <param name="mention"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">If the <paramref name="mention"/>'s <see cref="DiscordMention.MentionType"/> was unhandled</exception>
        private IMention ConvertMentionable(DiscordMention mention) {
            if (mention.MentionType == DiscordMention.DiscordMentionType.NONE) {
                throw new ArgumentException($"{nameof(DiscordMention)} has {nameof(DiscordMention.MentionType)} {nameof(DiscordMention.DiscordMentionType.NONE)}");
            } else if (mention.MentionType == DiscordMention.DiscordMentionType.ROLE) {
                return new RoleMention(((RoleDiscordMention)mention).RoleID);
            }

            throw new ArgumentException($"Unchecked {nameof(DiscordMention.MentionType)}: {mention.MentionType}");
        }

        /// <summary>
        ///     Transform the options used in an interaction into a string that can be viewed
        /// </summary>
        /// <param name="options"></param>
        private string GetCommandString(IEnumerable<DiscordInteractionDataOption>? options) {
            if (options == null) {
                options = new List<DiscordInteractionDataOption>();
            }

            string s = "";

            foreach (DiscordInteractionDataOption opt in options) {
                s += $"[{opt.Name}=";

                if (opt.Type == ApplicationCommandOptionType.Attachment) {
                    s += $"(Attachment)";
                } else if (opt.Type == ApplicationCommandOptionType.Boolean) {
                    s += $"(bool) {opt.Value}";
                } else if (opt.Type == ApplicationCommandOptionType.Channel) {
                    s += $"(channel) {opt.Value}";
                } else if (opt.Type == ApplicationCommandOptionType.Integer) {
                    s += $"(int) {opt.Value}";
                } else if (opt.Type == ApplicationCommandOptionType.Mentionable) {
                    s += $"(mentionable) {opt.Value}";
                } else if (opt.Type == ApplicationCommandOptionType.Number) {
                    s += $"(number) {opt.Value}";
                } else if (opt.Type == ApplicationCommandOptionType.Role) {
                    s += $"(role) {opt.Value}";
                } else if (opt.Type == ApplicationCommandOptionType.String) {
                    s += $"(string) '{opt.Value}'";
                } else if (opt.Type == ApplicationCommandOptionType.SubCommand) {
                    s += GetCommandString(opt.Options);
                } else if (opt.Type == ApplicationCommandOptionType.SubCommandGroup) {
                    s += GetCommandString(opt.Options);
                } else if (opt.Type == ApplicationCommandOptionType.User) {
                    s += $"(user) {opt.Value}";
                } else {
                    _Logger.LogError($"Unchecked {nameof(DiscordInteractionDataOption)}.{nameof(DiscordInteractionDataOption.Type)}: {opt.Type}, value={opt.Value}");
                    s += $"[{opt.Name}=(UNKNOWN {opt.Type}) {opt.Value}]";
                }

                s += "]";
            }

            return s;
        }

    }
}
