using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Models.PSB;
using watchtower.Services.Repositories.Readers;

namespace watchtower.Services.Repositories.PSB {

    public class PsbCalendarRepository {

        private readonly ILogger<PsbCalendarRepository> _Logger;
        private readonly IMemoryCache _Cache;

        private readonly FacilityRepository _FacilityRepository;
        private readonly ISheetsReader<PsbCalendarEntry> _CalendarReader;

        private readonly IOptions<PsbDriveSettings> _Settings;

        private bool _Initialized = false;
        private bool _FailedInit { get { return _FailedInitReason != null; } }
        private string? _FailedInitReason { get; set; } = null;

        private ServiceAccountCredential? _GoogleCredentials = null;
        private DriveService? _DriveService = null;
        private SheetsService? _SheetService = null;

        public PsbCalendarRepository(ILogger<PsbCalendarRepository> logger, IMemoryCache cache,
            IOptions<PsbDriveSettings> settings, ISheetsReader<PsbCalendarEntry> calendarReader,
            FacilityRepository facilityRepository) {

            _Logger = logger;
            _Cache = cache;

            _Settings = settings;
            _CalendarReader = calendarReader;
            _FacilityRepository = facilityRepository;
        }

        /// <summary>
        ///     Initialize the repository (if needed). If any error occurs, call <see cref="GetInitializeFailureReason"/> to see why
        /// </summary>
        /// <returns>If the repo was succesfully initialized or not</returns>
        private bool Initialize() {
            if (_Initialized == true) {
                return true;
            }

            if (_FailedInit == true) {
                return false;
            }

            if (_Settings.Value.CredentialFile.Trim().Length == 0) {
                _FailedInitReason = $"credential file from settings is blank";
                _Logger.LogError($"Failed to initialize psb drive repository: {_FailedInitReason}");
                return false;
            }

            if (_Settings.Value.CalendarFileId.Trim().Length == 0) {
                _FailedInitReason = $"calendar file id from settings is blank";
                _Logger.LogError($"Failed to initialize psb drive repository: {_FailedInitReason}");
                return false;
            }

            if (File.Exists(_Settings.Value.CredentialFile) == false) {
                _FailedInitReason = $"credential file '{_Settings.Value.CredentialFile}' does not exist (or no permission)";
                _Logger.LogError($"Failed to initialize psb drive repository: {_FailedInitReason}");
                return false;
            }

            string[] scopes = {
                DriveService.Scope.Drive,
                DriveService.Scope.DriveFile,
                DriveService.Scope.DriveMetadata,
                DriveService.Scope.DriveAppdata
            };

            try {
                using (FileStream stream = new FileStream(_Settings.Value.CredentialFile, FileMode.Open, FileAccess.Read)) {
                    _GoogleCredentials = ServiceAccountCredential.FromServiceAccountData(stream);

                    _GoogleCredentials = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(_GoogleCredentials.Id) {
                        User = _GoogleCredentials.User,
                        Key = _GoogleCredentials.Key,
                        KeyId = _GoogleCredentials.KeyId,
                        Scopes = scopes
                    });
                }
            } catch (Exception ex) {
                _FailedInitReason = $"exception while opening credential file: {ex.Message}";
                _Logger.LogError(ex, $"Failed to initilaize psb contact sheet repository: {_FailedInitReason}");
                return false;
            }

            if (_GoogleCredentials == null) {
                _FailedInitReason = $"google credentials is still null?";
                _Logger.LogError($"Failed to initialize psb contact sheet repository: {_FailedInitReason}");
                return false;
            }

            BaseClientService.Initializer gClient = new BaseClientService.Initializer() {
                ApplicationName = "Honu/Spark",
                HttpClientInitializer = _GoogleCredentials
            };

            _DriveService = new DriveService(gClient);
            _SheetService = new SheetsService(gClient);

            _Initialized = true;
            return true;
        }

        /// <summary>
        ///     Get the reason for initialization failure, or null if none
        /// </summary>
        public string? GetInitializeFailureReason() {
            return _FailedInitReason;
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
            if (Initialize() == false) {
                throw new SystemException($"Failed to initialize: {GetInitializeFailureReason()}");
            }

            if (_SheetService == null) {
                throw new SystemException($"Sheet service is not supposed to be null");
            }

            // https://developers.google.com/sheets/api/guides/concepts#cell
            SpreadsheetsResource.ValuesResource.GetRequest sheetR = _SheetService.Spreadsheets.Values.Get(_Settings.Value.CalendarFileId, "Current!A5:M");
            Google.Apis.Sheets.v4.Data.ValueRange res = await sheetR.ExecuteAsync();
            _Logger.LogDebug($"Loaded {res.Values.Count} rows from {_Settings.Value.ContactSheets.Practice}");

            List<PsbCalendarEntry> entries = _CalendarReader.ReadList(res.Values).Where(iter => iter.Valid == true).ToList();

            List<PsFacility> facilities = await _FacilityRepository.GetAll();

            foreach (PsbCalendarEntry entry in entries) {
                foreach (string baseName in entry.BaseNames) {
                    PsbCalendarBaseEntry baseEntry = new();
                    baseEntry.Name = baseName;

                    List<PsFacility> possible = facilities.Where(iter => iter.Name.ToLower().StartsWith(baseName.ToLower())).ToList();
                    baseEntry.PossibleBases = possible;

                    entry.Bases.Add(baseEntry);
                }
            }

            return entries;
        }

    }
}
