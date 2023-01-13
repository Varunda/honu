using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.PSB;

namespace watchtower.Services.Repositories.PSB {

    public class PsbDriveRepository {

        private readonly ILogger<PsbDriveRepository> _Logger;
        private readonly IMemoryCache _Cache;
        private const string CACHE_KEY_PRACTICE_SHEETS = "Psb.Drive.PracticeSheets";
        private const string CACHE_KEY_FILE_PERMISSIONS = "Psb.Drive.FilePermission.{0}"; // {0} => drive file ID

        private readonly IOptions<PsbDriveSettings> _Settings;

        private bool _Initialized = false;
        private bool _FailedInit { get { return _FailedInitReason != null; } }
        private string? _FailedInitReason { get; set; } = null;

        private ServiceAccountCredential? _GoogleCredentials = null;
        private DriveService? _DriveService = null;

        public PsbDriveRepository(ILogger<PsbDriveRepository> logger, IMemoryCache cache,
            IOptions<PsbDriveSettings> settings) {

            _Logger = logger;
            _Cache = cache;

            _Settings = settings;
        }

        /// <summary>
        ///     Initialize the repository (if needed). If any error occurs, call <see cref="GetInitializeFailureReason"/> to see why
        /// </summary>
        /// <returns>
        ///     If the repo was succesfully initialized or not.
        ///     If <c>false</c>, you can call <see cref="GetInitializeFailureReason"/> that will contain more information why
        /// </returns>
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

            if (_Settings.Value.PracticeFolderId.Trim().Length == 0) {
                _FailedInitReason = $"practice folder id from settings is blank";
                _Logger.LogError($"Failed to initialize psb drive repository: {_FailedInitReason}");
                return false;
            }

            if (File.Exists(_Settings.Value.CredentialFile) == false) {
                _FailedInitReason = $"credential file '{_Settings.Value.CredentialFile}' does not exist (or not permission)";
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
                _Logger.LogError(ex, $"Failed to initilaize psb drive repository: {_FailedInitReason}");
                return false;
            }

            if (_GoogleCredentials == null) {
                _FailedInitReason = $"google credentials is still null?";
                _Logger.LogError($"Failed to initialize psb drive repository: {_FailedInitReason}");
                return false;
            }

            BaseClientService.Initializer gClient = new BaseClientService.Initializer() {
                ApplicationName = "Honu/Spark",
                HttpClientInitializer = _GoogleCredentials
            };

            _DriveService = new DriveService(gClient);

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
        ///     Get the email of the service account performing the requests
        /// </summary>
        public string GetEmail() {
            return _GoogleCredentials?.Id ?? "";
        }

        /// <summary>
        ///     Get the practice sheets
        /// </summary>
        /// <returns>
        ///     A list of <see cref="PsbDriveFile"/>s, or null if the psb drive repository failed to initalize
        /// </returns>
        public async Task<List<PsbDriveFile>?> GetPracticeSheets() {
            if (_Cache.TryGetValue(CACHE_KEY_PRACTICE_SHEETS, out List<PsbDriveFile> files) == true) {
                return files;
            }

            if (Initialize() == false) {
                return null;
            }

            if (_DriveService == null) {
                throw new SystemException($"_DriveService is not supposed to be null now");
            }

            FilesResource.ListRequest list = _DriveService.Files.List();
            list.SupportsAllDrives = true;
            list.Q = $"('{_Settings.Value.PracticeFolderId}' in parents) and trashed=false";
            list.OrderBy = "name";
            list.PageSize = 5;

            string? nextPage = null;
            int backupLimit = 100;

            files = new();

            do {
                list.PageToken = nextPage ?? "";

                Google.Apis.Drive.v3.Data.FileList res = await list.ExecuteAsync();

                foreach (Google.Apis.Drive.v3.Data.File file in res.Files) {
                    if (file.MimeType == "application/vnd.google-apps.folder") {
                        continue;
                    }

                    PsbDriveFile psbFile = Convert(file);
                    files.Add(psbFile);
                }

                nextPage = res.NextPageToken;
            } while (nextPage != null && nextPage.Length > 0 && --backupLimit > 0);

            _Cache.Set(CACHE_KEY_PRACTICE_SHEETS, files, new MemoryCacheEntryOptions() {
                SlidingExpiration = TimeSpan.FromMinutes(30)
            });

            return files;
        }

        /// <summary>
        ///     Get the permissions of a specific file
        /// </summary>
        /// <param name="driveFileID">ID of the file to get the permissions of</param>
        /// <returns></returns>
        public async Task<List<PsbDrivePermission>?> GetPermissions(string driveFileID) {
            string cacheKey = string.Format(CACHE_KEY_FILE_PERMISSIONS, driveFileID);

            if (_Cache.TryGetValue(cacheKey, out List<PsbDrivePermission> perms) == true) { 
                return perms;
            }

            if (Initialize() == false) {
                return null;
            }

            if (_DriveService == null) {
                throw new SystemException($"not sure how we got here 423erf");
            }

            PermissionsResource.ListRequest permR = new PermissionsResource(_DriveService).List(driveFileID);
            permR.Fields = "*";
            permR.SupportsAllDrives = true;

            string? nextPage = null;
            int hardBreak = 100;

            perms = new();

            do {
                permR.PageToken = nextPage;

                Google.Apis.Drive.v3.Data.PermissionList gPerms = await permR.ExecuteAsync();

                nextPage = gPerms.NextPageToken;

                foreach (Google.Apis.Drive.v3.Data.Permission gPerm in gPerms.Permissions) {
                    PsbDrivePermission perm = Convert(driveFileID, gPerm);
                    perms.Add(perm);
                }
            } while (nextPage != null && nextPage.Length > 0 && --hardBreak > 0);

            _Cache.Set(cacheKey, perms, new MemoryCacheEntryOptions() {
                SlidingExpiration = TimeSpan.FromMinutes(15)
            });

            return perms;
        }

        /// <summary>
        ///     Create a new google drive API permission 
        /// </summary>
        /// <param name="driveFileID">file ID of the item to add the permission to</param>
        /// <param name="email">Email of the user to add the permission to</param>
        /// <returns>
        ///     The newly created <see cref="PsbDrivePermission"/>,
        ///     or <c>null</c> if the creation failed
        /// </returns>
        public async Task<PsbDrivePermission?> AddUserToDriveFile(string driveFileID, string email) {
            if (Initialize() == false) {
                return null;
            }

            if (_DriveService == null) {
                throw new SystemException($"drive service is not supposed to be null here");
            }

            // removed even if an exception occurs further down
            string cacheKey = string.Format(CACHE_KEY_FILE_PERMISSIONS, driveFileID);
            _Cache.Remove(cacheKey);

            Google.Apis.Drive.v3.Data.Permission perm = new();
            perm.EmailAddress = email;
            perm.Role = "writer";
            perm.Type = "user";

            PermissionsResource.CreateRequest request = _DriveService.Permissions.Create(perm, driveFileID);
            Google.Apis.Drive.v3.Data.Permission response = await request.ExecuteAsync();

            return Convert(driveFileID, response);
        }

        /// <summary>
        ///     Remove a permission from a file
        /// </summary>
        /// <param name="driveFileID">ID of the GDrive file to remove the permission from</param>
        /// <param name="permissionID">ID of the GDrive permission object that will be removed</param>
        /// <returns>
        ///     A boolean indicating if the operation was successful or not
        /// </returns>
        public async Task<bool> RemoveUserFromDriveFile(string driveFileID, string permissionID) {
            if (Initialize() == false) {
                return false;
            }

            if (_DriveService == null) {
                throw new SystemException($"drive service is not supposed to be null here");
            }

            // removed even if an exception occurs further down
            string cacheKey = string.Format(CACHE_KEY_FILE_PERMISSIONS, driveFileID);
            _Cache.Remove(cacheKey);

            PermissionsResource.DeleteRequest request = _DriveService.Permissions.Delete(driveFileID, permissionID);
            string response = await request.ExecuteAsync();

            return true;
        }

        public async Task<PsbDrivePermission?> AcceptOwnership(string driveFileID, string permissionID) {
            if (Initialize() == false) {
                return null;
            }

            if (_DriveService == null) {
                throw new SystemException($"");
            }

            Google.Apis.Drive.v3.Data.Permission perm = new();
            perm.Role = "owner";

            PermissionsResource.UpdateRequest request = _DriveService.Permissions.Update(perm, driveFileID, permissionID);
            request.TransferOwnership = true;

            Google.Apis.Drive.v3.Data.Permission response = await request.ExecuteAsync();

            return Convert(driveFileID, response);
        }

        private PsbDrivePermission Convert(string parentFileID, Google.Apis.Drive.v3.Data.Permission perm) {
            return new PsbDrivePermission(parentFileID) {
                ID = perm.Id,
                Email = perm.EmailAddress,
                DisplayName = perm.DisplayName,
                Role = perm.Role,
                Type = perm.Type,
                PendingOwnership = perm.PendingOwner,
                Details = perm.PermissionDetails?.Select(iter => Convert(iter)).ToList() ?? new List<PsbDrivePermissionDetail>()
            };
        }

        private PsbDrivePermissionDetail Convert(Google.Apis.Drive.v3.Data.Permission.PermissionDetailsData detail) {
            return new PsbDrivePermissionDetail() {
                Type = detail.PermissionType,
                Role = detail.Role,
                Inherited = detail.Inherited!.Value,
                InheritedFrom = detail.InheritedFrom
            };
        }

        private PsbDriveFile Convert(Google.Apis.Drive.v3.Data.File file) {
            return new PsbDriveFile() {
                ID = file.Id,
                Name = file.Name,
                Kind = file.Kind,
                DriveId = file.DriveId,
                MimeType = file.MimeType
            };
        }

    }
}
