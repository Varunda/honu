
import { PsItem } from "api/ItemApi";
import { Achievement } from "api/AchievementApi";
import { PsObjective } from "api/ObjectiveApi";
import { ObjectiveType } from "api/ObjectiveTypeApi";
import { PsVehicle } from "api/VehicleApi";

export type FlatCharacterAchievement = {
    achievementID: number;
    earnedCount: number;
    dateFinished: Date | null;
    dateStarted: Date;
    dateUpdated: Date | null;

    name: string;
    description: string;
    imageID: number;

    achievement: Achievement | null;
    item: PsItem | null;
    vehicle: PsVehicle | null;
    objTypeID: number;
    objective: PsObjective | null;
    objectiveType: ObjectiveType | null;
};

export type FlatSession = {
    id: number;
    characterID: string;
    start: Date;
    end: Date | null;
    outfitID: string | null;
    outfitName: string | null;
    outfitTag: string | null;
    outfitFaction: number | null;

    duration: number; // number of seconds

    summaryCalculated: Date | null;
    expGained: number;
    expPerMinute: number;

    kills: number;
    vkills: number;
    deaths: number;
    kd: number;
    kpm: number;
    vkpm: number;

    spawns: number;
    spawnsPerMinute: number;

    heals: number;
    revives: number;
    shieldRepairs: number;
    healsPerMinute: number;
    revivesPerMinute: number;
    shieldRepairsPerMinute: number;

    maxRepairs: number;
    resupplies: number;
    maxRepairsPerMinute: number;
    resuppliesPerMinute: number;
};
