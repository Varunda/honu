using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.PSB;
using watchtower.Services.Db;
using watchtower.Services.Repositories.PSB;

namespace watchtower.Services.Hosted.PSB {

    public class PsbNamedCheckerService : BackgroundService {

        private readonly ILogger<PsbNamedCheckerService> _Logger;
        private readonly PsbNamedDbStore _NamedDb;
        private readonly PsbAccountRepository _NamedRepository;

        private const string SERVICE_NAME = "psb_named_checker";
        private const int STARTUP_DELAY = 1000 * 60 * 5; // 5 minutes
        private const int INTERVAL_DELAY = 1000 * 60 * 60; // 60 mins / 1 hour

        public PsbNamedCheckerService(ILogger<PsbNamedCheckerService> logger,
            PsbNamedDbStore namedDb, PsbAccountRepository namedRepo) {

            _Logger = logger;
            _NamedDb = namedDb;
            _NamedRepository = namedRepo;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"{SERVICE_NAME}> Started. Waiting {STARTUP_DELAY / 1000} seconds before running");
            await Task.Delay(STARTUP_DELAY, stoppingToken);

            while (stoppingToken.IsCancellationRequested == false) {
                try {
                    List<PsbNamedAccount> accounts = await _NamedRepository.GetAll();

                    foreach (PsbNamedAccount account in accounts) {
                        stoppingToken.ThrowIfCancellationRequested();

                        // Ensure each character still matches the existing ID
                        PsbCharacterSet charSet = await _NamedRepository.GetCharacterSet(account.Tag, account.Name);

                        string acc = $"{account.Tag}x{account.Name}";

                        if (account.VsID == null && charSet.VS == null) {
                            _Logger.LogWarning($"{acc}> Missing VS character for {acc}");
                        } else if (account.VsID == null && charSet.VS != null) {
                            _Logger.LogWarning($"{acc}> Previously missing VS character {acc}VS now exists: {charSet.VS.ID}");
                        } else if (account.VsID != null && charSet.VS == null) {
                            _Logger.LogWarning($"{acc}> Previously existing VS character {acc}VS is deleted");
                        } else if (account.VsID != null && charSet.VS != null) {
                            if (account.VsID == charSet.VS.ID) {
                                _Logger.LogInformation($"{acc}> VS {account.VsID} == {charSet.VS.ID}");
                            } else {
                                _Logger.LogWarning($"{acc}> VS character recreated");
                            }
                        }
                    }

                    await Task.Delay(INTERVAL_DELAY, stoppingToken);
                } catch (Exception) when (stoppingToken.IsCancellationRequested == true) {
                    _Logger.LogInformation($"{SERVICE_NAME}> Stopping service");
                } catch (Exception ex) when (stoppingToken.IsCancellationRequested == false) {
                    _Logger.LogError(ex, $"exception in {SERVICE_NAME}");
                }
            }
        }

    }
}
