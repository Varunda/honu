import * as axios from "axios";

import { PsCharacter, CharacterApi } from "api/CharacterApi";
import { PsItem, ItemApi } from "api/ItemApi";

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

export class KillEvent {
    public attackerCharacterID: string = "";
    public attackerLoadoutID: number = 0;
    public attackerTeamID: number = 0;
    public attackerFireModeID: number = 0;
    public killedCharacterID: string = "";
    public killedLoadoutID: number = 0;
    public killedTeamID: number = 0;
    public worldID: number = 0;
    public zoneID: number = 0;
    public weaponID: string = "";
    public isHeadshot: boolean = false;
    public revivedEventID: number = 0;
    public timestamp: Date = new Date();
}

export class ExpandedKillEvent {
    public event: KillEvent = new KillEvent();
    public killed: PsCharacter | null = null;
    public attacker: PsCharacter | null = null;
    public item: PsItem | null = null;
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

    public static parseKillEvent(elem: any): KillEvent {
        return {
            ...elem,
            timestamp: new Date(elem.timestamp)
        };
    }

    public static parseExpandedKillEvent(elem: any): ExpandedKillEvent {
        return {
            event: KillStatApi.parseKillEvent(elem.event),
            attacker: elem.attacker == null ? null : CharacterApi.get().parse(elem.attacker),
            killed: elem.killed == null ? null : CharacterApi.get().parse(elem.killed),
            item: elem.item == null ? null : ItemApi.parse(elem.item)
        }
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

        return response.data.map((iter: any) => KillStatApi.parseCharacterWeaponKillEntry(iter));
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

        return response.data.map((iter: any) => KillStatApi.parseOutfitKillerEntry(iter));
    }

    public static async getSessionKills(sessionID: number): Promise<ExpandedKillEvent[]> {
        const response: axios.AxiosResponse<any> = await axios.default.get(`/api/kills/session/${sessionID}`);

        if (response.status != 200) {
            return [];
        }

        if (Array.isArray(response.data) == false) {
            throw `response.data is not an array`;
        }

        return response.data.map((iter: any) => KillStatApi.parseExpandedKillEvent(iter));
    }

}
