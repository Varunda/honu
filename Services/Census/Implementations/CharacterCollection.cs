using DaybreakGames.Census;
using DaybreakGames.Census.Exceptions;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

//using honu_census;

namespace watchtower.Services.Census.Implementations {

    public class CharacterCollection : ICharacterCollection {

        private readonly ILogger<CharacterCollection> _Logger;

        private readonly ICensusQueryFactory _Census;
        private readonly ICensusReader<PsCharacter> _Reader;
        //private readonly HonuCensus _HCensus;

        private const int BATCH_SIZE = 50;

        public CharacterCollection(ILogger<CharacterCollection> logger,
                ICensusQueryFactory factory, ICensusReader<PsCharacter> reader) { //, HonuCensus hc) {

            _Logger = logger;
            _Census = factory;
            _Reader = reader;
            //_HCensus = hc ?? throw new ArgumentNullException(nameof(hc));
        }

        public async Task<PsCharacter?> GetByName(string name) {
            /*
            _HCensus.AddServiceId("asdf");
            honu_census.Models.CensusQuery q = _HCensus.New("character");
            q.WhereEquals("name.first_lower", name.ToLower());

            JToken? ttt = await _HCensus.GetSingle(q, CancellationToken.None);
            if (ttt != null) {
                PsCharacter? cc = _ParseCharacter(ttt);
                return cc;
            }
            */

            PsCharacter? c = await _GetCharacterFromCensusByName(name, true);

            return c;
        }

        public async Task<PsCharacter?> GetByID(string ID) {
            PsCharacter? c = await _GetCharacterFromCensus(ID, true);

            return c;
        }

        public async Task<List<PsCharacter>> GetByIDs(List<string> IDs) {
            int batchCount = (int) Math.Ceiling(IDs.Count / (double) BATCH_SIZE);

            //_Logger.LogTrace($"Doing {batchCount} batches to get {IDs.Count} characters");

            List<PsCharacter> chars = new List<PsCharacter>(IDs.Count);

            for (int i = 0; i < batchCount; ++i) {
                //_Logger.LogTrace($"Getting indexes {i * BATCH_SIZE} - {i * BATCH_SIZE + BATCH_SIZE}");
                List<string> slice = IDs.Skip(i * BATCH_SIZE).Take(BATCH_SIZE).ToList();

                //_Logger.LogTrace($"Slize size: {slice.Count}");

                CensusQuery query = _Census.Create("character");
                foreach (string id in slice) {
                    query.Where("character_id").Equals(id);
                }
                query.SetLimit(10_000);
                query.AddResolve("outfit", "world");

                // If there is an exception, ignore census connection ones
                try {
                    List<PsCharacter> sliceCharacters = await _Reader.ReadList(query);
                    chars.AddRange(sliceCharacters);
                } catch (CensusConnectionException) {
                    _Logger.LogWarning($"Failed to get slice {i * BATCH_SIZE} - {(i + 1) * BATCH_SIZE}, had timeout");
                    continue;
                }
            }

            return chars;
        }

        public async Task<List<PsCharacter>> SearchByName(string name, CancellationToken stop) {
            // Cannot search less than 3 characters in Census
            if (name.Length < 3) {
                return new List<PsCharacter>();
            }

            CensusQuery query = _Census.Create("character");
            query.Where("name.first_lower").Contains(name);
            query.SetLimit(100);
            query.AddResolve("outfit", "world");

            List<PsCharacter> chars;

            try {
                chars = await _Reader.ReadList(query);
            } catch (Exception) when (stop.IsCancellationRequested == false) {
                throw;
            } catch (Exception) when (stop.IsCancellationRequested == true) {
                _Logger.LogWarning($"Stopping name search");
                return new List<PsCharacter>();
            }

            return chars;
        }

        private async Task<PsCharacter?> _GetCharacterFromCensus(string ID, bool retry) {
            CensusQuery query = _Census.Create("character");

            query.Where("character_id").Equals(ID);
            query.AddResolve("outfit", "world");

            try {
                return await _Reader.ReadSingle(query);
            } catch (CensusConnectionException ex) {
                if (retry == true) {
                    _Logger.LogWarning("Retrying {Char} from API", ID);
                    return await _GetCharacterFromCensus(ID, false); 
                } else {
                    _Logger.LogError(ex, "Failed to get {0} from API", ID);
                    throw;
                }
            }
        }

        private async Task<PsCharacter?> _GetCharacterFromCensusByName(string name, bool retry) {
            CensusQuery query = _Census.Create("character");

            query.Where("name.first_lower").Equals(name.ToLower());
            query.AddResolve("outfit", "world");

            try {
                return await _Reader.ReadSingle(query);
            } catch (CensusConnectionException ex) {
                if (retry == true) {
                    _Logger.LogWarning("Retrying {Char} from API", name);
                    return await _GetCharacterFromCensusByName(name, false); 
                } else {
                    _Logger.LogError(ex, "Failed to get {0} from API", name);
                    throw;
                }
            }
        }

    }
}
