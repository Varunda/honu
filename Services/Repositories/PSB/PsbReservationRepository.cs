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
using watchtower.Services.Db;
using watchtower.Services.Queues;

namespace watchtower.Services.Repositories.PSB {

    public class PsbReservationRepository {

        private readonly ILogger<PsbReservationRepository> _Logger;
        private readonly PsbContactSheetRepository _ContactRepository;
        private readonly FacilityRepository _FacilityRepository;
        private readonly PsbCalendarRepository _CalendarRepository;
        private readonly PsbParsedReservationDbStore _MetadataDb;
        private readonly MapRepository _MapRepository;

        private readonly DiscordMessageQueue _DiscordMessageQueue;
        private readonly IOptions<DiscordOptions> _DiscordOptions;

        /// <summary>
        ///     Is automated base bookings enabled?
        /// </summary>
        public static bool BookingEnabled = true;

        /// <summary>
        ///     Is automated account sheet creation enabled?
        /// </summary>
        public static bool AccountEnabled = true;

        /// <summary>
        ///     Possible time formats that can be used
        /// </summary>
        private static readonly string[] TIME_FORMATS = new string[] {
            "HH:mm",
            "H:mm",
            "HH",
        };

        /// <summary>
        ///     possible formats a day is parsed from
        /// </summary>
        private static readonly string[] DAY_FORMATS = new string[] {
            "YYYY-MM-DD",
            "dddd MMMM d",
            "dddd MMM d",
        };

        public PsbReservationRepository(ILogger<PsbReservationRepository> logger,
            PsbContactSheetRepository contactRepository, FacilityRepository facilityRepository,
            IOptions<DiscordOptions> discordOptions, DiscordMessageQueue discordMessageQueue,
            PsbCalendarRepository calendarRepository, PsbParsedReservationDbStore metadataDb,
            MapRepository mapRepository) {

            _Logger = logger;

            _ContactRepository = contactRepository;
            _FacilityRepository = facilityRepository;
            _DiscordOptions = discordOptions;
            _DiscordMessageQueue = discordMessageQueue;
            _CalendarRepository = calendarRepository;
            _MetadataDb = metadataDb;
            _MapRepository = mapRepository;
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
            parsed.MessageLink = message.JumpLink.ToString();
            if (message.Author != null) {
                parsed.PosterUserId = message.Author.Id;
            }

            List<string> errors = new();
            string timefeedback = "";
            string feedback = $"Parsed message:\n";

            _Logger.LogDebug($"{message.MentionedUsers.Count} mentioned users: {string.Join(", ", message.MentionedUsers.Select(iter => iter.Id))}");

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

                    List<string> outfits = value.Split(new string[] { ",", "&", "/", ";" }, StringSplitOptions.None).ToList();
                    res.Outfits = outfits.Select(iter => iter.Trim().ToLower()).ToList();

                    feedback += $"\tOutfits: {string.Join(", ", outfits)}\n";
                } else if (field == "accounts" || field == "number of accounts") {
                    feedback += $"Line `{line}` as account number\n";

                    if (value.ToLower() == "none") {
                        feedback += $"\tAccounts: none\n";
                        continue;
                    }

                    if (int.TryParse(value, out int accountCount) == true) {
                        res.Accounts = accountCount;
                        feedback += $"\tAccounts: {res.Accounts}\n";
                    } else {
                        errors.Add($"Failed to parse `{value}` to a valid number");
                    }
                } else if (field.StartsWith("rep") == true) {
                    feedback += $"Line `{line}` as rep line\n";

                    List<PsbOvOContact> ovo = await _ContactRepository.GetOvOContacts();
                    List<PsbPracticeContact> practice = await _ContactRepository.GetPracticeContacts();

                    // matches <@1234> or <@68043274232803328>, which are the text of a discord @
                    Regex mentions = new("<@(?<user>\\d*)>");
                    MatchCollection mentionMatches = mentions.Matches(message.Content);

                    foreach (Match match in mentionMatches) {
                        if (match.Groups.TryGetValue("user", out Group? userID) == false) {
                            errors.Add($"failed to get user from {match.Value}");
                            continue;
                        }

                        if (ulong.TryParse(userID.Value, out ulong id) == false) {
                            errors.Add($"failed to pser {userID.Value} to a valid UInt64");
                            continue;
                        }

                        PsbOvOContact? contact = ovo.FirstOrDefault(iter => iter.DiscordID == id);
                        if (contact == null) {
                            errors.Add($"failed to find a contact for <@{id}>: not a rep, or not a user");
                        } else {
                            feedback += $"\tFound OvO contact for <@{id}>: {string.Join("/", contact.Groups)}\n";
                            res.Contacts.Add(contact);
                        }
                    }
                } else if (field.Contains("date") || field.Contains("time") || field.Contains("when") || field.Contains("day")) {
                    feedback += $"Line `{line}` as when\n";

                    (DateTime? r, DateTime? r2) = ParseVeryInexact(v, out timefeedback);

                    feedback += timefeedback;

                    if (r != null && r2 != null) {
                        res.Start = r.Value;
                        res.End = r2.Value;

                        feedback += $"\tConverted '{v}' into {r:u} - {r2:u}\n";
                    } else {
                        errors.Add($"Failed to convert '{v}' into a valid start and end: >>>{timefeedback}");
                    }

                } else if (field.StartsWith("base")) {
                    feedback += $"Line `{line}` as bases\n";

                    string vLower = v.Trim().ToLower();
                    if (vLower.Length <= 1 || vLower.StartsWith("any") || vLower.StartsWith("none")) {
                        continue;
                    }

                    BaseParseResult bases = await ParseBases(parsed, string.Join(":", parts[1..]).Split(",").ToList());

                    res.Bases = bases.Bookings;
                    errors.AddRange(bases.Errors);
                    feedback += bases.Feedback;
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

            if (res.Bases.Count > 0) {
                errors.AddRange(await CheckBaseBookings(res));
            }

            parsed.Errors = errors;
            parsed.ContactErrors = repErrors;
            parsed.DebugText = feedback;
            parsed.TimeFeedback = timefeedback;
            parsed.Metadata = await _MetadataDb.GetOrCreate(message.Id);

            return parsed;
        }

        /// <summary>
        ///     Create a discord message builder that represents the information in a <see cref="ParsedPsbReservation"/>
        /// </summary>
        /// <param name="parsed">Reservation to build the message builder for</param>
        /// <param name="debug">If the debug output will be included</param>
        /// <returns>
        ///     A new <see cref="DiscordMessageBuilder"/> that contains all of the embeds and components
        ///     necessary to manage a reservation
        /// </returns>
        public DiscordMessageBuilder CreateMessage(ParsedPsbReservation parsed, bool debug) {
            DiscordMessageBuilder builder = new();
            builder.AddEmbed(parsed.Build(debug));

            List<DiscordComponent> comps = new();

            comps.Add(PsbButtonCommands.REFRESH_RESERVATION(parsed.MessageId));

            if (PsbReservationRepository.BookingEnabled == true) {
                DiscordButtonComponent bookingBtn = PsbButtonCommands.APPROVE_BOOKING(parsed.MessageId);
                if (parsed.Metadata.BookingApprovedById != null || parsed.Errors.Count != 0 || parsed.Reservation.Bases.Count == 0) {
                    bookingBtn.Disable();
                }
                comps.Add(bookingBtn);
            }

            if (PsbReservationRepository.AccountEnabled == true) {
                DiscordButtonComponent accountBtn = PsbButtonCommands.APPROVE_ACCOUNTS(parsed.MessageId);
                if (parsed.Metadata.AccountSheetApprovedById != null || parsed.Errors.Count != 0 || parsed.Reservation.Accounts == 0) {
                    accountBtn.Disable();
                }
                comps.Add(accountBtn);
            }

            comps.Add(PsbButtonCommands.RESET_RESERVATION(parsed.MessageId));

            builder.AddComponents(comps);

            return builder;
        }

        class BaseParseResult {

            public List<PsbBaseBooking> Bookings { get; set; } = new();

            public List<string> Errors { get; set; } = new();

            public string Feedback { get; set; } = "";

        }

        private async Task<BaseParseResult> ParseBases(ParsedPsbReservation parsed, List<string> names) {
            BaseParseResult result = new();
            List<PsbBaseBooking> bookings = new();

            bool providedTime = false;

            // for each name
            // find the base
            // and if a time is given, use that

            Regex r = new(@"(?<base>.*)\((?<start>\d{1,2}(:\d\d)?)\s*(-\s*(?<end>\d{1,2}(:\d\d)?)?)");
            List<PsFacility> facilities = await _FacilityRepository.GetAll();

            foreach (string name in names) {
                Match match = r.Match(name);

                Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                string baseName = rgx.Replace(name.Trim().ToLower(), "");
                //string baseName = name.Trim().ToLower();

                _Logger.LogDebug($"matching '{name}' => '{baseName}'");

                if (name.Trim().Length <= 2) {
                    result.Errors.Add($"3 or more characters must be provided. Only got {name.Trim().Length} from `{name.Trim()}`");
                    continue;
                }

                if (match.Success == true) {
                    _Logger.LogDebug($"provided time {name}");
                    providedTime = true;
                    _ = match.Groups.TryGetValue("base", out Group? baseGroup);
                    if (baseGroup != null) {
                        baseName = baseGroup.Value.Trim().ToLower();
                    }
                }

                List<PsFacility> possibleBases = await _FacilityRepository.SearchByName(baseName);

                if (possibleBases.Count == 0) {
                    result.Errors.Add($"Failed to find base `{baseName}`");
                    continue;
                } else if (possibleBases.Count > 1) {
                    result.Errors.Add($"Ambigious base name `{baseName}`: {string.Join(", ", possibleBases.Select(iter => iter.Name))}");
                    continue;
                }

                PsbBaseBooking book = new();
                book.Facilities = new List<PsFacility>() { possibleBases[0] };

                _Logger.LogDebug($"found {possibleBases[0].Name}/{possibleBases[0].FacilityID}");

                if (providedTime == true) {
                    bool hasStart = match.Groups.TryGetValue("start", out Group? startGroup);
                    bool hasEnd = match.Groups.TryGetValue("end", out Group? endGroup);

                    if (hasStart == true && hasEnd == true && startGroup != null && endGroup != null) {
                        DateTime? startTime = ParseTimeInexact(startGroup.Value, parsed.Reservation.Start.Date, out string sf);
                        if (startTime == null) {
                            result.Errors.Add($"Failed to parse `{startGroup.Value}` to a valid time");
                        } else {
                            _Logger.LogDebug($"{startGroup.Value} => {startTime}");
                            if (startTime.Value < parsed.Reservation.Start) {
                                book.Start = startTime.Value.AddDays(1);
                            } else {
                                book.Start = startTime.Value;
                            }
                        }

                        DateTime? endTime = ParseTimeInexact(endGroup.Value, parsed.Reservation.Start.Date, out string ef);
                        if (endTime == null) {
                            result.Errors.Add($"Failed to parse `{endGroup.Value}` to a valid time");
                        } else {
                            _Logger.LogDebug($"{endGroup.Value} => {endTime}");
                            if (endTime.Value < parsed.Reservation.Start) {
                                book.End = endTime.Value.AddDays(1);
                            } else {
                                book.End = endTime.Value;
                            }
                        }
                    } else {
                        result.Errors.Add($"Failed to get start and end values from `{name}`");
                    }
                }

                bookings.Add(book);
            }

            if (providedTime == false && bookings.Count > 0) {
                int baseCount = bookings.Count;
                TimeSpan reservationDuration = parsed.Reservation.End - parsed.Reservation.Start;
                TimeSpan per = reservationDuration / baseCount;

                bool isLanesmash = parsed.Reservation.Details.Contains("Lanesmash", StringComparison.InvariantCultureIgnoreCase);
                _Logger.LogDebug($"isLanesmash: {isLanesmash} / {parsed.Reservation.Details}");

                // if each base would take up less than 30 minutes, assume they wanted the bases for the whole duration
                if (isLanesmash == true || (per < TimeSpan.FromMinutes(30) && bookings.Count > 1)) {
                    _Logger.LogDebug($"Reservation has {baseCount} bases, but only {reservationDuration.TotalHours}, assuming all bases for full duration");

                    // put all the bases into one booking
                    for (int i = 1; i < bookings.Count; ++i) {
                        bookings[0].Facilities.AddRange(bookings[i].Facilities);
                    }

                    bookings[0].Start = parsed.Reservation.Start;
                    bookings[0].End = parsed.Reservation.End;
                    bookings = bookings.Take(1).ToList();
                } else {
                    _Logger.LogDebug($"the reservation lasts: {reservationDuration}, have {baseCount} bases booked, each will take up {per}");

                    for (int i = 0; i < bookings.Count; ++i) {
                        bookings[i].Start = parsed.Reservation.Start + (per * i);
                        bookings[i].End = parsed.Reservation.Start + (per * (i + 1));
                    }
                }
            }

            result.Bookings = bookings;

            return result;
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

                List<PsbOvOContact> groupContacts = contacts.Where(iter => iter.Groups.Contains(group.ToLower().Trim())).ToList();
                _Logger.LogTrace($"found {groupContacts.Count} contacts for {group}: {string.Join(", ", groupContacts.Select(iter => iter.DiscordID))}");

                if (groupContacts.Count == 0) {
                    errors.Add($"No contact for {group} found");
                    continue;
                }
            }

            foreach (PsbOvOContact contact in contacts) {
                _Logger.LogTrace($"Checking if {contact.DiscordID} is in a group in the reservation");

                bool contactHasGroup = false;
                foreach (string group in contact.Groups) {
                    if (groups.Contains(group)) {
                        contactHasGroup = true;
                        break;
                    }
                }

                if (contactHasGroup == false) {
                    errors.Add($"<@{contact.DiscordID}> is not a rep for any of the groups in this reservation");
                }
            }

            return errors;
        }

        /// <summary>
        ///     check if the reservation has any conflicting base bookings, or if 
        /// </summary>
        /// <param name="res">reservation to check against</param>
        /// <returns>
        ///     a list of conflicting bookings
        /// </returns>
        private async Task<List<string>> CheckBaseBookings(PsbReservation res) {
            List<string> errors = new();

            // load all facility links into a dictionary for those quicker look ups!
            List<PsFacilityLink> links = await _MapRepository.GetFacilityLinks();
            Dictionary<int, List<PsFacilityLink>> linksDict = new();
            foreach (PsFacilityLink link in links) {
                if (linksDict.ContainsKey(link.FacilityA) == false) {
                    linksDict.Add(link.FacilityA, new List<PsFacilityLink>());
                }

                linksDict[link.FacilityA].Add(link);
            }

            Dictionary<int, PsFacility> facs = (await _MapRepository.GetFacilities()).ToDictionary(iter => iter.FacilityID);

            try {
                List<PsbCalendarEntry> entries = await _CalendarRepository.GetAll();

                foreach (PsbBaseBooking booking in res.Bases) {

                    // get the bases of this booking, and the bases that are adjacent to it
                    List<PsFacility> bookingFacilities = new(booking.Facilities);
                    foreach (PsFacility fac in booking.Facilities) {
                        if (linksDict.TryGetValue(fac.FacilityID, out List<PsFacilityLink>? baseLinks) == false) {
                            continue;
                        }

                        foreach (PsFacilityLink link in baseLinks) {
                            if (facs.TryGetValue(link.FacilityB, out PsFacility? adjacentFac) == false) {
                                _Logger.LogError($"Failed to find facility {link.FacilityB} which is linked to facility {link.FacilityA}");
                                continue;
                            }
                            bookingFacilities.Add(adjacentFac);
                        }
                    }

                    _Logger.LogDebug($"Checking for no conflict on these bases: {string.Join(", ", bookingFacilities.Select(iter => iter.Name))}");

                    foreach (PsbCalendarEntry iter in entries) {
                        if (iter.Valid == false) {
                            continue;
                        }

                        // if this base is already booked by the same group who wants to book the base, this reservation is fine
                        bool broke = false;
                        foreach (string outfit in res.Outfits) {
                            if (iter.Groups.Select(iter => iter.ToLower()).Contains(outfit.ToLower())) {
                                broke = true;
                                break;
                            }
                        }

                        if (broke == true) {
                            break;
                        }

                        List<PsFacility> facilities = iter.Bases.SelectMany(iter => iter.PossibleBases).ToList();

                        // for each facility in the booking, make sure it's not already booked
                        foreach (PsFacility fac in bookingFacilities) {
                            if (facilities.FirstOrDefault(iter => iter.FacilityID == fac.FacilityID) == null) {
                                continue;
                            }

                            _Logger.LogDebug($"Checking if {iter.Start:u} to {iter.End:u} at {string.Join(", ", facilities.Select(iter => iter.Name))} overlaps with {booking.Start:u} - {booking.End:u}");

                            if ((booking.Start >= iter.Start && booking.Start <= iter.End)
                                || (booking.End >= iter.Start && booking.End <= iter.End)
                                || (booking.Start <= iter.Start && booking.End >= iter.End)) {

                                string err = $"{fac.Name} ";

                                if (booking.Facilities.FirstOrDefault(iter => fac.FacilityID == iter.FacilityID) == null) {
                                    err += $"(adjacent to a booked base)";
                                }

                                err += $" is in use by {string.Join(", ", iter.Groups)} from {iter.Start:u} to {iter.End:u}";

                                errors.Add(err);
                                broke = true;
                                break;
                            }
                        }

                        // if there was already a conflict, no need to check further, there was one
                        if (broke == true) {
                            break;
                        }
                    }
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
                @"^(?<day>\d{4}/\d\d/\d\d).*?(?<start>\d{1,2}(:\d\d)?)\s?.*?(?<end>\d{1,2}(:\d\d)?).*$", // yyyy/mm/dd hh:mm
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

            if (startDay != null && endDay != null && endDay < startDay) {
                endDay = endDay.Value.AddDays(1);
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
