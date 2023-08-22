using Google.Apis.Drive.v3;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.PSB;

namespace watchtower.Services.Repositories.PSB {

    public class PsbOvOAccountRepository {

        private readonly ILogger<PsbOvOAccountRepository> _Logger;
        private readonly GDriveRepository _GDrive;

        private const string CACHE_KEY = "Psb.OvOAccount.{0}"; // {0} => month string
        private readonly IMemoryCache _Cache;

        private readonly IOptions<PsbDriveSettings> _DriveSettings;

        public PsbOvOAccountRepository(ILogger<PsbOvOAccountRepository> logger,
            GDriveRepository gDrive, IOptions<PsbDriveSettings> driveSettings,
            IMemoryCache cache) {

            _Logger = logger;
            _Cache = cache;
            _GDrive = gDrive;

            _DriveSettings = driveSettings;

            if (_DriveSettings.Value.OvOAccountFolderId == "") {
                throw new ArgumentException($"No {nameof(PsbDriveSettings.OvOAccountFolderId)} provided. Set it using dotnet user-secrets set PsbDrive:OvOAccountFolderId $FILE_ID");
            }
        }

        /// <summary>
        ///     Get the ID of the spreedsheet that contains the accounts for a month.
        ///     An exception is thrown if an error occurs
        /// </summary>
        /// <param name="month">When the sheet is wanted. Only uses the month and year</param>
        /// <returns>
        ///     The ID of a google drive file
        /// </returns>
        public async Task<string> GetMonthSheetFileID(DateTime month) {
            string name = $"{month:MM}. {month:MMMM} {month:yyyy}";
            string cacheKey = string.Format(CACHE_KEY, name);
            if (_Cache.TryGetValue(cacheKey, out string fileID) == true) {
                return fileID;
            }

            _Logger.LogDebug($"Searching for file for {month:u} => {name}");

            //_DriveSettings.Value.OvORootFolderId

            FilesResource.ListRequest list = _GDrive.GetDriveService().Files.List();
            list.SupportsAllDrives = true;
            list.Q = $"('{_DriveSettings.Value.OvOAccountFolderId}' in parents) and trashed=false and name='{name}'";
            list.OrderBy = "name";
            list.PageSize = 5;

            string? nextPage = null;
            int backupLimit = 100;

            List<PsbDriveFile> files = new();

            do {
                list.PageToken = nextPage ?? "";

                Google.Apis.Drive.v3.Data.FileList res = await list.ExecuteAsync();

                foreach (Google.Apis.Drive.v3.Data.File file in res.Files) {
                    if (file.MimeType == "application/vnd.google-apps.folder") {
                        continue;
                    }

                    PsbDriveFile psbFile = PsbDriveFile.Convert(file);
                    files.Add(psbFile);
                }

                nextPage = res.NextPageToken;
            } while (nextPage != null && nextPage.Length > 0 && --backupLimit > 0);

            _Logger.LogTrace($"found {files.Count} possible matches for {name}");

            if (files.Count == 0) {
                string err = $"Failed to find file '{name}' in folder '{_DriveSettings.Value.OvOAccountFolderId}' (from {month:u}). "
                    + $"Does it exist, and does this account have permission to it?";

                throw new Exception(err);
            }

            if (files.Count > 1) {
                throw new Exception($"Found {files.Count} possible entries for {name} (from {month:u}). Not assuming one");
            }

            _Logger.LogDebug($"Found account master for {month:u} ({name}) with {files[0].ID}/{files[0].Name}");

            _Cache.Set(cacheKey, files[0].ID);

            return files[0].ID;
        }

        /// <summary>
        ///     Get the block of accounts and uses for a month
        /// </summary>
        /// <param name="when">What month to get the accounts and usage of</param>
        /// <returns></returns>
        /// <exception cref="Exception">If an error occured or sanity check failed</exception>
        /// <exception cref="ArgumentNullException">If a null or empty column was given when a non-blank one was expected</exception>
        public async Task<PsbOvOAccountUsageBlock> GetUsage(DateTime when) {
            PsbOvOAccountUsageBlock block = new();

            int daysInMonth = DateTime.DaysInMonth(when.Year, when.Month);
            string masterID = await GetMonthSheetFileID(when);
            block.FileID = masterID;

            _Logger.LogDebug($"Days in {when:u}: {daysInMonth}");

            SpreadsheetsResource.ValuesResource.GetRequest request = _GDrive.GetSheetService().Spreadsheets.Values.Get(masterID, $"Working Accounts");
            request.MajorDimension = SpreadsheetsResource.ValuesResource.GetRequest.MajorDimensionEnum.ROWS;
            Google.Apis.Sheets.v4.Data.ValueRange response = await request.ExecuteAsync();

            IList<IList<object>> data = response.Values;

            for (int i = 0; i < data.Count; ++i) {
                IList<object> columns = data[i];

                //_Logger.LogTrace($"Line {i} has {columns.Count} columns");

                // blank link
                if (i == 0) {
                    if (columns.Count > 0) {
                        throw new Exception($"Error parsing sheet {masterID}: Expected row {i} to contain 0 columns, instead had {columns.Count}");
                    }
                    continue;
                }

                // expect a number of columns equal to the number days in the month + 3 columns which are account #, username, and password
                if (i == 1) {
                    if (columns.Count != daysInMonth + 3) {
                        throw new Exception($"Error parsing sheet {masterID}: Expected {daysInMonth} + 3 ({daysInMonth + 3}) columns in row {i}, instead had {columns.Count}");
                    }
                    continue;
                }

                // expect the following: "Account #", "Username", "Password", then one column for each month
                if (i == 2) {
                    if (columns.Count != daysInMonth + 3) {
                        throw new Exception($"Error parsing sheet {masterID}: Expected {daysInMonth} + 3 ({daysInMonth + 3}) columns in row {i}, instead had {columns.Count}");
                    }

                    for (int j = 0; j < daysInMonth + 3; ++j) {
                        // force is NOT SAFE! this is intentional, we want to error here
                        string cell = columns[j].ToString()!;

                        // expect string "Account #"
                        if (j == 0) {
                            if (cell != "Account #") {
                                throw new Exception($"Error parsing sheet {masterID}: Expected row {i} column {j} to contain 'Account #'");
                            }
                            continue;
                        }

                        // expect string "Username"
                        if (j == 1) {
                            if (cell != "Username") {
                                throw new Exception($"Error parsing sheet {masterID}: Expected row {i} column {j} to contain 'Username'");
                            }
                            continue;
                        }

                        // expect string "Password
                        if (j == 2) {
                            if (cell != "Password") {
                                throw new Exception($"Error parsing sheet {masterID}: Expected row {i} column {j} to contain 'Password'");
                            }
                            continue;
                        }

                        // column 3 = day 1, so -3 from AddDays
                        DateTime month = new(when.Year, when.Month, 1);
                        month = month.AddDays(j - 3);

                        if (DateTime.TryParse(cell, out DateTime cellDate) == false) {
                            throw new Exception($"Error parsing sheet {masterID}: On row {i} column {j}, failed to parse '{cell}' to a valid DateTime");
                        }

                        if (month != cellDate) {
                            throw new Exception($"Error parsing sheet {masterID}: On row {i} column {j}, expected cell '{cell}' to parse to {month:u}, instead got {cellDate:u}");
                        }

                        block.Usage.Add($"{month:yyyy}-{month:MM}-{month:dd}", new List<PsbOvOAccountEntry>());
                    }

                    // we successfully validated the header, but now we need a day usage entry for each day

                    continue;
                }

                // parsing accounts and rows
                if (i >= 3) {
                    // parsing this is a bit weird, cause we're parsing row-wise, but for days, we want column-wise

                    if (columns.Count < 3) {
                        throw new Exception($"Error parsing sheet {masterID}: On row {i}, expected at least 3 columns, got {columns.Count}");
                    }

                    string accountNumber = columns[0].ToString()
                        ?? throw new ArgumentNullException($"Error parsing sheet {masterID}: On row {i}, expected column 0 to be non-null");
                    string username = columns[1].ToString()
                        ?? throw new ArgumentNullException($"Error parsing sheet {masterID}: On row {i}, expected column 1 to be non-null");
                    string password = columns[2].ToString()
                        ?? throw new ArgumentNullException($"Error parsing sheet {masterID}: On row {i}, expected column 2 to be non-null");

                    if (int.TryParse(accountNumber, out int number) == false) {
                        throw new Exception($"Error parsing sheet {masterID}: On row {i}, expected column 0 ('{accountNumber}') to parse to a valid Int32");
                    }

                    PsbOvOAccount account = new();
                    account.Number = number;
                    account.Username = username;
                    account.Password = password;
                    account.Index = i - 3;

                    foreach (PsbOvOAccount iter in block.Accounts) {
                        if (iter.Number == account.Number) {
                            throw new Exception($"Error parsing sheet {masterID}: On row {i}, duplicate account found, account number {iter.Number} already exists");
                        }
                        if (iter.Username == account.Username) {
                            throw new Exception($"Error parsing sheet {masterID}: On row {i}, duplicate account found, username {iter.Username} already exists");
                        }
                    }

                    block.Accounts.Add(account);

                    // +3 for the header columns
                    for (int j = 3; j < daysInMonth + 3; ++j) {
                        // -3 as there are 3 header columns, then +1 as days are 1-indexed instead of 0
                        DateTime columnDate = new(when.Year, when.Month, j - 3 + 1);

                        List<PsbOvOAccountEntry> entries = block.GetDayUsage(columnDate);

                        // sanity check
                        if (entries.Count != i - 3) {
                            throw new Exception($"Error parsing sheet {masterID}: On row {i}, unexpected count of account entry (poke varunda! {i} {i - 3} {entries.Count})");
                        }

                        PsbOvOAccountEntry entry = new();
                        entry.Number = account.Number;

                        // this means we've hit the days the account is used
                        if (j > columns.Count - 1) {
                            entry.UsedBy = null;
                        } else if (columns[j] == null || columns[j].ToString() == null || columns[j].ToString() == "") {
                            entry.UsedBy = null;
                        } else {
                            entry.UsedBy = columns[j].ToString();
                        }

                        //_Logger.LogDebug($"column {j}, used? {entry.UsedBy}, date {columnDate:u}");

                        entries.Add(entry);
                    }
                }
            }

            foreach (KeyValuePair<string, List<PsbOvOAccountEntry>> day in block.Usage) {
                if (day.Value.Count != block.Accounts.Count) {
                    throw new Exception($"Expected {block.Accounts.Count} entries for day {day.Key}");
                }
            }

            return block;
        }

        /// <summary>
        ///     Mark a given time as used by a group for a list of accounts. It is assumed the accounts
        ///     have a continious range
        /// </summary>
        /// <param name="when">When the accounts will be used</param>
        /// <param name="accounts">List of continous numbered accounts that will be used</param>
        /// <param name="usedBy">A description of what group will be using the account</param>
        public async Task MarkUsed(DateTime when, List<PsbOvOAccount> accounts, string usedBy) {
            if (accounts.Count == 0) {
                return;
            }

            int lastIndex = accounts[0].Index;
            for (int i = 1; i < accounts.Count; ++i) {
                if (accounts[i].Index != lastIndex + 1) {
                    throw new ArgumentException($"Non-continious account block given. Went from {lastIndex} to {accounts[i].Index}");
                }
                lastIndex = accounts[i].Index;
            }

            string masterID = await GetMonthSheetFileID(when);

            int rowIndex = accounts[0].Index + 4; // +3 row offset, +1 1-indexed
            int columnIndex = when.Day - 1 + 3; // Day is 1-indexed

            string columnLetter = "";
            if (columnIndex >= 26) {
                columnLetter = "A" + ((char)('A' + (char)(columnIndex % 26)));
            } else {
                columnLetter = "" + ((char)('A' + (char)columnIndex));
            }

            _Logger.LogDebug($"Row index: {rowIndex + 1}, index {accounts[0].Index}, columnLetter: {columnLetter}");

            List<ValueRange> data = new() {
                new() {
                    MajorDimension = "COLUMNS",
                    Range = $"Working Accounts!{columnLetter}{rowIndex}:{columnLetter}",
                    Values = new List<IList<object>> {
                        new List<object>(Enumerable.Repeat(usedBy, accounts.Count))
                    }
                }
            };

            _Logger.LogDebug($"Range: {data[0].Range}, Data: {data[0].Values}");

            await _GDrive.GetSheetService().Spreadsheets.Values.BatchUpdate(new BatchUpdateValuesRequest() {
                Data = data,
                ValueInputOption = "USER_ENTERED"
            }, masterID).ExecuteAsync();
        }

    }
}
