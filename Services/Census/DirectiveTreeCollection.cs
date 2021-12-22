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

namespace watchtower.Services.Census {

    public class DirectiveTreeCollection {

        private readonly ILogger<DirectiveTreeCollection> _Logger;
        private readonly ICensusQueryFactory _Census;
        private readonly ICensusReader<DirectiveTree> _Reader;

        public DirectiveTreeCollection(ILogger<DirectiveTreeCollection> logger,
            ICensusQueryFactory census, ICensusReader<DirectiveTree> reader) {

            _Logger = logger;
            _Census = census;
            _Reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public async Task<List<DirectiveTree>> GetAll() {
            CensusQuery query = _Census.Create("directive_tree");
            query.SetLimit(10_000);

            List<DirectiveTree> trees = await _Reader.ReadList(query);
            return trees;
        }

    }
}
