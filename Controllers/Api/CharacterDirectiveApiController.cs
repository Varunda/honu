
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using watchtower.Models;
using watchtower.Models.Api;
using watchtower.Models.Census;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("/api/character")]
    public class CharacterDirectiveApiController : ApiControllerBase {

        private readonly ILogger<CharacterDirectiveApiController> _Logger;

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

        public CharacterDirectiveApiController(ILogger<CharacterDirectiveApiController> logger,
            CharacterDirectiveRepository charDirRepo, CharacterDirectiveTreeRepository charDirTreeRepo,
            CharacterDirectiveTierRepository charDirTierRepo, CharacterDirectiveObjectiveRepository charDirObjRepo,
            DirectiveRepository dirRepo, DirectiveTreeRepository dirTreeRepo,
            DirectiveTierRepository dirTierRepo, DirectiveTreeCategoryRepository dirTreeCatRepo,
            ObjectiveRepository objRepo, ObjectiveTypeRepository objTypeRepo,
            ObjectiveSetRepository objSetRepo) {

            _Logger = logger;
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
        }

        /// <summary>
        ///     Get the directive progress of a character
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

            foreach (CharacterDirectiveTree charTree in charDirTrees) {
                DirectiveTree? tree = await _DirectiveTreeRepository.GetByID(charTree.TreeID);

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

                List<CharacterDirective> charDirsInTree = charDirs.Where(iter => iter.TreeID == charTree.TreeID).ToList();

                Dictionary<int, ExpandedCharacterDirectiveTier> tierMap = new();
                foreach (CharacterDirective charDir in charDirsInTree) {
                    PsDirective? dir = await _DirectiveRepository.GetByID(charDir.DirectiveID);

                    int tierID = dir == null ? -1 : dir.TierID;
                    if (tierMap.TryGetValue(tierID, out ExpandedCharacterDirectiveTier? tier) == false) {
                        tier = new();
                        tier.Entry = charDirTiers.FirstOrDefault(iter => iter.TierID == tierID && iter.TreeID == charDir.TreeID);
                        tier.TierID = tierID;
                        tierMap.Add(tierID, tier);
                    }

                    // There are 3 different ways the objective can be found
                    // 1. the ObjectiveSetID of the PsDirective is PsDirective.ID
                    // 2. the ObjectiveSetID of the PsDirective is DirectiveSet.SetID, and DirectiveSet.SetID, is the PsDirective.ID
                    // 3. the ObjectiveSetID of the PsDirective is DirectiveSet.SetID, and DirectiveSet.GroupID is the PsDirective.ID

                    ObjectiveSet? objSet = (dir == null) ? null : await _ObjectiveSetRepository.GetByID(dir.ObjectiveSetID);

                    string source = "no valid path found";
                    // Way 1: The objective_set_id is equal to the objective's ID, and a lookup on id using objective_set_id finds it
                    // Below is a psuedo query to get the objective, I think it's an easier way to think about the relation
                    // SELECT * FROM objective 
                    //      WHERE objective.id = directive.objective_set_id
                    PsObjective? obj = null;
                    if (dir != null) {
                        obj = await _ObjectiveRepository.GetByID(dir.ObjectiveSetID);
                        if (obj != null) {
                            source = $"directly by directive.objection_set_id - {dir.ObjectiveSetID} > {obj.ID}";
                        }
                    }

                    // Way 2: The objective_set_id is equal to an entry in objective_set, use the group_id as the lookup id 
                    // SELECT * FROM objective 
                    //      LEFT JOIN objective_set ON objective_set.set_id = directive.objective_set_id 
                    //      WHERE objective.id = objective_set.group_id
                    if (objSet != null && obj == null) {
                        obj = await _ObjectiveRepository.GetByID(objSet.GroupID);
                        // Way 3: Use the objective_set, and search on group_id instead of id
                        // SELECT * FROM objective 
                        //      LEFT JOIN objective_set ON objective_set.set_id = directive.objective_set_id 
                        //      WHERE objective.group_id = objective_set.group_id
                        if (obj == null) {
                            obj = await _ObjectiveRepository.GetByGroupID(objSet.GroupID);
                            if (obj != null) {
                                source = $"indirect by objective.group_id from set_group.group_id - {dir?.ObjectiveSetID} > {objSet.GroupID} > {obj.ID}";
                            }
                        } else {
                            if (obj.GroupID == obj.ID) {
                                source = $"indirect by objective.id from set_group.group_id (safe) - {dir?.ObjectiveSetID} > {objSet.GroupID} > {obj.GroupID}";
                            } else {
                                source = $"indirect by objective.id from set_group.group_id (UNSAFE) - {dir?.ObjectiveSetID} > {objSet.GroupID} > {obj.GroupID}";
                            }
                        }
                    }

                    ExpandedCharacterDirective aaa = new ExpandedCharacterDirective() {
                        Entry = charDir,
                        Directive = await _DirectiveRepository.GetByID(charDir.DirectiveID),
                        Objective = obj,
                        ObjectiveType = (obj == null) ? null : await _ObjectiveTypeRepository.GetByID(obj.TypeID),
                        CharacterObjective = charDirObjectives.FirstOrDefault(obj => obj.DirectiveID == charDir.DirectiveID),
                        ObjectiveSource = source,
                    };

                    tier.Directives.Add(aaa);
                }

                exTree.Tiers = tierMap.Values.ToList();

                exCat.Trees.Add(exTree);
            }

            set.CharacterID = charID;
            set.Categories = charCategories.Values.ToList();

            return ApiOk(set);
        }

    }

}