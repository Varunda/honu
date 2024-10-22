import { Loading } from "Loading";
import ApiWrapper from "api/ApiWrapper";

export class PsAlert {
    public id: number = 0;
    public type: string = "unset";
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

        // 2024-10-21 TODO: this is stupid. load this from census instead
        if (alert.alertID == 0 && alert.zoneID == 0) {
            alert.type = "Daily";
        } else if (alert.alertID == 0 && alert.zoneID != 0) {
            alert.type = "Event";
        } else if ([176, 177, 178, 179, 186, 187, 188, 189, 190, 191, 192, 193, 248, 249, 250].indexOf(alert.alertID) > -1) {
            alert.type = "Meltdown Lock";
        } else if ([198, 199, 200, 201].indexOf(alert.alertID) > -1) {
            alert.type = "Maximum Pressure";
        } else if ([228, 229, 230, 231, 232].indexOf(alert.alertID) > -1) {
            alert.type = "Aerial Anomaly";
        } else if ([236, 237, 238, 239, 240, 241].indexOf(alert.alertID) > -1) { // no idea why 6 of them
            alert.type = "Sudden Death";
        } else if ([242, 243, 244, 245, 246].indexOf(alert.alertID) > -1) {
            alert.type = "Forgotten Fleet Carrier";
        } else if (alert.duration == 90 * 60) {
            alert.type = "Continent Lock";
        } else {
            alert.type = `unchecked type ${alert.alertID}`;
        }

        return alert;
    }

    public static async getByID(alertID: number): Promise<Loading<PsAlert>> {
        return AlertApi.get().readSingle(`/api/alerts/${alertID}`, AlertApi.readEntry);
    }

    public static async getAll(): Promise<Loading<PsAlert[]>> {
        return AlertApi.get().readList(`/api/alerts/`, AlertApi.readEntry);
    }

    public static async getRecent(): Promise<Loading<PsAlert[]>> {
        return AlertApi.get().readList(`/api/alerts/recent`, AlertApi.readEntry);
    }

    public static async insert(alert: PsAlert): Promise<Loading<number>> {
        return AlertApi.get().postReplyForm(`/api/alerts`, alert, (iter: any) => iter);
    }

}
