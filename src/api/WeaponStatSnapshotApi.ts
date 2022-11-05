import ApiWrapper from "api/ApiWrapper";
import { Loading, Loadable } from "Loading";

export class WeaponStatSnapshot {
    public id: number = 0;
    public itemID: number = 0;
    public timestamp: Date = new Date();
    public users: number = 0;
    public kills: number = 0;
    public deaths: number = 0;
    public shots: number = 0;
    public shotsHit: number = 0;
    public headshots: number = 0;
    public vehicleKills: number = 0;
    public secondsWith: number = 0;
}

export class WeaponStatSnapshotApi extends ApiWrapper<WeaponStatSnapshot> {
    private static _instance: WeaponStatSnapshotApi = new WeaponStatSnapshotApi();
    public static get(): WeaponStatSnapshotApi { return WeaponStatSnapshotApi._instance; }

    public static parse(elem: any): WeaponStatSnapshot {
        return {
            ...elem,
            timestamp: new Date(elem.timestamp)
        };
    }

    public static async getByItemID(itemID: number): Promise<Loading<WeaponStatSnapshot[]>> {
        return WeaponStatSnapshotApi.get().readList(`/api/item/${itemID}/snapshots`, WeaponStatSnapshotApi.parse);
    }

}
