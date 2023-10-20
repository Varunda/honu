using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.PSB;

namespace watchtower.Services.Repositories.PSB {

    public class PsbOvOSheetRepository {

        private readonly ILogger<PsbOvOSheetRepository> _Logger;
        private readonly IOptions<PsbDriveSettings> _Options;

        private readonly GDriveRepository _GRepository;
        private readonly PsbOvOAccountRepository _OvOAccountRepository;

        public PsbOvOSheetRepository(ILogger<PsbOvOSheetRepository> logger,
            IOptions<PsbDriveSettings> options, GDriveRepository gRepository,
            PsbOvOAccountRepository ovOAccountRepository) {

            _Logger = logger;
            _Options = options;
            _GRepository = gRepository;

            if (_Options.Value.TemplateFileId.Trim().Length == 0) {
                throw new ArgumentException($"No {nameof(PsbDriveSettings.TemplateFileId)} provided. Set it using dotnet user-secrets set PsbDrive:TemplateFileId $FILE_ID");
            }
            _OvOAccountRepository = ovOAccountRepository;
        }

        /// <summary>
        ///     Approve an account booking
        /// </summary>
        /// <param name="parsed"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> ApproveAccounts(ParsedPsbReservation parsed) {
            if (parsed.Reservation.Accounts == 0) {
                return "";
            }

            PsbOvOAccountUsageBlock block = await _OvOAccountRepository.GetUsage(parsed.Reservation.Start);

            _Logger.LogDebug($"Accounts are located in {block.FileID}");

            int daysOfMonth = DateTime.DaysInMonth(parsed.Reservation.Start.Year, parsed.Reservation.Start.Month);

            List<List<PsbOvOAccountEntry>> blockingUses = new();

            blockingUses.Add(block.GetDayUsage(parsed.Reservation.Start));

            // if it's not the first day
            if (parsed.Reservation.Start.Day > 1) {
                blockingUses.Add(block.GetDayUsage(parsed.Reservation.Start.AddDays(-1)));
            }

            // if it's not the last day
            if (parsed.Reservation.Start.Day < daysOfMonth) {
                blockingUses.Add(block.GetDayUsage(parsed.Reservation.Start.AddDays(1)));
            }

            int daysFree = 0;
            int index = 0;
            for (int i = 0; i < block.Accounts.Count; ++i) {
                bool dayOpen = true;
                foreach (List<PsbOvOAccountEntry> uses in blockingUses) {
                    if (uses[i].UsedBy != null) {
                        daysFree = 0;
                        dayOpen = false;
                        break;
                    }
                }

                if (dayOpen == true) {
                    ++daysFree;
                }

                if (daysFree >= parsed.Reservation.Accounts) {
                    index = i;
                    break;
                }
            }

            if (index == 0) {
                throw new Exception($"Cannot approve accounts, there is not {parsed.Reservation.Accounts} booked");
            }

            // +1, we're comparing an index and count
            int rowStart = index - parsed.Reservation.Accounts + 1;
            _Logger.LogDebug($"daysFree on index {index}, accounts: {parsed.Reservation.Accounts} = {rowStart}");

            List<PsbOvOAccount> accounts = block.Accounts.Skip(rowStart)
                .Take(parsed.Reservation.Accounts).ToList();

            _Logger.LogDebug($"got {accounts.Count} accounts, numbers: {string.Join(", ", accounts.Select(iter => iter.Number))}");

            // find account master
            // find accounts from the account master to use
            // create the sheet
            // no need to share the sheet, that is done by Edward

            string fileID = await CreateSheet(parsed.Reservation, accounts);

            await _OvOAccountRepository.MarkUsed(parsed.Reservation.Start, accounts, $"{string.Join("/", parsed.Reservation.Outfits)}");

            return fileID;
        }

        /// <summary>
        ///     create a sheet based on the parameters of a reservation
        /// </summary>
        /// <param name="res">reservation that contains the data to use</param>
        /// <param name="accounts">Accounts that will be given for this reservation</param>
        /// <returns>
        ///     the file ID of the google drive file that was created for the sheet
        /// </returns>
        private async Task<string> CreateSheet(PsbReservation res, List<PsbOvOAccount> accounts) {
            if (res.Accounts != accounts.Count) {
                throw new Exception($"Sanity check failed, requested {res.Accounts} accounts, given {accounts.Count}");
            }

            string date = $"{res.Start.Date:yyyy-MM-dd}";

            _Logger.LogTrace($"using template {_Options.Value.TemplateFileId}, parent: {_Options.Value.OvORootFolderId}");

            FilesResource.CopyRequest gReq = _GRepository.GetDriveService().Files.Copy(new Google.Apis.Drive.v3.Data.File() {
                Name = $"{date} [{string.Join("/", res.Outfits)}]",
                Parents = new List<string> { _Options.Value.OvORootFolderId }
            }, _Options.Value.TemplateFileId);

            Google.Apis.Drive.v3.Data.File gRes = await gReq.ExecuteAsync();

            List<ValueRange> data = new() {
                // contact emails
                new() {
                    MajorDimension = "ROWS",
                    Range = "Sheet1!B1:D1",
                    Values = new List<IList<object>> { new List<object> {
                        string.Join(";", res.Contacts.Select(iter => iter.Email))
                    } }
                },

                // date
                new() {
                    MajorDimension = "ROWS",
                    Range = "Sheet1!B2:D2",
                    Values = new List<IList<object>> { new List<object> {
                        date
                    } }
                },

                // time
                new() {
                    MajorDimension = "ROWS",
                    Range = "Sheet1!B3:C3",
                    Values = new List<IList<object>> { new List<object> {
                        $"{res.Start:HH:mm}"
                    } }
                },

                new() {
                    MajorDimension = "ROWS",
                    Range = "Sheet1!B4:D4",
                    Values = new List<IList<object>> { new List<object> {
                        $"Ready to Share"
                    } }
                },

                // accounts
                new() {
                    MajorDimension = "ROWS",
                    Range = "Sheet1!A7:C",
                    Values = new List<IList<object>>(
                        accounts.Select(iter => {
                            return new List<object> {
                                $"{iter.Number}",
                                iter.Username,
                                iter.Password
                            };
                        }).ToList()
                    )
                }
            };

            _Logger.LogDebug($"file id {gRes.Id}");

            await _GRepository.GetSheetService().Spreadsheets.Values.BatchUpdate(new BatchUpdateValuesRequest() {
                Data = data,
                ValueInputOption = "USER_ENTERED"
            }, gRes.Id).ExecuteAsync();

            return gRes.Id;
        }

        public async Task<PsbOvOAccountSheet?> GetByID(string fileID) {

            // https://developers.google.com/sheets/api/guides/concepts#cell
            SpreadsheetsResource.ValuesResource.GetRequest sheetR = _GRepository.GetSheetService().Spreadsheets.Values.Get(fileID, "Sheet1!A1:D");
            Google.Apis.Sheets.v4.Data.ValueRange res = await sheetR.ExecuteAsync();
            _Logger.LogDebug($"Loaded {res.Values.Count} rows from {fileID}");

            PsbOvOAccountSheet sheet = new();
            sheet.FileID = fileID;

            IList<IList<object>> values = res.Values;

            int rowIndex = 0;

            DateTimeStyles style = DateTimeStyles.AllowInnerWhite | DateTimeStyles.AllowTrailingWhite 
                | DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal;

            DateTime day = DateTime.UtcNow;
            DateTime time;

            foreach (IList<object> row in values) {
                if (rowIndex == 0) {
                    if (row.Count < 2) {
                        throw new Exception($"expected 2 columns in row 0 (emails), had {row.Count} instead");
                    }

                    string header = row.ElementAt(0).ToString()!;
                    string value = row.ElementAt(1).ToString()!;

                    sheet.Emails = new List<string>(value.Split(","));
                } else if (rowIndex == 1) {
                    if (row.Count < 2) {
                        throw new Exception($"expected 2 columns in row 1 (day), had {row.Count} instead");
                    }

                    string value = row.ElementAt(1).ToString()!;
                    if (DateTime.TryParseExact(value, "yyyy-MM-dd", null, style, out day) == false) {
                        throw new FormatException($"failed to parse '{value}' to a valid DateTime");
                    }
                    day = DateTime.SpecifyKind(day, DateTimeKind.Utc);
                } else if (rowIndex == 2) {
                    if (row.Count < 2) {
                        throw new Exception($"expected 2 columns in row 2 (time), had {row.Count} instead");
                    }

                    string value = row.ElementAt(1).ToString()!;
                    if (DateTime.TryParseExact(value, "HH:mm", null, style, out time) == false) {
                        throw new FormatException($"failed to parse '{value}' to a valid DateTime");
                    }

                    time = DateTime.SpecifyKind(time, DateTimeKind.Utc);

                    sheet.When = day + time.TimeOfDay;
                    sheet.When = DateTime.SpecifyKind(sheet.When, DateTimeKind.Utc);

                } else if (rowIndex == 3) {
                    if (row.Count < 2) {
                        throw new Exception($"expected 2 columns in row 3 (status), had {row.Count} instead");
                    }

                    sheet.State = row.ElementAt(1).ToString()!;
                } else if (rowIndex == 4) {
                    if (row.Count < 2) {
                        throw new Exception($"expected 2 columns in row 4 (type), had {row.Count} instead");
                    }

                    sheet.Type = row.ElementAt(1).ToString()!;
                } else if (rowIndex == 5) {
                    // header

                } else if (rowIndex >= 6) {
                    if (row.Count < 3) {
                        throw new Exception($"expected >=3 columns in row {rowIndex} (usage), had {row.Count} instead");
                    }

                    PsbOvOAccountSheetUsage usage = new();
                    usage.AccountNumber = row.ElementAt(0).ToString()!;
                    usage.Username = row.ElementAt(1).ToString()!;

                    if (row.Count >= 4) {
                        usage.Player = row.ElementAt(3).ToString()!;
                    }

                    sheet.Accounts.Add(usage);
                }

                rowIndex += 1;
            }

            return sheet;
        }

    }
}
