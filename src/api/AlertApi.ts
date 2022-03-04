import { Loading } from "Loading";
import ApiWrapper from "api/ApiWrapper";

export class PsAlert {
    public id: number = 0;
    public timestamp: Date = new Date();
    public duration: number = 0;
    public zoneID: number = 0;
    public worldID: number = 0;
    public alertID: number = 0;
    public victorFactionID: number | null = null;

    public warpgateVS: number = 0;
    public warpgateNC: number = 0;
    public warpgateTR: number = 0;
    public zoneFacilityCount: number = 0;

    public countVS: number | null = null;
    public countNC: number | null = null;
    public countTR: number | null = null;
}

export class AlertApi extends ApiWrapper<PsAlert> {
    private static _instance: AlertApi = new AlertApi();
    public static get(): AlertApi { return AlertApi._instance; }

    public static readEntry(elem: any): PsAlert {
        return {
            timestamp: new Date(elem.timestamp),
            ...elem
        };
    }

    public static async getByID(alertID: number): Promise<Loading<PsAlert>> {
        return AlertApi.get().readSingle(`/api/alerts/${alertID}`, AlertApi.readEntry);
    }

    public static async getAll(): Promise<Loading<PsAlert[]>> {
        return AlertApi.get().readList(`/api/alerts/`, AlertApi.readEntry);
    }

}
