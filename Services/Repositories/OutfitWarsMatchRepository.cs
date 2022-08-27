using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using watchtower.Models.OutfitWarsNexus;

namespace watchtower.Services.Repositories {

    public class OutfitWarsMatchRepository {

        private readonly ILogger<OutfitWarsMatchRepository> _Logger;

        private readonly List<OutfitWarsMatch> _Matches = new List<OutfitWarsMatch>();

        public OutfitWarsMatchRepository(ILogger<OutfitWarsMatchRepository> logger) {
            _Logger = logger;
        }

        public OutfitWarsMatch? Get(short worldID, uint zoneID) {
            lock (_Matches) {
                return _Matches.FirstOrDefault(iter => iter.WorldID == worldID && iter.ZoneID == zoneID);
            }
        }

        public void Add(OutfitWarsMatch match) {
            if (match.WorldID == default) { throw new ArgumentException($"WorldID must be set (cannot be default)"); }
            if (match.ZoneID == default) { throw new ArgumentException("ZoneID must be set (cannot be default)"); }
            if (match.Timestamp == default) { throw new ArgumentException("Timestamp must be set (cannot be default)"); }

            if (Get(match.WorldID, match.ZoneID) != null) {
                throw new ArgumentException($"A match with WorldID {match.WorldID} ZoneID {match.ZoneID} already exists");
            }

            lock (_Matches) {
                _Matches.Add(match);
            }
        }

    }
}
