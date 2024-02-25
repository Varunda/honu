import { PsVehicle, VehicleApi } from "./VehicleApi";

export class VehicleUsageData {
    public worldID: number = 0;
    public zoneID: number = 0;
    public timestamp: Date = new Date();
    public total: number = 0;

    public vs: VehicleUsageFaction = new VehicleUsageFaction();
    public nc: VehicleUsageFaction = new VehicleUsageFaction();
    public tr: VehicleUsageFaction = new VehicleUsageFaction();
    public other: VehicleUsageFaction = new VehicleUsageFaction();
}

export class VehicleUsageFaction {
    public factionID: number = 0;
    public total: number = 0;
    public totalVehicles: number = 0;
    public usage: Map<number, VehicleUsageEntry> = new Map();
}

export class VehicleUsageEntry {
    public vehicleID: number = 0;
    public vehicle: PsVehicle | null = null;
    public vehicleName: string = "";
    public count: number = 0;
}

export class VehicleDataApi {

    public static parse(elem: any): VehicleUsageData {
        return {
            ...elem,
            timestamp: new Date(elem.timestamp),

            vs: VehicleDataApi.parseFaction(elem.vs),
            nc: VehicleDataApi.parseFaction(elem.nc),
            tr: VehicleDataApi.parseFaction(elem.tr),
            other: VehicleDataApi.parseFaction(elem.other)
        }
    }

    public static parseFaction(elem: any): VehicleUsageFaction {
        const fac: VehicleUsageFaction = new VehicleUsageFaction();

        fac.factionID = elem.factionID;
        fac.total = elem.total;
        fac.totalVehicles = elem.totalVehicles;

        const keys: string[] = Object.keys(elem.usage);

        for (const key of keys) {
            fac.usage.set(Number.parseInt(key), elem.usage[key]);
        }

        return fac;
    }

    public static parseEntry(elem: any): VehicleUsageEntry {
        return {
            ...elem,
            vehicle: elem.vehicle == null ? null : VehicleApi.parse(elem.vehicle)
        }
    }

}
