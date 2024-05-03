import ApiWrapper from "api/ApiWrapper";
import { Loading, Loadable } from "Loading";
import { Achievement, AchievementApi } from "./AchievementApi";
import { ExperienceAwardType, ExpStatApi } from "./ExpStatApi";
import { ItemApi, PsItem } from "./ItemApi";
import { ObjectiveApi, PsObjective } from "./ObjectiveApi";
import { ObjectiveType, ObjectiveTypeApi } from "./ObjectiveTypeApi";
import { PsVehicle, VehicleApi } from "./VehicleApi";

export class CharacterAchievement {
    public characterID: string = "";
    public achievementID: number = 0;
    public earnedCount: number = 0;
    public startDate: Date = new Date();
    public finishDate: Date = new Date();
    public lastSaveDate: Date = new Date();
}

export class ExpandedCharacterAchievement {
    public entry: CharacterAchievement = new CharacterAchievement();
    public achievement: Achievement | null = null;
    public objective: PsObjective | null = null;
    public item: PsItem | null = null;
}

export class CharacterAchievementBlock {
    public characterID: string = "";
    public entries: CharacterAchievement[] = [];
    public achievements: Achievement[] = [];
    public objectives: PsObjective[] = [];
    public objectiveTypes: ObjectiveType[] = [];
    public items: PsItem[] = [];
    public vehicles: PsVehicle[] = [];
    public awardTypes: ExperienceAwardType[] = [];
}

export class CharacterAchievementApi extends ApiWrapper<CharacterAchievement> {
    private static _instance: CharacterAchievementApi = new CharacterAchievementApi();
    public static get(): CharacterAchievementApi { return CharacterAchievementApi._instance; }

    public static parse(elem: any): CharacterAchievement {
        return {
            ...elem,
            startDate: new Date(elem.startDate),
            finishDate: elem.finishDate == null ? null : new Date(elem.finishDate),
            lastSaveDate: new Date(elem.lastSaveDate)
        }
    }

    public static parseBlock(elem: any): CharacterAchievementBlock {
        return {
            characterID: elem.characterID,
            entries: elem.entries.map((iter: any) => CharacterAchievementApi.parse(iter)),
            achievements: elem.achievements.map((iter: any) => AchievementApi.parse(iter)),
            objectives: elem.objectives.map((iter: any) => ObjectiveApi.parse(iter)),
            objectiveTypes: elem.objectiveTypes.map((iter: any) => ObjectiveTypeApi.parse(iter)),
            items: elem.items.map((iter: any) => ItemApi.parse(iter)),
            vehicles: elem.vehicles.map((iter: any) => VehicleApi.parse(iter)),
            awardTypes: elem.awardTypes.map((iter: any) => ExpStatApi.parseAwardType(iter))
        }
    }

    public static parseExpanded(elem: any): ExpandedCharacterAchievement {
        return {
            entry: CharacterAchievementApi.parse(elem.entry),
            achievement: elem.achievement == null ? null : AchievementApi.parse(elem.achievement),
            objective: elem.objective == null ? null : ObjectiveApi.parse(elem.objective),
            item: elem.item == null ? null : ItemApi.parse(elem.item)
        };
    }

    public static getByCharacterID(charID: string): Promise<Loading<CharacterAchievementBlock>> {
        return CharacterAchievementApi.get().readSingle(`/api/character/${charID}/achievements`, CharacterAchievementApi.parseBlock);
    }

}
