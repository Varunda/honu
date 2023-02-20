using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Queues;
using watchtower.Services.Db;

namespace watchtower.Services.Queues {

    /// <summary>
    ///     A database buffer of characters to perform updates on, after Census has updated their data.
    ///     This is different from usual queues, as this one is stored in a DB, not in memory
    /// </summary>
    public class LogoutUpdateBuffer {

        private readonly ILogger<LogoutUpdateBuffer> _Logger;
        private LogoutBufferDbStore _LogoutDb;

        public LogoutUpdateBuffer(ILogger<LogoutUpdateBuffer> logger,
            LogoutBufferDbStore db) {

            _Logger = logger;
            _LogoutDb = db ?? throw new ArgumentNullException(nameof(db));
        }

        /// <summary>
        ///     Queue a new <see cref="LogoutBufferEntry"/>
        /// </summary>
        /// <param name="entry">Parmeters inserted</param>
        public async Task Queue(LogoutBufferEntry entry) {
            try {
                await _LogoutDb.Upsert(entry, CancellationToken.None);
            } catch (Exception ex) {
                _Logger.LogError(ex, "failed to insert LogoutBufferEntry");
            }
        }

    }

}
