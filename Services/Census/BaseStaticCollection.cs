using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using DaybreakGames.Census;
using DaybreakGames.Census.Exceptions;
using DaybreakGames.Census.Operators;

namespace watchtower.Services.Census {

    public class BaseStaticCollection<T> : IStaticCollection<T> where T : class {

        internal readonly ICensusQueryFactory _Census;
        internal readonly ICensusReader<T> _Reader;
        internal readonly string _CollectionName;

        public BaseStaticCollection(string collectionName,
            ICensusQueryFactory census, ICensusReader<T> reader) {

            _CollectionName = collectionName;
            _Census = census;
            _Reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

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

            return entries;
        }

    }

}