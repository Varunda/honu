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
    ///     A queue of <see cref="LogoutBufferEntry"/>s 
    /// </summary>
    public class BackgroundLogoutBufferQueue {

        private readonly ILogger<BackgroundLogoutBufferQueue> _Logger;
        private LogoutBufferDbStore _LogoutDb;

        public BackgroundLogoutBufferQueue(ILogger<BackgroundLogoutBufferQueue> logger,
            LogoutBufferDbStore db) {

            _Logger = logger;
            _LogoutDb = db ?? throw new ArgumentNullException(nameof(db));
        }

        //private SemaphoreSlim _Signal = new SemaphoreSlim(0);

        /// <summary>
        ///     Queue a new <see cref="LogoutBufferEntry"/>
        /// </summary>
        /// <param name="entry">Parmeters inserted</param>
        public async void Queue(LogoutBufferEntry entry) {
            try {
                await _LogoutDb.Upsert(entry, CancellationToken.None);
            } catch (Exception ex) {
                _Logger.LogError(ex, "failed to insert LogoutBufferEntry");
            }
        }

    }

}
