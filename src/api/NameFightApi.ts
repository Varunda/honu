import { Loading } from "Loading";
import ApiWrapper from "api/ApiWrapper";

export class NameFightEntry {
    public timestamp: Date = new Date();
    public worldID: number = 0;
    public total: number = 0;
    public winsVs: number = 0;
    public winsNc: number = 0;
    public winsTr: number = 0;
}

export class NameFightApi extends ApiWrapper<NameFightEntry> {
    private static _instance: NameFightApi = new NameFightApi();
    public static get(): NameFightApi { return NameFightApi._instance; }

    public static parse(elem: any): NameFightEntry {
        return {
            timestamp: new Date(elem.timestamp),
            worldID: elem.worldID,
            total: elem.total,
            winsVs: elem.winsVs,
            winsNc: elem.winsNc,
            winsTr: elem.winsTr,
        }
    }

    public static getEntry(): Promise<Loading<NameFightEntry[]>> {
        return NameFightApi.get().readList(`/api/name-fight/`, NameFightApi.parse);
    }

}