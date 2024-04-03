using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.PSB;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories.PSB {
    public class PsbOvOAccountUsageRepository {

        private readonly ILogger<PsbOvOAccountUsageRepository> _Logger;

        private readonly IMemoryCache _Cache;
        private const string CACHE_KEY = "Psb.AccountUsage.{0}"; // {0} => gdrive file sheet ID

        private readonly CharacterRepository _CharacterRepository;
        private readonly SessionDbStore _SessionDb;

        public PsbOvOAccountUsageRepository(ILogger<PsbOvOAccountUsageRepository> logger,
            CharacterRepository characterRepository, SessionDbStore sessionDb,
            IMemoryCache cache) {

            _Logger = logger;
            _Cache = cache;

            _CharacterRepository = characterRepository;
            _SessionDb = sessionDb;
        }

        /// <summary>
        ///     check the usage of the accounts within a OvO account sheet given out
        /// </summary>
        /// <param name="sheet">sheet used that has all the information. This can be found by using <see cref="PsbOvOSheetRepository.GetByID(string)"/></param>
        /// <returns>
        ///     a list of <see cref="PsbOvOAccountUsage"/>s for the accounts used
        /// </returns>
        public async Task<List<PsbOvOAccountUsage>> CheckUsage(PsbOvOAccountSheet sheet) {
            string cacheKey = string.Format(CACHE_KEY, sheet.FileID);

            if (_Cache.TryGetValue(cacheKey, out List<PsbOvOAccountUsage> usages) == true) {
                return usages;
            }

            // get all the character names that could have sessions (TR, NS and VS)
            List<string> characters = sheet.Accounts.Select(iter => $"PSBx{iter.AccountNumber.PadLeft(4, '0')}")
                .Aggregate(new List<string>(), (acc, iter) => {
                    acc.Add($"{iter}VS");
                    acc.Add($"{iter}NC");
                    acc.Add($"{iter}TR");
                    acc.Add($"{iter}NS");
                    return acc;
                });
            _Logger.LogInformation($"checking usage for accounts: [{string.Join(", ", characters)}]");

            // find all the characters that have one of the names we care about
            List<PsCharacter> chars = new(characters.Count);
            foreach (string charName in characters) {
                // fast, as we know the character exists in the db
                PsCharacter? c = await _CharacterRepository.GetFirstByName(charName, fast: true);
                if (c != null) {
                    chars.Add(c);
                } else {
                    _Logger.LogError($"missing character {charName}!!!");
                }
            }

            // *4 cause each account has 4 characters to check
            _Logger.LogInformation($"loaded {chars.Count}/{sheet.Accounts.Count * 4} characters");

            // for each character ID that we care about, get their sessions around the account booking
            List<Session> sessions = new();
            foreach (string charID in chars.Select(iter => iter.ID)) {
                DateTime rangeStart = sheet.When - TimeSpan.FromHours(4);
                DateTime rangeEnd = sheet.When + TimeSpan.FromHours(12);

                _Logger.LogDebug($"loading sessions for single character [charID={charID}] [rangeStart={rangeStart:u}] [rangeEnd={rangeEnd:u}]");

                sessions.AddRange(await _SessionDb.GetByRangeAndCharacterID(charID, rangeStart, rangeEnd));
            }

            _Logger.LogInformation($"loaded sessions for characters [sessions.Count={sessions.Count}] [chars.Count={chars.Count}]");

            usages = new List<PsbOvOAccountUsage>();

            foreach (PsbOvOAccountSheetUsage account in sheet.Accounts) {
                // get all character IDs that match the PSB account names (PSBx####)
                string accountPrefix = $"PSBx{account.AccountNumber.PadLeft(4, '0')}";
                List<string> accountChars = chars.Where(iter => iter.Name.StartsWith(accountPrefix)).Select(iter => iter.ID).ToList();

                PsbOvOAccountUsage usage = new(account);
                usage.Sessions = sessions.Where(iter => accountChars.Contains(iter.CharacterID)).ToList();

                usages.Add(usage);
            }

            _Cache.Set(cacheKey, usages, new MemoryCacheEntryOptions() {
                SlidingExpiration = TimeSpan.FromMinutes(5)
            });

            return usages;
        }

    }
}
