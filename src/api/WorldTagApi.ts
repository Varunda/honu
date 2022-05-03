import { Loading, Loadable } from "Loading";
import ApiWrapper from "api/ApiWrapper";

export class WorldTagEntry {
    public id: number = 0;
    public characterID: string = "";
    public worldID: number = 0;
    public timestamp: Date = new Date();
    public lastKill: Date = new Date();
    public kills: number = 0;
    public targetKilled: Date | null = null;
    public wasKilled: boolean | null = null;
}

export class WorldTagApi extends ApiWrapper<WorldTagEntry> {
    private static _instance: WorldTagApi = new WorldTagApi();
    public static get(): WorldTagApi { return WorldTagApi._instance; }

    public static readEntry(elem: any): WorldTagEntry {
        return {
            id: elem.id,
            characterID: elem.characterID,
            worldID: elem.worldID,
            timestamp: new Date(elem.timestamp),
            targetKilled: (elem.targetKilled == null) ? null : new Date(elem.targetKilled),
            kills: elem.kills,
            wasKilled: elem.wasKilled,
            lastKill: new Date(elem.lastKill)
        };
    }

}
