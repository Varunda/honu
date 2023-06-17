using DSharpPlus.Entities;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.Discord;
using watchtower.Models.PSB;
using watchtower.Services.Queues;
using watchtower.Services.Repositories.Readers;

namespace watchtower.Services.Repositories.PSB {

    public class PsbCalendarRepository {

        private readonly ILogger<PsbCalendarRepository> _Logger;
        private readonly IMemoryCache _Cache;
        private readonly GDriveRepository _Drive;

        private readonly FacilityRepository _FacilityRepository;
        private readonly ISheetsReader<PsbCalendarEntry> _CalendarReader;
        private readonly DiscordMessageQueue _DiscordMessageQueue;

        private readonly IOptions<PsbDriveSettings> _Settings;
        private readonly IOptions<DiscordOptions> _DiscordOptions;

        public PsbCalendarRepository(ILogger<PsbCalendarRepository> logger, IMemoryCache cache,
            IOptions<PsbDriveSettings> settings, ISheetsReader<PsbCalendarEntry> calendarReader,
            FacilityRepository facilityRepository, GDriveRepository drive,
            DiscordMessageQueue discordMessageQueue, IOptions<DiscordOptions> discordOptions) {

            _Logger = logger;
            _Cache = cache;
            _Drive = drive;

            _CalendarReader = calendarReader;
            _FacilityRepository = facilityRepository;
            _DiscordMessageQueue = discordMessageQueue;

            _Settings = settings;
            _DiscordOptions = discordOptions;
        }

        /// <summary>
        ///     Load all the data from the calendar. This will include invalid entries (<see cref="PsbCalendarEntry.Valid"/> = <c>false</c>).
        ///     See remarks for more information
        /// </summary>
        /// <remarks>
        ///     A <see cref="PsbCalendarEntry"/> can be invalid if the data in the sheet is not correct. The data won't be correct if:
        ///     <ul>
        ///         <li>The row is a header for a day. In this case <see cref="PsbCalendarEntry.Error"/> will be empty</li>
        ///         <li>The row is missing a field. In this case <see cref="PsbCalendarEntry.Error"/> will contain more information</li>
        ///     </ul>
        /// </remarks>
        /// <returns>
        ///     A list of <see cref="PsbCalendarEntry"/>s from the Jaeger Events calendar
        /// </returns>
        public async Task<List<PsbCalendarEntry>> GetAll() {
            // https://developers.google.com/sheets/api/guides/concepts#cell
            SpreadsheetsResource.ValuesResource.GetRequest sheetR = _Drive.GetSheetService().Spreadsheets.Values.Get(_Settings.Value.CalendarFileId, "Current!A5:M");
            Google.Apis.Sheets.v4.Data.ValueRange res = await sheetR.ExecuteAsync();
            _Logger.LogDebug($"Loaded {res.Values.Count} rows from {_Settings.Value.CalendarFileId}");

            List<PsbCalendarEntry> entries = _CalendarReader.ReadList(res.Values).Where(iter => iter.Valid == true).ToList();

            List<PsFacility> facilities = await _FacilityRepository.GetAll();

            foreach (PsbCalendarEntry entry in entries) {
                foreach (string baseName in entry.BaseNames) {
                    PsbCalendarBaseEntry baseEntry = new();
                    baseEntry.Name = baseName;

                    bool isZone = false;
                    foreach (uint zoneID in Zone.StaticZones) {
                        if (Zone.GetName(zoneID).ToLower() != baseName.ToLower()) {
                            continue;
                        }

                        baseEntry.PossibleBases = facilities.Where(iter => iter.ZoneID == zoneID).ToList();

                        isZone = true;
                        break;
                    }

                    if (isZone == false) {
                        List<PsFacility> possible = facilities.Where(iter => iter.Name.ToLower().StartsWith(baseName.ToLower())).ToList();
                        baseEntry.PossibleBases = possible;
                    }

                    entry.Bases.Add(baseEntry);
                }
            }

            return entries;
        }

        /// <summary>
        ///     Insert a <see cref="PsbBaseBooking"/> to the calendar. If the booking spans over 2 days,
        ///     it is inserted as different bookings into the calendar
        /// </summary>
        /// <param name="reservation">Reservation the booking comes from. Used to build the info</param>
        /// <param name="booking">Booking being added to the calendar</param>
        public async Task Insert(ParsedPsbReservation reservation, PsbBaseBooking booking) {
            // if a booking is broken into 2 days, will need to do two inserts
            //      but, if a booking ends at midnight, don't add a 0 duration booking on the next day
            if (booking.Start.Date != booking.End.Date && booking.End.TimeOfDay != TimeSpan.FromHours(0)) {
                PsbBaseBooking day1 = new(booking);
                day1.End = day1.Start.Date.AddHours(23).AddMinutes(59);

                PsbBaseBooking day2 = new(booking);
                day2.Start = day2.End.Date;

                _Logger.LogDebug($"Booking between {booking.Start:u} and {booking.End:u} is split on days, splitting into two entries: "
                    + $"{day1.Start:u} - {day1.End:u} and {day2.Start:u} {day2.End:u}");

                await InsertInternal(reservation, day1);
                await InsertInternal(reservation, day2);
            } else {

                // if the booking isn't the same day, that means this booking ends at midnight.
                //      but we'd still like the reservation in the same day, so put the end at 23:59 instead
                if (booking.Start.Date != booking.End.Date) {
                    booking.End = booking.Start.Date.AddHours(23).AddMinutes(59);
                }

                await InsertInternal(reservation, booking);
            }
        }

        /// <summary>
        ///     Actually insert
        /// </summary>
        /// <param name="reservation"></param>
        /// <param name="booking"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        private async Task InsertInternal(ParsedPsbReservation reservation, PsbBaseBooking booking) {
            if (booking.Start.Date != booking.End.Date) {
                throw new ArgumentException($"A booking cannot span more than 1 day. Goes from {booking.Start:u} to {booking.End:u}");
            }

            Spreadsheet sheetFile = await _Drive.GetSheetService().Spreadsheets.Get(_Settings.Value.CalendarFileId).ExecuteAsync();

            Sheet? currentSheet = sheetFile.Sheets.FirstOrDefault(iter => iter.Properties.Title == "Current");
            if (currentSheet == null) {
                throw new ArgumentException($"Failed to find tab named 'Current' in file {_Settings.Value.CalendarFileId}. Make sure it exists!");
            }

            SpreadsheetsResource.ValuesResource.GetRequest sheetR = _Drive.GetSheetService().Spreadsheets.Values.Get(_Settings.Value.CalendarFileId, "Current!A5:M");
            ValueRange res = await sheetR.ExecuteAsync();

            int? rowToInsert = GetInsertRow(booking, res);
            if (rowToInsert == null) {
                throw new Exception($"Failed to find the row to insert booking at. Booking starts at {booking.Start:u}");
            }

            int r = rowToInsert.Value;

            _Logger.LogDebug($"Inserting new row at row index {r - 1}");

            // insert a new row
            await _Drive.GetSheetService().Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest() {
                Requests = new List<Request>() {
                    new Request() {
                        InsertDimension = new InsertDimensionRequest() {
                            Range = new DimensionRange() {
                                SheetId = currentSheet.Properties.SheetId,
                                Dimension = "ROWS",
                                StartIndex = r - 1,
                                EndIndex = r
                            }
                        }
                    }
                }
            }, _Settings.Value.CalendarFileId).ExecuteAsync();

            List<ValueRange> data = new() {
                new ValueRange() {
                    MajorDimension = "ROWS",
                    Range = $"Current!A{r}:L{r}",
                    Values = new List<IList<object>>() {
                        new List<object> {
                            // day
                            $"=A{r - 1}",
                            // starting soon
                            $"=if(isblank(L{r}),if(and(I{r}<=$E$2,J{r}>=$E$2),\"IN PROGRESS\",if(and(K{r}<=$E$2,I{r}>=$E$2),\"Starting Soon\",)),if(and(I{r}<=$E$2,L{r}>=$E$2),\"IN PROGRESS\",if(and(K{r}<=$E$2,I{r}>=$E$2),\"Starting Soon\",)))",
                            // outfits
                            $"{string.Join(" / ", reservation.Reservation.Outfits)}",
                            // bases
                            ((booking.ZoneID == null) ? $"{string.Join(", ", booking.Facilities.Select(iter => iter.Name))}" : Zone.GetName(booking.ZoneID.Value)),
                            // from time
                            $"{booking.Start:HH:mm}",
                            // to time
                            $"{booking.End:HH:mm}",
                            // factions
                            $"",
                            // notes
                            $"{reservation.Reservation.Details}",
                            // internal from utc
                            $"=if(isblank(E{r}),,A{r}+E{r})",
                            // internal from + 2 hours utc
                            $"=if(isblank(I{r}),,I{r}+$J$2)",
                            // internal from - 45 minutes utc
                            $"=if(isblank(I{r}),,I{r}-$J$3)",
                            // internal to
                            $"=if(isblank(F{r}),,if(F{r}<E{r},A{r}+F{r}+$L$2,A{r}+F{r}))"
                        }
                    }
                }
            };

            if (_Settings.Value.DebugChannelId != null) {
                HonuDiscordMessage msg = new();
                msg.ChannelID = _Settings.Value.DebugChannelId;
                msg.GuildID = _DiscordOptions.Value.GuildId;
                msg.Message = $"Booking from ID {reservation.MessageId} starts at {booking.Start:u} and will be inserted after row {rowToInsert}:\n```json\n{JToken.FromObject(data)}```";
                _DiscordMessageQueue.Queue(msg);
            }

            _Logger.LogDebug($"booking at {booking.Start:u} will go after row {rowToInsert}: {JToken.FromObject(data)}");

            await _Drive.GetSheetService().Spreadsheets.Values.BatchUpdate(new BatchUpdateValuesRequest() {
                Data = data,
                ValueInputOption = "USER_ENTERED"
            }, _Settings.Value.CalendarFileId).ExecuteAsync();
        }

        private int? GetInsertRow(PsbBaseBooking booking, ValueRange rows) {
            bool inCurrentDay = false;
            
            // find the day of the booking
            // find the first event that starts after the booking
            //      if there are no bookings, add it as the first
            // insert the booking before that first event

            for (int i = 0; i < rows.Values.Count; ++i) {
                IList<object> row = rows.Values[i];

                if (row.Count != 1 && row.Count != 12) {
                    throw new Exception($"Expected 1 or 12 columns in row {i + 5}. Was given {row.Count}");
                }

                bool isDayHeader = row.Count != 1 && (string)row[1] == "Status";
                if (inCurrentDay == true) {
                    // a blank row means the day has no reservations
                    if (row.Count == 1) {
                        _Logger.LogDebug($"Day {booking.Start.Date:u} has no bookings");
                        return i + 5;
                    }

                    // reached the start of the next day
                    if (isDayHeader == true) {
                        _Logger.LogDebug($"end of day for {booking.Start.Date:u} reached, adding to the previous line");
                        return i + 5 - 1;
                    }

                    string start = (string)row[8];

                    if (DateTime.TryParse(start, out DateTime startDate) == false) {
                        throw new FormatException($"Failed to parse '{start}' to a valid DateTime. Found in row {i + 5}");
                    }

                    if (startDate < booking.Start) {
                        continue;
                    }

                    // go back one row, as the current row is the one where the start is after the booking to insert
                    return i + 5;
                } else {
                    if (row.Count == 1) { // empty days
                        continue;
                    }

                    if (isDayHeader == false) { // skip non-entry rows
                        continue;
                    }

                    string day = (string)row[0];

                    DateTimeStyles style = DateTimeStyles.AllowInnerWhite | DateTimeStyles.AllowTrailingWhite 
                        | DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal;

                    if (DateTime.TryParseExact(day, "MMM-dd", null, style, out DateTime rowDay) == false) {
                        throw new FormatException($"Failed to parse '{day}' to a valid DateTime using format 'MMM-dd'. Found in row {i + 6}");
                    }

                    if (rowDay.Date != booking.Start.Date) {
                        continue;
                    }

                    inCurrentDay = true;

                    _Logger.LogDebug($"booking day found at row {i + 5}: {rowDay:u}, booking {booking.Start:u}");
                }
            }

            return null;
        }

    }
}
