import * as axios from "axios";
import { CharacterApi, PsCharacter } from "api/CharacterApi";
import { Loading } from "Loading";
import ApiWrapper from "api/ApiWrapper";

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

export class ExperienceType {
    public id: number = 0;
    public name: string = "";
    public amount: number = 0;
}

export class ExpandedExpEvent {
    public event: ExpEvent = new ExpEvent();
    public source: PsCharacter | null = null;
    public other: PsCharacter | null = null;
}

export class ExperienceBlock {
    public inputCharacters: string[] = [];
    public periodStart: Date = new Date();
    public periodEnd: Date = new Date();
    public characters: PsCharacter[] = [];
    public experienceTypes: ExperienceType[] = [];
    public events: ExpEvent[] = [];
}

export class Experience {

    public static KILL: number = 1;
    public static PRIORITY_KILL: number = 278;
    public static HIGH_PRIORITY_KILL: number = 279;
    public static HEADSHOT: number = 37;

    public static ASSIST: number = 2;
    public static SPAWN_ASSIST: number = 3;
    public static PRIORITY_ASSIST: number = 371;
    public static HIGH_PRIORITY_ASSIST: number = 372;

    public static HEAL: number = 4;
    public static MAX_REPAIR: number = 6;
    public static REVIVE: number = 7;
    public static RESUPPLY: number = 34;
    public static SHIELD_REPAIR: number = 438;
    public static VEHICLE_RESUPPLY: number = 240;

    public static SQUAD_HEAL: number = 51;
    public static SQUAD_REVIVE: number = 53;
    public static SQUAD_RESUPPLY: number = 55;
    public static SQUAD_MAX_REPAIR: number = 142;
    public static SQUAD_SHIELD_REPAIR: number = 439;
    public static SQUAD_VEHICLE_RESUPPLY: number = 241;
    
	public static SQUAD_SPAWN: number = 56;
	public static GALAXY_SPAWN_BONUS: number = 201;
	public static SUNDERER_SPAWN_BONUS: number = 233;
	public static SQUAD_VEHICLE_SPAWN_BONUS: number = 355;
	public static GENERIC_NPC_SPAWN: number = 1410;
    public static ANT_SPAWN: number = 1988;

    public static MOTION_DETECT: number = 293;
    public static SQUAD_MOTION_DETECT: number = 294;
    public static SCOUT_RADAR_DETECT: number = 353;
    public static SQUAD_SCOUT_RADAR_DETECT: number = 354;

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

    public static VASSIST_FLASH: number = 101;
	public static VASSIST_GALAXY: number = 105;
	public static VASSIST_LIBERATOR: number = 106;
	public static VASSIST_LIGHTNING: number = 107;
	public static VASSIST_MAGRIDER: number = 108;
	public static VASSIST_MOSQUITO: number = 109;
	public static VASSIST_PROWLER: number = 110;
	public static VASSIST_REAVER: number = 111;
	public static VASSIST_SCYTHE: number = 112;
	public static VASSIST_VANGUARD: number = 114;
	public static VASSIST_HARASSER: number = 304;
	public static VASSIST_VALKYRIE: number = 504;
	public static VASSIST_ANT: number = 654;
	public static VASSIST_COLOSSUS: number = 1453;
	public static VASSIST_JAVELIN: number = 1484;
	public static VASSIST_CHIMERA: number = -1;
	public static VASSIST_DERVISH: number = -1;

    public static REPAIR_FLASH: number = 31;
    public static REPAIR_ENGI_TURRET: number = 88;
    public static REPAIR_PHALANX: number = 89;
    public static REPAIR_DROP_POD: number = 90;
    public static REPAIR_GALAXY: number = 91;
    public static REPAIR_LIBERATOR: number = 92;
    public static REPAIR_LIGHTNING: number = 93;
    public static REPAIR_MAGRIDER: number = 94;
    public static REPAIR_MOSQUITO: number = 95;
    public static REPAIR_PROWLER: number = 96;
    public static REPAIR_REAVER: number = 97;
    public static REPAIR_SCYTHE: number = 98;
    public static REPAIR_SUNDERER: number = 99;
    public static REPAIR_VANGUARD: number = 100;
    public static REPAIR_HARASSER: number = 303;
    public static REPAIR_VALKYRIE: number = 503;
    public static REPAIR_ANT: number = 653;
    public static REPAIR_HARDLIGHT_BARRIER: number = 1375;
    public static REPAIR_COLOSSUS: number = 1451;
    public static REPAIR_JAVELIN: number = 1482;
    public static REPAIR_CHIMERA: number = 1571;
    public static REPAIR_DERVISH: number = 1638;

    public static SQUAD_REPAIR_FLASH: number = 28;
    public static SQUAD_REPAIR_ENGI_TURRET: number = 129;
    public static SQUAD_REPAIR_PHALANX: number = 130;
    public static SQUAD_REPAIR_DROP_POD: number = 131;
    public static SQUAD_REPAIR_GALAXY: number = 132;
    public static SQUAD_REPAIR_LIBERATOR: number = 133;
    public static SQUAD_REPAIR_LIGHTNING: number = 134;
    public static SQUAD_REPAIR_MAGRIDER: number = 135;
    public static SQUAD_REPAIR_MOSQUITO: number = 136;
    public static SQUAD_REPAIR_PROWLER: number = 137;
    public static SQUAD_REPAIR_REAVER: number = 138;
    public static SQUAD_REPAIR_SCYTHE: number = 139;
    public static SQUAD_REPAIR_SUNDERER: number = 140;
    public static SQUAD_REPAIR_VANGUARD: number = 141;
    public static SQUAD_REPAIR_HARASSER: number = 302;
    public static SQUAD_REPAIR_VALKYRIE: number = 505;
    public static SQUAD_REPAIR_ANT: number = 656;
    public static SQUAD_REPAIR_HARDLIGHT_BARRIER: number = 1378;
    public static SQUAD_REPAIR_COLOSSUS: number = 1452;
    public static SQUAD_REPAIR_JAVELIN: number = 1481;
    public static SQUAD_REPAIR_CHIMERA: number = 1571;
    public static SQUAD_REPAIR_DERVISH: number = 1638;

    public static VehicleRepairs: number[] = [
        Experience.REPAIR_FLASH, Experience.REPAIR_ENGI_TURRET, Experience.REPAIR_PHALANX, Experience.REPAIR_DROP_POD, Experience.REPAIR_GALAXY,
        Experience.REPAIR_LIBERATOR, Experience.REPAIR_LIGHTNING, Experience.REPAIR_MAGRIDER, Experience.REPAIR_MOSQUITO, Experience.REPAIR_PROWLER,
        Experience.REPAIR_REAVER, Experience.REPAIR_SCYTHE, Experience.REPAIR_SUNDERER, Experience.REPAIR_VANGUARD, Experience.REPAIR_HARASSER,
        Experience.REPAIR_VALKYRIE, Experience.REPAIR_ANT, Experience.REPAIR_COLOSSUS, Experience.REPAIR_JAVELIN,
        Experience.REPAIR_CHIMERA, Experience.REPAIR_DERVISH
    ];

    public static SquadVehicleRepairs: number[] = [
        Experience.SQUAD_REPAIR_FLASH, Experience.SQUAD_REPAIR_ENGI_TURRET, Experience.SQUAD_REPAIR_PHALANX, Experience.SQUAD_REPAIR_DROP_POD,
        Experience.SQUAD_REPAIR_GALAXY, Experience.SQUAD_REPAIR_LIBERATOR, Experience.SQUAD_REPAIR_LIGHTNING, Experience.SQUAD_REPAIR_MAGRIDER,
        Experience.SQUAD_REPAIR_MOSQUITO, Experience.SQUAD_REPAIR_PROWLER, Experience.SQUAD_REPAIR_REAVER, Experience.SQUAD_REPAIR_SCYTHE,
        Experience.SQUAD_REPAIR_SUNDERER, Experience.SQUAD_REPAIR_VANGUARD, Experience.SQUAD_REPAIR_HARASSER, Experience.SQUAD_REPAIR_VALKYRIE,
        Experience.SQUAD_REPAIR_ANT, Experience.SQUAD_REPAIR_COLOSSUS, Experience.SQUAD_REPAIR_JAVELIN,
        Experience.SQUAD_REPAIR_CHIMERA, Experience.SQUAD_REPAIR_DERVISH
    ];

    /**
     * Is this exp ID a kill event?
     */
    public static isKill(expID: number): boolean {
        return expID == this.KILL || expID == this.PRIORITY_KILL
            || expID == this.HIGH_PRIORITY_KILL;
    }

    /**
     * Is this exp ID an assist event?
     */
    public static isAssist(expID: number): boolean {
        return expID == this.ASSIST || expID == this.SPAWN_ASSIST
            || expID == this.PRIORITY_ASSIST || expID == this.HIGH_PRIORITY_ASSIST;
	}

    /**
     * Is this exp ID an assist event?
     */
    public static isHeal(expID: number): boolean {
        return expID == this.HEAL || expID == this.SQUAD_HEAL;
    }

    /**
     * Is this exp ID a revive exp event
     */
    public static isRevive(expID: number): boolean {
        return expID == this.REVIVE || expID == this.SQUAD_REVIVE;
    }

    /**
     * Is this exp ID a resupply exp event
     */
    public static isResupply(expID: number): boolean {
        return expID == this.RESUPPLY || expID == this.SQUAD_RESUPPLY;
    }

    /**
     * Is this exp ID a max repair exp event
     */
    public static isMaxRepair(expID: number): boolean {
        return expID == this.MAX_REPAIR || expID == this.SQUAD_MAX_REPAIR;
    }

    /**
     * Is this exp ID a shield repair exp event
     */
    public static isShieldRepair(expID: number): boolean {
        return expID == this.SHIELD_REPAIR || expID == this.SQUAD_SHIELD_REPAIR;
    }

    /**
     * Is this exp ID a spawn event
     */
    public static isSpawn(expID: number): boolean {
        return expID == this.SQUAD_SPAWN || expID == this.GALAXY_SPAWN_BONUS || expID == this.SUNDERER_SPAWN_BONUS
            || expID == this.SQUAD_VEHICLE_SPAWN_BONUS || expID == this.GENERIC_NPC_SPAWN || expID == this.ANT_SPAWN;
    }

    /**
     * Is this exp ID a vehicle kill exp event
     */
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

    public static isVehicleAssist(expID: number): boolean {
        return expID == this.VKILL_FLASH || expID == this.VASSIST_GALAXY
            || expID == this.VASSIST_LIBERATOR || expID == this.VASSIST_LIGHTNING
            || expID == this.VASSIST_MAGRIDER || expID == this.VASSIST_MOSQUITO
            || expID == this.VASSIST_PROWLER || expID == this.VASSIST_REAVER
            || expID == this.VASSIST_SCYTHE || expID == this.VASSIST_VANGUARD
            || expID == this.VASSIST_HARASSER || expID == this.VASSIST_VALKYRIE
            || expID == this.VASSIST_ANT || expID == this.VASSIST_COLOSSUS
            || expID == this.VASSIST_JAVELIN || expID == this.VASSIST_CHIMERA
            || expID == this.VASSIST_DERVISH;
    }

    public static isVehicleRepair(expID: number): boolean {
        return Experience.isNonSquadVehicleRepair(expID) || Experience.isSquadVehicleRepair(expID);
    }

    public static isNonSquadVehicleRepair(expID: number): boolean {
        return Experience.VehicleRepairs.indexOf(expID) > -1;
    }

    public static isSquadVehicleRepair(expID: number): boolean {
        return Experience.SquadVehicleRepairs.indexOf(expID) > -1;
    }

}

export class ExpStatApi extends ApiWrapper<ExpEvent> {

    private static _instance: ExpStatApi = new ExpStatApi();
    public static get(): ExpStatApi { return ExpStatApi._instance; }

    public static parseExpEvent(elem: any): ExpEvent {
        return {
            ...elem,
            timestamp: new Date(elem.timestamp)
        }
    }

    public static parseCharacterExpSupportEntry(elem: any): CharacterExpSupportEntry {
        return {
            characterID: elem.characterID,
            characterName: elem.characterName,
            amount: elem.amount
        };
    }

    public static parseOutfitExpSupportEntry(elem: any): OutfitExpEntry {
        return {
            characterID: elem.characterID,
            characterName: elem.characterName,
            amount: elem.amount
        };
    }

    public static parseExpandedExpEntry(elem: any): ExpandedExpEvent {
        return {
            event: ExpStatApi.parseExpEvent(elem.event),
            source: elem.source == null ? null : CharacterApi.parse(elem.source),
            other: elem.other == null ? null : CharacterApi.parse(elem.other),
        };
    }

    public static parseBlock(elem: any): ExperienceBlock {
        return {
            inputCharacters: elem.inputCharacters,
            periodStart: new Date(elem.periodStart),
            periodEnd: new Date(elem.periodEnd),
            characters: elem.characters.map((iter: any) => CharacterApi.parse(iter)),
            events: elem.events.map((iter: any) => ExpStatApi.parseExpEvent(iter)),
            experienceTypes: elem.experienceTypes.map((iter: any) => ExpStatApi.parseExperienceType(iter))
        };
    }

    public static parseExperienceType(elem: any): ExperienceType {
        return {
            id: elem.id,
            name: elem.name,
            amount: elem.amount
        };
    }

    public static async getBySessionID(sessionID: number): Promise<Loading<ExperienceBlock>> {
        return ExpStatApi.get().readSingle(`/api/exp/sessions/${sessionID}`, ExpStatApi.parseBlock);
    }

    public static async getByCharacterIDAndRange(charID: string, start: Date, end: Date): Promise<Loading<ExpandedExpEvent[]>> {
        return ExpStatApi.get().readList(`/api/exp/${charID}/period?start=${start.toISOString()}&end=${end.toISOString()}`, ExpStatApi.parseExpandedExpEntry);
    }

    public static async getCharacterHealEntries(charID: string, useShort: boolean): Promise<Loading<CharacterExpSupportEntry[]>> {
        return ExpStatApi.get().readList(`/api/exp/character/${charID}/heals?useShort=${useShort}`, ExpStatApi.parseCharacterExpSupportEntry);
    }

    public static async getOutfitHealEntries(outfitID: string, useShort: boolean, worldID: number, teamID: number): Promise<Loading<OutfitExpEntry[]>> {
        return ExpStatApi.get().readList(`/api/exp/outfit/${outfitID}/heals/${worldID}/${teamID}?useShort=${useShort}`, ExpStatApi.parseCharacterExpSupportEntry);
    }

    public static async getCharacterShieldEntries(charID: string, useShort: boolean): Promise<Loading<CharacterExpSupportEntry[]>> {
        return ExpStatApi.get().readList(`/api/exp/character/${charID}/shield_repair?useShort=${useShort}`, ExpStatApi.parseCharacterExpSupportEntry);
    }

    public static async getOutfitShieldEntries(outfitID: string, useShort: boolean, worldID: number, teamID: number): Promise<Loading<OutfitExpEntry[]>> {
        return ExpStatApi.get().readList(`/api/exp/outfit/${outfitID}/shield_repair/${worldID}/${teamID}?useShort=${useShort}`, ExpStatApi.parseCharacterExpSupportEntry);
    }

    public static async getCharacterReviveEntries(charID: string, useShort: boolean): Promise<Loading<CharacterExpSupportEntry[]>> {
        return ExpStatApi.get().readList(`/api/exp/character/${charID}/revives?useShort=${useShort}`, ExpStatApi.parseCharacterExpSupportEntry);
    }

    public static async getOutfitReviveEntries(outfitID: string, useShort: boolean, worldID: number, teamID: number): Promise<Loading<OutfitExpEntry[]>> {
        return ExpStatApi.get().readList(`/api/exp/outfit/${outfitID}/revives/${worldID}/${teamID}?useShort=${useShort}`, ExpStatApi.parseCharacterExpSupportEntry);
    }

    public static async getCharacterResupplyEntries(charID: string, useShort: boolean): Promise<Loading<CharacterExpSupportEntry[]>> {
        return ExpStatApi.get().readList(`/api/exp/character/${charID}/resupplies?useShort=${useShort}`, ExpStatApi.parseCharacterExpSupportEntry);
    }

    public static async getOutfitResupplyEntries(outfitID: string, useShort: boolean, worldID: number, teamID: number): Promise<Loading<OutfitExpEntry[]>> {
        return ExpStatApi.get().readList(`/api/exp/outfit/${outfitID}/resupplies/${worldID}/${teamID}?useShort=${useShort}`, ExpStatApi.parseCharacterExpSupportEntry);
    }

    public static async getCharacterSpawnEntries(charID: string, useShort: boolean): Promise<Loading<CharacterExpSupportEntry[]>> {
        return ExpStatApi.get().readList(`/api/exp/character/${charID}/spawns?useShort=${useShort}`, ExpStatApi.parseCharacterExpSupportEntry);
    }

    public static async getOutfitSpawnEntries(outfitID: string, useShort: boolean, worldID: number, teamID: number): Promise<Loading<OutfitExpEntry[]>> {
        return ExpStatApi.get().readList(`/api/exp/outfit/${outfitID}/spawns/${worldID}/${teamID}?useShort=${useShort}`, ExpStatApi.parseCharacterExpSupportEntry);
    }

    public static async getCharacterVehicleKillEntries(charID: string, useShort: boolean): Promise<Loading<CharacterExpSupportEntry[]>> {
        return ExpStatApi.get().readList(`/api/exp/character/${charID}/vehicleKills?useShort=${useShort}`, ExpStatApi.parseCharacterExpSupportEntry);
    }

    public static async getOutfitVehicleKillEntries(outfitID: string, useShort: boolean, worldID: number, teamID: number): Promise<Loading<OutfitExpEntry[]>> {
        return ExpStatApi.get().readList(`/api/exp/outfit/${outfitID}/vehicleKills/${worldID}/${teamID}?useShort=${useShort}`, ExpStatApi.parseCharacterExpSupportEntry);
    }

}
