using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.PSB;
using watchtower.Services.Repositories.PSB;

namespace watchtower.Services.Hosted.Startup {

    public class PsbNamedImportStartupService : BackgroundService {

        private readonly ILogger<PsbNamedImportStartupService> _Logger;
        private readonly PsbNamedRepository _NamedRepository;

        public PsbNamedImportStartupService(ILogger<PsbNamedImportStartupService> logger,
            PsbNamedRepository namedRepo) {

            _Logger = logger;
            _NamedRepository = namedRepo;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            string filePath = "./PSB-Named-Import.csv";

            string[] file;

            try {
                file = await File.ReadAllLinesAsync(filePath, stoppingToken);
            } catch (Exception ex) {
                _Logger.LogError(ex, $"failed to open file '{filePath}'");
                return;
            }

            _Logger.LogInformation($"Have {file.Length} accounts to import");

            List<PsbNamedAccount> existing = await _NamedRepository.GetAll();

            for (int i = 0; i < file.Length; ++i) {
                string line = file[i];

                _Logger.LogTrace($"Importing line {i}/{file.Length}: '{line}'");

                string[] parts = line.Split(",");

                if (parts.Length != 2) {
                    _Logger.LogError($"Invalid line: when split on ',', didn't get 2 parts: '{line}'");
                    continue;
                }

                string? tag = parts[0].Trim();
                string name = parts[1].Trim();

                // Header line
                if (tag.ToLower() == "tag" || name.ToLower() == "name") {
                    continue;
                }

                if (tag == ".") {
                    tag = null;
                }

                _Logger.LogDebug($"Creating named account {tag}x{name}");

                if (existing.FirstOrDefault(iter => iter.Tag == tag && iter.Name == name) != null) {
                    _Logger.LogInformation($"Skipping duplicate named account {tag}x{name}");
                    continue;
                }

                await _NamedRepository.Create(tag, name);
                _Logger.LogInformation($"Inserted new named account {tag}x{name}");
            }

            _Logger.LogInformation($"Done!");

        }

    }
}
