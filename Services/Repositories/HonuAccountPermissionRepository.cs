using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    /// <summary>
    ///     Repository to interact with <see cref="HonuAccountPermission"/>s
    /// </summary>
    public class HonuAccountPermissionRepository {

        private readonly ILogger<HonuAccountPermissionRepository> _Logger;
        private readonly IMemoryCache _Cache;

        private const string CACHE_KEY = "Honu.AccountPermission.{0}"; // {0} => Account ID

        private readonly HonuAccountPermissionDbStore _PermissionDb;

        public HonuAccountPermissionRepository(ILogger<HonuAccountPermissionRepository> logger, IMemoryCache cache,
            HonuAccountPermissionDbStore permissionDb) {

            _Logger = logger;
            _Cache = cache;

            _PermissionDb = permissionDb;
        }

        /// <summary>
        ///     Get a specific <see cref="HonuAccountPermission"/> by its ID, or null if it doens't exist
        /// </summary>
        public Task<HonuAccountPermission?> GetByID(long ID) {
            return _PermissionDb.GetByID(ID);
        }

        /// <summary>
        ///     Get the <see cref="HonuAccountPermission"/>s for a user
        /// </summary>
        /// <param name="accountID">ID of the account</param>
        public async Task<List<HonuAccountPermission>> GetByAccountID(long accountID) {
            string cacheKey = string.Format(CACHE_KEY, accountID);

            if (_Cache.TryGetValue(cacheKey, out List<HonuAccountPermission> perms) == false) {
                perms = await _PermissionDb.GetByAccountID(accountID);

                _Cache.Set(cacheKey, perms, new MemoryCacheEntryOptions() {
                    SlidingExpiration = TimeSpan.FromMinutes(30)
                });
            }

            return perms;
        }

        /// <summary>
        ///     Insert a new <see cref="HonuAccountPermission"/>, returning the ID it has after being inserted
        /// </summary>
        public Task<long> Insert(HonuAccountPermission perm) {
            string cacheKey = string.Format(CACHE_KEY, perm.AccountID);
            _Cache.Remove(cacheKey);

            return _PermissionDb.Insert(perm);
        }

        /// <summary>
        ///     Delete a <see cref="HonuAccountPermission"/>
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public async Task DeleteByID(long ID) {
            HonuAccountPermission? perm = await GetByID(ID);
            if (perm != null) {
                string cacheKey = string.Format(CACHE_KEY, perm.AccountID);
                _Cache.Remove(cacheKey);
            }

            await _PermissionDb.DeleteByID(ID);
        }

    }

    /// <summary>
    ///     Useful extensions method for a <see cref="HonuAccountPermissionRepository"/>
    /// </summary>
    public static class HonuAccountPermissionRepositoryExtensionMethods {

        /// <summary>
        ///     Get the <see cref="HonuAccountPermission"/> for a <see cref="HonuAccount"/> based on a list of permissions
        /// </summary>
        /// <param name="repo">Extension instance</param>
        /// <param name="account">Account to get the permission of</param>
        /// <param name="permissions">Permissions to return</param>
        /// <returns>
        ///     The first <see cref="HonuAccountPermission"/> that the account <paramref name="account"/> has that matches
        ///     one of the permission keys in <paramref name="permissions"/>.
        ///     Or <c>null</c> if the user does not have any of those permissions
        /// </returns>
        public static Task<HonuAccountPermission?> GetPermissionByAccount(this HonuAccountPermissionRepository repo, HonuAccount account, params string[] permissions) {
            return repo.GetPermissionByAccountID(account.ID, permissions);
        }

        /// <summary>
        ///     Get the <see cref="HonuAccountPermission"/> for a <see cref="HonuAccount"/> based on a list of permissions
        /// </summary>
        /// <param name="repo">Extension instance</param>
        /// <param name="accountID">ID of the account to get the permission of</param>
        /// <param name="permissions">Permissions to return</param>
        /// <returns>
        ///     The first <see cref="HonuAccountPermission"/> that the account with <see cref="HonuAccount.ID"/>
        ///     of <paramref name="accountID"/> has that matches one of the permission keys in <paramref name="permissions"/>.
        ///     Or <c>null</c> if the user does not have any of those permissions
        /// </returns>
        public static async Task<HonuAccountPermission?> GetPermissionByAccountID(this HonuAccountPermissionRepository repo, long accountID, params string[] permissions) {
            List<HonuAccountPermission> perms = await repo.GetByAccountID(accountID);

            foreach (HonuAccountPermission perm in perms) {
                foreach (string testPerm in permissions) {
                    if (perm.Permission.ToLower() == testPerm.ToLower()) {
                        return perm;
                    }
                }
            }

            return null;
        }

    }


}
