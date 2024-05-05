
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
