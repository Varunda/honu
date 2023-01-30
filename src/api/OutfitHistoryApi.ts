import { Loading } from "../Loading";
import ApiWrapper from "./ApiWrapper";
import { OutfitApi, PsOutfit } from "./OutfitApi";

export class OutfitHistoryEntry {
    public outfitID: string = "";
    public start: Date = new Date();
    public end: Date = new Date();
}

export class OutfitHistoryBlock {
    public characterID: string = "";
    public entries: OutfitHistoryEntry[] = [];
    public outfits: PsOutfit[] = [];
}

export class OutfitHistoryApi extends ApiWrapper<OutfitHistoryBlock> {

    private static _instance: OutfitHistoryApi = new OutfitHistoryApi();
    public static get(): OutfitHistoryApi { return OutfitHistoryApi._instance; }

    public static parseEntry(elem: any): OutfitHistoryEntry {
        return {
            outfitID: elem.outfitID,
            start: new Date(elem.start),
            end: new Date(elem.end)
        };
    }

    public static parseBlock(elem: any): OutfitHistoryBlock {
        return {
            characterID: elem.characterID,
            entries: elem.entries.map((iter: any) => OutfitHistoryApi.parseEntry(iter)),
            outfits: elem.outfits.map((iter: any) => OutfitApi.parse(iter))
        };
    }

    public static getByCharacterID(charID: string): Promise<Loading<OutfitHistoryBlock>> {
        return OutfitHistoryApi.get().readSingle(`/api/character/${charID}/outfit_history`, OutfitHistoryApi.parseBlock);
    }

}