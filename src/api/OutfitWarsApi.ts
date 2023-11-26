
import { Loading, Loadable } from "Loading";
import ApiWrapper from "api/ApiWrapper";

import { OutfitApi, PsOutfit } from "api/OutfitApi";

export class OutfitWarsOutfit {
    public outfitID: string = "";
    public factionID: number = 0;
    public worldID: number = 0;
    public outfitWarID: number = 0;
    public registrationOrder: number = 0;
    public status: string = "";
    public signupCount: number = 0;
}

export class ExpandedOutfitWarsOutfit {
    public entry: OutfitWarsOutfit = new OutfitWarsOutfit();
    public outfit: PsOutfit | null = null;
}

export class FlatOutfitWarsOutfit {
    public outfitID: string = "";
    public factionID: number = 0;
    public worldID: number = 0;
    public outfitWarID: number = 0;
    public registrationOrder: number = 0;
    public status: string = "";
    public signupCount: number = 0;

    public outfit: PsOutfit | null = null;
}

export class OutfitWarsOutfitApi extends ApiWrapper<OutfitWarsOutfit> {
    private static _instance: OutfitWarsOutfitApi = new OutfitWarsOutfitApi;
    public static get(): OutfitWarsOutfitApi { return OutfitWarsOutfitApi._instance; }

    public static parse(elem: any): OutfitWarsOutfit {
        return {
            ...elem,
            signupCount: (elem.status == "Full") ? 24 : elem.signupCount
        };
    }

    public static parseExpanded(elem: any): ExpandedOutfitWarsOutfit {
        return {
            entry: OutfitWarsOutfitApi.parse(elem.entry),
            outfit: elem.outfit != null ? OutfitApi.parse(elem.outfit) : null
        };
    }

    public static parseFlat(elem: any): FlatOutfitWarsOutfit {
        return {
            ...elem.entry,
            signupCount: (elem.entry.status == "Full") ? 24 : elem.entry.signupCount,
            outfit: elem.outfit != null ? OutfitApi.parse(elem.outfit) : null
        };
    }

    public static getAll(): Promise<Loading<FlatOutfitWarsOutfit[]>> {
        return OutfitWarsOutfitApi.get().readList(`/api/outfit-wars/registration`, OutfitWarsOutfitApi.parseFlat);
    }

}

export class OutfitWarsMatch {
    public matchID: string = "";
    public roundID: string = "";
    public outfitWarsID: number = 0;
    public outfitAId: string = "";
    public outfitBId: string = "";
    public timestamp: Date = new Date();
    public worldID: number = 0;
    public outfitAFactionId: number = 0;
    public outfitBFactionId: number = 0;
}

export class FlatOutfitWarsMatch {

    public match: OutfitWarsMatch = new OutfitWarsMatch();

    public outfitAId: string = "";
    public outfitADisplay: string = "";
    public outfitAFactionId: number = 0;

    public outfitBId: string = "";
    public outfitBDisplay: string = "";
    public outfitBFactionId: number = 0;

    public timestamp: Date = new Date();
    public worldID: number = 0;

}

export class OutfitWarsMatchApi extends ApiWrapper<OutfitWarsMatch> {

    private static _instance: OutfitWarsMatchApi = new OutfitWarsMatchApi;
    public static get(): OutfitWarsMatchApi { return OutfitWarsMatchApi._instance; }

    public static parse(elem: any): OutfitWarsMatch {
        return {
            ...elem,
            timestamp: new Date(elem.timestamp)
        };
    }

    public static parseFlat(elem: any): FlatOutfitWarsMatch {
        let flat: FlatOutfitWarsMatch = new FlatOutfitWarsMatch();

        flat.match = OutfitWarsMatchApi.parse(elem.match);
        flat.timestamp = flat.match.timestamp;
        flat.worldID = flat.match.worldID;

        flat.outfitAId = flat.match.outfitAId;
        flat.outfitAFactionId = flat.match.outfitAFactionId;
        if (elem.outfitA != null) {
            const outfitA: PsOutfit = OutfitApi.parse(elem.outfitA);
            if (outfitA.tag != null) {
                flat.outfitADisplay = `[${outfitA.tag}] ${outfitA.name}`;
            } else {
                flat.outfitADisplay = `${outfitA.name}`;
            }
        }

        flat.outfitBId = flat.match.outfitBId;
        flat.outfitBFactionId = flat.match.outfitBFactionId;
        if (elem.outfitB != null) {
            const outfitB: PsOutfit = OutfitApi.parse(elem.outfitB);
            if (outfitB.tag != null) {
                flat.outfitBDisplay = `[${outfitB.tag}] ${outfitB.name}`;
            } else {
                flat.outfitBDisplay = `${outfitB.name}`;
            }
        }

        return flat;
    }

    public static getAll(): Promise<Loading<FlatOutfitWarsMatch[]>> {
        return OutfitWarsMatchApi.get().readList(`/api/outfit-wars/match`, OutfitWarsMatchApi.parseFlat);
    }

}
