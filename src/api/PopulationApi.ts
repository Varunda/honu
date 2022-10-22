import { Loading } from "Loading";
import ApiWrapper from "api/ApiWrapper";

export class PopulationEntry {
    public id: number = 0;
    public timestamp: Date = new Date();
    public duration: number = 0;
    public worldID: number = 0;
    public factionID: number = 0;
    public total: number = 0;
    public logins: number = 0;
    public logouts: number = 0;
    public uniqueCharacters: number = 0;
    public secondsPlayed: number = 0;
    public averageSessionLength: number = 0;
}

export class PopulationApi extends ApiWrapper<PopulationEntry> {

    private static _instance: PopulationApi = new PopulationApi();
    public static get(): PopulationApi { return PopulationApi._instance; }

    public static parse(elem: any): PopulationEntry {
        return {
            ...elem,
            timestamp: new Date(elem.timestamp)
        };
    }

    public static async getHistorical(start: Date, end: Date, worlds: number[] = [], factions: number[] = []): Promise<Loading<PopulationEntry[]>> {
        const params: URLSearchParams = new URLSearchParams();
        params.set("start", start.toISOString());
        params.set("end", end.toISOString());

        for (const world of worlds) {
            params.append("worlds", world.toString());
        }

        for (const faction of factions) {
            params.append("factions", faction.toString());
        }

        return PopulationApi.get().readList(`/api/population/historical?${params.toString()}`, PopulationApi.parse);
    }

}
