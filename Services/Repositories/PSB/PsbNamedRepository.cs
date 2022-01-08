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

        public async Task<PsbNamedAccount> Create(string? tag, string name) {
            PsbNamedAccount? dbAccount = await _Db.GetByTagAndName(tag, name);
            if (dbAccount != null) {
                throw new ArgumentException($"PSB named account with tag '{tag}' and name '{name}' already exists");
            }

            string baseName = $"{tag}x{name}";
            string trName = $"{tag}x{name}TR";
            string ncName = $"{tag}x{name}NC";
            string vsName = $"{tag}x{name}VS";
            string nsName = $"{tag}x{name}NS";
            string doNotUseName = $"{tag}x{name}DONOTUSE";
            string doNotUse2Name = $"DONOTUSEx{name}";

            List<string> names = new List<string>() { trName, ncName, vsName, nsName, doNotUseName, doNotUse2Name };

            List<PsCharacter> characters = new List<PsCharacter>();

            foreach (string iter in names) {
                PsCharacter? c = await _CharacterCollection.GetByName(iter);

                if (c != null) {
                    characters.Add(c);
                }
            }

            PsCharacter? vsChar = characters.FirstOrDefault(iter => iter.Name == vsName);
            PsCharacter? ncChar = characters.FirstOrDefault(iter => iter.Name == ncName);
            PsCharacter? trChar = characters.FirstOrDefault(iter => iter.Name == trName);
            PsCharacter? nsChar = characters.FirstOrDefault(iter => iter.Name == nsName)
                ?? characters.FirstOrDefault(iter => iter.Name == doNotUseName)
                ?? characters.FirstOrDefault(iter => iter.Name == doNotUse2Name);

            PsbNamedAccount acc = new PsbNamedAccount() {
                Tag = tag,
                Name = name,
                VsID = vsChar?.ID,
                NcID = ncChar?.ID,
                TrID = trChar?.ID,
                NsID = nsChar?.ID,
                Notes = ""
            };

            await Upsert(acc);

            return acc;
        }

    }
}
