using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.PSB;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories.PSB {

    public class PsbAccountRepository {

        private readonly ILogger<PsbAccountRepository> _Logger;
        private readonly PsbNamedDbStore _Db;
        private readonly CharacterRepository _CharacterRepository;
        private readonly CharacterCollection _CharacterCollection;
        private readonly PsbAccountNoteDbStore _NoteDb;

        private readonly IMemoryCache _Cache;

        public PsbAccountRepository(ILogger<PsbAccountRepository> logger,
            PsbNamedDbStore db, CharacterRepository charRepo,
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
        ///     Get all <see cref="PsbNamedAccount"/>s
        /// </summary>
        public async Task<List<PsbNamedAccount>> GetAll() {
            return await _Db.GetAll();
        }

        /// <summary>
        ///     Get a specific <see cref="PsbNamedAccount"/> by ID
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public Task<PsbNamedAccount?> GetByID(long ID) {
            return _Db.GetByID(ID);
        }

        /// <summary>
        ///     Insert a new <see cref="PsbNamedAccount"/>
        /// </summary>
        /// <param name="acc">Parameters used to insert</param>
        /// <returns>The new <see cref="PsbNamedAccount.ID"/> that was assigned</returns>
        public Task<long> Insert(PsbNamedAccount acc) {
            return _Db.Insert(acc);
        }

        /// <summary>
        ///     Get a specific <see cref="PsbNamedAccount"/> by it's tag and name
        /// </summary>
        /// <param name="tag">Tag</param>
        /// <param name="name">Name</param>
        /// <returns>
        ///     The <see cref="PsbNamedAccount"/> with <see cref="PsbNamedAccount.Tag"/> of <paramref name="tag"/>,
        ///     and <see cref="PsbNamedAccount.Name"/> of <paramref name="name"/>
        /// </returns>
        public Task<PsbNamedAccount?> GetByTagAndName(string? tag, string name) {
            return _Db.GetByTagAndName(tag, name);
        }

        /// <summary>
        ///     Create a new <see cref="PsbNamedAccount"/> using the tag and name passed in
        /// </summary>
        /// <param name="tag">Optional name of the account</param>
        /// <param name="name">Name of the person who the account is for</param>
        /// <returns>
        ///     The <see cref="PsbNamedAccount"/> created
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     If a <see cref="PsbNamedAccount"/> with <see cref="PsbNamedAccount.Tag"/> of <paramref name="tag"/>
        ///     and <see cref="PsbNamedAccount.Name"/> of <paramref name="name"/> already exists, this is thrown
        /// </exception>
        public async Task<PsbNamedAccount> Create(string? tag, string name) {
            PsbNamedAccount? dbAccount = await _Db.GetByTagAndName(tag, name);
            if (dbAccount != null) {
                throw new ArgumentException($"PSB named account with tag '{tag}' and name '{name}' already exists");
            }

            PsbCharacterSet charSet = await GetCharacterSet(tag, name);

            PsbNamedAccount acc = new PsbNamedAccount() {
                Tag = tag,
                Name = name,
                PlayerName = name,
                VsID = charSet.VS?.ID,
                NcID = charSet.NC?.ID,
                TrID = charSet.TR?.ID,
                NsID = charSet.NS?.ID,
                VsStatus = charSet.VS == null ? PsbCharacterStatus.DOES_NOT_EXIST : PsbCharacterStatus.OK,
                NcStatus = charSet.NC == null ? PsbCharacterStatus.DOES_NOT_EXIST : PsbCharacterStatus.OK,
                TrStatus = charSet.TR == null ? PsbCharacterStatus.DOES_NOT_EXIST : PsbCharacterStatus.OK,
                NsStatus = charSet.NS == null ? PsbCharacterStatus.DOES_NOT_EXIST : PsbCharacterStatus.OK,
            };

            long ID = await Insert(acc);
            acc.ID = ID;

            return acc;
        }

        /// <summary>
        ///     Rename an existing account
        /// </summary>
        /// <param name="ID">ID of the account to rename</param>
        /// <param name="tag">New tag</param>
        /// <param name="name">New name</param>
        /// <returns>
        ///     The <see cref="PsbNamedAccount"/> after the change is mode,
        ///     or <c>null</c> if the change failed
        /// </returns>
        public async Task<PsbNamedAccount?> Rename(long ID, string? tag, string name) {
            PsbNamedAccount? acc = await GetByID(ID);

            if (acc == null) {
                _Logger.LogWarning($"Cannot rename {nameof(PsbNamedAccount)} to {tag}x{name}, ID {ID} does not exist");
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

            return acc;
        }

        /// <summary>
        ///     Set the <see cref="PsbNamedAccount.PlayerName"/>
        /// </summary>
        /// <param name="ID">ID of the <see cref="PsbNamedAccount"/> to set the player name of</param>
        /// <param name="playerName">New player name</param>
        /// <returns>
        ///     The <see cref="PsbNamedAccount"/> with the new <see cref="PsbNamedAccount.PlayerName"/> if successful,
        ///     else <c>null</c>
        /// </returns>
        public async Task<PsbNamedAccount?> SetPlayerName(long ID, string playerName) {
            PsbNamedAccount? acc = await GetByID(ID);

            if (acc == null) {
                _Logger.LogWarning($"Cannot set player name {nameof(PsbNamedAccount)} to {playerName}, ID {ID} does not exist");
                return null;
            }

            acc.PlayerName = playerName;

            await _Db.UpdateByID(ID, acc);

            return acc;
        }

        public async Task RecheckByStatus(int? status) {
            List<PsbNamedAccount> accounts = await GetAll();

            if (status != null) {
                accounts = accounts.Where(iter => iter.VsStatus == status || iter.NcStatus == status || iter.TrStatus == status || iter.NsStatus == status).ToList();
            }

            _Logger.LogDebug($"Rechecking {accounts.Count} accounts");

            foreach (PsbNamedAccount acc in accounts) {
                await RecheckAccount(acc);
            }
        }

        /// <summary>
        ///     Recheck a specific account by ID
        /// </summary>
        /// <param name="ID">ID of the account to recheck</param>
        /// <returns></returns>
        public async Task<PsbNamedAccount?> RecheckByID(long ID) {
            PsbNamedAccount? acc = await GetByID(ID);
            if (acc == null) {
                _Logger.LogWarning($"Cannot recheck {nameof(PsbNamedAccount)} {ID}: Does not exist");
                return null;
            }

            return await RecheckAccount(acc);
        }

        /// <summary>
        ///     Mark a PSB account as deleted
        /// </summary>
        /// <param name="ID">ID of the account to mark as deleted</param>
        /// <param name="deletedByID">ID of the honu account that is marking the account as deleted</param>
        public async Task DeleteByID(long ID, long deletedByID) {
            PsbNamedAccount? acc = await GetByID(ID);
            if (acc != null && (acc.DeletedAt != null || acc.DeletedAt != null)) {
                _Logger.LogWarning($"Cannot delete account {ID}, already deleted by {acc.DeletedBy} at {acc.DeletedAt:u}");
                return;
            }

            await _Db.Delete(ID, deletedByID);
        }

        private async Task<PsbNamedAccount> RecheckAccount(PsbNamedAccount acc) {
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

            List<PsCharacter> characters = new List<PsCharacter>();

            foreach (string iter in names) {
                PsCharacter? c = await _CharacterCollection.GetByName(iter);

                if (c != null) {
                    characters.Add(c);
                }
            }

            return characters;
        }

        private async Task<int> GetStatus(string? charID, List<string> names) {
            PsCharacter? byID = (charID != null) ? await _CharacterCollection.GetByID(charID) : null;

            string? usedName = null;
            PsCharacter? byName = null;
            foreach (string name in names) {
                byName = await _CharacterCollection.GetByName(name);
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
