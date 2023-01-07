import { Loading } from "../Loading";
import { Achievement, AchievementApi } from "./AchievementApi";
import ApiWrapper from "./ApiWrapper";

export class AchievementEarned {
    public id: number = 0;
    public characterID: string = "";
    public timestamp: Date = new Date();
    public achievementID: number = 0;
    public zoneID: number = 0;
    public worldID: number = 0;
}

export class AchievementEarnedBlock {
    public events: AchievementEarned[] = [];
    public achievements: Map<number, Achievement> = new Map();
}

export class AchievementEarnedApi extends ApiWrapper<AchievementEarned> {
    private static _instance: AchievementEarnedApi = new AchievementEarnedApi();
    public static get(): AchievementEarnedApi { return AchievementEarnedApi._instance; }

    public static parse(elem: any): AchievementEarned {
        return {
            ...elem,
            timestamp: new Date(elem.timestamp)
        };
    }

    public static parseBlock(elem: any): AchievementEarnedBlock {
        const achievements: Achievement[] = elem.achievements.map((iter: any) => AchievementApi.parse(iter));

        const map: Map<number, Achievement> = new Map();
        for (const a of achievements) {
            map.set(a.id, a);
        }

        return {
            events: elem.events.map((iter: any) => AchievementEarnedApi.parse(iter)),
            achievements: map
        };
    }

    public static getByCharacterID(charID: string, start: Date, end: Date): Promise<Loading<AchievementEarned[]>> {
        const params: URLSearchParams = new URLSearchParams();
        params.set("start", start.toISOString());
        params.set("end", end.toISOString());

        return AchievementEarnedApi.get().readList(`/api/achievement-earned/${charID}?${params.toString()}`, AchievementEarnedApi.parse);
    }

    public static getBySessionID(sessionID: number): Promise<Loading<AchievementEarned[]>> {
        return AchievementEarnedApi.get().readList(`/api/achievement-earned/session/${sessionID}`, AchievementEarnedApi.parse);
    }

    public static getBlockBySessionID(sessionID: number): Promise<Loading<AchievementEarnedBlock>> {
        return AchievementEarnedApi.get().readSingle(`/api/achievement-earned/session/${sessionID}/block`, AchievementEarnedApi.parseBlock);
    }

}