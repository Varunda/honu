import ApiWrapper from "api/ApiWrapper";
import { Loading } from "Loading";

export class RealtimeReconnectEntry {

    /**
     * Unique ID
     */
    public id: number = 0;

    /**
     * How many seconds the stream went without events
     */
    public duration: number = 0;

    /**
     * How many times the stream failed the health check before it reconnected
     */
    public failedCount: number = 0;

    /**
     * What stream failed the health check
     */
    public streamType: string = "";

    /**
     * ID of the world 
     */
    public worldID: number = 0;

    /**
     * When this failure ended
     */
    public timestamp: Date = new Date();

    /**
     * How many events on this stream were received before failure
     */
    public eventCount: number = 0;

}

export class RealtimeReconnectApi extends ApiWrapper<RealtimeReconnectEntry> {

    private static _instance: RealtimeReconnectApi = new RealtimeReconnectApi();
    public static get(): RealtimeReconnectApi { return RealtimeReconnectApi._instance; }

    public static parse(elem: any): RealtimeReconnectEntry {
        return {
            ...elem,
            timestamp: new Date(elem.timestamp)
        };
    }

    public static getByInterval(start: Date, end: Date, worldID: number | null = null): Promise<Loading<RealtimeReconnectEntry[]>> {
        const params: URLSearchParams = new URLSearchParams();
        params.set("start", start.toISOString());
        params.set("end", end.toISOString());
        if (worldID != null) {
            params.set("worldID", worldID.toString());
        }

        return RealtimeReconnectApi.get().readList(`/api/health/reconnects/?${params.toString()}`, RealtimeReconnectApi.parse);
    }

}