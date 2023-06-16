import * as axios from "axios";
import { Loading } from "Loading";
import ApiWrapper from "api/ApiWrapper";

import { PsCharacter, CharacterApi } from "api/CharacterApi";
import { PsItem, ItemApi } from "api/ItemApi";
import { FireGroupToFireMode } from "api/FireGroupToFireModeApi";
import { ItemCategory } from "api/ItemCategoryApi";

export class CharacterWeaponKillEntry {
    public weaponID: number = 0;
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
    public attackerVehicleID: number = 0;
    public killedCharacterID: string = "";
    public killedLoadoutID: number = 0;
    public killedTeamID: number = 0;
    public worldID: number = 0;
    public zoneID: number = 0;
    public weaponID: number = 0;
    public isHeadshot: boolean = false;
    public revivedEventID: number = 0;
    public timestamp: Date = new Date();
}

export class ExpandedKillEvent {
    public event: KillEvent = new KillEvent();
    public killed: PsCharacter | null = null;
    public attacker: PsCharacter | null = null;
    public item: PsItem | null = null;
    public fireGroupToFireMode: FireGroupToFireMode | null = null;
}

export class KillDeathBlock {
    public kills: KillEvent[] = [];
    public deaths: KillEvent[] = [];
    public characters: Map<string, PsCharacter> = new Map();
    public weapons: Map<number, PsItem> = new Map();
    public fireModes: Map<number, FireGroupToFireMode> = new Map();
    public itemCategories: Map<number, ItemCategory> = new Map();
}

export class KillStatApi extends ApiWrapper<KillEvent> {

    private static _instance: KillStatApi = new KillStatApi();
    public static get(): KillStatApi { return KillStatApi._instance; }

    public static parseCharacterWeaponKillEntry(elem: any): CharacterWeaponKillEntry {
        return {
            weaponID: elem.weaponID,
            weaponName: elem.weaponName,
            kills: elem.kills,
            headshotKills: elem.headshotKills
        };
    }

    public static parseOutfitKillerEntry(elem: any): OutfitKillerEntry {
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
            attacker: elem.attacker == null ? null : CharacterApi.parse(elem.attacker),
            killed: elem.killed == null ? null : CharacterApi.parse(elem.killed),
            item: elem.item == null ? null : ItemApi.parse(elem.item),
            fireGroupToFireMode: elem.fireGroupToFireMode == null ? null : { ...elem.fireGroupToFireMode }
        }
    }

    public static parseBlock(elem: any): KillDeathBlock {
        const block: KillDeathBlock = new KillDeathBlock();

        for (const cat of elem.itemCategories) {
            block.itemCategories.set(cat.id, { ...cat });
        }

        for (const kill of elem.kills) {
            block.kills.push(KillStatApi.parseKillEvent(kill));
        }

        for (const death of elem.deaths) {
            block.deaths.push(KillStatApi.parseKillEvent(death));
        }

        for (const char of elem.characters) {
            block.characters.set(char.id, CharacterApi.parse(char));
        }

        for (const weapon of elem.weapons) {
            block.weapons.set(weapon.id, ItemApi.parse(weapon));
        }

        for (const fireMode of elem.fireModes) {
            block.fireModes.set(fireMode.fireModeID, { ...fireMode });
        }

        return block;
    }

    public static async getWeaponEntries(charID: string, useShort: boolean): Promise<Loading<CharacterWeaponKillEntry[]>> {
        return KillStatApi.get().readList(`/api/kills/character/${charID}?useShort=${useShort}`, KillStatApi.parseCharacterWeaponKillEntry);
    }

    public static async getOutfitKillers(outfitID: string): Promise<Loading<OutfitKillerEntry[]>> {
        return KillStatApi.get().readList(`/api/kills/outfit/${outfitID}`, KillStatApi.parseOutfitKillerEntry);
    }

    public static async getSessionKills(sessionID: number): Promise<Loading<ExpandedKillEvent[]>> {
        return KillStatApi.get().readList(`/api/kills/session/${sessionID}`, KillStatApi.parseExpandedKillEvent);
    }

    public static async getSessionBlock(sessionID: number): Promise<Loading<KillDeathBlock>> {
        return KillStatApi.get().readSingle(`/api/kills/session/${sessionID}/block`, KillStatApi.parseBlock);
    }

    public static async getByRange(charID: string, start: Date, end: Date): Promise<Loading<ExpandedKillEvent[]>> {
        return KillStatApi.get().readList(`/api/kills/character/${charID}/period?start=${start.toISOString()}&end=${end.toISOString()}`, KillStatApi.parseExpandedKillEvent);
    }

}
