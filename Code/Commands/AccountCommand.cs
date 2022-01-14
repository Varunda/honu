using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Commands;
using watchtower.Models;
using watchtower.Services.Db;

namespace watchtower.Code.Commands {

    [Command]
    public class AccountCommand {

        private readonly ILogger<AccountCommand> _Logger;
        private readonly HonuAccountDbStore _AccountDb;

        public AccountCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<AccountCommand>>();
            _AccountDb = services.GetRequiredService<HonuAccountDbStore>();
        }

        public async Task Create(string name, string email, string discord, ulong discordID) {
            HonuAccount account = new HonuAccount() {
                Name = name,
                Email = email,
                Discord = discord,
                DiscordID = discordID
            };

            long ID = await _AccountDb.Insert(account, CancellationToken.None);
            _Logger.LogInformation($"Created new account {ID} for {name}");
        }

    }
}
