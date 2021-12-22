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

    public class DirectiveTreeCategoryCollection {

        private readonly ILogger<DirectiveTreeCategoryCollection> _Logger;
        private readonly ICensusQueryFactory _Census;
        private readonly ICensusReader<DirectiveTreeCategory> _Reader;

        public DirectiveTreeCategoryCollection(ILogger<DirectiveTreeCategoryCollection> logger,
            ICensusQueryFactory census, ICensusReader<DirectiveTreeCategory> reader) {

            _Logger = logger;
            _Census = census;
            _Reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public async Task<List<DirectiveTreeCategory>> GetAll() {
            CensusQuery query = _Census.Create("directive_tree_category");
            query.SetLimit(10_000);

            List<DirectiveTreeCategory> dirs = await _Reader.ReadList(query);
            return dirs;
        }

    }
}
