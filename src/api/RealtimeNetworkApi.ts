import { Loading } from "Loading";
import ApiWrapper from "api/ApiWrapper";

export class RealtimeNetwork {
    public worldID: number = 0;
    public timestamp: Date = new Date();
    public players: RealtimeNetworkPlayer[] = [];
}

export class RealtimeNetworkPlayer {
    public characterID: string = "";
    public display: string = "";
    public factionID: number = 0;
    public interactions: RealtimeNetworkInteraction[] = [];
}

export class RealtimeNetworkInteraction {
    public otherID: string = "";
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

    public static async getByWorldID(worldID: number): Promise<Loading<RealtimeNetwork>> {
        return RealtimeNetworkApi.get().readSingle(`/api/realtime-network/${worldID}`, RealtimeNetworkApi.parse);
    }

}
