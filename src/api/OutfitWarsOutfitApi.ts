
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
            outfit: elem.outfit != null ? OutfitApi.parse(elem.outfit) : null
        };
    }

    public static getAll(): Promise<Loading<FlatOutfitWarsOutfit[]>> {
        return OutfitWarsOutfitApi.get().readList(`/api/outfit-wars/registration`, OutfitWarsOutfitApi.parseFlat);
    }

}
