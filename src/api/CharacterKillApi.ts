import * as axios from "axios";

export class CharacterWeaponKillEntry {
    public weaponID: string = "";
    public weaponName: string = "";
    public kills: number = 0;
    public headshotKills: number = 0;
}

export class CharacterKillApi {

    private static _instance: CharacterKillApi = new CharacterKillApi();
    public static get(): CharacterKillApi { return CharacterKillApi._instance; }

    private static parse(elem: any): CharacterWeaponKillEntry {
        return {
            weaponID: elem.weaponID,
            weaponName: elem.weaponName,
            kills: elem.kills,
            headshotKills: elem.headshotKills
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
            const elem: CharacterWeaponKillEntry = CharacterKillApi.parse(datum);
            ret.push(elem);
        }

        return ret;
    }

}
