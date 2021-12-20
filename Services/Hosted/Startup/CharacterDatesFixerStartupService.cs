using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Hosted.Startup {

    public class CharacterDatesFixerStartupService : BackgroundService {

        private const string SERVICE_NAME = "char_date_fixer";
        private int BLOCK_SIZE = 50;

        private readonly ILogger<CharacterDatesFixerStartupService> _Logger;
        private readonly CharacterDbStore _CharacterDb;
        private readonly ICharacterCollection _CharacterCensus;

        public CharacterDatesFixerStartupService(ILogger<CharacterDatesFixerStartupService> logger,
            CharacterDbStore charDb, ICharacterCollection charColl) {

            _Logger = logger;
            _CharacterDb = charDb;
            _CharacterCensus = charColl;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            try {
                Stopwatch timer = Stopwatch.StartNew();
                List<PsCharacter> characters = await _CharacterDb.GetMissingDates();
                long dbTime = timer.ElapsedMilliseconds;
                timer.Restart();

                _Logger.LogDebug($"{SERVICE_NAME}> Took {dbTime}ms to load missing dates from DB");

                if (characters.Count == 0) {
                    _Logger.LogInformation($"{SERVICE_NAME}> No characters to fix");
                    return;
                }

                int blocks = (int) Math.Ceiling((double) characters.Count / BLOCK_SIZE);
                _Logger.LogInformation($"{SERVICE_NAME}> {characters.Count} characters have missing dates, inserting into queue over {blocks} blocks");

                for (int i = 0; i < blocks; ++i) {
                    stoppingToken.ThrowIfCancellationRequested();

                    List<string> slice = characters.Skip(i * BLOCK_SIZE).Take(BLOCK_SIZE).Select(iter => iter.ID).ToList();

                    List<PsCharacter> block = await _CharacterCensus.GetByIDs(slice);

                    int missing = 0;

                    for (int j = 0; j < BLOCK_SIZE; ++j) {
                        PsCharacter c = characters[i * 50 + j];
                        PsCharacter? census = block.FirstOrDefault(iter => iter.ID == c.ID);

                        if (census != null) {
                            await _CharacterDb.Upsert(census);
                        } else {
                            ++missing;
                        }
                    }

                    if (i % 10 == 0) {
                        _Logger.LogDebug($"{SERVICE_NAME}> Performed block {i}, did {i * 50} - {(i + 1) * 50}, missing {missing}/{slice.Count}");
                    }
                }

                _Logger.LogDebug($"{SERVICE_NAME}> Took {timer.ElapsedMilliseconds}ms to insert {characters.Count} entries into queue");
            } catch (Exception ex) when (stoppingToken.IsCancellationRequested == false) {
                _Logger.LogError(ex, "");
            } catch (Exception) when (stoppingToken.IsCancellationRequested == true) {
                _Logger.LogInformation($"{SERVICE_NAME}> stopped");
            }
        }

    }
}
