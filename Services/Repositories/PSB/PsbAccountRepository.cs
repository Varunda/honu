using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.PSB;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories.PSB {

    public class PsbAccountRepository {

        private readonly ILogger<PsbAccountRepository> _Logger;
        private readonly PsbAccountDbStore _Db;
        private readonly CharacterRepository _CharacterRepository;
        private readonly CharacterCollection _CharacterCollection;
        private readonly PsbAccountNoteDbStore _NoteDb;

        private readonly IMemoryCache _Cache;
        private const string CACHE_KEY = "PsbNamed.All";

        public PsbAccountRepository(ILogger<PsbAccountRepository> logger,
            PsbAccountDbStore db, CharacterRepository charRepo,
            IMemoryCache cache, CharacterCollection charColl,
            PsbAccountNoteDbStore noteDb) {

            _Logger = logger;
            _Cache = cache;

            _Db = db;
            _CharacterRepository = charRepo;
            _CharacterCollection = charColl;
            _NoteDb = noteDb;
        }

        /// <summary>
        ///     Get all <see cref="PsbAccount"/>s
        /// </summary>
        public async Task<List<PsbAccount>> GetAll() {
            if (_Cache.TryGetValue(CACHE_KEY, out List<PsbAccount>? accounts) == false || accounts == null) {
                accounts = await _Db.GetAll();

                _Cache.Set(CACHE_KEY, accounts, new MemoryCacheEntryOptions() {
                    SlidingExpiration = TimeSpan.FromHours(4)
                });
            }

            return accounts;
        }

        /// <summary>
        ///     Get all <see cref="PsbAccount"/>s with a matching <see cref="PsbAccount.AccountType"/>
        /// </summary>
        /// <param name="typeID">ID of the account type to get the accounts of</param>
        /// <returns></returns>
        public async Task<List<PsbAccount>> GetByTypeID(long typeID) {
            return (await GetAll()).Where(iter => iter.AccountType == typeID).ToList();
        }

        /// <summary>
        ///     Get a specific <see cref="PsbAccount"/> by ID
        /// </summary>
        /// <param name="ID">The ID of the account to get</param>
        /// <returns>
        ///     The <see cref="PsbAccount"/> with <see cref="PsbAccount.ID"/> of <paramref name="ID"/>,
        ///     or <c>null</c> if it doesn't exist
        /// </returns>
        public async Task<PsbAccount?> GetByID(long ID) {
            return (await GetAll()).FirstOrDefault(iter => iter.ID == ID);
        }

        /// <summary>
        ///     Insert a new <see cref="PsbAccount"/>
        /// </summary>
        /// <param name="acc">Parameters used to insert</param>
        /// <returns>The new <see cref="PsbAccount.ID"/> that was assigned</returns>
        [Obsolete("Use Create instead")]
        protected Task<long> Insert(PsbAccount acc) {
            _Cache.Remove(CACHE_KEY);
            return _Db.Insert(acc);
        }

        /// <summary>
        ///     Get a specific <see cref="PsbAccount"/> by it's tag and name
        /// </summary>
        /// <param name="tag">Tag</param>
        /// <param name="name">Name</param>
        /// <returns>
        ///     The <see cref="PsbAccount"/> with <see cref="PsbAccount.Tag"/> of <paramref name="tag"/>,
        ///     and <see cref="PsbAccount.Name"/> of <paramref name="name"/>
        /// </returns>
        public async Task<PsbAccount?> GetByTagAndName(string? tag, string name) {
            List<PsbAccount> acc = (await GetAll()).Where(iter => iter.Tag == tag && iter.Name == name && iter.DeletedAt == null).ToList();
            if (acc.Count > 1) {
                throw new Exception($"Multiple named accounts named {tag}x{name} exist");
            }

            if (acc.Count == 0) {
                return null;
            }

            return acc.ElementAt(0);
        }

        /// <summary>
        ///     Create a new <see cref="PsbAccount"/> using the tag and name passed in
        /// </summary>
        /// <param name="tag">Optional name of the account</param>
        /// <param name="name">Name of the person who the account is for</param>
        /// <param name="accountTypeID">ID of the account type. See <see cref="PsbAccountType"/></param>
        /// <returns>
        ///     The <see cref="PsbAccount"/> created
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     If a <see cref="PsbAccount"/> with <see cref="PsbAccount.Tag"/> of <paramref name="tag"/>
        ///     and <see cref="PsbAccount.Name"/> of <paramref name="name"/> already exists, this is thrown
        /// </exception>
        public async Task<PsbAccount> Create(string? tag, string name, short accountTypeID) {
            PsbAccount? dbAccount = await _Db.GetByTagAndName(tag, name);
            if (dbAccount != null && dbAccount.DeletedAt == null) {
                throw new ArgumentException($"PSB named account with tag '{tag}' and name '{name}' already exists");
            }

            PsbCharacterSet charSet = await GetCharacterSet(tag, name);

            PsbAccount acc = new PsbAccount() {
                Tag = tag,
                Name = name,
                PlayerName = name,
                AccountType = accountTypeID,

                VsID = charSet.VS?.ID,
                NcID = charSet.NC?.ID,
                TrID = charSet.TR?.ID,
                NsID = charSet.NS?.ID,
                VsStatus = charSet.VS == null ? PsbCharacterStatus.DOES_NOT_EXIST : PsbCharacterStatus.OK,
                NcStatus = charSet.NC == null ? PsbCharacterStatus.DOES_NOT_EXIST : PsbCharacterStatus.OK,
                TrStatus = charSet.TR == null ? PsbCharacterStatus.DOES_NOT_EXIST : PsbCharacterStatus.OK,
                NsStatus = charSet.NS == null ? PsbCharacterStatus.DOES_NOT_EXIST : PsbCharacterStatus.OK,
            };

#pragma warning disable CS0618 // Type or member is obsolete -- Okay, it's still used internally
            long ID = await Insert(acc);
#pragma warning restore CS0618 // Type or member is obsolete
            acc.ID = ID;

            _Cache.Remove(CACHE_KEY);

            return acc;
        }

        /// <summary>
        ///     Rename an existing account
        /// </summary>
        /// <param name="ID">ID of the account to rename</param>
        /// <param name="tag">New tag</param>
        /// <param name="name">New name</param>
        /// <returns>
        ///     The <see cref="PsbAccount"/> after the change is mode,
        ///     or <c>null</c> if the change failed
        /// </returns>
        public async Task<PsbAccount?> Rename(long ID, string? tag, string name) {
            PsbAccount? acc = await GetByID(ID);

            if (acc == null) {
                _Logger.LogWarning($"Cannot rename {nameof(PsbAccount)} to {tag}x{name}, ID {ID} does not exist");
                return null;
            }

            PsbCharacterSet set = await GetCharacterSet(tag, name);

            bool missing = set.VS == null || set.NC == null || set.TR == null || set.NS == null;
            if (missing == true) {
                _Logger.LogWarning($"One of the characters was missing. VS: {set.VS?.ID}, NC {set.NC?.ID}, TR {set.TR?.ID}, NS {set.NS?.ID} ({set.NsName})");
                return null;
            }

            PsbAccountNote note = new PsbAccountNote();
            note.AccountID = ID;
            note.HonuID = HonuAccount.SystemID;
            note.Message = $"Renamed {acc.Tag}x{acc.Name} to {tag}x{name}";

            acc.Tag = tag;
            acc.Name = name;
            acc.VsID = set.VS!.ID;
            acc.NcID = set.NC!.ID;
            acc.TrID = set.TR!.ID;
            acc.NsID = set.NS!.ID;
            acc.VsStatus = set.VS == null ? PsbCharacterStatus.DOES_NOT_EXIST : PsbCharacterStatus.OK;
            acc.NcStatus = set.NC == null ? PsbCharacterStatus.DOES_NOT_EXIST : PsbCharacterStatus.OK;
            acc.TrStatus = set.TR == null ? PsbCharacterStatus.DOES_NOT_EXIST : PsbCharacterStatus.OK;
            acc.NsStatus = set.NS == null ? PsbCharacterStatus.DOES_NOT_EXIST : PsbCharacterStatus.OK;

            await _Db.UpdateByID(ID, acc);
            await _NoteDb.Insert(acc.ID, note, CancellationToken.None);
            _Cache.Remove(CACHE_KEY);

            return acc;
        }

        /// <summary>
        ///     Set the <see cref="PsbAccount.PlayerName"/>
        /// </summary>
        /// <param name="ID">ID of the <see cref="PsbAccount"/> to set the player name of</param>
        /// <param name="playerName">New player name</param>
        /// <returns>
        ///     The <see cref="PsbAccount"/> with the new <see cref="PsbAccount.PlayerName"/> if successful,
        ///     else <c>null</c>
        /// </returns>
        public async Task<PsbAccount?> SetPlayerName(long ID, string playerName) {
            PsbAccount? acc = await GetByID(ID);

            if (acc == null) {
                _Logger.LogWarning($"Cannot set player name {nameof(PsbAccount)} to {playerName}, ID {ID} does not exist");
                return null;
            }

            acc.PlayerName = playerName;

            await _Db.UpdateByID(ID, acc);
            _Cache.Remove(CACHE_KEY);

            return acc;
        }

        /// <summary>
        ///     Update a <see cref="PsbAccount"/>
        /// </summary>
        /// <param name="accountID">ID of the account to update</param>
        /// <param name="account">Parameters used to update the account</param>
        public Task UpdateByID(long accountID, PsbAccount account) {
            _Cache.Remove(CACHE_KEY);
            return _Db.UpdateByID(accountID, account);
        }

        /// <summary>
        ///     Recheck all accounts that have any of the characters match the status passed
        /// </summary>
        /// <param name="status">Status to limit the recheck by. Pass <c>null</c> to recheck all accounts</param>
        public async Task RecheckByStatus(int? status) {
            List<PsbAccount> accounts = await GetAll();

            if (status != null) {
                accounts = accounts.Where(iter => iter.VsStatus == status || iter.NcStatus == status || iter.TrStatus == status || iter.NsStatus == status).ToList();
            }

            _Logger.LogDebug($"Rechecking {accounts.Count} accounts");

            foreach (PsbAccount acc in accounts) {
                await RecheckAccount(acc);
            }

            _Cache.Remove(CACHE_KEY);
        }

        /// <summary>
        ///     Recheck a specific account by ID
        /// </summary>
        /// <param name="ID">ID of the account to recheck</param>
        /// <returns></returns>
        public async Task<PsbAccount?> RecheckByID(long ID) {
            PsbAccount? acc = await GetByID(ID);
            if (acc == null) {
                _Logger.LogWarning($"Cannot recheck {nameof(PsbAccount)} {ID}: Does not exist");
                return null;
            }
            _Cache.Remove(CACHE_KEY);

            return await RecheckAccount(acc);
        }

        /// <summary>
        ///     Mark a PSB account as deleted
        /// </summary>
        /// <param name="ID">ID of the account to mark as deleted</param>
        /// <param name="deletedByID">ID of the honu account that is marking the account as deleted</param>
        public async Task DeleteByID(long ID, long deletedByID) {
            PsbAccount? acc = await GetByID(ID);
            if (acc != null && (acc.DeletedAt != null || acc.DeletedAt != null)) {
                _Logger.LogWarning($"Cannot delete account {ID}, already deleted by {acc.DeletedBy} at {acc.DeletedAt:u}");
                return;
            }

            await _Db.Delete(ID, deletedByID);
            _Cache.Remove(CACHE_KEY);
        }

        private async Task<PsbAccount> RecheckAccount(PsbAccount acc) {
            if (acc.VsID == null) {
                PsCharacter? c = await _CharacterCollection.GetByName(PsbNameTemplate.VS(acc.Tag, acc.Name));
                if (c != null) {
                    acc.VsID = c.ID;
                    acc.VsStatus = PsbCharacterStatus.OK;
                }
            } else {
                acc.VsStatus = await GetStatus(acc.VsID, new List<string>() { PsbNameTemplate.VS(acc.Tag, acc.Name) });
            }

            if (acc.NcID == null) {
                PsCharacter? c = await _CharacterCollection.GetByName(PsbNameTemplate.NC(acc.Tag, acc.Name));
                if (c != null) {
                    acc.NcID = c.ID;
                    acc.NcStatus = PsbCharacterStatus.OK;
                }
            } else {
                acc.NcStatus = await GetStatus(acc.NcID, new List<string>() { PsbNameTemplate.NC(acc.Tag, acc.Name) });
            }

            if (acc.TrID == null) {
                PsCharacter? c = await _CharacterCollection.GetByName(PsbNameTemplate.TR(acc.Tag, acc.Name));
                if (c != null) {
                    acc.TrID = c.ID;
                    acc.TrStatus = PsbCharacterStatus.OK;
                }
            } else {
                acc.TrStatus = await GetStatus(acc.TrID, new List<string>() { PsbNameTemplate.TR(acc.Tag, acc.Name) });
            }

            if (acc.NsID == null) {
                PsbCharacterSet set = await GetCharacterSet(acc.Tag, acc.Name);

                if (set.NS != null) {
                    acc.NsID = set.NS.ID;
                    acc.NsStatus = PsbCharacterStatus.OK;
                }
            } else {
                acc.NsStatus = await GetStatus(acc.NsID, PsbNameTemplate.NS(acc.Tag, acc.Name));
            }

            await _Db.UpdateByID(acc.ID, acc);

            return acc;
        }

        /// <summary>
        ///     Get the <see cref="PsbCharacterSet"/> of a tag and name
        /// </summary>
        /// <param name="tag">Optional tag</param>
        /// <param name="name">Name of the character</param>
        /// <returns></returns>
        public async Task<PsbCharacterSet> GetCharacterSet(string? tag, string name) {
            string ncName = PsbNameTemplate.NC(tag, name);
            string vsName = PsbNameTemplate.VS(tag, name);
            string trName = PsbNameTemplate.TR(tag, name);
            List<string> nsNames = PsbNameTemplate.NS(tag, name);

            List<PsCharacter> characters = await GetCharacters(tag, name);

            PsbCharacterSet set = new PsbCharacterSet();
            set.VS = characters.FirstOrDefault(iter => iter.Name == vsName);
            set.NC = characters.FirstOrDefault(iter => iter.Name == ncName);
            set.TR = characters.FirstOrDefault(iter => iter.Name == trName);

            foreach (string nsName in nsNames) {
                PsCharacter? ns = characters.FirstOrDefault(iter => iter.Name == nsName);
                if (ns != null) {
                    set.NS = ns;
                    set.NsName = nsName;
                    break;
                }
            }

            return set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<List<PsCharacter>> GetCharacters(string? tag, string name) {
            string ncName = PsbNameTemplate.NC(tag, name);
            string vsName = PsbNameTemplate.VS(tag, name);
            string trName = PsbNameTemplate.TR(tag, name);
            List<string> nsNames = PsbNameTemplate.NS(tag, name);

            List<string> names = new List<string>() {
                trName, ncName, vsName
            };
            names.AddRange(nsNames);

            List<PsCharacter> characters = new();

            foreach (string iter in names) {
                PsCharacter? c = await _CharacterCollection.GetByName(iter);

                if (c != null) {
                    characters.Add(c);
                }
            }

            return characters;
        }

        private async Task<int> GetStatus(string? charID, List<string> names) {
            PsCharacter? byID = (charID != null) ? await _CharacterCollection.GetByID(charID, CensusEnvironment.PC) : null;

            List<PsCharacter> charNames = await _CharacterCollection.GetByNames(names);

            string? usedName = null;
            PsCharacter? byName = null;
            foreach (string name in names) {
                byName = charNames.FirstOrDefault(iter => iter.Name == name);
                if (byName != null) {
                    usedName = name;
                    break;
                }
            }

            _Logger.LogTrace($"Getting character status for ID={charID}, Names={string.Join(", ", names)} :: byID={byID?.Name}, byName={byName?.ID} ({usedName})");

            if (charID != null) {
                _Logger.LogTrace($"charID {charID} is not null, checking if the character exists");
                if (byID == null) {
                    _Logger.LogTrace($"Character {charID} does not exist, checking if the name exists");
                    if (byName != null) {
                        _Logger.LogTrace($"Character {charID} is null, but a character with the name of {string.Join(", ", names)} does exist, REMADE");
                        return PsbCharacterStatus.REMADE;
                    } else {
                        _Logger.LogTrace($"Character {charID} is null, and no character with name of {string.Join(", ", names)} exists, DELETED");
                        return PsbCharacterStatus.DELETED;
                    }
                } else {
                    _Logger.LogTrace($"Character {charID} exists, all is good");
                    return PsbCharacterStatus.OK;
                }
            }

            _Logger.LogTrace($"charID is null, character does not exist");

            return PsbCharacterStatus.DOES_NOT_EXIST;
        }

    }
}
