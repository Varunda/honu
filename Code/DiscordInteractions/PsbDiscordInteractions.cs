using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
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
        public async Task PsbWhoIsContext(ContextMenuContext ctx) {
            DiscordMember source = ctx.Member;
            DiscordMember target = ctx.TargetMember;

            HonuAccount? currentUser = await _AccountDb.GetByDiscordID(source.Id, CancellationToken.None);
            if (currentUser == null) {
                await ctx.CreateImmediateText($"You do not have a Honu account");
                return;
            }

            await ctx.CreateDeferredText($"Loading contacts...");

            List<PsbOvOContact> ovo = new();
            List<PsbPracticeContact> practice = new();

            string feedback = $"<@{target.Id}> ({target.Username}#{target.Discriminator}/{target.Id}) is a rep for:\n";

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

            try {
                await source.SendMessageAsync(feedback);
                await ctx.EditResponseText($"Sent PSB rep information in a DM");
            } catch (Exception ex) {
                _Logger.LogError(ex, $"failed to send message to {source.Id}/{source.Username}#{source.Discriminator}");
                await ctx.EditResponseText($"failed to send response in DM");
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

            await ctx.CreateDeferredText($"Loading...");

            // 2 ways a user has permission to use the command:
            //      1. they have a honu account
            //      2. they are an ovo rep for that group
            bool hasPerm = false;
            HonuAccount? currentUser = await _CurrentUser.GetDiscord(ctx);
            hasPerm = (currentUser != null);

            List<PsbOvOContact> contacts = (await _ContactRepository.GetOvOContacts())
                .Where(iter => iter.Group.ToLower() == tag.ToLower()).ToList();

            if (hasPerm == false) {
                hasPerm = contacts.FirstOrDefault(iter => iter.DiscordID == ctx.Member.Id) != null;
            }

            if (hasPerm == false) {
                await ctx.EditResponseText($"You lack permission to get the OvO reps for {tag}: You need a Honu account or to be an OvO rep of this outfit");
                return;
            }

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
                await ctx.Member.SendMessageAsync(feedback);
                await ctx.EditResponseText($"Response sent in DMs");
            } catch (Exception ex) {
                _Logger.LogError(ex, $"failed to send message to {ctx.Member.Id}/{ctx.Member.Username}#{ctx.Member.Discriminator}");
                await ctx.EditResponseText($"failed to send response in DM");
            }
        }


    }
}
