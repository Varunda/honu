using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using watchtower.Models.PSB;
using watchtower.Services.Repositories.PSB;

namespace watchtower.Services.Repositories {

    /// <summary>
    ///     Wrapper service around GDrive api services
    /// </summary>
    public class GDriveRepository {

        private readonly ILogger<GDriveRepository> _Logger;
        private readonly IOptions<PsbDriveSettings> _Options;

        private string? _FailedInitReason { get; set; } = null;

        private ServiceAccountCredential _GoogleCredentials;
        private DriveService _DriveService;
        private SheetsService _SheetService;

        public GDriveRepository(ILogger<GDriveRepository> logger,
            IOptions<PsbDriveSettings> options) {

            _Logger = logger;
            _Options = options;

            if (_Options.Value.CredentialFile.Trim().Length == 0) {
                _FailedInitReason = $"credential file from settings is blank";
                _Logger.LogError($"Failed to create gdrive repository: {_FailedInitReason}");
                throw new Exception(_FailedInitReason);
            }

            if (File.Exists(_Options.Value.CredentialFile) == false) {
                _FailedInitReason = $"credential file '{_Options.Value.CredentialFile}' does not exist (or no permission)";
                _Logger.LogError($"Failed to create gdrive repository: {_FailedInitReason}");
                throw new Exception(_FailedInitReason);
            }

            string[] scopes = {
                DriveService.Scope.Drive,
                DriveService.Scope.DriveFile,
                DriveService.Scope.DriveMetadata,
                DriveService.Scope.DriveAppdata
            };

            try {
                using (FileStream stream = new FileStream(_Options.Value.CredentialFile, FileMode.Open, FileAccess.Read)) {
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
                _Logger.LogError(ex, $"Failed to create gdrive repository: {_FailedInitReason}");
                throw new Exception(_FailedInitReason);
            }

            if (_GoogleCredentials == null) {
                _FailedInitReason = $"google credentials is still null?";
                _Logger.LogError($"Failed to create gdrive repository: {_FailedInitReason}");
                throw new Exception(_FailedInitReason);
            }

            BaseClientService.Initializer gClient = new BaseClientService.Initializer() {
                ApplicationName = "Honu/Spark",
                HttpClientInitializer = _GoogleCredentials
            };

            _DriveService = new DriveService(gClient);
            _SheetService = new SheetsService(gClient);
        }

        /// <summary>
        ///     Get the <see cref="DriveService"/> other services can use
        /// </summary>
        public DriveService GetDriveService() => _DriveService;

        /// <summary>
        ///     Get the <see cref="SheetsService"/> other services can use
        /// </summary>
        public SheetsService GetSheetService() => _SheetService;

    }
}
