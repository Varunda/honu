using DSharpPlus;
using DSharpPlus.ButtonCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.Internal;
using watchtower.Models.PSB;
using watchtower.Services.Db;
using watchtower.Services.Repositories;
using watchtower.Services.Repositories.PSB;

namespace watchtower.Code.DiscordInteractions {

    public class PsbDiscordInteractions : PermissionSlashCommand {

        public ILogger<PsbDiscordInteractions> _Logger { set; private get; } = default!;
        public HonuAccountDbStore _AccountDb { set; private get; } = default!;

        public PsbContactSheetRepository _ContactRepository { set; private get; } = default!;
        public FacilityRepository _FacilityRepository { set; private get; } = default!;
        public PsbCalendarRepository _CalendarRepository { set; private get; } = default!;
        public PsbReservationRepository _ReservationRepository { set; private get; } = default!;

        public IOptions<PsbDriveSettings> _PsbDriveSettings { set; private get; } = default!;
        public IOptions<DiscordOptions> _DiscordOptions { set; private get; } = default!;

        /// <summary>
        ///     User context menu to see what reps a user has (ovo, practice, etc.)
        /// </summary>
        /// <param name="ctx">Provided context</param>
        [ContextMenu(ApplicationCommandType.UserContextMenu, "Get rep status")]
        [RequiredRoleContext(RequiredRoleCheck.OVO_STAFF)]
        public async Task PsbWhoIsContext(ContextMenuContext ctx) {
            DiscordMember source = ctx.Member;
            DiscordMember target = ctx.TargetMember;

            await ctx.CreateDeferred(true);

            List<PsbOvOContact> ovo = new();
            List<PsbPracticeContact> practice = new();

            string feedback = $"{target.GetPing()} {target.GetDisplay()} is a rep for:\n";

            try {
                ovo = (await _ContactRepository.GetOvOContacts()).Where(iter => iter.DiscordID == target.Id).ToList();
            } catch (Exception ex) {
                feedback += $"An error occured while loading OvO contacts: {ex.Message}\n";
                _Logger.LogError(ex, $"error occured while loading OvO contacts");
            }

            try {
                practice = (await _ContactRepository.GetPracticeContacts()).Where(iter => iter.DiscordID == target.Id).ToList();
            } catch (Exception ex) {
                feedback += $"An error occured while loading practice contacts: {ex.Message}\n";
                _Logger.LogError(ex, $"error occured while loading practice contacts");
            }

            if (ovo.Count > 0) {
                feedback += "**Community rep**:\n" + string.Join("\n", ovo.Select(iter => $"{iter.RepType} for {iter.Group}")) + "\n";
            }

            if (practice.Count > 0) {
                if (ovo.Count > 0) {
                    feedback += "\n";
                }

                feedback += "**Practice rep**:\n" + string.Join("\n", practice.Select(iter => $"{iter.Tag}")) + "\n";
            }

            await ctx.EditResponseText(feedback);
        }

        /// <summary>
        ///     Slash command to list the ovo reps of an outfit
        /// </summary>
        /// <param name="ctx">Provided context</param>
        /// <param name="tag">Tag to find the reps for</param>
        [SlashCommand("ovo-rep", "List the OvO reps of an outfit")]
        public async Task ListOvOCommand(InteractionContext ctx,
            [Option("Tag", "Outfit Tag to list the OVO reps of")] string tag) {

            await ctx.CreateDeferred(true);

            List<PsbOvOContact> contacts = (await _ContactRepository.GetOvOContacts())
                .Where(iter => iter.Group.ToLower() == tag.ToLower()).ToList();

            string feedback = $"The following users are OvO reps for {tag}:\n";

            foreach (PsbOvOContact contact in contacts) {
                DiscordMember? member = null;

                try {
                    member = await ctx.Guild.GetMemberAsync(contact.DiscordID);
                } catch (Exception ex) {
                    _Logger.LogError(ex, $"failed to load discord ID {contact.DiscordID} from guild {ctx.Guild.Name}/{ctx.Guild.Id}");
                }

                feedback += $"<@{contact.DiscordID}> (";

                if (member != null) {
                    feedback += $"{member.Username}#{member.Discriminator} ";
                } else {
                    feedback += $"!CANNOT GET! ";
                }

                feedback += $" / {contact.DiscordID})\n";
            }

            try {
                await ctx.EditResponseText(feedback);
            } catch (Exception ex) {
                _Logger.LogError(ex, $"failed to send message to {ctx.Member.Id}/{ctx.Member.Username}#{ctx.Member.Discriminator}");
                await ctx.EditResponseText($"failed to send response in DM");
            }
        }

        /// <summary>
        ///     Message context menu to get the emails of the author and mentioned users in a message
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [ContextMenu(ApplicationCommandType.MessageContextMenu, "Get emails")]
        [RequiredRoleContext(RequiredRoleCheck.OVO_STAFF)]
        public async Task GetEmailsContext(ContextMenuContext ctx) {
            if (ctx.TargetMessage == null) {
                await ctx.CreateImmediateText($"cannot execute command: target message is null?", true);
                return;
            }

            await ctx.CreateDeferred(true);

            DiscordMessage msg = ctx.TargetMessage;

            List<DiscordUser> mentions = msg.MentionedUsers.ToList();
            mentions.Add(ctx.TargetMessage.Author);

            List<PsbOvOContact> ovo = await _ContactRepository.GetOvOContacts();
            List<PsbPracticeContact> practice = await _ContactRepository.GetPracticeContacts();

            string feedback = $"Found {mentions.Count} users in message:";

            HashSet<ulong> foundUsers = new();

            foreach (DiscordUser user in mentions) {
                if (foundUsers.Contains(user.Id)) {
                    continue;
                }
                foundUsers.Add(user.Id);

                PsbContact? contact = ovo.FirstOrDefault(iter => iter.DiscordID == user.Id);
                if (contact == null) {
                    contact = practice.FirstOrDefault(iter => iter.DiscordID == user.Id);
                }

                if (contact == null) {
                    feedback += $"\n{user.GetPing()} `{user.GetDisplay()}`: no email!";
                } else {
                    feedback += $"\n{user.GetPing()} `{user.GetDisplay()}`: `{contact.Email}`";
                }
            }

            await ctx.EditResponseText(feedback);
        }

        /// <summary>
        ///     Slash command to get the upcoming reservations on the Jaeger calendar
        /// </summary>
        /// <param name="ctx">Provided context</param>
        /// <param name="hours">How many hours in both directions to include reservations in</param>
        [SlashCommand("calendar", "Check the Jaeger Event's calendar")]
        public async Task Calendar(InteractionContext ctx,
            [Option("Hours", "How many hours back and forward to include")] long hours = 6) {

            await ctx.CreateDeferred(true);

            List<PsbCalendarEntry> entries = await _CalendarRepository.GetAll();

            List<PsbCalendarEntry> relevant = new();

            DateTime now = DateTime.UtcNow;
            DateTime nowBack = now - TimeSpan.FromHours(hours);
            DateTime nowForward = now + TimeSpan.FromHours(hours);
            foreach (PsbCalendarEntry entry in entries) {
                if ((entry.Start > nowBack && entry.End < nowForward)
                    || (entry.Start < nowBack && entry.End > nowForward)) {

                    relevant.Add(entry);
                }
            }

            // sort based on when the reservation will start, or end if the start is the same
            relevant.Sort((a, b) => {
                if (a.Start == b.Start) {
                    return a.End.CompareTo(b.End);
                }
                return a.Start.CompareTo(b.Start);
            });

            DiscordWebhookBuilder hookBuilder = new();
            DiscordEmbedBuilder builder = new();
            builder.Title = $"{relevant.Count} reservations between {nowBack.GetDiscordFullTimestamp()} and {nowForward.GetDiscordFullTimestamp()}";
            builder.Url = $"https://docs.google.com/spreadsheets/d/{_PsbDriveSettings.Value.CalendarFileId}/";

            foreach (PsbCalendarEntry entry in relevant) {
                builder.AddField(
                    name: $"{string.Join(", ", entry.Groups)} ({entry.Start.GetDiscordFullTimestamp()} - {entry.End.GetDiscordFullTimestamp()})",
                    value: $"{string.Join(", ", entry.Bases.Select(iter => iter.Name))}"
                );

                if (builder.Fields.Count >= 25) {
                    break;
                }
            }

            hookBuilder.AddEmbed(builder);

            await ctx.EditResponseAsync(hookBuilder);
        }

        /// <summary>
        ///     message context menu to check how honu will parse a reservation
        /// </summary>
        /// <param name="ctx">Provided context</param>
        [ContextMenu(ApplicationCommandType.MessageContextMenu, "(debug) Check reservation")]
        public Task DebugParseReservation(ContextMenuContext ctx) => ParseReservationInternal(ctx, true);

        /// <summary>
        ///     message context menu to check how honu will parse a reservation
        /// </summary>
        /// <param name="ctx">Provided context</param>
        [ContextMenu(ApplicationCommandType.MessageContextMenu, "Check reservation")]
        public Task ParseReservation(ContextMenuContext ctx) => ParseReservationInternal(ctx, false);

        /// <summary>
        ///     Parse a message containing a reservation
        /// </summary>
        /// <param name="ctx">Context menu</param>
        /// <param name="debug">If extra debug info will be printed</param>
        /// <returns></returns>
        private async Task ParseReservationInternal(ContextMenuContext ctx, bool debug) {
            await ctx.CreateDeferred(true);

            DiscordMessage? msg = ctx.TargetMessage;
            if (msg == null) {
                await ctx.EditResponseText($"message is null?");
                return;
            }

            ParsedPsbReservation parsed = await _ReservationRepository.Parse(ctx.TargetMessage);
            DiscordEmbedBuilder builder = parsed.Build(debug);

            await ctx.EditResponseEmbed(builder);
        }

    }

    public class PsbButtonCommands : ButtonCommandModule {

        public static DiscordButtonComponent REFRESH_RESERVATION(ulong msgID) => new(ButtonStyle.Secondary, $"@refresh-reservation.{msgID}", "Refresh");

        public ILogger<PsbButtonCommands> _Logger { set; private get; } = default!;
        public PsbReservationRepository _ReservationRepository { set; private get; } = default!;
        public IOptions<DiscordOptions> _DiscordOptions { set; private get; } = default!;

        [ButtonCommand("refresh-reservation")]
        public async Task RefreshReservation(ButtonContext ctx, ulong msgID) {
            await ctx.Interaction.CreateDeferred(true);

            if (ctx.Message == null) {
                _Logger.LogError($"missing message for refresh-reservation");
                await ctx.Interaction.EditResponseText($"Error: missing message?");
                return;
            }

            DiscordGuild? guild = await ctx.Client.TryGetGuild(_DiscordOptions.Value.GuildId);
            if (guild == null) {
                _Logger.LogWarning($"cannot refresh-reservation {msgID}: guild {_DiscordOptions.Value.GuildId} is null");
                await ctx.Interaction.EditResponseText($"cannot refresh-reservation {msgID}: guild {_DiscordOptions.Value.GuildId} is null");
                return;
            }

            DiscordChannel? channel = guild.TryGetChannel(_DiscordOptions.Value.ReservationChannelId);
            if (channel == null) {
                _Logger.LogWarning($"cannot refresh-reservation {msgID}: channel {_DiscordOptions.Value.ReservationChannelId} was not found");
                await ctx.Interaction.EditResponseText($"cannot refresh-reservation {msgID}: channel {_DiscordOptions.Value.ReservationChannelId} is null");
                return;
            }

            DiscordMessage? msg = await channel.TryGetMessage(msgID);
            if (msg == null) {
                _Logger.LogWarning($"cannot refresh-reservation {msgID}: message was null");
                await ctx.Interaction.EditResponseText($"cannot refresh-reservation {msgID}: message was null");
                return;
            }

            ParsedPsbReservation parsed = await _ReservationRepository.Parse(msg);

            await ctx.Message.ModifyAsync(Optional.FromValue(parsed.Build(false).Build()));
            await ctx.Interaction.EditResponseText("Refreshed!");
        }

    }

}
