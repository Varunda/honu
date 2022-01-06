using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Report;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class ReportRepository {

        private readonly ILogger<ReportRepository> _Logger;
        private readonly OutfitCollection _OutfitCensus;
        private readonly ICharacterRepository _CharacterRepository;
        private readonly OutfitRepository _OutfitRepository;
        private readonly ISessionDbStore _SessionDb;

        public ReportRepository(ILogger<ReportRepository> logger,
            OutfitCollection outfitCensus, ICharacterRepository charRepo,
            OutfitRepository outfitRepo, ISessionDbStore sessionDb) {

            _Logger = logger;
            _OutfitCensus = outfitCensus;
            _CharacterRepository = charRepo;
            _OutfitRepository = outfitRepo;
            _SessionDb = sessionDb;
        }

        public enum GenState {

            GET_START,

            GET_END,

            GET_TEAM_ID,

            READ_NEXT,

            READING_TAG,

            READING_CHAR_ID,

            READING_OUTFIT_ID,

            READING_SKIPPED,

            READING_ID

        }

        public async Task<OutfitReport> ParseGenerator(string input) {
            List<string> ignored = new List<string>();

            GenState state = GenState.GET_START;
            OutfitReport report = new OutfitReport();

            string word = "";
            foreach (char i in input) {
                //_Logger.LogTrace($"STATE {Enum.GetName(state)} >>> {i} >>> {word}");

                if (state == GenState.GET_START) {
                    if (i == '#') {
                        state = GenState.READING_ID;
                    } else if (i == ',') {
                        if (int.TryParse(word, out int startEpoch) == false) {
                            throw new FormatException($"In state GET_START, needed to parse '{word}' into an int32");
                        }

                        report.PeriodStart = DateTimeOffset.FromUnixTimeSeconds(startEpoch).DateTime;
                        state = GenState.GET_END;
                        word = "";
                        //_Logger.LogTrace($"PeriodStart = {report.PeriodStart}");
                    } else {
                        if (char.IsDigit(i) == false) {
                            throw new FormatException($"In state GET_START, expected {i} to be a digit. Current word: {word}");
                        }

                        word += i;
                    }
                } else if (state == GenState.GET_END) {
                    if (i == ';' || i == ',') {
                        if (int.TryParse(word, out int endEpoch) == false) {
                            throw new FormatException($"In state GET_END, needed to parse '{word}' into an int32");
                        }

                        report.PeriodEnd = DateTimeOffset.FromUnixTimeSeconds(endEpoch).DateTime;
                        word = "";
                        //_Logger.LogTrace($"PeriodEnd = {report.PeriodEnd}");

                        if (i == ';') {
                            state = GenState.READ_NEXT;
                        } else if (i == ',') {
                            state = GenState.GET_TEAM_ID;
                        } else {
                            throw new SystemException($"Unchecked value of i in GET_END: {i}. Expected ';' or ','");
                        }
                    } else {
                        if (char.IsDigit(i) == false) {
                            throw new FormatException($"In state GET_END, expected {i} to be a digit. Current word: {word}");
                        }

                        word += i;
                    }
                } else if (state == GenState.GET_TEAM_ID) {
                    if (i == ';') {
                        state = GenState.READ_NEXT;
                    } else if (char.IsDigit(i) == true) {
                        report.TeamID = short.Parse($"{i}");
                    } else {
                        throw new FormatException($"In state GET_TEAM_ID, expected {i} to either be ';' or a digit");
                    }
                } else if (state == GenState.READ_NEXT) {
                    if (i == '[') {
                        state = GenState.READING_TAG;
                    } else if (i == '+') {
                        state = GenState.READING_CHAR_ID;
                    } else if (i == 'o') {
                        state = GenState.READING_OUTFIT_ID;
                    } else if (i == '-') {
                        state = GenState.READING_SKIPPED;
                    } else if (i == '#') {
                        state = GenState.READING_ID;
                    } else {
                        throw new FormatException($"In state READ_NEXT, unhandled token {i}");
                    }
                } else if (state == GenState.READING_TAG) {
                    if (i == ';') {
                        PsOutfit? outfit = await _OutfitCensus.GetByTag(word);
                        if (outfit != null) {
                            List<OutfitMember> members = await _OutfitCensus.GetMembers(outfit.ID);
                            //_Logger.LogTrace($"outfit tag {word} has {members.Count} members");
                            foreach (OutfitMember member in members) {
                                if (ignored.Contains(member.CharacterID)) {
                                    continue;
                                }
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
                        PsCharacter? c = await _CharacterRepository.GetByID(word);
                        if (c != null && report.TeamID == -1) {
                            report.TeamID = c.FactionID;
                        }
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
                            List<Session> sessions = await _SessionDb.GetByRangeAndOutfit(outfit.ID, report.PeriodStart, report.PeriodEnd);
                            //_Logger.LogTrace($"outfit ID {word} has {sessions.Count} sessions");
                            foreach (Session s in sessions) {
                                if (ignored.Contains(s.CharacterID)) {
                                    continue;
                                }

                                report.Sessions.Add(s);
                                report.CharacterIDs.Add(s.CharacterID);

                                if (report.TeamID == -1 && s.TeamID > 0) {
                                    report.TeamID = s.TeamID;
                                }
                            }
                        }

                        word = "";
                        state = GenState.READ_NEXT;
                    } else {
                        word += i;
                    }
                } else if (state == GenState.READING_SKIPPED) {
                    if (i == ';') {
                        ignored.Add(word);
                        report.CharacterIDs = report.CharacterIDs.Where(iter => iter != word).ToList();
                        word = "";
                        state = GenState.READ_NEXT;
                    } else {
                        word += i;
                    }
                } else if (state == GenState.READING_ID) {
                    if (i == ';') {
                        _Logger.LogDebug($"In state READING_ID> ID is '{word}'");
                        bool valid = Guid.TryParse(word, out Guid id);
                        if (valid == true) {
                            report.ID = id;
                        } else {
                            throw new FormatException($"Failed to parse '{word}' to a valid Guid");
                        }
                        word = "";
                        state = GenState.READ_NEXT;
                    } else {
                        word += i;
                    }
                } else {
                    throw new ArgumentException($"Unchecked state {state}, current word: '{word}'");
                }
            }

            return report;
        }

    }

}
