using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

        public IOptions<PsbDriveSettings> _PsbDriveSettings { set; private get; } = default!;

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

        [SlashCommand("calendar", "Check the Jaeger Event's calendar")]
        public async Task Calendar(InteractionContext ctx,
            [Option("Hours", "How many hours back and forward to include")] long hours = 6) {

            await ctx.CreateDeferredText("Loading...", true);

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

        [ContextMenu(ApplicationCommandType.MessageContextMenu, "[DEBUG] Parse reservation")]
        [RequiredRoleContext(RequiredRoleCheck.OVO_STAFF)]
        public async Task DebugParseReservation(ContextMenuContext ctx) {
            await ctx.CreateDeferredText("Loading...", true);

            DiscordMessage? msg = ctx.TargetMessage;
            if (msg == null) {
                await ctx.EditResponseText($"message is null?");
                return;
            }

            PsbReservation res = new();

            string feedback = $"Parsed message:\n";

            List<string> lines = msg.Content.Split("\n").ToList();
            foreach (string line in lines) {
                _Logger.LogDebug($"line: '{line}'");

                List<string> parts = line.Split(":").ToList();

                if (parts.Count < 2) {
                    feedback += $"Line '{line}' failed to split on ':', had {parts.Count} parts\n";
                    continue;
                }

                string field = parts[0].Trim().ToLower();
                string value = parts[1].Trim();
                string v = string.Join(":", parts.ToArray()[1..]).Trim();

                if (field == "outfit" || field == "outfits") {
                    feedback += $"Line `{line}` as outfits\n";

                    List<string> outfits = value.Split(",").ToList();
                    feedback += $"\tOutfits: {string.Join(", ", outfits)}\n";
                    res.Outfits = outfits.Select(iter => iter.ToLower()).ToList();
                } else if (field == "accounts" || field == "number of accounts") {
                    feedback += $"Line `{line}` as account number\n";

                    if (int.TryParse(value, out int accountCount) == true) {
                        res.Accounts = accountCount;
                        feedback += $"\tAccounts: {res.Accounts}\n";
                    } else {
                        feedback += $"\tFailed to parse '{value}' to a valid number\n";
                    }
                } else if (field.StartsWith("rep") == true) {
                    feedback += $"Line `{line}` as rep line\n";

                    List<PsbOvOContact> ovo = await _ContactRepository.GetOvOContacts();
                    List<DiscordUser> users = msg.MentionedUsers.ToList();

                    foreach (DiscordUser user in users) {
                        PsbOvOContact? contact = ovo.FirstOrDefault(iter => iter.DiscordID == user.Id);
                        if (contact == null) {
                            feedback += $"\t{user.GetPing()} {user.GetDisplay()} does not have a OvO contact entry\n";
                        } else {
                            feedback += $"\tFound OvO contact for {user.GetPing()}: {contact.Group}\n";
                        }
                    }
                } else if (field.StartsWith("date and time") || field == "time" || field == "date" || field == "when") {
                    feedback += $"Line `{line}` as when\n";

                    DateTime? r = ParseVeryInexact(v, out string f);
                    feedback += f;

                    if (r != null) {
                        feedback += $"\tConverted '{v}' into {r:u} (<t:{new DateTimeOffset(r.Value).ToUnixTimeSeconds()}:R>)\n";
                    } else {
                        feedback += $"\tFailed to convert '{v}' into a valid datetime\n";
                    }

                } else if (field == "bases") {
                    feedback += $"Line `{line}` as bases\n";

                    List<string> bases = value.ToLower().Split(",").Select(iter => iter.Trim()).ToList();
                    List<PsFacility> facilities = await _FacilityRepository.GetAll();

                    foreach (string baseName in bases) {
                        List<PsFacility> possibleBases = new();

                        foreach (PsFacility fac in facilities) {
                            if (fac.Name.ToLower().StartsWith(baseName) == true) {
                                possibleBases.Add(fac);
                            }
                        }

                        if (possibleBases.Count == 0) {
                            feedback += $"\tFailed to find base `{baseName}`\n";
                        } else if (possibleBases.Count > 1) {
                            feedback += $"\tAmbigious base name `{baseName}`: {string.Join(", ", possibleBases.Select(iter => iter.Name))}\n";
                        } else if (possibleBases.Count == 1) {
                            PsFacility fac = possibleBases[0];
                            feedback += $"\tBase `{baseName}` found {fac.Name}/{fac.FacilityID}\n";
                            res.Bases.Add(possibleBases[0]);
                        }
                    }
                } else if (field == "details") {
                    feedback += $"Line `{line}` as details\n";
                    res.Details = value.Trim();
                } else {
                    feedback += $"Unchecked field '{field}'\n";
                }
            }

            feedback += $"\n\n**Parsed**: ```json\n{JToken.FromObject(res)}```";

            await ctx.EditResponseText(feedback);
        }

        private DateTime? ParseVeryInexact(string when, out string feedback) {
            feedback = $"Parsing `{when}`:\n";

            if (DateTime.TryParse(when, out DateTime result) == true) {
                feedback += $"\tfound {result:u} using DateTime.TryParse\n";
                return result;
            }

            Regex dateCommaStartDashEnd = new Regex(@"^(?<day>.*),\s*?(?<start>\d\d:\d\d)\s.*?(?<end>\d\d:\d\d).*$");
            Match match = dateCommaStartDashEnd.Match(when);
            if (match.Success == true) {
                bool dayGroup = match.Groups.TryGetValue("day", out Group? day);
                bool startGroup = match.Groups.TryGetValue("start", out Group? start);
                bool endGroup = match.Groups.TryGetValue("end", out Group? end);
                feedback += $"\tFound using Regex `{dateCommaStartDashEnd}`, day `{day}` ({dayGroup}), start `{start}` ({startGroup}), end `{end}` ({endGroup})\n";

                if (day != null && DateTime.TryParse(day.Value, out DateTime dayValue) == true) {
                    feedback += $"\tParsed `{day.Value}` to {dayValue:u}\n";
                } else {
                    feedback += $"\tFailed to parse `{day?.Value}` into a DateTime\n";
                }

                if (start != null && DateTime.TryParseExact(start.Value, "", null, System.Globalization.DateTimeStyles.AssumeUniversal, out DateTime startValue) == true) {
                    feedback += $"Parsed `{start.Value}` to {startValue:u}\n";
                } else {
                    feedback += $"Failed to parse `{start?.Value}` into a DateTime\n";
                }

            } else {
                feedback += $"\tdoes not match pattern `{dateCommaStartDashEnd}`\n";
            }

            return null;
        }

    }
}
