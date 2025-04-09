
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using watchtower.Code.Constants;
using watchtower.Models;
using watchtower.Models.Api;
using watchtower.Models.Census;
using watchtower.Models.CharacterViewer.WeaponStats;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("/api/character")]
    public class CharacterDirectiveApiController : ApiControllerBase {

        private readonly ILogger<CharacterDirectiveApiController> _Logger;

        private readonly CharacterRepository _CharacterRepository;
        private readonly CharacterDirectiveRepository _CharacterDirectiveRepository;
        private readonly CharacterDirectiveTreeRepository _CharacterDirectiveTreeRepository;
        private readonly CharacterDirectiveTierRepository _CharacterDirectiveTierRepository;
        private readonly CharacterDirectiveObjectiveRepository _CharacterDirectiveObjectiveRepository;
        private readonly DirectiveRepository _DirectiveRepository;
        private readonly DirectiveTreeRepository _DirectiveTreeRepository;
        private readonly DirectiveTierRepository _DirectiveTierRepository;
        private readonly DirectiveTreeCategoryRepository _DirectiveTreeCategoryRepository;
        private readonly ObjectiveRepository _ObjectiveRepository;
        private readonly ObjectiveTypeRepository _ObjectiveTypeRepository;
        private readonly ObjectiveSetRepository _ObjectiveSetRepository;
        private readonly AchievementRepository _AchievementRepository;
        private readonly CharacterWeaponStatDbStore _CharacterWeaponDb;
        private readonly ItemRepository _ItemRepository;
        private readonly CharacterAchievementRepository _CharacterAchievementRepository;

        public CharacterDirectiveApiController(ILogger<CharacterDirectiveApiController> logger,
            CharacterDirectiveRepository charDirRepo, CharacterDirectiveTreeRepository charDirTreeRepo,
            CharacterDirectiveTierRepository charDirTierRepo, CharacterDirectiveObjectiveRepository charDirObjRepo,
            DirectiveRepository dirRepo, DirectiveTreeRepository dirTreeRepo,
            DirectiveTierRepository dirTierRepo, DirectiveTreeCategoryRepository dirTreeCatRepo,
            ObjectiveRepository objRepo, ObjectiveTypeRepository objTypeRepo,
            ObjectiveSetRepository objSetRepo, AchievementRepository achRepo,
            CharacterWeaponStatDbStore charWeaponDb, ItemRepository itemRepo,
            CharacterRepository charRepo, CharacterAchievementRepository charAchRepo) {

            _Logger = logger;

            _CharacterRepository = charRepo;
            _CharacterDirectiveRepository = charDirRepo;
            _CharacterDirectiveTreeRepository = charDirTreeRepo;
            _CharacterDirectiveTierRepository = charDirTierRepo;
            _CharacterDirectiveObjectiveRepository = charDirObjRepo;
            _DirectiveRepository = dirRepo;
            _DirectiveTreeRepository = dirTreeRepo;
            _DirectiveTierRepository = dirTierRepo;
            _DirectiveTreeCategoryRepository = dirTreeCatRepo;

            _ObjectiveRepository = objRepo;
            _ObjectiveTypeRepository = objTypeRepo;
            _ObjectiveSetRepository = objSetRepo;
            _AchievementRepository = achRepo;
            _CharacterWeaponDb = charWeaponDb;
            _ItemRepository = itemRepo;
            _CharacterAchievementRepository = charAchRepo;
        }

        /// <summary>
        ///     Get the directive progress of a PC character
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <response code="200">
        ///     The response will contain the <see href="CharacterDirectiveSet"/> for the character.
        ///     If the character does not exist, it will be blank
        /// </response>
        [HttpGet("{charID}/directives")]
        public async Task<ApiResponse<CharacterDirectiveSet>> GetByCharacterID(string charID) {
            CharacterDirectiveSet set = new();
            set.CharacterID = charID;

            List<CharacterDirective> charDirs = await _CharacterDirectiveRepository.GetByCharacterID(charID);
            List<CharacterDirectiveTree> charDirTrees = await _CharacterDirectiveTreeRepository.GetByCharacterID(charID);
            List<CharacterDirectiveTier> charDirTiers = await _CharacterDirectiveTierRepository.GetByCharacterID(charID);
            List<CharacterDirectiveObjective> charDirObjectives = await _CharacterDirectiveObjectiveRepository.GetByCharacterID(charID);

            List<DirectiveTreeCategory> categories = await _DirectiveTreeCategoryRepository.GetAll();

            Dictionary<int, ExpandedCharacterDirectiveCategory> charCategories = new();

            PsCharacter? character = await _CharacterRepository.GetByID(charID, CensusEnvironment.PC);
            if (character == null) {
                _Logger.LogError($"Failed to find character {charID}, some extra directives not for this faction will be included");
            }

            // Get the directive trees the character has
            //      Get the directives in that tree
            //      Iterate thru directives, group by tier, and find the character directive for each directive
            //          get the objective and progress of each directive
            foreach (CharacterDirectiveTree charTree in charDirTrees) {
                DirectiveTree? tree = await _DirectiveTreeRepository.GetByID(charTree.TreeID);
                if (tree == null) {
                    continue;
                }

                int categoryID = tree != null ? tree.CategoryID : -1;

                if (charCategories.TryGetValue(categoryID, out ExpandedCharacterDirectiveCategory? exCat) == false) {
                    exCat = new ExpandedCharacterDirectiveCategory();
                    exCat.CategoryID = categoryID;
                    exCat.Category = categories.FirstOrDefault(iter => iter.ID == categoryID);
                    charCategories.Add(categoryID, exCat);
                }

                ExpandedCharacterDirectiveTree exTree = new();
                exTree.Entry = charTree;
                exTree.Tree = await _DirectiveTreeRepository.GetByID(charTree.TreeID);

                List<PsDirective> dirsInTree = await _DirectiveRepository.GetByTreeID(charTree.TreeID);
                List<WeaponStatEntry> weaponStats = await _CharacterWeaponDb.GetByCharacterID(charID);
                List<CharacterAchievement> characterAchs = await _CharacterAchievementRepository.GetByCharacterID(charID);

                Dictionary<int, ExpandedCharacterDirectiveTier> tierMap = new();
                foreach (PsDirective dir in dirsInTree) {
                    int tierID = dir.TierID;
                    if (tierMap.TryGetValue(tierID, out ExpandedCharacterDirectiveTier? tier) == false) {
                        tier = new();
                        tier.Entry = charDirTiers.FirstOrDefault(iter => iter.TierID == tierID && iter.TreeID == dir.TreeID);
                        tier.TierID = tierID;
                        tier.Tier = await _DirectiveTierRepository.GetByTierAndTree(tierID, dir.TreeID);
                        tierMap.Add(tierID, tier);
                    }

                    ExpandedCharacterDirective dirEntry = new ExpandedCharacterDirective() {
                        Entry = charDirs.FirstOrDefault(iter => iter.DirectiveID == dir.ID),
                        Directive = dir,
                        CharacterObjective = charDirObjectives.FirstOrDefault(obj => obj.DirectiveID == dir.ID),
                    };

                    PsObjective? obj = dir.ObjectiveSetID == null ? null : await GetObjective(dir.ObjectiveSetID.Value);

                    // Skip weapons not for the faction of the character
                    // 66 = Achievement, which can have an item attached to it
                    if (obj != null) {
                        if (character != null && obj.TypeID == 66) {
                            if (await IncludeObjective(obj, character.FactionID) == false) {
                                continue;
                            }
                        }

                        dirEntry.Goal = await GetObjectiveGoal(obj);

                        if (obj.TypeID == 66) {
                            dirEntry.Progress = await GetAchievementProgress(obj, weaponStats, characterAchs);
                            if (dirEntry.Progress == null) {
                                dirEntry.Progress = dirEntry.CharacterObjective?.StateData;
                            }
                        } else {
                            if (dirEntry.Entry != null && dirEntry.Entry.CompletionDate != null) {
                                dirEntry.Progress = dirEntry.Goal;
                            } else {
                                dirEntry.Progress = dirEntry.CharacterObjective?.StateData;
                            }
                        }
                    }

                    dirEntry.Name = dir.Name;
                    dirEntry.Description = dir.Description;

                    tier.Directives.Add(dirEntry);
                }

                exTree.Tiers = tierMap.Values.OrderBy(iter => iter.TierID).ToList();

                exCat.Trees.Add(exTree);
            }

            set.CharacterID = charID;
            set.Categories = charCategories.Values.ToList();

            return ApiOk(set);
        }

        private async Task<PsObjective?> GetObjective(int objectiveSetID) {
            PsObjective? obj = null;
            ObjectiveSet? objSet = await _ObjectiveSetRepository.GetByID(objectiveSetID);
            if (objSet != null) {
                obj = await _ObjectiveRepository.GetByGroupID(objSet.GroupID);
            }

            return obj;
        }

        /// <summary>
        ///     will this objective be included based on the faction, 
        ///     i.e. don't include VS weapons on TR characters
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="factionID"></param>
        /// <returns></returns>
        private async Task<bool> IncludeObjective(PsObjective obj, short factionID) {
            if (obj.TypeID == 66) {
                if (int.TryParse(obj.Param1, out int achID) == false) {
                    _Logger.LogWarning($"Failed to parse objective {obj.ID}'s Param1 ({obj.Param1}) into a valid int");
                    return true;
                }

                Achievement? ach = await _AchievementRepository.GetByID(achID);
                if (ach == null) {
                    _Logger.LogWarning($"Failed to get Achievement [id={achID}]");
                    return true;
                }

                PsObjective? achObj = await _ObjectiveRepository.GetByGroupID(ach.ObjectiveGroupID);
                if (achObj == null) {
                    _Logger.LogWarning($"Failed to get Objective {ach.ObjectiveGroupID} by group ID");
                    return true;
                }
                
                // 12 = Kills, check the weapon the kills are meant for
                if (achObj.TypeID == 12) {
                    if (achObj.Param5 != null) {
                        string weaponID = achObj.Param5;

                        if (weaponID == "0" || weaponID == "1") {
                            return true;
                        }

                        PsItem? weaponItem = await _ItemRepository.GetByID(int.Parse(weaponID));

                        if (weaponItem == null) {
                            _Logger.LogWarning($"Failed to get weapon ID {weaponID} from objective {achObj.ID}");
                            return true;
                        } else {
                            // If the weapon is for a different faction, don't add it to the stats
                            if (weaponItem.FactionID != -1 && weaponItem.FactionID != 0 && weaponItem.FactionID != 4 && (weaponItem.FactionID != factionID)) {
                                return false;
                            }
                        }
                    } else if (achObj.Param6 != null) {
                        // 2024-09-04: we aren't given item classification data, which is what this is
                        // _Logger.LogWarning($"do Param6 stuff");
                        return true;
                    } else {
                        _Logger.LogError($"Missing Param5 'Item' on objective id {achObj.ID}");
                        return true;
                    }
                }
            }

            return true;
        }

        /// <summary>
        ///     Get the total count needed to complete an objective
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private async Task<int?> GetObjectiveGoal(PsObjective obj) {
            string? param = null;

            if (obj.TypeID == 66) { // 66: achievement count
                if (obj.Param1 == null) {
                    _Logger.LogWarning($"Param1 of {obj.ID} was null, cannot get Achievement");
                    return null;
                }

                if (int.TryParse(obj.Param1, out int achID) == false) {
                    _Logger.LogWarning($"Failed to parse '{obj.Param1}' to a valid Int32");
                    return null;
                }

                Achievement? achievement = await _AchievementRepository.GetByID(achID);
                if (achievement == null) {
                    _Logger.LogWarning($"Failed to get achievement {achID} from objective {obj.ID}");
                    return null;
                }

                PsObjective? achObj = await _ObjectiveRepository.GetByGroupID(achievement.ObjectiveGroupID);
                if (achObj == null) {
                    _Logger.LogWarning($"Failed to get objective {achievement.ObjectiveGroupID} from achievement {achievement.ID}");
                    return null;
                }

                // If Param3 (achievement count) is not null, get use how many of them are required, else use how many the achievement objective needs
                if (obj.Param3 != null) {
                    if (int.TryParse(obj.Param3, out int achCount) == false) {
                        _Logger.LogWarning($"Failed to parse Param3 of objective {obj.ID} to a valid Int32");
                        return null;
                    }

                    return achCount;
                }

                param = achObj.Param1;
            } else {
                ObjectiveType? type = await _ObjectiveTypeRepository.GetByID(obj.TypeID);
                if (type == null) {
                    _Logger.LogError($"missing objective type [typeID={obj.TypeID}]");
                    return null;
                }

                switch (obj.TypeID) {
                    case 3: param = obj.Param1; break;
                    case 10: param = obj.Param2; break; // token count
                    case 12: param = obj.Param1; break;
                    case 14: param = obj.Param1; break;
                    case 15: param = obj.Param1; break;
                    case 17: param = obj.Param1; break;
                    case 19: param = obj.Param2; break;
                    case 20: param = obj.Param1; break;
                    case 23: param = obj.Param1; break; // character flag
                    case 27: param = obj.Param1; break; // 27: Change profile, param1=goal count, param2=profile, param3=profile type
                    case 30: param = obj.Param1; break; // meters traveled
                    case 31: param = "1"; break;        // 31: Join outfit, all params are null
                    case 33: param = obj.Param1; break; // 33: Player design event, param1=goal count, param2=custom param1, param3=custom param3
                    case 34: param = obj.Param2; break; // 34: Damage taken: param1=amount, param2=item, param3=item classification
                    case 35: param = obj.Param1; break;
                    case 40: param = obj.Param2; break; // 40: Item Count, param1=item id, param2=item count
                    case 69: param = obj.Param1; break;
                    case 66: _Logger.LogError("some logic broke here!"); break;
                    case 70: param = obj.Param1; break;
                    case 74: param = obj.Param1; break; // 74: add friends, param1=count
                    case 89: param = obj.Param5; break;
                    case 90: param = obj.Param1; break;
                    case 91: param = obj.Param1; break;
                    case 92: param = obj.Param1; break;
                    case 93: param = obj.Param1; break;
                    case 96: param = obj.Param2; break; // 96: Scan Fish, param1=fish, param2=count, param3=any fish
                    case 97: param = obj.Param2; break; // 97: Earn Currency, param1=current, param2=count
                    case 98: param = obj.Param2; break; // 98: Consume Item, param1=item, param2=count

                    default:
                        _Logger.LogError($"Unchecked objective type id {obj.TypeID}: {JToken.FromObject(type)}");
                        break;
                }
            }

            if (param == null) {
                _Logger.LogWarning($"param was still null, objective type {obj.TypeID}, objective {obj.ID}");
                return null;
            } else {
                if (int.TryParse(param, out int goal) == false) {
                    _Logger.LogWarning($"Failed to parse '{param}' into a valid Int32, from objectve {obj.ID}");
                    return null;
                }

                return goal;
            }
        }

        /// <summary>
        ///     Get how far a character has gotten in an objective that uses an achievement for progression
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="weaponStats"></param>
        /// <param name="charAchs"></param>
        /// <returns></returns>
        private async Task<int?> GetAchievementProgress(PsObjective obj, List<WeaponStatEntry> weaponStats, List<CharacterAchievement> charAchs) {
            if (obj.TypeID != 66) {
                throw new ArgumentException($"Expected {nameof(obj)} to have a TypeID of 66, but it was {obj.TypeID}");
            }

            if (obj.Param1 == null) {
                _Logger.LogError($"Param1 of {obj.ID} was null, cannot get Achievement");
                return null;
            }

            if (int.TryParse(obj.Param1, out int achID) == false) {
                _Logger.LogError($"Failed to parse '{obj.Param1}' to a valid Int32");
                return null;
            }

            Achievement? achievement = await _AchievementRepository.GetByID(achID);
            if (achievement == null) {
                _Logger.LogError($"Failed to get achievement {achID} from objective {obj.ID}");
                return null;
            }

            PsObjective? achObj = await _ObjectiveRepository.GetByGroupID(achievement.ObjectiveGroupID);
            if (achObj == null) {
                _Logger.LogError($"Failed to get objective {achievement.ObjectiveGroupID} from achievement {achievement.ID}");
                return null;
            }

            if (achObj.TypeID == 12) {
                if (achObj.Param5 != null) {
                    string weaponID = achObj.Param5;
                    WeaponStatEntry? weaponEntry = weaponStats.FirstOrDefault(iter => iter.WeaponID == weaponID);

                    return weaponEntry?.Kills;
                } else if (achObj.Param6 != null) {
                    return null;
                } else {
                    _Logger.LogError($"Param5 and Param6 of {achObj.ID} is null, which is the weapon ID");
                    return null;
                }
            } else {
                CharacterAchievement? charAch = charAchs.FirstOrDefault(iter => iter.AchievementID == achID);
                if (charAch != null) {
                    return charAch.EarnedCount;
                }

                //_Logger.LogError($"Unchecked objective type {achObj.TypeID} when getting achievement progress");
                return null;
            }
        }
            
    }

}