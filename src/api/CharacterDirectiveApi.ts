import { Loading } from "Loading";
import ApiWrapper from "api/ApiWrapper";

import { CharacterDirectiveObjective } from "api/CharacterDirectiveObjectiveApi";
import { PsDirective } from "api/DirectiveApi";
import { PsObjective } from "api/ObjectiveApi";
import { ObjectiveType } from "api/ObjectiveTypeApi";
import { CharacterDirectiveTier } from "api/CharacterDirectiveTierApi";
import { CharacterDirectiveTree } from "api/CharacterDirectiveTreeApi";
import { DirectiveTier } from "api/DirectiveTierApi";
import { DirectiveTree } from "api/DirectiveTreeApi";
import { DirectiveTreeCategory } from "api/DirectiveTreeCategoryApi";

export class CharacterDirective {
    public characterID: string = "";
    public directiveID: number = 0;
    public treeID: number = 0;
    public completionDate: Date | null = null;
}

export class CharacterDirectiveSet {
    public characterID: string = "";
    public categories: ExpandedCharacterDirectiveCategory[] = [];
}

export class ExpandedCharacterDirectiveCategory {
    public categoryID: number = 0;
    public category: DirectiveTreeCategory | null = null;
    public trees: ExpandedCharacterDirectiveTree[] = [];
}

export class ExpandedCharacterDirectiveTree {
    public entry: CharacterDirectiveTree = new CharacterDirectiveTree();
    public tree: DirectiveTree | null = null;
    public tiers: ExpandedCharacterDirectiveTier[] = [];
}

export class ExpandedCharacterDirectiveTier {
    public tierID: number = 0;
    public entry: CharacterDirectiveTier | null = null;
    public tier: DirectiveTier | null = null;
    public directives: ExpandedCharacterDirective[] = [];
}

export class ExpandedCharacterDirective {
    public entry: CharacterDirective | null = null;
    public directive: PsDirective = new PsDirective();
    public characterObjective: CharacterDirectiveObjective | null = null;
    public objective: PsObjective | null = null;
    public objectiveType: ObjectiveType | null = null;
    public objectiveSource: string = "";
    public achievementObjective: PsObjective | null = null;
    public achievementObjectiveType: ObjectiveType | null = null;
}

export class CharacterDirectiveApi extends ApiWrapper<CharacterDirective> {
    private static _instance: CharacterDirectiveApi = new CharacterDirectiveApi();
    public static get(): CharacterDirectiveApi { return CharacterDirectiveApi._instance; }

    public static parse(elem: any): CharacterDirective {
        return {
            ...elem,
            completionDate: elem.completionDate == null ? null : new Date(elem.completionDate)
        };
    }

    public static parseSet(elem: any): CharacterDirectiveSet {
        return {
            ...elem
        };
    }

    public static getByCharacterID(charID: string): Promise<Loading<CharacterDirectiveSet>> {
        return CharacterDirectiveApi.get().readSingle(`/api/character/${charID}/directives`, CharacterDirectiveApi.parseSet);
    }

}