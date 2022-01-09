using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Models.PSB;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories.PSB {

    public class PsbNamedRepository {

        private readonly ILogger<PsbNamedRepository> _Logger;
        private readonly PsbNamedDbStore _Db;
        private readonly ICharacterRepository _CharacterRepository;
        private readonly ICharacterCollection _CharacterCollection;

        private readonly IMemoryCache _Cache;

        public PsbNamedRepository(ILogger<PsbNamedRepository> logger,
            PsbNamedDbStore db, ICharacterRepository charRepo,
            IMemoryCache cache, ICharacterCollection charColl) {

            _Logger = logger;
            _Db = db;
            _CharacterRepository = charRepo;
            _CharacterCollection = charColl;
            _Cache = cache;
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
                VsID = charSet.VS?.ID,
                NcID = charSet.NC?.ID,
                TrID = charSet.TR?.ID,
                NsID = charSet.NS?.ID,
                Notes = null
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
        /// <returns></returns>
        public async Task<bool> Rename(long ID, string? tag, string name) {
            PsbNamedAccount? acc = await GetByID(ID);

            if (acc == null) {
                _Logger.LogWarning($"Cannot rename {nameof(PsbNamedAccount)} to {tag}x{name}, ID {ID} does not exist");
                return false;
            }

            PsbCharacterSet set = await GetCharacterSet(tag, name);

            bool missing = set.VS == null || set.NC == null || set.TR == null || set.NS == null;
            if (missing == true) {
                _Logger.LogWarning($"One of the characters was missing. VS: {set.VS?.ID}, NC {set.NC?.ID}, TR {set.TR?.ID}, NS {set.NS?.ID} ({set.NsName})");
                return false;
            }

            acc.Tag = tag;
            acc.Name = name;
            acc.VsID = set.VS!.ID;
            acc.NcID = set.NC!.ID;
            acc.TrID = set.TR!.ID;
            acc.NsID = set.NS!.ID;

            await _Db.UpdateByID(ID, acc);

            return true;
        }

        /// <summary>
        ///     Get the <see cref="PsbCharacterSet"/> of a tag and name
        /// </summary>
        /// <param name="tag">Optional tag</param>
        /// <param name="name">Name of the character</param>
        /// <returns></returns>
        public async Task<PsbCharacterSet> GetCharacterSet(string? tag, string name) {
            string ncName = PsbNameTemplates.NC(tag, name);
            string vsName = PsbNameTemplates.VS(tag, name);
            string trName = PsbNameTemplates.TR(tag, name);
            List<string> nsNames = PsbNameTemplates.NS(tag, name);

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
            string ncName = PsbNameTemplates.NC(tag, name);
            string vsName = PsbNameTemplates.VS(tag, name);
            string trName = PsbNameTemplates.TR(tag, name);
            List<string> nsNames = PsbNameTemplates.NS(tag, name);

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

    }
}
