import ApiWrapper from "api/ApiWrapper";
import { Loading } from "Loading";

export class RealtimeMapState {
    public id: number = 0;
    public worldID: number = 0;
    public zoneID: number = 0;
    public regionID: number = 0;
    public timestamp: Date = new Date();
    public saveTimestamp: Date = new Date();
    public owningFactionID: number = 0;
    public contested: boolean = false;
    public captureTimeMs: number = 0;
    public captureTimeLeftMs: number = 0;
    public captureFlagsCount: number = 0;
    public captureFlagsLeft: number = 0;
    public factionBounds = {
        vs: 0 as number,
        nc: 0 as number,
        tr: 0 as number,
        ns: 0 as number
    };
    public factionPercentage = {
        vs: 0 as number,
        nc: 0 as number,
        tr: 0 as number,
        ns: 0 as number
    };
}

export class RealtimeMapStateApi extends ApiWrapper<RealtimeMapState> {

    private static _instance: RealtimeMapStateApi = new RealtimeMapStateApi();
    public static get(): RealtimeMapStateApi { return RealtimeMapStateApi._instance; }

    public static parse(elem: any): RealtimeMapState {
        return {
            ...elem,
            timestamp: new Date(elem.timestamp),
            saveTimestamp: new Date(elem.saveTimestamp)
        };
    }

    public static async getHistorical(worldID: number, regionID: number): Promise<Loading<RealtimeMapState[]>> {
        return RealtimeMapStateApi.get().readList(`/api/realtime-map-state/history/${worldID}/${regionID}`, RealtimeMapStateApi.parse);
    }

}
