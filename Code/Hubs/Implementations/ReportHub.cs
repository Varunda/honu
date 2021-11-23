using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Models.Events;
using watchtower.Models.Report;
using watchtower.Services.Census;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Code.Hubs.Implementations {

    public class ReportHub : Hub<IReportHub> {

        private readonly ILogger<ReportHub> _Logger;

        private readonly IOutfitRepository _OutfitRepository;
        private readonly IOutfitCollection _OutfitCensus;
        private readonly ICharacterRepository _CharacterRepository;
        private readonly IKillEventDbStore _KillDb;

        public ReportHub(ILogger<ReportHub> logger,
            ICharacterRepository charRepo, IOutfitRepository outfitRepo,
            IOutfitCollection outfitCensus,
            IKillEventDbStore killDb
            ) {

            _Logger = logger;

            _OutfitRepository = outfitRepo;
            _OutfitCensus = outfitCensus;
            _CharacterRepository = charRepo;
            _KillDb = killDb;
        }

        public async Task GenerateReport(string generator) {
            OutfitReport report = new OutfitReport();
            report.Generator = generator;
            report.Timestamp = DateTime.UtcNow;
            report.PeriodEnd = DateTime.UtcNow;
            report.PeriodStart = report.PeriodEnd - TimeSpan.FromHours(6);

            await Clients.Caller.SendReport(report);

            try {
                await ParseGenerator(generator, report);
            } catch (Exception ex) {
                return;
            }

            await Clients.Caller.UpdateCharacterIDs(report);

            List<KillEvent> killDeaths = await _KillDb.GetKillsByCharacterIDs(report.CharacterIDs, report.PeriodStart, report.PeriodEnd);

            report.KillDeaths = killDeaths;

            await Clients.Caller.UpdateKillDeaths(report);

        }

        private async Task ParseGenerator(string input, OutfitReport report) {
            List<string> tags = new List<string>();

            List<string> outfits = new List<string>();
            List<string> characters = new List<string>();
            List<string> ignored = new List<string>();

            GenState state = GenState.GET_START;

            string word = "";
            foreach (char i in input) {
                _Logger.LogTrace($"STATE {Enum.GetName(state)} >>> {i} >>> {word}");

                if (state == GenState.GET_START) {
                    if (i == ',') {
                        if (int.TryParse(word, out int startEpoch) == false) {
                            throw new FormatException($"In state GET_START, needed to parse '{word}' into an int32");
                        }

                        report.PeriodStart = DateTimeOffset.FromUnixTimeSeconds(startEpoch).DateTime;
                        state = GenState.GET_END;
                        word = "";
                        _Logger.LogTrace($"PeriodStart = {report.PeriodStart}");
                    } else {
                        if (char.IsDigit(i) == false) {
                            throw new FormatException($"In state GET_START, expected {i} to be a digit. Current word: {word}");
                        }

                        word += i;
                    }
                } else if (state == GenState.GET_END) {
                    if (i == ';') {
                        if (int.TryParse(word, out int endEpoch) == false) {
                            throw new FormatException($"In state GET_END, needed to parse '{word}' into an int32");
                        }

                        report.PeriodStart = DateTimeOffset.FromUnixTimeSeconds(endEpoch).DateTime;
                        state = GenState.READ_NEXT;
                        word = "";
                        _Logger.LogTrace($"PeriodStart = {report.PeriodEnd}");
                    } else {
                        if (char.IsDigit(i) == false) {
                            throw new FormatException($"In state GET_END, expected {i} to be a digit. Current word: {word}");
                        }

                        word += i;
                    }
                } else if (state == GenState.READ_NEXT) {
                    if (i == '[') {
                        state = GenState.READING_TAG;
                    } else if (i == '+') {
                        state = GenState.READING_CHAR_ID;
                    } else if (i == 'o') {
                        state = GenState.READING_OUTFIT_ID;
                    } else {
                        throw new FormatException($"In state READ_NEXT, unhandled token {i}");
                    }
                } else if (state == GenState.READING_TAG) {
                    if (i == ';') {
                        PsOutfit? outfit = await _OutfitCensus.GetByTag(word);
                        if (outfit != null) {
                            List<OutfitMember> members = await _OutfitCensus.GetMembers(outfit.ID);
                            _Logger.LogTrace($"outfit tag {word} has {members.Count} members");
                            foreach (OutfitMember member in members) {
                                report.CharacterIDs.Add(member.CharacterID);
                            }

                            word = "";
                            state = GenState.READ_NEXT;
                        }
                    } else {
                        word += i;
                    }
                } else if (state == GenState.READING_CHAR_ID) {
                    if (i == ';') {
                        report.CharacterIDs.Add(word);
                        word = "";
                        state = GenState.READ_NEXT;
                    } else {
                        word += i;
                    }
                } else if (state == GenState.READING_OUTFIT_ID) {
                    if (i == ';') {
                        PsOutfit? outfit = await _OutfitRepository.GetByID(word);
                        if (outfit != null) {
                            List<OutfitMember> members = await _OutfitCensus.GetMembers(outfit.ID);
                            _Logger.LogTrace($"outfit ID {word} has {members.Count} members");
                            foreach (OutfitMember member in members) {
                                report.CharacterIDs.Add(member.CharacterID);
                            }
                        }

                        word = "";
                        state = GenState.READ_NEXT;
                    } else {
                        word += i;
                    }
                }
            }

        }

    }

    public enum GenState {

        GET_START,

        GET_END,

        READ_NEXT,

        READING_TAG,

        READING_CHAR_ID,

        READING_OUTFIT_ID

    }

}
