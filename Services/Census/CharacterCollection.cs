using DaybreakGames.Census;
using DaybreakGames.Census.Exceptions;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public class CharacterCollection {

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

        /// <summary>
        ///     Get a <see cref="PsCharacter"/> by name
        /// </summary>
        /// <param name="name">Name of the <see cref="PsCharacter"/> to get</param>
        /// <returns>
        ///     The <see cref="PsCharacter"/> with <see cref="PsCharacter.Name"/> of <paramref name="name"/>,
        ///     or <c>null</c> if it doesn't exist
        /// </returns>
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

            CensusQuery query = _Census.Create("character");

            query.Where("name.first_lower").Equals(name.ToLower());
            query.AddResolve("outfit", "world");

            PsCharacter? c = await _Reader.ReadSingle(query);

            return c;
        }

        /// <summary>
        ///     Get characters (case-insensitive) based on names
        /// </summary>
        /// <param name="names"></param>
        /// <returns></returns>
        public async Task<List<PsCharacter>> GetByNames(IEnumerable<string> names) {
            CensusQuery query = _Census.Create("character");
            foreach (string name in names) {
                query.Where("name.first_lower").Equals(name.ToLower());
            }
            //query.Where("name.first_lower").Equals(names.Select(iter => iter.ToLower()));
            query.AddResolve("outfit", "world");

            List<PsCharacter> chars = await _Reader.ReadList(query);
            return chars;
        }

        /// <summary>
        ///     Get a <see cref="PsCharacter"/> by ID
        /// </summary>
        /// <param name="ID">ID of the character to get</param>
        /// <param name="env">What census environment to make the call in</param>
        /// <returns>
        ///     The <see cref="PsCharacter"/> with <see cref="PsCharacter.ID"/> of <paramref name="ID"/>,
        ///     or <c>null</c> if it doesn't exist
        /// </returns>
        public async Task<PsCharacter?> GetByID(string ID, CensusEnvironment env) {
            CensusQuery query = _Census.Create("character");
            query.SetServiceNamespace(CensusEnvironmentHelper.ToNamespace(env));

            query.Where("character_id").Equals(ID);
            query.AddResolve("outfit", "world");

            PsCharacter? c = await _Reader.ReadSingle(query);

            return c;
        }

        /// <summary>
        ///     Get a list of characters by IDs
        /// </summary>
        /// <param name="IDs">IDs to get from Census</param>
        /// <param name="env">What census environment to make the call in</param>
        /// <returns>
        ///     A list of <see cref="PsCharacter"/> with a <see cref="PsCharacter.ID"/>
        ///     as an element of <paramref name="IDs"/>
        /// </returns>
        public async Task<List<PsCharacter>> GetByIDs(List<string> IDs, CensusEnvironment env) {
            if (IDs.Count == 0) {
                return new List<PsCharacter>();
            }

            int batchCount = (int) Math.Ceiling(IDs.Count / (double) BATCH_SIZE);

            //_Logger.LogTrace($"Doing {batchCount} batches to get {IDs.Count} characters");

            List<PsCharacter> chars = new List<PsCharacter>(IDs.Count);

            int errorCount = 0;

            for (int i = 0; i < batchCount; ++i) {
                //_Logger.LogTrace($"Getting indexes {i * BATCH_SIZE} - {i * BATCH_SIZE + BATCH_SIZE}");
                List<string> slice = IDs.Skip(i * BATCH_SIZE).Take(BATCH_SIZE).ToList();

                //_Logger.LogTrace($"Slize size: {slice.Count}");

                CensusQuery query = _Census.Create("character");
                query.SetServiceNamespace(CensusEnvironmentHelper.ToNamespace(env));
                foreach (string id in slice) {
                    query.Where("character_id").Equals(id);
                }
                query.SetLimit(10_000);
                query.AddResolve("outfit", "world");

                // If there is an exception, ignore census connection ones
                try {
                    List<PsCharacter> sliceCharacters = await _Reader.ReadList(query);
                    chars.AddRange(sliceCharacters);
                } catch (Exception ex) when (ex is CensusConnectionException || ex is TaskCanceledException) {
                    ++errorCount;
                    _Logger.LogWarning($"failed to get slice {i * BATCH_SIZE} - {(i + 1) * BATCH_SIZE}, had timeout");

                    if (errorCount >= 5) {
                        _Logger.LogError($"failed {errorCount} times, assuming Census is down");
                        break;
                    }

                    continue;
                } catch (Exception ex) {
                    ++errorCount;
                    _Logger.LogError(ex, $"failed to get slice {i * BATCH_SIZE} - {(i + 1) * BATCH_SIZE}");

                    if (errorCount >= 5) {
                        _Logger.LogError($"failed {errorCount} times, assuming Census is down");
                        break;
                    }

                    continue;
                }
            }

            return chars;
        }

        /// <summary>
        ///     Search for characters by name
        /// </summary>
        /// <remarks>
        ///     Census does not support wildcard searching with less than 3 characters, and if
        ///     less than 3 characters are passed, an empty list will be returned
        /// </remarks>
        /// <param name="name">Name to search by</param>
        /// <param name="stop">Stopping token</param>
        /// <returns>
        ///     A list of <see cref="PsCharacter"/>s that match the name given
        /// </returns>
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

    }

}
