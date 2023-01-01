using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.PSB;
using watchtower.Models.Queues;
using watchtower.Services.Db;
using watchtower.Services.Queues;
using watchtower.Services.Repositories.PSB;

namespace watchtower.Services.Hosted.PSB {

    /// <summary>
    ///     Hosted queue processor that will update the <see cref="PsbAccount.SecondsUsage"/>
    /// </summary>
    public class HostedPsbAccountPlaytimeQueue : BackgroundService {

        private readonly ILogger<HostedPsbAccountPlaytimeQueue> _Logger;
        private readonly PsbAccountPlaytimeUpdateQueue _Queue;

        private readonly PsbAccountDbStore _PsbAccountDb;
        private readonly PsbAccountRepository _PsbAccountRepository;

        public HostedPsbAccountPlaytimeQueue(ILogger<HostedPsbAccountPlaytimeQueue> logger,
            PsbAccountPlaytimeUpdateQueue queue, PsbAccountDbStore psbAccountDb,
            PsbAccountRepository psbAccountRepository) {

            _Logger = logger;
            _Queue = queue;

            _PsbAccountDb = psbAccountDb;
            _PsbAccountRepository = psbAccountRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            while (stoppingToken.IsCancellationRequested == false) {
                try {
                    PsbAccountPlaytimeUpdateQueueEntry entry = await _Queue.Dequeue(stoppingToken);

                    _Logger.LogTrace($"updating playtime for {entry.AccountID}");

                    //PsbNamedAccount? account = await _PsbAccountDb.GetByID(entry.AccountID);
                    PsbAccount? account = await _PsbAccountRepository.GetByID(entry.AccountID);
                    if (account == null) {
                        _Logger.LogError($"Missing {nameof(PsbAccount)} {entry.AccountID} when updating playtime");
                        continue;
                    }

                    _Logger.LogTrace($"account {account.ID} is {account.Tag}x{account.Name}");

                    long playTime = await _PsbAccountDb.GetPlaytime(account.ID);
                    account.SecondsUsage = (int) playTime;

                    _Logger.LogTrace($"account {account.ID} has played {account.SecondsUsage} seconds");

                    await _PsbAccountRepository.UpdateByID(account.ID, account);
                } catch (Exception ex) {
                    _Logger.LogError(ex, $"error while updating psb account playtime");
                }
            }
        }

    }
}
