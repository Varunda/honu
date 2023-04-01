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
                feedback += "**Community rep**:\n" + string.Join("\n", ovo.Select(iter => $"{iter.RepType} for: {string.Join(", ", iter.Groups)}")) + "\n";
            }

            if (practice.Count > 0) {
                if (ovo.Count > 0) {
                    feedback += "\n";
                }

                feedback += "**Practice rep**:\n" + string.Join("\n", practice.Select(iter => $"{iter.Tag}")) + "\n";
            }

            await ctx.EditResponseText(feedback);
        }

        //[SlashCommand("toggle-account-automation", "Toggle account automation")]
        [RequiredRoleSlash("ovo-admin")]
        public async Task ToggleAccountAutomation(InteractionContext ctx) {
            PsbReservationRepository.AccountEnabled = !PsbReservationRepository.AccountEnabled;

            if (PsbReservationRepository.AccountEnabled == true) {
                await ctx.CreateImmediateText("Account automation enabled");
            } else {
                await ctx.CreateImmediateText("Account automation disabled");
            }
        }

        [SlashCommand("toggle-booking-automation", "Toggle booking automation")]
        [RequiredRoleSlash("ovo-admin")]
        public async Task ToggleBookingAutomation(InteractionContext ctx) {
            PsbReservationRepository.BookingEnabled = !PsbReservationRepository.BookingEnabled;

            if (PsbReservationRepository.BookingEnabled == true) {
                await ctx.CreateImmediateText("Booking automation enabled");
            } else {
                await ctx.CreateImmediateText("Booking automation disabled");
            }
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
                .Where(iter => iter.Groups.Contains(tag)).ToList();

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

        [RequiredRoleSlash("ovo-staff")]
        [SlashCommand("audit-ovo-reps", "Check which ovo reps are still in the Discord")]
        public async Task AuditOvOContacts(InteractionContext ctx,
            [Option("offset", "offset to start from")] long? offset = 0) {
            await ctx.CreateDeferred(false);

            List<PsbOvOContact> ovo = await _ContactRepository.GetOvOContacts();

            DiscordWebhookBuilder interactionBuilder = new();
            DiscordEmbedBuilder builder = new();

            DiscordGuild? homeGuild = await ctx.Client.TryGetGuild(_DiscordOptions.Value.GuildId);
            if (homeGuild == null) {
                builder.Title = $"Error";
                builder.Color = DiscordColor.Red;
                builder.Description = $"Error: failed to find guild id {_DiscordOptions.Value.GuildId}";
                await ctx.EditResponseEmbed(builder);
                return;
            }

            builder.Title = $"OvO reps not in Discord";
            builder.Color = DiscordColor.Red;

            for (int i = 0; i < ovo.Count; ++i) {
                if (i < offset) {
                    continue;
                }

                PsbOvOContact contact = ovo[i];

                DiscordMember? member = await homeGuild.TryGetMember(contact.DiscordID);
                if (member != null) {
                    continue;
                }

                builder.Description += $"{i} :: [{string.Join("/", contact.Groups)}] {contact.Email}/{contact.DiscordID} <@{contact.DiscordID}>\n";

                if (builder.Description.Length > 1500) {
                    builder.Description += $"{ovo.Count - i} more...";
                    break;
                }
            }

            await ctx.EditResponseEmbed(builder);
        }

        [RequiredRoleSlash("practice-staff")]
        [SlashCommand("audit-practice-reps", "Check which practice reps are still in the Discord")]
        public async Task AuditPracticeContacts(InteractionContext ctx,
            [Option("offset", "offset to start from")] long? offset = 0) {
            await ctx.CreateDeferred(false);

            List<PsbPracticeContact> ovo = await _ContactRepository.GetPracticeContacts();

            DiscordWebhookBuilder interactionBuilder = new();
            DiscordEmbedBuilder builder = new();

            DiscordGuild? homeGuild = await ctx.Client.TryGetGuild(_DiscordOptions.Value.GuildId);
            if (homeGuild == null) {
                builder.Title = $"Error";
                builder.Color = DiscordColor.Red;
                builder.Description = $"Error: failed to find guild id {_DiscordOptions.Value.GuildId}";
                await ctx.EditResponseEmbed(builder);
                return;
            }

            builder.Title = $"OvO reps not in Discord";
            builder.Color = DiscordColor.Red;

            for (int i = 0; i < ovo.Count; ++i) {
                if (i < offset) {
                    continue;
                }

                PsbPracticeContact contact = ovo[i];

                DiscordMember? member = await homeGuild.TryGetMember(contact.DiscordID);
                if (member != null) {
                    continue;
                }

                builder.Description += $"{i} :: [{contact.Tag}] {contact.Email}/{contact.DiscordID} <@{contact.DiscordID}>\n";

                if (builder.Description.Length > 1500) {
                    builder.Description += $"use the offset parameter to show others";
                    break;
                }
            }

            await ctx.EditResponseEmbed(builder);
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

        /// <summary>
        ///     Button that when pressed will refresh the message the button is attached to
        ///     with a re-parsed reservation from the <see cref="DiscordMessage"/> with
        ///     <see cref="SnowflakeObject.Id"/> of <paramref name="msgID"/>
        /// </summary>
        /// <param name="msgID">ID of the message to pull the information from</param>
        public static DiscordButtonComponent REFRESH_RESERVATION(ulong msgID) => new(ButtonStyle.Secondary, $"@refresh-reservation.{msgID}", "Refresh");

        /// <summary>
        ///     Button that when pressed when approve the base bookings of a reservation
        ///     that can be found in the <see cref="DiscordMessage"/> with <see cref="SnowflakeObject.Id"/>
        ///     of <paramref name="msgID"/>
        /// </summary>
        /// <param name="msgID">ID of the message that contains the reservation</param>
        public static DiscordButtonComponent APPROVE_BOOKING(ulong msgID) => new(ButtonStyle.Primary, $"@approve-booking.{msgID}", "Approve booking");

        /// <summary>
        ///     Button that when pressed will approve the account request of a reservation
        ///     that can be found in the <see cref="DiscordMessage"/> with <see cref="SnowflakeObject.Id"/>
        ///     of <paramref name="msgID"/>
        /// </summary>
        /// <param name="msgID">ID of the message that contains the reservation</param>
        public static DiscordButtonComponent APPROVE_ACCOUNTS(ulong msgID) => new(ButtonStyle.Primary, $"@approve-accounts.{msgID}", "Approve accounts");

        public static DiscordButtonComponent RESET_RESERVATION(ulong msgID) => new(ButtonStyle.Danger, $"@reset-reservation.{msgID}", "Reset (DANGER)");

        public ILogger<PsbButtonCommands> _Logger { set; private get; } = default!;
        public PsbReservationRepository _ReservationRepository { set; private get; } = default!;
        public PsbOvOSheetRepository _SheetRepository { set; private get; } = default!;
        public PsbCalendarRepository _CalendarRepository { set; private get; } = default!;
        public PsbParsedReservationDbStore _MetadataDb { set; private get; } = default!;

        public IOptions<DiscordOptions> _DiscordOptions { set; private get; } = default!;
        public IOptions<PsbRoleMapping> _RoleMapping { set; private get; } = default!;

        /// <summary>
        ///     Helper method to get the message of a reservation based on only the ID. The guild and channel are assumed
        ///     based on the options provided. Additionally, the error response is handled, so if the return value is null,
        ///     all you need to do is return, as the response was already handled
        /// </summary>
        /// <param name="ctx">context the interaction is being performed in</param>
        /// <param name="action">what action is being done. This is just to make the errors a bit more descripive</param>
        /// <param name="msgID">ID of the message that contains the reservation</param>
        /// <returns>
        ///     The <see cref="DiscordMessage"/> with <see cref="SnowflakeObject.Id"/> of <paramref name="msgID"/>,
        ///     or <c>null</c> if it doesn't exist. If null is returned, the interaction will also be edited with
        ///     an error explaining why
        /// </returns>
        private async Task<DiscordMessage?> GetReservationMessage(ButtonContext ctx, string action, ulong msgID) {
            DiscordGuild? guild = await ctx.Client.TryGetGuild(_DiscordOptions.Value.GuildId);
            if (guild == null) {
                _Logger.LogWarning($"cannot {action} {msgID}: guild {_DiscordOptions.Value.GuildId} is null");
                await ctx.Interaction.EditResponseText($"cannot {action} {msgID}: guild {_DiscordOptions.Value.GuildId} is null");
                return null;
            }

            DiscordChannel? channel = guild.TryGetChannel(_DiscordOptions.Value.ReservationChannelId);
            if (channel == null) {
                _Logger.LogWarning($"cannot {action} {msgID}: channel {_DiscordOptions.Value.ReservationChannelId} was not found");
                await ctx.Interaction.EditResponseText($"cannot {action} {msgID}: channel {_DiscordOptions.Value.ReservationChannelId} is null");
                return null;
            }

            DiscordMessage? msg = await channel.TryGetMessage(msgID);
            if (msg == null) {
                _Logger.LogWarning($"cannot {action} {msgID}: message was null");
                await ctx.Interaction.EditResponseText($"cannot {action} {msgID}: message was null");
                return null;
            }

            return msg;
        }

        /// <summary>
        ///     Button command to refresh the parsing of a reservation. Uses the configured reservations channel
        /// </summary>
        /// <param name="ctx">provided context</param>
        /// <param name="msgID">ID of the message to be re-parsed</param>
        [ButtonCommand("refresh-reservation")]
        public async Task RefreshReservation(ButtonContext ctx, ulong msgID) {
            await ctx.Interaction.CreateDeferred(true);

            if (ctx.Message == null) {
                _Logger.LogError($"missing message for refresh-reservation");
                await ctx.Interaction.EditResponseText($"Error: missing message?");
                return;
            }

            DiscordMessage? msg = await GetReservationMessage(ctx, "refresh-reservation", msgID);
            if (msg == null) {
                return;
            }

            ParsedPsbReservation parsed = await _ReservationRepository.Parse(msg);

            DiscordMessageBuilder builder = new();
            builder.AddEmbed(parsed.Build(false));

            List<DiscordComponent> comps = new();
            comps.Add(PsbButtonCommands.REFRESH_RESERVATION(msgID));

            if (PsbReservationRepository.BookingEnabled == true) {
                DiscordButtonComponent bookingBtn = PsbButtonCommands.APPROVE_BOOKING(parsed.MessageId);
                if (parsed.Metadata.BookingApprovedById != null || parsed.Errors.Count != 0) {
                    bookingBtn.Disable();
                }
                comps.Add(bookingBtn);
            }

            if (PsbReservationRepository.AccountEnabled == true) {
                DiscordButtonComponent accountBtn = PsbButtonCommands.APPROVE_ACCOUNTS(parsed.MessageId);
                if (parsed.Metadata.AccountSheetApprovedById != null || parsed.Errors.Count != 0) {
                    accountBtn.Disable();
                }
                comps.Add(accountBtn);
            }

            comps.Add(PsbButtonCommands.RESET_RESERVATION(msgID));

            builder.AddComponents(comps);

            await ctx.Message.ModifyAsync(builder);
            await ctx.Interaction.EditResponseText("Refreshed!");
        }

        /// <summary>
        ///     button command to accept the base booking of a reservation
        /// </summary>
        /// <param name="ctx">provided context</param>
        /// <param name="msgID">ID of the message that will contain the reservation</param>
        [ButtonCommand("approve-booking")]
        public async Task ApproveBooking(ButtonContext ctx, ulong msgID) {
            await ctx.Interaction.CreateDeferred(true);

            if (PsbReservationRepository.BookingEnabled == false) {
                await ctx.Interaction.EditResponseText($"bot booking approvals are disabled. Have an admin user `/toggle-booking-automation` to toggle");
                return;
            }

            if (ctx.Member == null) {
                await ctx.Interaction.EditResponseErrorEmbed($"unexpected condition: member was null");
                return;
            }

            if (_RoleMapping.Value.Mappings.TryGetValue("ovo-staff", out ulong staffID) == false) {
                await ctx.Interaction.EditResponseErrorEmbed("setup error: role mapping for `ovo-staff` is missing. Use `dotnet user-secrets set PsbRoleMapping:Mappings:ovo-staff $ROLE_ID`");
                return;
            }

            if (ctx.Member.Roles.FirstOrDefault(iter => iter.Id == staffID) == null) {
                await ctx.Interaction.EditResponseErrorEmbed($"you lack the ovo-staff role");
                return;
            }

            DiscordMessage? msg = await GetReservationMessage(ctx, "approve-booking", msgID);
            if (msg == null) {
                return;
            }

            ParsedPsbReservation parsed = await _ReservationRepository.Parse(msg);
            if (parsed.Metadata.BookingApprovedById != null) {
                await ctx.Interaction.EditResponseErrorEmbed($"Cannot approved base booking\nAlready approved by <@{parsed.Metadata.BookingApprovedById}>");
                return;
            }

            foreach (PsbBaseBooking booking in parsed.Reservation.Bases) {
                await _CalendarRepository.Insert(parsed, booking);
            }

            parsed.Metadata.BookingApprovedById = ctx.User.Id;
            await _MetadataDb.Upsert(parsed.Metadata);

            await ctx.Interaction.EditResponseEmbed(new DiscordEmbedBuilder()
                .WithTitle("Success")
                .WithDescription($"Successfully booked {parsed.Reservation.Bases.Count} bases")
                .WithColor(DiscordColor.Green)
            );
        }

        //[ButtonCommand("approve-accounts")]
        public async Task ApproveAccounts(ButtonContext ctx, ulong msgID) {
            await ctx.Interaction.CreateDeferred(true);

            if (PsbReservationRepository.AccountEnabled == false) {
                await ctx.Interaction.EditResponseText($"bot account approvals are disabled. Have an admin user `/toggle-account-automation` to toggle");
                return;
            }

            if (ctx.Member == null) {
                await ctx.Interaction.EditResponseErrorEmbed($"unexpected condition: member was null");
                return;
            }

            if (_RoleMapping.Value.Mappings.TryGetValue("ovo-staff", out ulong staffID) == false) {
                await ctx.Interaction.EditResponseErrorEmbed("setup error: role mapping for `ovo-staff` is missing");
                return;
            }

            if (ctx.Member.Roles.FirstOrDefault(iter => iter.Id == staffID) == null) {
                await ctx.Interaction.EditResponseErrorEmbed($"you lack the ovo-staff role");
                return;
            }

            DiscordMessage? msg = await GetReservationMessage(ctx, "approve-accounts", msgID);
            if (msg == null) {
                return;
            }

            ParsedPsbReservation parsed = await _ReservationRepository.Parse(msg);

            string fileID = await _SheetRepository.CreateSheet(parsed.Reservation);

            await ctx.Interaction.EditResponseEmbed(new DiscordEmbedBuilder()
                .WithTitle("Success")
                .WithDescription($"Successfully created account sheet for reservation at `{fileID}`")
                .WithColor(DiscordColor.Green)
            );
        }

        [ButtonCommand("reset-reservation")]
        public async Task ResetReservation(ButtonContext ctx, ulong msgID) {
            await ctx.Interaction.CreateDeferred(true);

            if (ctx.Member == null) {
                await ctx.Interaction.EditResponseErrorEmbed($"unexpected condition: member was null");
                return;
            }

            if (_RoleMapping.Value.Mappings.TryGetValue("ovo-admin", out ulong staffID) == false) {
                await ctx.Interaction.EditResponseErrorEmbed("setup error: role mapping for `ovo-admin` is missing");
                return;
            }

            if (ctx.Member.Roles.FirstOrDefault(iter => iter.Id == staffID) == null) {
                await ctx.Interaction.EditResponseErrorEmbed($"you lack the ovo-admin role");
                return;
            }

            DiscordMessage? msg = await GetReservationMessage(ctx, "refresh-reservation", msgID);
            if (msg == null) {
                return;
            }

            ParsedPsbReservation parsed = await _ReservationRepository.Parse(msg);

            parsed.Metadata.AccountSheetApprovedById = null;
            parsed.Metadata.AccountSheetId = null;
            parsed.Metadata.BookingApprovedById = null;
            await _MetadataDb.Upsert(parsed.Metadata);

            await ctx.Interaction.EditResponseText($"Reset reservation");
        }

    }

}
