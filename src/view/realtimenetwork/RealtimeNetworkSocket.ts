import * as sR from "signalR";

import { RealtimeNetwork } from "api/RealtimeNetworkApi";
import { PsCharacter, CharacterApi } from "api/CharacterApi";
import { PsOutfit, OutfitApi } from "api/OutfitApi";
import { Loading } from "Loading";

type CachedEntry<T> = {
    data: T | null;
    lastPulled: Date
};

type CachedCharacter = CachedEntry<PsCharacter>;
type CachedOutfit = CachedEntry<PsOutfit>;

export default class RealtimeNetworkSocket {
    private connection: sR.HubConnection | null = null;
    public socketState: string = "unconnected";

    private currentWorldID: number | null = null;

    private charMap: Map<string, CachedCharacter> = new Map();
    private outfitMap: Map<string, CachedOutfit> = new Map();

    private pendingCharIDs: Set<string> = new Set();
    private pendingOutfitIDs: Set<string> = new Set();

    private gettingCharIDs: Set<string> = new Set();
    private gettingOutfitIDs: Set<string> = new Set();

    /**
     * Callback for when the socket sends new data
     */
    public onUpdateNetwork: ((data: RealtimeNetwork) => void) | null = null;

    public onCharacterLoaded: ((character: PsCharacter) => void) | null = null;

    public onOutfitLoaded: ((outfit: PsOutfit) => void) | null = null;

    public constructor() {
        setInterval(() => {
            if (this.pendingCharIDs.size > 0) {
                this.getCharacters(Array.from(this.pendingCharIDs.values()));
            }

            if (this.pendingOutfitIDs.size > 0) {
                this.getOutfits(Array.from(this.pendingOutfitIDs.values()));
            }
        }, 1000) as unknown as number;
    }

    /**
     * Connect to the websocket and subscribe to the world we want info for
     * @param worldID ID of the world to subscribe to
     */
    public connect(worldID: number): void {
        this.currentWorldID = worldID;

        this.connection = new sR.HubConnectionBuilder()
            .withUrl("/ws/realtime-network")
            .withAutomaticReconnect([5000, 10000, 20000, 20000])
            .build();

        this.connection.on("UpdateNetwork", (data: any) => {
            if (this.onUpdateNetwork != null) {
                this.onUpdateNetwork(data);
            }
        });

        this.connection.start().then(() => {
            this.socketState = "opened";
            console.log(`socket> connected`);
            if (this.currentWorldID != null) {
                this.subscribe(this.currentWorldID);
            }
        }).catch(err => {
            console.error(err);
        });

        this.connection.onreconnected(() => {
            console.log(`socket> reconnected`);
            this.socketState = "opened";
            if (this.currentWorldID != null) {
                this.subscribe(this.currentWorldID);
            }
        });

        this.connection.onclose((err?: Error) => {
            this.socketState = "closed";
            if (err) {
                console.error("socket> onclose: ", err);
            }
        });

        this.connection.onreconnecting((err?: Error) => {
            this.socketState = "reconnecting";
            if (err) {
                console.error("socket> onreconnecting: ", err);
            }
        });
    }

    /**
     * Subscribe to the realtime network of a world (server)
     * @param worldID ID of the world to subscribe to
     * @returns void
     */
    private subscribe(worldID: number): void {
        if (this.connection == null) { return console.warn(`cannot connect to socket: connection is null`); }
        if (this.connection.state != sR.HubConnectionState.Connected) { return console.warn(`cannot connect to socket: state is no 'Connected': ${this.connection.state}`); }

        this.currentWorldID = worldID;

        this.connection.invoke("Initalize", worldID).then(() => {
            console.log(`socket> subscribed`);
        }).catch((err: any) => {
            console.error(`socket> Error subscribing to realtime network for ${worldID}: ${err}`);
        });
    }

    /**
     * Get a character in a non-blocking manner. If the character hasn't been loaded,
     *      null will be returned, but it will be put into a list of pending
     *      characters to get
     * 
     * @param charID ID of the character to get
     * @returns The character with the matching ID, or null if it hasn't been loaded,
     *      or doesn't exist
     */
    public getCharacter(charID: string): PsCharacter | null {
        if (this.charMap.has(charID)) {
            return this.charMap.get(charID)!.data;
        }

        this.pendingCharIDs.add(charID);
        return null;
    }

    public getOutfit(outfitID: string): PsOutfit | null {
        if (this.outfitMap.has(outfitID)) {
            return this.outfitMap.get(outfitID)!.data;
        }

        this.pendingOutfitIDs.add(outfitID);
        return null;
    }

    public cacheCharacter(charID: string): void {
        if (this.charMap.has(charID)) {
            return;
        }

        this.pendingCharIDs.add(charID);
    }

    public cacheOutfit(outfitID: string): void {
        if (this.outfitMap.has(outfitID)) {
            return;
        }

        this.pendingOutfitIDs.add(outfitID);
    }

    private async getOutfits(outfitIDs: string[]): Promise<PsOutfit[]> {
        const outfits: PsOutfit[] = [];

        const left: Set<string> = new Set(outfitIDs);

        const now: Date = new Date();

        for (const id of outfitIDs) {
            if (this.outfitMap.has(id)) {
                const o: CachedOutfit = this.outfitMap.get(id)!;

                if (now.getTime() - o.lastPulled.getTime() <= (1000 * 60 * 2)) {
                    left.delete(id);

                    if (o.data != null) {
                        outfits.push(o.data);
                        o.lastPulled = now;
                    }
                }
            }
        }

        for (const id of outfitIDs) {
            if (this.gettingOutfitIDs.has(id)) {
                left.delete(id);
            }
        }

        const fetchApi: number = left.size;

        if (left.size > 0) {
            const leftArr: string[] = Array.from(left.values());

            for (const iter of leftArr) {
                this.gettingOutfitIDs.add(iter);
            }

            const api: Loading<PsOutfit[]> = await OutfitApi.getByIDs(leftArr);

            if (api.state == "loaded") {
                for (const o of api.data) {
                    this.outfitMap.set(o.id, {
                        data: o,
                        lastPulled: new Date()
                    });
                    outfits.push(o);
                    left.delete(o.id);

                    if (this.pendingOutfitIDs.has(o.id)) {
                        if (this.onOutfitLoaded != null) {
                            this.onOutfitLoaded(o);
                        }
                        this.pendingOutfitIDs.delete(o.id);
                    }
                }
            }

            for (const iter of leftArr) {
                this.gettingOutfitIDs.delete(iter);
            }
        }

        const missing: number = left.size;

        for (const id of left) {
            this.outfitMap.set(id, {
                data: null,
                lastPulled: new Date()
            });
        }

        console.log(`socket:getOutfits> found ${outfitIDs.length - left.size} in local map, getting ${fetchApi} from api, missing ${missing}`);
        
        return outfits;
    }

    private async getCharacters(charIDs: string[]): Promise<PsCharacter[]> {
        const chars: PsCharacter[] = [];

        const left: Set<string> = new Set(charIDs);

        const now: Date = new Date();

        for (const charID of charIDs) {
            if (this.charMap.has(charID)) {
                const c: CachedCharacter = this.charMap.get(charID)!;

                if (now.getTime() - c.lastPulled.getTime() <= (1000 * 60 * 2)) {
                    left.delete(charID);

                    if (c.data != null) {
                        chars.push(c.data);
                        c.lastPulled = now;
                    }
                }
            }
        }

        for (const id of charIDs) {
            if (this.gettingCharIDs.has(id)) {
                left.delete(id);
            }
        }

        const fetchApi: number = left.size;

        if (left.size > 0) {
            const arr: string[] = Array.from(left.values());
            const leftArr: string[] = Array.from(left.values());

            for (const iter of leftArr) {
                this.gettingCharIDs.add(iter);
            }

            // There's an upper bound to how many characters you can get at once, causing getting like 700
            //      at once threw a HTTP2_NETWORK_ERROR. By capping how many can be requested at once,
            //      this fixes the issue
            while (arr.length > 0) {
                const iter: string[] = arr.splice(0, 200);
                const api: Loading<PsCharacter[]> = await CharacterApi.getByIDs(iter);

                if (api.state == "loaded") {
                    for (const c of api.data) {
                        this.charMap.set(c.id, {
                            data: c,
                            lastPulled: new Date()
                        });
                        chars.push(c);
                        left.delete(c.id);

                        if (this.pendingCharIDs.has(c.id)) {
                            if (this.onCharacterLoaded != null) {
                                this.onCharacterLoaded(c);
                            }
                            this.pendingCharIDs.delete(c.id);
                        }
                    }
                }
            }

            for (const iter of leftArr) {
                this.gettingCharIDs.delete(iter);
            }
        }

        const missing: number = left.size;

        for (const id of left) {
            this.charMap.set(id, {
                data: null,
                lastPulled: new Date()
            });
        }

        console.log(`socket:getCharacters> found ${charIDs.length - left.size} in local map, getting ${fetchApi} from api, missing ${missing}`);
        
        return chars;
    }

}