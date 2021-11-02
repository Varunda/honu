import * as axios from "axios";
import { CharacterApi, PsCharacter } from "api/CharacterApi";

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

export class ExpEvent {
    public sourceID: string = "";
    public experienceID: number = 0;
    public loadoutID: number = 0;
    public teamID: number = 0;
    public otherID: string = "";
    public amount: number = 0;
    public worldID: number = 0;
    public zoneID: number = 0;
    public timestamp: Date = new Date();
}

export class ExpandedExpEvent {
    public event: ExpEvent = new ExpEvent();
    public source: PsCharacter | null = null;
    public other: PsCharacter | null = null;
}

export class Experience {

    public static ASSIST: number = 2;
    public static SPAWN_ASSIST: number = 3;
    public static PRIORITY_ASSIST: number = 371;
    public static HIGH_PRIORITY_ASSIST: number = 372;

    public static HEAL: number = 4;
    public static MAX_REPAIR: number = 6;
    public static REVIVE: number = 7;
    public static RESUPPLY: number = 34;
    public static SHIELD_REPAIR: number = 438;

    public static SQUAD_HEAL: number = 51;
    public static SQUAD_REVIVE: number = 53;
    public static SQUAD_RESUPPLY: number = 55;
    public static SQUAD_MAX_REPAIR: number = 142;
    public static SQUAD_SHIELD_REPAIR: number = 439;
    
	public static SQUAD_SPAWN: number = 56;
	public static GALAXY_SPAWN_BONUS: number = 201;
	public static SUNDERER_SPAWN_BONUS: number = 233;
	public static SQUAD_VEHICLE_SPAWN_BONUS: number = 355;
	public static GENERIC_NPC_SPAWN: number = 1410;

    public static isAssist(expID: number): boolean {
        return expID == this.ASSIST || expID == this.SPAWN_ASSIST
            || expID == this.PRIORITY_ASSIST || expID == this.HIGH_PRIORITY_ASSIST;
	}

    public static isHeal(expID: number): boolean {
        return expID == this.HEAL || expID == this.SQUAD_HEAL;
    }

    public static isRevive(expID: number): boolean {
        return expID == this.REVIVE || expID == this.SQUAD_REVIVE;
    }

    public static isResupply(expID: number): boolean {
        return expID == this.RESUPPLY || expID == this.SQUAD_RESUPPLY;
    }

    public static isMaxRepair(expID: number): boolean {
        return expID == this.MAX_REPAIR || expID == this.SQUAD_MAX_REPAIR;
    }

    public static isShieldRepair(expID: number): boolean {
        return expID == this.SHIELD_REPAIR || expID == this.SQUAD_SHIELD_REPAIR;
    }

    public static isSpawn(expID: number): boolean {
        return expID == this.SQUAD_SPAWN || expID == this.GALAXY_SPAWN_BONUS || expID == this.SUNDERER_SPAWN_BONUS
            || expID == this.SQUAD_VEHICLE_SPAWN_BONUS || expID == this.GENERIC_NPC_SPAWN;
    }

    public static isVehicleKill(expID: number): boolean {
        return expID == this.VKILL_FLASH || expID == this.VKILL_GALAXY
            || expID == this.VKILL_LIBERATOR || expID == this.VKILL_LIGHTNING
            || expID == this.VKILL_MAGRIDER || expID == this.VKILL_MOSQUITO
            || expID == this.VKILL_PROWLER || expID == this.VKILL_REAVER
            || expID == this.VKILL_SCYTHE || expID == this.VKILL_VANGUARD
            || expID == this.VKILL_HARASSER || expID == this.VKILL_VALKYRIE
            || expID == this.VKILL_ANT || expID == this.VKILL_COLOSSUS
            || expID == this.VKILL_JAVELIN || expID == this.VKILL_CHIMERA
            || expID == this.VKILL_DERVISH;
    }

	public static VKILL_FLASH: number = 24;
	public static VKILL_GALAXY: number = 60;
	public static VKILL_LIBERATOR: number = 61;
	public static VKILL_LIGHTNING: number = 62;
	public static VKILL_MAGRIDER: number = 63;
	public static VKILL_MOSQUITO: number = 64;
	public static VKILL_PROWLER: number = 65;
	public static VKILL_REAVER: number = 66;
	public static VKILL_SCYTHE: number = 67;
	public static VKILL_VANGUARD: number = 69;
	public static VKILL_HARASSER: number = 301;
	public static VKILL_VALKYRIE: number = 501;
	public static VKILL_ANT: number = 651;
	public static VKILL_COLOSSUS: number = 1449;
	public static VKILL_JAVELIN: number = 1480;
	public static VKILL_CHIMERA: number = 1565;
	public static VKILL_DERVISH: number = 1635;
}

export class ExpStatApi {

    private static _instance: ExpStatApi = new ExpStatApi();
    public static get(): ExpStatApi { return ExpStatApi._instance; }

    public static parseExpEvent(elem: any): ExpEvent {
        return {
            ...elem,
            timestamp: new Date(elem.timestamp)
        }
    }

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

    private static parseExpandedExpEntry(elem: any): ExpandedExpEvent {
        return {
            event: ExpStatApi.parseExpEvent(elem.event),
            source: elem.source == null ? null : CharacterApi.parse(elem.source),
            other: elem.other == null ? null : CharacterApi.parse(elem.other),
        };
    }

    public static async getBySessionID(sessionID: number): Promise<ExpandedExpEvent[]> {
        const response: axios.AxiosResponse<any> = await axios.default.get(`/api/exp/session/${sessionID}`);

        if (response.status != 200) {
            return [];
        }

        if (Array.isArray(response.data) == false) {
            throw ``;
        }

        return response.data.map((iter: any) => ExpStatApi.parseExpandedExpEntry(iter));
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

    public static async getCharacterShieldEntries(charID: string): Promise<CharacterExpSupportEntry[]> {
        return ExpStatApi.getList(`/api/exp/character/${charID}/shield_repair`, ExpStatApi.parseCharacterExpSupportEntry);
    }

    public static async getOutfitShieldEntries(outfitID: string, worldID: number, teamID: number): Promise<OutfitExpEntry[]> {
        return ExpStatApi.getList(`/api/exp/outfit/${outfitID}/shield_repair/${worldID}/${teamID}`, ExpStatApi.parseOutfitExpSupportEntry);
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
