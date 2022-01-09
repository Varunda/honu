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

        public async Task<List<PsbNamedAccount>> GetAll() {
            return await _Db.GetAll();
        }

        public Task Upsert(PsbNamedAccount acc) {
            return _Db.Upsert(acc);
        }

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

            await Upsert(acc);

            return acc;
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
            string nsName = PsbNameTemplates.NS(tag, name);
            string doNotUseName = $"{tag}x{name}DONOTUSE";
            string doNotUse2Name = $"DONOTUSEx{name}";

            List<PsCharacter> characters = await GetCharacters(tag, name);

            PsbCharacterSet set = new PsbCharacterSet();
            set.VS = characters.FirstOrDefault(iter => iter.Name == vsName);
            set.NC = characters.FirstOrDefault(iter => iter.Name == ncName);
            set.TR = characters.FirstOrDefault(iter => iter.Name == trName);
            set.NS = characters.FirstOrDefault(iter => iter.Name == nsName)
                ?? characters.FirstOrDefault(iter => iter.Name == doNotUseName)
                ?? characters.FirstOrDefault(iter => iter.Name == doNotUse2Name);

            return set;
        }

        public async Task<List<PsCharacter>> GetCharacters(string? tag, string name) {
            string ncName = PsbNameTemplates.NC(tag, name);
            string vsName = PsbNameTemplates.VS(tag, name);
            string trName = PsbNameTemplates.TR(tag, name);
            string nsName = PsbNameTemplates.NS(tag, name);
            string doNotUseName = $"{tag}x{name}DONOTUSE";
            string doNotUse2Name = $"DONOTUSEx{name}";

            List<string> names = new List<string>() { trName, ncName, vsName, nsName, doNotUseName, doNotUse2Name };

            List<PsCharacter> characters = new List<PsCharacter>();

            foreach (string iter in names) {
                PsCharacter? c = await _CharacterCollection.GetByName(iter);

                if (c != null && c.WorldID == 19) {
                    characters.Add(c);
                }
            }

            return characters;
        }

    }
}
