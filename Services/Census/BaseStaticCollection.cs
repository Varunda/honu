using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text.Json;
using System.Threading.Tasks;
using DaybreakGames.Census;
using DaybreakGames.Census.Exceptions;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using watchtower.Code.ExtensionMethods;

namespace watchtower.Services.Census {

    /// <summary>
    ///     Static collection wrapper
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseStaticCollection<T> : IStaticCollection<T> where T : class {

        internal readonly ICensusQueryFactory _Census;
        internal readonly ICensusReader<T> _Reader;
        internal readonly string _CollectionName;

        /// <summary>
        ///     What file will be used to patch the Census response, adding new data and updating entries
        /// </summary>
        internal string? _PatchFile;
        internal Func<T, string>? _KeyFunc;
        internal Action<T, T>? _CopyFunc;

        internal ILogger<BaseStaticCollection<T>> _Logger;

        public BaseStaticCollection(ILogger<BaseStaticCollection<T>> logger,
            string collectionName, ICensusQueryFactory census,
            ICensusReader<T> reader) {

            _CollectionName = collectionName;

            _Logger = logger;

            _Census = census;
            _Reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        /// <summary>
        ///     Get all entries in this collection, applying a patch file if it exists
        /// </summary>
        /// <remarks>
        ///     This method will get an entire collection, even if it can't fit into the limit of one return.
        ///     For example, the item colleciton has >20k entries. Census caps us at 5k entries per request,
        ///     so Honu will continue to get pages with an offset
        ///     
        ///     There are 2 possible downsides to this method:
        ///         1. If the number of entries if a multiple of the page size, an extra request which will return 0 entries
        ///             is made. I can't think of a way to prevent this, but I doubt it'll come up very often
        ///         2. If one of the "pages" times out, that data will be missing
        /// </remarks>
        /// <returns>
        ///     A list of <typeparamref name="T"/> that was read from Census
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">
        ///     The static collection had more pages than Honu retrieved. Honu currently gets 10 pages of 5k entries, 
        ///     so if a static collection has more than 50k entries, this limit will need to be upped
        /// </exception>
        public async Task<List<T>> GetAll() {
            CensusQuery query = _Census.Create(_CollectionName);
            query.SetLimit(5_000);

            List<T> entries = new List<T>();

            // For static collections that have more than 5k entries, we'll need to loop over
            //      them to read the whole list. Using a for loop instead of a while (true)
            //      makes me sleep a bit better a night knowing it will eventually throw an
            //      exception if for some reason there's more than 50k entries in a static collection  
            for (int i = 0; i < 10; ++i) {
                query.SetStart(i * (query.Limit ?? 5_000));

                List<T> block;
                try {
                    block = await _Reader.ReadList(query);

                    entries.AddRange(block);

                    if (block.Count < query.Limit) {
                        break;
                    }

                    if (i == 9 && block.Count == query.Limit) {
                        throw new IndexOutOfRangeException($"After {i} iterations, collection '{_CollectionName}' returned {block.Count} entries with a limit of {query.Limit}. Increase i or the limit per iteration");
                    }
                } catch (CensusConnectionException) {
                    // Catch timeout exceptions here
                }
            }

            if (_PatchFile != null) {
                _Logger.LogInformation($"Patching using file {_PatchFile}, using a copy func? {_CopyFunc != null}");
                if (_KeyFunc == null) {
                    throw new Exception($"If _PatchFile is set, _KeyFunc must also be set");
                }

                Dictionary<string, T> dict = new();
                foreach (T entry in entries) {
                    dict[_KeyFunc(entry)] = entry;
                }

                if (File.Exists(_PatchFile) == false) {
                    throw new FileNotFoundException($"Failed to find patch file {_PatchFile}");
                }


                JsonElement readElement(byte[] file) {
                    Utf8JsonReader reader = new(file);
                    JsonElement patchedJson = JsonElement.ParseValue(ref reader);
                    return patchedJson;
                }

                // Make sure the token has the data we need
                byte[] bytes = await File.ReadAllBytesAsync(_PatchFile);
                JsonElement patchedJson = readElement(bytes);
                JsonElement? list = patchedJson.GetChild($"{_CollectionName}_list");
                if (list == null) {
                    throw new NullReferenceException($"missing token '{_CollectionName}_list' from {_PatchFile}");
                }

                IEnumerable<JsonElement> arr = list.Value.EnumerateArray();
                _Logger.LogDebug($"Loaded {arr.Count()} entries to patch");

                int newEntries = 0;
                int updatedEntries = 0;

                // Iterate thru each entry, updating the existing entry if the patch has it, or adding it to the entries
                foreach (JsonElement token in arr) {
                    T? entry = _Reader.ReadEntry(token);
                    if (entry != null) {
                        string key = _KeyFunc(entry);
                        if (dict.TryGetValue(key, out T? dictEntry) == true) {
                            if (_CopyFunc != null) {
                                _CopyFunc(dictEntry, entry);
                            }
                            dict[key] = entry;
                            ++updatedEntries;
                        } else {
                            dict.Add(key, entry);
                            ++newEntries;
                        }
                    }
                }

                _Logger.LogDebug($"Updated {updatedEntries} and created {newEntries} new entries");

                entries = dict.Values.ToList();
            }

            return entries;
        }

    }

}