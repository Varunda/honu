using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Commands;
using watchtower.Models;
using watchtower.Models.PSB;
using watchtower.Services.Queues;
using watchtower.Services.Repositories.PSB;

namespace watchtower.Code.Commands {

    [Command]
    public class PsbNamedCommand {

        private readonly ILogger<PsbNamedCommand> _Logger;
        private readonly PsbAccountRepository _NamedRepository;
        private readonly PsbAccountPlaytimeUpdateQueue _Queue;

        public PsbNamedCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<PsbNamedCommand>>();
            _NamedRepository = services.GetRequiredService<PsbAccountRepository>();
            _Queue = services.GetRequiredService<PsbAccountPlaytimeUpdateQueue>();
        }

        public async Task Create(string tagg, string name) {
            string? tag = tagg;
            if (tag == ".") {
                tag = null;
            }

            PsbNamedAccount acc = await _NamedRepository.Create(tag, name);

            _Logger.LogInformation($"Created new named account {acc.VsID} {acc.NcID} {acc.TrID} {acc.NsID}");
        }

        public async Task Get(string tagg, string name) {
            string? tag = tagg == "." ? null : tagg;

            PsbNamedAccount? acc = await _NamedRepository.GetByTagAndName(tag, name);

            if (acc == null) {
                _Logger.LogWarning($"Failed to find {tag}x{name}");
                return;
            }

            _Logger.LogInformation($"{tag}x{name} => \n{JToken.FromObject(acc)}");
        }

        public async Task GetID(long ID) {
            PsbNamedAccount? acc = await _NamedRepository.GetByID(ID);

            if (acc == null) {
                _Logger.LogWarning($"No {nameof(PsbNamedAccount)} {ID} exists");
                return;
            }

            _Logger.LogInformation($"{JToken.FromObject(acc)}");
        }

        public async Task Rename(long ID, string tagg, string name) {
            string? tag = tagg == "." ? null : tagg;

            PsbNamedAccount? acc = await _NamedRepository.Rename(ID, tag, name);
            if (acc != null) {
                _Logger.LogInformation($"Successfully renamed {ID} to {tag}x{name}");
            } else {
                _Logger.LogWarning($"Failed to update {ID} to {tag}x{name}");
            }
        }

        public async Task SetPlayerName(long ID, string playerName) {
            PsbNamedAccount? acc = await _NamedRepository.SetPlayerName(ID, playerName);
            if (acc != null) {
                _Logger.LogInformation($"Successfully set player name for {ID} to {playerName}");
            } else {
                _Logger.LogWarning($"Failed to set player name for {ID} to {playerName}");
            }
        }

        public async Task Recheck(long ID) {
            PsbNamedAccount? acc = await _NamedRepository.RecheckByID(ID);
            if (acc == null) {
                _Logger.LogWarning($"Failed to update status of {nameof(PsbNamedAccount)}");
            } else {
                string vs = PsbCharacterStatus.GetName(acc.VsStatus);
                string nc = PsbCharacterStatus.GetName(acc.NcStatus);
                string tr = PsbCharacterStatus.GetName(acc.TrStatus);
                string ns = PsbCharacterStatus.GetName(acc.NsStatus);
                _Logger.LogInformation($"{acc.Tag}x{acc.Name} => VS: {vs}, NC: {nc}, TR: {tr}, NS: {ns}");
            }
        }

        public Task Retime(long ID) {
            _Logger.LogInformation($"Adding account {ID} into queue for retiming");

            _Queue.Queue(new Models.Queues.PsbAccountPlaytimeUpdateQueueEntry() {
                AccountID = ID
            });

            return Task.CompletedTask;
        }

        public async Task RetimeAll() {
            List<PsbNamedAccount> accounts = await _NamedRepository.GetAll();

            _Logger.LogInformation($"Queuing {accounts.Count} accounts for retiming");

            foreach (PsbNamedAccount account in accounts) {
                _Queue.Queue(new Models.Queues.PsbAccountPlaytimeUpdateQueueEntry() {
                    AccountID = account.ID
                });
            }
        }

        public async Task RecheckStatus(int statuss) {
            int? status = statuss == 0 ? null : statuss;

            await _NamedRepository.RecheckByStatus(status);
        }

        public async Task Delete(long ID) {
            PsbNamedAccount? acc = await _NamedRepository.GetByID(ID);
            if (acc == null) {
                _Logger.LogWarning($"Account {ID} does not exist");
                return;
            }

            await _NamedRepository.DeleteByID(ID, HonuAccount.SystemID);
            _Logger.LogInformation($"Successfully marked {ID} as deleted");
        }

    }
}
