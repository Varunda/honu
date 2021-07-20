import * as axios from "axios";

export class CharacterWeaponKillEntry {
    public weaponID: string = "";
    public weaponName: string = "";
    public kills: number = 0;
    public headshotKills: number = 0;
}

export class OutfitKillerEntry {
    public characterID: string = "";
    public characterName: string = "";
    public kills: number = 0;
}

export class KillStatApi {

    private static _instance: KillStatApi = new KillStatApi();
    public static get(): KillStatApi { return KillStatApi._instance; }

    private static parseCharacterWeaponKillEntry(elem: any): CharacterWeaponKillEntry {
        return {
            weaponID: elem.weaponID,
            weaponName: elem.weaponName,
            kills: elem.kills,
            headshotKills: elem.headshotKills
        };
    }

    private static parseOutfitKillerEntry(elem: any): OutfitKillerEntry {
        return {
            characterID: elem.characterID,
            characterName: elem.characterName,
            kills: elem.kills
        };
    }

    public static async getWeaponEntries(charID: string): Promise<CharacterWeaponKillEntry[]> {
        const response: axios.AxiosResponse<any> = await axios.default.get(`/api/kills/character/${charID}`);

        if (response.status != 200) {
            return [];
        }

        if (Array.isArray(response.data) == false) {
            console.warn(`response data is not an array: ${response.data}`);
            return [];
        }

        const ret: CharacterWeaponKillEntry[] = [];
        for (const datum of response.data) {
            const elem: CharacterWeaponKillEntry = KillStatApi.parseCharacterWeaponKillEntry(datum);
            ret.push(elem);
        }

        return ret;
    }

    public static async getOutfitKillers(outfitID: string): Promise<OutfitKillerEntry[]> {
        const response: axios.AxiosResponse<any> = await axios.default.get(`/api/kills/outfit/${outfitID}`);

        if (response.status != 200) {
            return [];
        }

        if (Array.isArray(response.data) == false) {
            console.warn(`response data is not an array: ${response.data}`);
            return [];
        }

        const ret: OutfitKillerEntry[] = [];
        for (const datum of response.data) {
            const elem: OutfitKillerEntry = KillStatApi.parseOutfitKillerEntry(datum);
            ret.push(elem);
        }

        return ret;
    }

}
