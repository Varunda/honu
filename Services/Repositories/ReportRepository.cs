using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
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

    /// <summary>
    ///     Parses the generator strings for outfit reports
    /// </summary>
    public class ReportRepository {

        private readonly ILogger<ReportRepository> _Logger;
        private readonly OutfitCollection _OutfitCensus;
        private readonly CharacterRepository _CharacterRepository;
        private readonly OutfitRepository _OutfitRepository;
        private readonly SessionDbStore _SessionDb;
        private readonly OutfitReportParameterDbStore _ReportParametersDb;

        public ReportRepository(ILogger<ReportRepository> logger,
            OutfitCollection outfitCensus, CharacterRepository charRepo,
            OutfitRepository outfitRepo, SessionDbStore sessionDb,
            OutfitReportParameterDbStore reportParametersDb) {

            _Logger = logger;
            _OutfitCensus = outfitCensus;
            _CharacterRepository = charRepo;
            _OutfitRepository = outfitRepo;
            _SessionDb = sessionDb;
            _ReportParametersDb = reportParametersDb;
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

            READING_ID,

            READING_OPTION

        }

        /// <summary>
        ///     Parses the generator string into the parameters used to generate a report
        /// </summary>
        /// <param name="input">Input generator string</param>
        /// <returns>
        ///     <see cref="OutfitReportParameters"/> that is used to pull all the information necessary
        /// </returns>
        /// <exception cref="FormatException">If one of the parameters could not be parsed to the type needed from the string</exception>
        /// <exception cref="SystemException">There was an error in the parser, and it reached an invalidate state. This is on Honu's end, my bad</exception>
        /// <exception cref="ArgumentException">
        ///     The parameter in the generator string could be parsed to the valid type,
        ///     but the value is not valid in the context.
        ///     For example, setting the team ID to -1 can be parse, but is not valid
        /// </exception>
        public async Task<OutfitReportParameters> ParseGenerator(string input) {
            GenState state = GenState.GET_START;
            OutfitReportParameters parms = new();
            parms.Timestamp = DateTime.UtcNow;
            parms.Generator = input;

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

                        parms.PeriodStart = DateTimeOffset.FromUnixTimeSeconds(startEpoch).DateTime;
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

                        parms.PeriodEnd = DateTimeOffset.FromUnixTimeSeconds(endEpoch).DateTime;
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
                        parms.TeamID = short.Parse($"{i}");
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
                    } else if (i == '$') {
                        state = GenState.READING_OPTION;
                    } else {
                        throw new FormatException($"In state READ_NEXT, unhandled token {i}");
                    }
                } else if (state == GenState.READING_TAG) {
                    if (i == ';') {
                        PsOutfit? outfit = await _OutfitCensus.GetByTag(word);
                        if (outfit != null) {
                            parms.OutfitIDs.Add(outfit.ID);

                            word = "";
                            state = GenState.READ_NEXT;
                        }
                    } else {
                        word += i;
                    }
                } else if (state == GenState.READING_CHAR_ID) {
                    if (i == ';') {
                        parms.CharacterIDs.Add(word);

                        word = "";
                        state = GenState.READ_NEXT;
                    } else {
                        word += i;
                    }
                } else if (state == GenState.READING_OUTFIT_ID) {
                    if (i == ';') {
                        parms.OutfitIDs.Add(word);

                        word = "";
                        state = GenState.READ_NEXT;
                    } else {
                        word += i;
                    }
                } else if (state == GenState.READING_SKIPPED) {
                    if (i == ';') {
                        parms.IgnoredPlayers.Add(word);

                        word = "";
                        state = GenState.READ_NEXT;
                    } else {
                        word += i;
                    }
                } else if (state == GenState.READING_ID) {
                    if (i == ';') {
                        //_Logger.LogTrace($"In state READING_ID> ID is '{word}'");
                        bool valid = Guid.TryParse(word, out Guid id);
                        if (valid == true && id != Guid.Empty) {
                            parms.ID = id;
                        } else {
                            throw new FormatException($"Failed to parse '{word}' to a valid Guid");
                        }
                        word = "";
                        state = GenState.READ_NEXT;
                    } else {
                        word += i;
                    }
                } else if (state == GenState.READING_OPTION) {
                    if (i == ';') {
                        string[] parts = word.Split("=");
                        if (parts.Length != 2) {
                            throw new ArgumentException($"Failed to split '{word}' using '=' (expected 2 parts, got {parts.Length})");
                        }

                        string key = parts[0];
                        string value = parts[1];

                        //_Logger.LogDebug($"Option {key} => {value}");

                        if (key == "rd" && value == "1") {
                            parms.IncludeRevivedDeaths = true;
                        } else if (key == "itk" && value == "1") {
                            parms.IncludeTeamkilled = true;
                            parms.IncludeTeamkills = true;
                        } else if (key == "iae" && value == "1") {
                            parms.IncludeAchievementsEarned = true;
                        } else if (key == "ioe" && value == "1") {
                            parms.IncludeOtherIdExpEvents = true;
                        } else {
                            throw new ArgumentException($"Unchecked key '{key}' in READING_OPTION (value = {value})");
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

            return parms;
        }

    }

}
