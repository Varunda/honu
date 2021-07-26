import * as axios from "axios";

export class CharacterExpSupportEntry {
    public characterID: string = "";
    public characterName: string = "";
    public amount: number = 0;
}

export class OutfitExpEntry {
    public characterID: string = "";
    public characterName: string = "";
    public amount: number = 0;
}

export class ExpStatApi {

    private static _instance: ExpStatApi = new ExpStatApi();
    public static get(): ExpStatApi { return ExpStatApi._instance; }

    private static parseCharacterExpSupportEntry(elem: any): CharacterExpSupportEntry {
        return {
            characterID: elem.characterID,
            characterName: elem.characterName,
            amount: elem.amount
        };
    }

    private static parseOutfitExpSupportEntry(elem: any): OutfitExpEntry {
        return {
            characterID: elem.characterID,
            characterName: elem.characterName,
            amount: elem.amount
        };
    }

    private static async getList<T>(url: string, reader: (elem: any) => T): Promise<T[]> {
        const response: axios.AxiosResponse<any> = await axios.default.get(url);

        if (response.status != 200) {
            return [];
        }

        if (Array.isArray(response.data) == false) {
            console.warn(`response data is not an array: ${response.data}`);
            return [];
        }

        const ret: T[] = [];
        for (const datum of response.data) {
            const elem: T = reader(datum);
            ret.push(elem);
        }

        return ret;
    }

    public static async getCharacterHealEntries(charID: string): Promise<CharacterExpSupportEntry[]> {
        return ExpStatApi.getList(`/api/exp/character/${charID}/heals`, ExpStatApi.parseCharacterExpSupportEntry);
    }

    public static async getOutfitHealEntries(outfitID: string, worldID: number, teamID: number): Promise<OutfitExpEntry[]> {
        return ExpStatApi.getList(`/api/exp/outfit/${outfitID}/heals/${worldID}/${teamID}`, ExpStatApi.parseOutfitExpSupportEntry);
    }

    public static async getCharacterReviveEntries(charID: string): Promise<CharacterExpSupportEntry[]> {
        return ExpStatApi.getList(`/api/exp/character/${charID}/revives`, ExpStatApi.parseCharacterExpSupportEntry);
    }

    public static async getOutfitReviveEntries(outfitID: string, worldID: number, teamID: number): Promise<OutfitExpEntry[]> {
        return ExpStatApi.getList(`/api/exp/outfit/${outfitID}/revives/${worldID}/${teamID}`, ExpStatApi.parseOutfitExpSupportEntry);
    }

    public static async getCharacterResupplyEntries(charID: string): Promise<CharacterExpSupportEntry[]> {
        return ExpStatApi.getList(`/api/exp/character/${charID}/resupplies`, ExpStatApi.parseCharacterExpSupportEntry);
    }

    public static async getOutfitResupplyEntries(outfitID: string, worldID: number, teamID: number): Promise<OutfitExpEntry[]> {
        return ExpStatApi.getList(`/api/exp/outfit/${outfitID}/resupplies/${worldID}/${teamID}`, ExpStatApi.parseOutfitExpSupportEntry);
    }

    public static async getCharacterSpawnEntries(charID: string): Promise<CharacterExpSupportEntry[]> {
        return ExpStatApi.getList(`/api/exp/character/${charID}/spawns`, ExpStatApi.parseCharacterExpSupportEntry);
    }

    public static async getOutfitSpawnEntries(outfitID: string, worldID: number, teamID: number): Promise<OutfitExpEntry[]> {
        return ExpStatApi.getList(`/api/exp/outfit/${outfitID}/spawns/${worldID}/${teamID}`, ExpStatApi.parseOutfitExpSupportEntry);
    }

    public static async getCharacterVehicleKillEntries(charID: string): Promise<CharacterExpSupportEntry[]> {
        return ExpStatApi.getList(`/api/exp/character/${charID}/vehicleKills`, ExpStatApi.parseCharacterExpSupportEntry);
    }

    public static async getOutfitVehicleKillEntries(outfitID: string, worldID: number, teamID: number): Promise<OutfitExpEntry[]> {
        return ExpStatApi.getList(`/api/exp/outfit/${outfitID}/vehicleKills/${worldID}/${teamID}`, ExpStatApi.parseOutfitExpSupportEntry);
    }

}