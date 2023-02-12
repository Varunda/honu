using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using watchtower.Code.DiscordInteractions;
using watchtower.Code.ExtensionMethods;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.Discord;
using watchtower.Models.PSB;
using watchtower.Services.Queues;

namespace watchtower.Services.Repositories.PSB {

    public class PsbReservationRepository {

        private readonly ILogger<PsbReservationRepository> _Logger;
        private readonly PsbContactSheetRepository _ContactRepository;
        private readonly FacilityRepository _FacilityRepository;
        private readonly PsbCalendarRepository _CalendarRepository;

        private readonly DiscordMessageQueue _DiscordMessageQueue;
        private readonly IOptions<DiscordOptions> _DiscordOptions;

        private static readonly string[] TIME_FORMATS = new string[] {
            "HH:mm",
            "H:mm",
            "HH",
        };


        private static readonly string[] DAY_FORMATS = new string[] {
            "YYYY-MM-DD",
            "dddd MMMM d",
            "dddd MMM d",
        };

        public PsbReservationRepository(ILogger<PsbReservationRepository> logger,
            PsbContactSheetRepository contactRepository, FacilityRepository facilityRepository,
            IOptions<DiscordOptions> discordOptions, DiscordMessageQueue discordMessageQueue,
            PsbCalendarRepository calendarRepository) {

            _Logger = logger;

            _ContactRepository = contactRepository;
            _FacilityRepository = facilityRepository;
            _DiscordOptions = discordOptions;
            _DiscordMessageQueue = discordMessageQueue;
            _CalendarRepository = calendarRepository;
        }

        /// <summary>
        ///     Parse a Discord message into a <see cref="ParsedPsbReservation"/>
        /// </summary>
        /// <param name="message">Discord message to parse</param>
        /// <returns>
        ///     A parsed <see cref="ParsedPsbReservation"/>
        /// </returns>
        public async Task<ParsedPsbReservation> Parse(DiscordMessage message) {
            PsbReservation res = new();

            ParsedPsbReservation parsed = new();
            parsed.Reservation = res;
            parsed.Input = message.Content;
            parsed.MessageId = message.Id;
            parsed.PosterUserId = message.Author.Id;
            parsed.MessageLink = message.JumpLink.ToString();

            List<string> errors = new();
            string timefeedback = "";
            string feedback = $"Parsed message:\n";

            List<string> lines = message.Content.Split("\n").ToList();
            foreach (string line in lines) {
                _Logger.LogDebug($"line: '{line}'");

                string[] parts = line.Split(":");

                if (parts.Length < 2) {
                    errors.Add($"Line `{line}`, failed to split on ':', has {parts.Length}");
                    continue;
                }

                string field = parts[0].Trim().ToLower();
                string value = parts[1].Trim();
                string v = string.Join(":", parts[1..]).Trim();

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
                    List<DiscordUser> users = message.MentionedUsers.ToList();

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
                        res.Start = r.Value;
                        res.End = r2.Value;

                        if (res.End < res.Start) {
                            feedback += $"\tend is before start, adding a day\n";
                            res.End = res.End.AddDays(1);
                        }

                        feedback += $"\tConverted '{v}' into {r:u} - {r2:u}\n";
                    } else {
                        errors.Add($"Failed to convert '{v}' into a valid start and end: >>>{timefeedback}");
                    }

                } else if (field.StartsWith("base")) {
                    feedback += $"Line `{line}` as bases\n";

                    if (v.Trim().ToLower().StartsWith("any")) {
                        continue;
                    }

                    List<string> bases = v.ToLower().Split(",").Select(iter => iter.Trim()).ToList();
                } else if (field == "details") {
                    feedback += $"Line `{line}` as details\n";
                    res.Details = value.Trim();
                } else {
                    feedback += $"Unchecked field '{field}'\n";
                }
            }

            List<string> repErrors = new();

            // misc errors
            if (res.Contacts.Count == 0) {
                errors.Add($"0 contacts were given in this reservation");
            } else {
                repErrors = await CheckReps(res);
                if (res.Accounts > 0) {
                    errors.AddRange(repErrors);
                }
            }
            if (res.Outfits.Count == 0) { errors.Add($"0 groups were given in this reservation"); }
            if (res.End <= res.Start) { errors.Add($"Cannot have a reservation end before it starts (this may be a parsing error!)"); }

            parsed.Errors = errors;
            parsed.ContactErrors = repErrors;
            parsed.DebugText = feedback;
            parsed.TimeFeedback = timefeedback;

            return parsed;
        }

        /// <summary>
        ///     Send a parsed reservation to the parsed reservation channel ID
        /// </summary>
        /// <param name="parsed">Parsed reservation</param>
        /// <param name="debug">Will debug output be included?</param>
        public void Send(ParsedPsbReservation parsed, bool debug) {
            HonuDiscordMessage msg = new();
            msg.ChannelID = _DiscordOptions.Value.ParsedChannelId;
            msg.GuildID = _DiscordOptions.Value.GuildId;

            msg.Embeds.Add(parsed.Build(debug));
            msg.Components.Add(PsbButtonCommands.REFRESH_RESERVATION(parsed.MessageId));

            _DiscordMessageQueue.Queue(msg);
        }

        public async Task<List<PsbBaseBooking>> ParseBases(ParsedPsbReservation parsed, List<string> names) {
            List<PsbBaseBooking> bookings = new();

            bool providedTime = false;

            // for each name
            // find the base
            // and if a time is given, use that

            Regex r = new(@"(?<base>.*)\((?<start>\d{1,2}(:\d\d)?)\s*(-\s*(?<end>\d{1,2}(:\d\d)?)?)");
            List<PsFacility> facilities = await _FacilityRepository.GetAll();

            foreach (string name in names) {
                Match match = r.Match(name);

                string baseName = name;

                if (match.Success == true) {
                    providedTime = true;
                    _ = match.Groups.TryGetValue("base", out Group? baseGroup);
                    if (baseGroup != null) {
                        baseName = baseGroup.Value.Trim().ToLower();
                    }
                }

                List<PsFacility> possibleBases = new();

                foreach (PsFacility fac in facilities) {
                    // if the name of a base is a perfect match, then they probably meant that one
                    if (fac.Name.ToLower() == baseName) {
                        possibleBases.Add(fac);
                        break;
                    }

                    string facName = $"{fac.Name} {fac.TypeName}".ToLower();
                    if (facName.StartsWith(baseName) == true) {
                        possibleBases.Add(fac);
                    }
                }

                if (possibleBases.Count == 0) {
                    parsed.Errors.Add($"Failed to find base `{baseName}`");
                    continue;
                } else if (possibleBases.Count > 1) {
                    parsed.Errors.Add($"Ambigious base name `{baseName}`: {string.Join(", ", possibleBases.Select(iter => iter.Name))}");
                    continue;
                }

                PsbBaseBooking book = new();
                book.Facility = possibleBases[0];
                book.FacilityID = book.Facility.FacilityID;

                if (providedTime == true) {

                }

                //feedback += $"\tBase `{baseName}` found {fac.Name}/{fac.FacilityID}\n";
                //bookings.Add(possibleBases[0]);
            }

            return bookings;
        }

        /// <summary>
        ///     Check if the reps listed on a reservation are valid
        /// </summary>
        /// <param name="res">Reservation to check</param>
        /// <returns>A list of errors. A list with a length of 0 means no errors</returns>
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

        private async Task<List<string>> CheckBaseBookings(PsbReservation res) {
            List<string> errors = new();

            try {
                List<PsbCalendarEntry> entries = await _CalendarRepository.GetAll();

                List<PsbCalendarEntry> colliding = entries.Where(iter => {
                    if (iter.Valid == false) {
                        return false;
                    }

                    return false;
                }).ToList();

                if (colliding.Count > 0) {
                    errors.AddRange(colliding.Select(iter => {
                        return $"";
                    }));
                }
            } catch (Exception ex) {
                errors.Add($"failed to check calendar: {ex.Message}");
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

            DateTime? startDay = null;
            DateTime? endDay = null;

            // yes, assume local is correct, idk why
            DateTimeStyles style = DateTimeStyles.AllowInnerWhite | DateTimeStyles.AllowTrailingWhite 
                | DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal;

            string[] regexs = new string[] {
                @"^(?<day>.*?\d{1,2}).*?(?<start>\d{1,2}(:\d\d)?)\s?.*?(?<end>\d{1,2}(:\d\d)?).*$", // Month Day - Time
                @"^(?<day>\d{4}-\d\d-\d\d).*?(?<start>\d{1,2}(:\d\d)?)\s?.*?(?<end>\d{1,2}(:\d\d)?).*$", // yyyy-mm-dd hh:mm
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
                            foreach (string format in DAY_FORMATS) {
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
                        startDay = ParseTimeInexact(start.Value, startDay.Value, out string f);
                        feedback += f;
                        parsedStart = startDay != null;
                    }

                    if (parsedStart == false) {
                        feedback += $"failed to parse a start time\n";
                        startDay = endDay = null;
                        continue;
                    }

                    bool parsedEnd = false;
                    if (end != null) {
                        endDay = ParseTimeInexact(end.Value, endDay.Value, out string f);
                        feedback += f;
                        parsedEnd = endDay != null;
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

        public static DateTime? ParseTimeInexact(string time, DateTime day, out string feedback) {
            feedback = "";

            DateTimeStyles style = DateTimeStyles.AllowInnerWhite | DateTimeStyles.AllowTrailingWhite 
                | DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal;

            foreach (string format in TIME_FORMATS) {
                if (DateTime.TryParseExact(time.PadLeft(2, '0'), format, null, style, out DateTime iter) == true) {
                    iter = DateTime.SpecifyKind(iter, DateTimeKind.Utc);
                    feedback += $"\tParsed time of `{time}` using format `{format}` => {iter:u}\n";

                    iter = day.AddHours(iter.Hour).AddMinutes(iter.Minute);

                    return iter;
                }
            }

            return null;
        }

    }
}
