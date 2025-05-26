import ApiWrapper from "api/ApiWrapper";
import { Loading, Loadable } from "Loading";

import { PsCharacter, CharacterApi } from "api/CharacterApi";
import { PsItem, ItemApi } from "api/ItemApi";
import { PsVehicle, VehicleApi } from "api/VehicleApi";
import { ItemCategory } from "./ItemCategoryApi";

export class VehicleDestroyEvent {
    public id: number = 0;
    public attackerCharacterID: string = "";
    public attackerVehicleID: string = "";
    public attackerWeaponID: number = 0;
    public attackerLoadoutID: number = 0;
    public attackerFactionID: number = 0;
    public attackerTeamID: number = 0;
    public killedCharacterID: string = "";
    public killedFactionID: number = 0;
    public killedTeamID: number = 0;
    public killedVehicleID: string = "";
    public facilityID: string = "";
    public worldID: number = 0;
    public zoneID: number = 0;
    public timestamp: Date = new Date();
}

export class ExpandedVehicleDestroyEvent {
    public event: VehicleDestroyEvent = new VehicleDestroyEvent();
    public attacker: PsCharacter | null = null;
    public attackerVehicle: PsVehicle | null = null;
    public killed: PsCharacter | null = null;
    public killedVehicle: PsVehicle | null = null;
    public item: PsItem | null = null;
}

export class VehicleKillDeathBlock {
    public kills: VehicleDestroyEvent[] = [];
    public deaths: VehicleDestroyEvent[] = [];

    public characters: Map<string, PsCharacter> = new Map();
    public weapons: Map<number, PsItem> = new Map();
    public itemCategories: Map<number, ItemCategory> = new Map();
    public vehicles: Map<number, PsVehicle> = new Map();
}

export class VehicleDestroyEventApi extends ApiWrapper<VehicleDestroyEvent> {

    private static _instance: VehicleDestroyEventApi = new VehicleDestroyEventApi();
    public static get(): VehicleDestroyEventApi { return VehicleDestroyEventApi._instance; }

    public static parse(elem: any): VehicleDestroyEvent {
        return {
            ...elem,
            timestamp: new Date(elem.timestamp)
        }
    }

    public static parseExpanded(elem: any): ExpandedVehicleDestroyEvent {
        return {
            event: VehicleDestroyEventApi.parse(elem.event),
            killed: (elem.killed == null) ? null : CharacterApi.parse(elem.killed),
            killedVehicle: (elem.killedVehicle == null) ? null : VehicleApi.parse(elem.killedVehicle),
            attacker: (elem.attacker == null) ? null : CharacterApi.parse(elem.attacker),
            attackerVehicle: (elem.attackerVehicle == null) ? null : VehicleApi.parse(elem.attackerVehicle),
            item: (elem.item == null) ? null : ItemApi.parse(elem.item)
        };
    }

    public static parseBlock(elem: any): VehicleKillDeathBlock {
        const block: VehicleKillDeathBlock = new VehicleKillDeathBlock();

        for (const cat of elem.itemCategories) {
            block.itemCategories.set(cat.id, { ...cat });
        }

        for (const kill of elem.kills) {
            block.kills.push(VehicleDestroyEventApi.parse(kill));
        }

        for (const death of elem.deaths) {
            block.deaths.push(VehicleDestroyEventApi.parse(death));
        }

        for (const char of elem.characters) {
            block.characters.set(char.id, CharacterApi.parse(char));
        }

        for (const weapon of elem.weapons) {
            block.weapons.set(weapon.id, ItemApi.parse(weapon));
        }

        for (const veh of elem.vehicles) {
            block.vehicles.set(veh.id, VehicleApi.parse(veh));
        }

        return block;
    }

    public static getBySessionID(sessionID: number): Promise<Loading<VehicleKillDeathBlock>> {
        return VehicleDestroyEventApi.get().readSingle(`/api/vehicle-destroy/session/${sessionID}/block`, VehicleDestroyEventApi.parseBlock);
    }

}