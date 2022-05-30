import { Loading } from "Loading";
import ApiWrapper from "api/ApiWrapper";

export class RealtimeNetwork {
    public worldID: number = 0;
    public timestamp: Date = new Date();
    public players: RealtimeNetworkPlayer[] = [];
}

export class RealtimeNetworkPlayer {
    public characterID: string = "";
    public outfitID: string | null = null;
    public factionID: number = 0;
    public interactions: RealtimeNetworkInteraction[] = [];
}

export class RealtimeNetworkInteraction {
    public otherID: string = "";
    public outfitID: string | null = null;
    public factionID: number = 0;
    public strength: number = 0;
}

export class RealtimeNetworkApi extends ApiWrapper<RealtimeNetwork> {
    private static _instance: RealtimeNetworkApi = new RealtimeNetworkApi();
    public static get(): RealtimeNetworkApi { return RealtimeNetworkApi._instance; }

    public static parse(elem: any): RealtimeNetwork {
        return {
            ...elem,
            timestamp: new Date(elem.timestamp),
            players: elem.players.map((iter: any) => RealtimeNetworkApi.parsePlayer(iter))
        }
    }

    public static parsePlayer(elem: any): RealtimeNetworkPlayer {
        return {
            ...elem,
            interactions: elem.interactions.map((iter: any) => RealtimeNetworkApi.parseInteraction(iter))
        };
    }

    public static parseInteraction(elem: any): RealtimeNetworkInteraction {
        return {
            ...elem
        };
    }

    public static async getByWorldID(worldID: number, start: Date | null = null, end: Date | null = null, zoneID: number | null = null): Promise<Loading<RealtimeNetwork>> {
        const params: URLSearchParams = new URLSearchParams();

        if (start != null) { params.append("start", start.toISOString()); }
        if (end != null) { params.append("end", end.toISOString()); }
        if (zoneID != null) { params.append("zoneID", zoneID.toString()); }

        return RealtimeNetworkApi.get().readSingle(`/api/realtime-network/${worldID}?${params.toString()}`, RealtimeNetworkApi.parse);
    }

}
