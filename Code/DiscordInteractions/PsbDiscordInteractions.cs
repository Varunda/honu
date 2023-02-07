using DSharpPlus;
using DSharpPlus.Entities;
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

            PsbReservation res = new();

            List<string> errors = new();

            string timefeedback = "";

            string feedback = $"Parsed message:\n";

            List<string> lines = msg.Content.Split("\n").ToList();
            foreach (string line in lines) {
                _Logger.LogDebug($"line: '{line}'");

                List<string> parts = line.Split(":").ToList();

                if (parts.Count < 2) {
                    errors.Add($"Line `{line}`, failed to split on ':', has {parts.Count}");
                    continue;
                }

                string field = parts[0].Trim().ToLower();
                string value = parts[1].Trim();
                string v = string.Join(":", parts.ToArray()[1..]).Trim();

                if (field.StartsWith("outfit") || field.StartsWith("team") || field.StartsWith("group")) {
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
                        errors.Add($"Failed to parse `{value}` to a valid number");
                    }
                } else if (field.StartsWith("rep") == true) {
                    feedback += $"Line `{line}` as rep line\n";

                    List<PsbOvOContact> ovo = await _ContactRepository.GetOvOContacts();
                    List<DiscordUser> users = msg.MentionedUsers.ToList();

                    foreach (DiscordUser user in users) {
                        PsbOvOContact? contact = ovo.FirstOrDefault(iter => iter.DiscordID == user.Id);
                        if (contact == null) {
                            errors.Add($"{user.GetPing()} is not an OvO rep!");
                        } else {
                            feedback += $"\tFound OvO contact for {user.GetPing()}: {contact.Group}\n";
                            res.Contacts.Add(contact);
                        }
                    }
                } else if (field.StartsWith("date and time") || field == "time" || field == "date" || field == "when" || field.StartsWith("date/time")) {
                    feedback += $"Line `{line}` as when\n";

                    (DateTime? r, DateTime? r2) = ParseVeryInexact(v, out timefeedback);

                    feedback += timefeedback;

                    if (r != null && r2 != null) {
                        feedback += $"\tConverted '{v}' into {r:u} - {r2:u}\n";
                        res.Start = r.Value;
                        res.End = r2.Value;
                    } else {
                        errors.Add($"Failed to convert '{v}' into a valid start and end: >>>{timefeedback}");
                    }

                } else if (field.StartsWith("base")) {
                    feedback += $"Line `{line}` as bases\n";

                    if (v.Trim().ToLower().StartsWith("any")) {
                        continue;
                    }

                    List<string> bases = v.ToLower().Split(",").Select(iter => iter.Trim()).ToList();
                    List<PsFacility> facilities = await _FacilityRepository.GetAll();

                    foreach (string baseName in bases) {
                        List<PsFacility> possibleBases = new();

                        foreach (PsFacility fac in facilities) {
                            string facName = $"{fac.Name} {fac.TypeName}".ToLower();
                            if (facName.StartsWith(baseName) == true) {
                                possibleBases.Add(fac);
                            }
                        }

                        if (possibleBases.Count == 0) {
                            errors.Add($"Failed to find base `{baseName}`");
                        } else if (possibleBases.Count > 1) {
                            errors.Add($"Ambigious base name `{baseName}`: {string.Join(", ", possibleBases.Select(iter => iter.Name))}");
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

            // misc errors
            if (res.Contacts.Count == 0) {
                errors.Add($"0 contacts were given in this reservation");
            } else {
                errors.AddRange(await CheckReps(res));
            }
            if (res.Outfits.Count == 0) { errors.Add($"0 groups were given in this reservation"); }
            if (res.End <= res.Start) { errors.Add($"Cannot have a reservation end before it starts (this may be a parsing error!)"); }

            DiscordEmbedBuilder builder = new();
            builder.Title = $"Reservation";

            if (errors.Count > 0) {
                builder.Color = DiscordColor.Red;
                builder.Description = $"**Reservation parsed with errors:**\n{string.Join("\n", errors.Select(iter => $"- {iter}"))}";
            } else {
                builder.Color = DiscordColor.HotPink;
                builder.Description = $"Reservation parsed successfully, but this does not mean the information is correct! Double check it!";
            }

            if (res.Outfits.Count > 0) {
                builder.AddField("Groups in reservation", string.Join(", ", res.Outfits));
            } else {
                builder.AddField("Groups in reservation", "**missing**");
            }
            builder.AddField("Accounts requested", $"{res.Accounts}");
            builder.AddField("Start time", $"`{res.Start:u}` ({res.Start.GetDiscordFullTimestamp()} - {res.Start.GetDiscordRelativeTimestamp()})");
            builder.AddField("End time", $"`{res.End:u}` ({res.End.GetDiscordFullTimestamp()} - {res.End.GetDiscordRelativeTimestamp()})");
            if (res.Bases.Count > 0) {
                builder.AddField("Bases", string.Join(", ", res.Bases.Select(iter => iter.Name)));
            }
            if (res.Details.Length > 0) {
                builder.AddField("Details", res.Details);
            }

            if (debug == true) {
                builder.Description += "\n\n" + feedback;
                if (builder.Description.Length > 2000) {
                    builder.Description = builder.Description[..1994] + "...";
                }
            }

            await ctx.EditResponseEmbed(builder);
        }

        private async Task<List<string>> CheckReps(PsbReservation res) {
            List<string> errors = new();

            if (res.Contacts.Count == 0) {
                errors.Add($"given 0 contacts");
                return errors;
            }

            List<string> groups = new(res.Outfits);
            List<PsbOvOContact> contacts = new(res.Contacts);

            List<PsbOvOContact> allContacts = await _ContactRepository.GetOvOContacts();

            foreach (string group in groups) {
                _Logger.LogTrace($"finding rep for '{group}'");

                List<PsbOvOContact> groupContacts = contacts.Where(iter => iter.Group.Trim().ToLower() == group.ToLower().Trim()).ToList();
                _Logger.LogTrace($"found {groupContacts.Count} contacts for {group}: {string.Join(", ", groupContacts.Select(iter => iter.DiscordID))}");

                if (groupContacts.Count == 0) {
                    errors.Add($"No contact for {group} found");
                    continue;
                }
            }

            foreach (PsbOvOContact contact in contacts) {
                _Logger.LogTrace($"Checking if {contact.DiscordID} is in a group in the reservation");

                List<string> contactGroups = groups.Where(iter => iter.ToLower().Trim() == contact.Group.ToLower().Trim()).ToList();
                _Logger.LogTrace($"{contact.DiscordID} is in these groups: {string.Join(", ", contactGroups)}");

                if (contactGroups.Count == 0) {
                    errors.Add($"<@{contact.DiscordID}> is not a rep for any of the groups in this reservation");
                }
            }

            return errors;
        }

        /// <summary>
        ///     this code is awful
        /// </summary>
        /// <param name="when"></param>
        /// <param name="feedback"></param>
        /// <returns></returns>
        public static (DateTime? start, DateTime? end) ParseVeryInexact(string when, out string feedback) {
            when = when.Replace(".", "").Replace(",", "").Replace("(", "").Replace(")", "");
            feedback = $"Parsing `{when}`:\n";

            string[] dayFormats = new string[] {
                "YYYY-MM-DD",
                "dddd MMMM d",
                "dddd MMM d",
            };

            string[] timeFormats = new string[] {
                "HH:mm",
                "H:mm",
                "HH",
            };

            DateTime? startDay = null;
            DateTime? endDay = null;

            // yes, assume local is correct, idk why
            DateTimeStyles style = DateTimeStyles.AllowInnerWhite | DateTimeStyles.AllowTrailingWhite 
                | DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal;

            string[] regexs = new string[] {
                @"^(?<day>.*?\d{1,2}).*?(?<start>\d{1,2}(:\d\d)?)\s?.*?(?<end>\d{1,2}(:\d\d)?).*$",
                @"^(?<day>\d{4}-\d\d-\d\d).*?(?<start>\d{1,2}(:\d\d)?)\s?.*?(?<end>\d{1,2}(:\d\d)?).*$",
            };

            foreach (string reg in regexs) {
                Regex r = new Regex(reg);
                Match match = r.Match(when);

                if (match.Success == true) {
                    bool dayGroup = match.Groups.TryGetValue("day", out Group? day);
                    bool startGroup = match.Groups.TryGetValue("start", out Group? start);
                    bool endGroup = match.Groups.TryGetValue("end", out Group? end);
                    feedback += $"Found using Regex `{reg}`, day `{day}`, start `{start}`, end `{end}`\n";

                    if (day != null) {
                        if (DateTime.TryParse(day.Value, null, style, out DateTime d) == true) {
                            startDay = DateTime.SpecifyKind(d, DateTimeKind.Utc);
                            endDay = new DateTime(startDay.Value.Ticks, DateTimeKind.Utc);
                            feedback += $"\tParsed date of {day.Value} to {startDay:u}\n";
                        }

                        if (startDay != null) {
                            foreach (string format in dayFormats) {
                                if (DateTime.TryParseExact(day.Value, format, null, style, out DateTime iter) == true) {
                                    feedback += $"\tParsed date of `{day.Value}` using format `{format}` => {iter:u}\n";
                                    startDay = DateTime.SpecifyKind(iter, DateTimeKind.Utc);
                                    endDay = new DateTime(startDay.Value.Ticks, DateTimeKind.Utc);
                                    break;
                                }
                            }
                        }
                    }

                    if (startDay == null || endDay == null) {
                        feedback += $"Not parsing time, no day\n";
                        continue;
                    }

                    bool parsedStart = false;
                    if (start != null) {
                        foreach (string format in timeFormats) {
                            if (DateTime.TryParseExact(start.Value.PadLeft(2, '0'), format, null, style, out DateTime iter) == true) {
                                parsedStart = true;
                                iter = DateTime.SpecifyKind(iter, DateTimeKind.Utc);
                                feedback += $"\tParsed time of `{start.Value}` using format `{format}` => {iter:u}\n";

                                startDay = startDay.Value.AddHours(iter.Hour);
                                startDay = startDay.Value.AddMinutes(iter.Minute);
                                break;
                            }
                        }
                    }

                    if (parsedStart == false) {
                        feedback += $"failed to parse a start time\n";
                        startDay = endDay = null;
                        continue;
                    }

                    bool parsedEnd = false;
                    if (end != null) {
                        foreach (string format in timeFormats) {
                            if (DateTime.TryParseExact(end.Value.PadLeft(2, '0'), format, null, style, out DateTime iter) == true) {
                                parsedEnd = true;
                                iter = DateTime.SpecifyKind(iter, DateTimeKind.Utc);
                                feedback += $"\tParsed time of `{end.Value}` using format `{format}` => {iter:u}\n";

                                endDay = endDay.Value.AddHours(iter.Hour);
                                endDay = endDay.Value.AddMinutes(iter.Minute);

                                // for reservation that go over a utc day, they'll be input as @ 23 - 01 UTC
                                // which would be the previous day, so we take one away from the day
                                if (startDay != null && endDay <= startDay) {
                                    endDay = endDay.Value.AddDays(1);
                                }

                                break;
                            }
                        }
                    }

                    if (parsedEnd == false) {
                        feedback += $"failed to parse end time\n";
                        startDay = endDay = null;
                        continue;
                    }

                    break;
                } else {
                    feedback += $"no match using Regex `{reg}`\n";
                }
            }

            return (startDay, endDay);
        }

    }
}
