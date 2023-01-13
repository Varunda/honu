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
using watchtower.Code.SlashCommands;
using Newtonsoft.Json.Linq;
using DSharpPlus.SlashCommands.EventArgs;
using watchtower.Code.ExtensionMethods;

namespace watchtower.Services.Hosted {

    public class DiscordService : BackgroundService {

        private readonly ILogger<DiscordService> _Logger;

        private readonly DiscordMessageQueue _MessageQueue;

        private readonly DiscordClient _Discord;
        private readonly SlashCommandsExtension _SlashCommands;
        private IOptions<DiscordOptions> _DiscordOptions;

        private bool _IsConnected = false;
        private const string SERVICE_NAME = "discord";

        public DiscordService(ILogger<DiscordService> logger,
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
                    TokenType = TokenType.Bot
                });
            } catch (Exception) {
                throw;
            }

            _Discord.Ready += Client_Ready;
            _Discord.InteractionCreated += Interaction_Created;

            _SlashCommands = _Discord.UseSlashCommands(new SlashCommandsConfiguration() {
                Services = services
            });
            _SlashCommands.RegisterCommands<PingSlashCommand>(_DiscordOptions.Value.GuildId);
            _SlashCommands.RegisterCommands<AddSlashCommand>(_DiscordOptions.Value.GuildId);
            _SlashCommands.RegisterCommands<AccessSlashCommands>(_DiscordOptions.Value.GuildId);
            _SlashCommands.RegisterCommands<HonuInternalSlashCommands>(_DiscordOptions.Value.GuildId);

            _SlashCommands.SlashCommandErrored += async (SlashCommandsExtension etx, SlashCommandErrorEventArgs args) => {
                _Logger.LogError(args.Exception, $"Error executing slash command: {args.Context.CommandName}");
                await args.Context.CreateImmediateText($"Error executing slash command: {args.Exception.Message}");
            };
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

                    HonuDiscord.DiscordMessage msg = await _MessageQueue.Dequeue(stoppingToken);

                    DiscordChannel? channel = await _Discord.GetChannelAsync(_DiscordOptions.Value.ChannelId);
                    if (channel == null) {
                        _Logger.LogWarning($"Failed to find channel {_DiscordOptions.Value.ChannelId}, cannot send message");
                        continue;
                    }

                    DiscordMessageBuilder builder = new DiscordMessageBuilder();

                    if (msg.Embeds.Count > 0) {
                        foreach (HonuDiscord.DiscordEmbed embed in msg.Embeds) {
                            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();
                            embedBuilder.Color = new DiscordColor(embed.Color);
                            embedBuilder.Description = embed.Description;
                            embedBuilder.Title = embed.Description;

                            foreach (HonuDiscord.DiscordEmbedField field in embed.Fields) {
                                embedBuilder.AddField(field.Name, field.Value, field.Inline);
                            }

                            builder.AddEmbed(embedBuilder.Build());
                        }
                    } else {
                        builder.Content = msg.Message;
                    }

                    await channel.SendMessageAsync(builder);
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

        private async Task Interaction_Created(DiscordClient sender, InteractionCreateEventArgs args) {
            DiscordInteraction interaction = args.Interaction;
            ulong discordID = interaction.User.Id;

            if (interaction.Type == InteractionType.Ping) {
                _Logger.LogDebug($"Ping interaction");
            } else if (interaction.Type == InteractionType.ApplicationCommand) {
                string commandName = interaction.Data.Name;
                string[] commandArgs = interaction.Data.Values;

                _Logger.LogDebug($"Application command used by {discordID}: {GetCommandString(interaction.Data.Options)}");
            } else if (interaction.Type == InteractionType.AutoComplete) {
                _Logger.LogDebug($"AutoComplete interaction");
            } else if (interaction.Type == InteractionType.Component) {
                _Logger.LogDebug($"Component interaction");
            } else if (interaction.Type == InteractionType.ModalSubmit) {
                _Logger.LogDebug($"ModalSubmit interaction");
            }

            return;
        }

        private string GetCommandString(IEnumerable<DiscordInteractionDataOption> options) {
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
