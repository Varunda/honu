using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using watchtower.Models.Health;

namespace watchtower.Services.Repositories {

    public class BadHealthRepository {

        private readonly ILogger<BadHealthRepository> _Logger;

        private List<BadHealthEntry> _Entries = new List<BadHealthEntry>();

        public BadHealthRepository(ILogger<BadHealthRepository> logger) {
            _Logger = logger;
        }

        /// <summary>
        ///     Insert a new <see cref="BadHealthEntry"/>
        /// </summary>
        /// <param name="entry">Entry to be inserted</param>
        public void Insert(BadHealthEntry entry) {
            lock (_Entries) {
                _Entries.Add(entry);
                if (_Entries.Count > 50) {
                    _Entries.RemoveAt(0);
                }
            }
        }

        public List<BadHealthEntry> GetRecent() {
            return _Entries;
        }

    }
}
