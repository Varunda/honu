import { Loading } from "Loading";
import ApiWrapper from "api/ApiWrapper";

export class PsAlert {
    public id: number = 0;
    public timestamp: Date = new Date();
    public end: Date = new Date();
    public duration: number = 0;
    public zoneID: number = 0;
    public worldID: number = 0;
    public alertID: number = 0;
    public victorFactionID: number | null = null;
    public instanceID: number = 0;
    public name: string = "";
    public displayID: string = "";

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
        const alert: PsAlert = {
            ...elem,
            end: new Date(),
            timestamp: new Date(elem.timestamp)
        };

        const start: number = alert.timestamp.getTime();
        const endms: number = start + (alert.duration * 1000);

        alert.end = new Date(endms);

        if (alert.name.length == 0) {
            alert.displayID = `${alert.worldID}-${alert.instanceID}`;
        } else {
            alert.displayID = alert.name;
        }

        return alert;
    }

    public static async getByID(alertID: number): Promise<Loading<PsAlert>> {
        return AlertApi.get().readSingle(`/api/alerts/${alertID}`, AlertApi.readEntry);
    }

    public static async getAll(): Promise<Loading<PsAlert[]>> {
        return AlertApi.get().readList(`/api/alerts/`, AlertApi.readEntry);
    }

}
