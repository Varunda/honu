using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Constants;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Events;
using watchtower.Models.Report;
using watchtower.Services.Census;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Code.Hubs.Implementations {

    public class ReportHub : Hub<IReportHub> {

        private readonly ILogger<ReportHub> _Logger;

        private readonly IMemoryCache _Cache;
        private const string CACHE_KEY = "Report.{0}"; // {0} => Generator hash

        private readonly IOutfitRepository _OutfitRepository;
        private readonly IOutfitCollection _OutfitCensus;
        private readonly ICharacterRepository _CharacterRepository;
        private readonly ICharacterDbStore _CharacterDb;
        private readonly IItemRepository _ItemRepository;
        private readonly IKillEventDbStore _KillDb;
        private readonly IExpEventDbStore _ExpDb;
        private readonly ISessionDbStore _SessionDb;
        private readonly IReportDbStore _ReportDb;

        public ReportHub(ILogger<ReportHub> logger, IMemoryCache cache,
            ICharacterRepository charRepo, IOutfitRepository outfitRepo,
            IOutfitCollection outfitCensus, ISessionDbStore sessionDb,
            IKillEventDbStore killDb, IExpEventDbStore expDb,
            IItemRepository itemRepo, ICharacterDbStore charDb,
            IReportDbStore reportDb) {

            _Logger = logger;
            _Cache = cache;

            _OutfitRepository = outfitRepo;
            _OutfitCensus = outfitCensus;
            _CharacterRepository = charRepo;
            _CharacterDb = charDb;
            _ItemRepository = itemRepo;
            _KillDb = killDb;
            _ExpDb = expDb;
            _SessionDb = sessionDb;
            _ReportDb = reportDb;
        }

        public async Task GenerateReport(string generator) {
            string cacheKey = string.Format(CACHE_KEY, generator);

            if (_Cache.TryGetValue(cacheKey, out OutfitReport report) == true) {
                await Clients.Caller.SendReport(report);
                await Clients.Caller.UpdateCharacterIDs(report.CharacterIDs);
                await Clients.Caller.UpdateSessions(report.Sessions);
                await Clients.Caller.UpdateKills(report.Kills);
                await Clients.Caller.UpdateDeaths(report.Deaths);
                await Clients.Caller.UpdateExp(report.Experience);
                await Clients.Caller.UpdateItems(report.Items);
                await Clients.Caller.UpdateCharacters(report.Characters);
                await Clients.Caller.UpdateOutfits(report.Outfits);

                return;
            }

            report = new OutfitReport();
            report.Generator = generator;
            report.Timestamp = DateTime.UtcNow;

            await Clients.Caller.SendReport(report);

            try {
                await ParseGenerator(generator, report);
            } catch (Exception ex) {
                await Clients.Caller.SendError(ex.Message);
                return;
            }

            if (report.TeamID <= 0) {
                await Clients.Caller.SendError($"The TeamID of the report is currently {report.TeamID}, it must be above 0. Try setting the faction");
                return;
            }

            report.ID = await _ReportDb.Insert(report);

            await Clients.Caller.SendReport(report);
            await Clients.Caller.UpdateCharacterIDs(report.CharacterIDs);
            await Clients.Caller.UpdateSessions(report.Sessions);

            HashSet<string> outfits = new();
            HashSet<string> chars = new();
            HashSet<string> items = new();

            foreach (string id in report.CharacterIDs) {
                chars.Add(id);
            }

            List<KillEvent> killDeaths = await _KillDb.GetKillsByCharacterIDs(report.CharacterIDs, report.PeriodStart, report.PeriodEnd);
            foreach (KillEvent ev in killDeaths) {
                chars.Add(ev.KilledCharacterID);
                chars.Add(ev.AttackerCharacterID);
                items.Add(ev.WeaponID);
            }

            report.Kills = killDeaths.Where(iter => iter.AttackerTeamID == report.TeamID && iter.AttackerTeamID != iter.KilledTeamID).ToList();
            await Clients.Caller.UpdateKills(report.Kills);

            report.Deaths = killDeaths.Where(iter => iter.KilledTeamID == report.TeamID && iter.KilledTeamID != iter.AttackerTeamID && iter.RevivedEventID == null).ToList();
            await Clients.Caller.UpdateDeaths(report.Deaths);

            List<ExpEvent> expEvents = await _ExpDb.GetByCharacterIDs(report.CharacterIDs, report.PeriodStart, report.PeriodEnd);
            foreach (ExpEvent ev in expEvents) {
                chars.Add(ev.SourceID);

                if (Experience.OtherIDIsCharacterID(ev.ExperienceID)) {
                    chars.Add(ev.OtherID);
                }
            }

            report.Experience = expEvents.Where(iter => iter.TeamID == report.TeamID).ToList();
            await Clients.Caller.UpdateExp(report.Experience);

            foreach (string itemID in items) {
                PsItem? item = await _ItemRepository.GetByID(itemID);
                if (item != null) {
                    report.Items.Add(item);
                }
            }

            await Clients.Caller.UpdateItems(report.Items);

            report.Characters = await GetCharacters(chars.ToList());
            await Clients.Caller.UpdateCharacters(report.Characters);

            foreach (PsCharacter c in report.Characters) {
                if (c.OutfitID != null) {
                    outfits.Add(c.OutfitID);
                }
            }

            foreach (string outfitID in outfits) {
                PsOutfit? outfit = await _OutfitRepository.GetByID(outfitID);
                if (outfit != null) {
                    report.Outfits.Add(outfit);
                }
            }

            await Clients.Caller.UpdateOutfits(report.Outfits);

            _Cache.Set(cacheKey, report, new MemoryCacheEntryOptions() {
                SlidingExpiration = TimeSpan.FromMinutes(30)
            });
        }

        private async Task<List<PsCharacter>> GetCharacters(List<string> IDs) {
            List<PsCharacter> chars =  await _CharacterDb.GetByIDs(IDs);

            HashSet<string> found = new HashSet<string>();
            foreach (PsCharacter c in chars) {
                found.Add(c.ID);
            }

            List<string> left = new List<string>();
            foreach (string charID in IDs) {
                if (found.Contains(charID) == false) {
                    left.Add(charID);
                }
            }
            _Logger.LogTrace($"Found {found.Count}/{chars.Count} characters from DB, getting {left.Count} from repo");

            foreach (string charID in left) {
                PsCharacter? c = await _CharacterRepository.GetByID(charID);
                if (c != null) {
                    chars.Add(c);
                }
            }

            return chars;
        }

        /// <summary>
        ///     Report generator string parsing
        /// </summary>
        /// <param name="input"></param>
        /// <param name="report"></param>
        /// <returns></returns>
        private async Task ParseGenerator(string input, OutfitReport report) {
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
                    if (i == ';' || i == ',') {
                        if (int.TryParse(word, out int endEpoch) == false) {
                            throw new FormatException($"In state GET_END, needed to parse '{word}' into an int32");
                        }

                        report.PeriodEnd = DateTimeOffset.FromUnixTimeSeconds(endEpoch).DateTime;
                        word = "";
                        _Logger.LogTrace($"PeriodEnd = {report.PeriodEnd}");

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
                            _Logger.LogTrace($"outfit ID {word} has {sessions.Count} sessions");
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
                }
            }

        }

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

    }

}
