using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models;
using watchtower.Models.Internal;
using watchtower.Models.PSB;
using watchtower.Services.Db;
using watchtower.Services.Repositories.PSB;

namespace watchtower.Code.DiscordInteractions {

    public class PsbDiscordInteractions : PermissionSlashCommand {

        public ILogger<PsbDiscordInteractions> _Logger { set; private get; } = default!;
        public HonuAccountDbStore _AccountDb { set; private get; } = default!;

        public PsbContactSheetRepository _ContactRepository { set; private get; } = default!;

        /// <summary>
        ///     User context menu to see what reps a user has (ovo, practice, etc.)
        /// </summary>
        /// <param name="ctx">Provided context</param>
        [ContextMenu(ApplicationCommandType.UserContextMenu, "Get rep status")]
        [RequiredRoleContext(RequiredRoleCheck.OVO_STAFF)]
        public async Task PsbWhoIsContext(ContextMenuContext ctx) {
            DiscordMember source = ctx.Member;
            DiscordMember target = ctx.TargetMember;

            await ctx.CreateDeferredText($"Loading contacts...", true);

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

            await ctx.CreateDeferredText($"Loading...", true);

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

            await ctx.CreateDeferredText("Loading...", true);

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


    }
}
