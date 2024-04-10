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
using watchtower.Models.PSB;
using watchtower.Services.Repositories.PSB;

namespace watchtower.Services.Repositories {

    /// <summary>
    ///     Wrapper service around GDrive api services
    /// </summary>
    public class GDriveRepository {

        private readonly ILogger<GDriveRepository> _Logger;
        private readonly IOptions<PsbDriveSettings> _Options;
        private readonly IMemoryCache _Cache;

        const string CACHE_KEY_TRAVERSE = "GDrive.Traverse.{0}"; // {0} => folder ID

        private string? _FailedInitReason { get; set; } = null;

        private ServiceAccountCredential _GoogleCredentials;
        private DriveService _DriveService;
        private SheetsService _SheetService;

        public GDriveRepository(ILogger<GDriveRepository> logger,
            IOptions<PsbDriveSettings> options, IMemoryCache cache) {

            _Logger = logger;
            _Options = options;
            _Cache = cache;

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

        public async Task<List<PsbDriveFile>> TraverseDirectory(string fileID) {
            string cacheKey = string.Format(CACHE_KEY_TRAVERSE, fileID);
            if (_Cache.TryGetValue(cacheKey, out List<PsbDriveFile>? files) == true && files != null) {
                return files;
            }

            List<string> foldersToTraverse = new() { fileID };
            files = new List<PsbDriveFile>();

            HashSet<string> traversedFolders = new();

            int failsafe = 0;

            while (foldersToTraverse.Count > 0) {

                List<string> slice = foldersToTraverse.Take(8).ToList();

                string query = string.Join(" or ", slice.Select(iter => $" ('{iter}' in parents) "));
                _Logger.LogDebug($"traversing into {string.Join(", ", slice)} [query={query}]");

                foldersToTraverse.RemoveRange(0, slice.Count);

                FilesResource.ListRequest list = GetDriveService().Files.List();
                list.SupportsAllDrives = true;
                list.Q = $"({query}) and trashed=false";
                list.OrderBy = "name";
                list.PageSize = 100;
                list.Fields = "nextPageToken, files(id, name, mimeType, parents, kind, driveId)";

                string? nextPage = null;
                int backupLimit = 100;

                do {
                    list.PageToken = nextPage ?? "";

                    Google.Apis.Drive.v3.Data.FileList res = await list.ExecuteAsync();

                    foreach (Google.Apis.Drive.v3.Data.File file in res.Files) {
                        if (file.MimeType != "application/vnd.google-apps.folder") {
                            continue;
                        }

                        PsbDriveFile psbFile = PsbDriveFile.Convert(file);
                        files.Add(psbFile);

                        if (traversedFolders.Contains(psbFile.ID) == false) {
                            _Logger.LogDebug($"found new folder {psbFile.Name} / {psbFile.ID}");
                            foldersToTraverse.Add(psbFile.ID);
                            traversedFolders.Add(psbFile.ID);
                        }
                    }

                    nextPage = res.NextPageToken;
                } while (nextPage != null && nextPage.Length > 0 && --backupLimit > 0);

                if (++failsafe > 100) {
                    _Logger.LogWarning($"tripped failsafe");
                    break;
                }
            }

            _Cache.Set(cacheKey, files, new MemoryCacheEntryOptions() {
                SlidingExpiration = TimeSpan.FromMinutes(30)
            });

            return files;
        }

    }
}
