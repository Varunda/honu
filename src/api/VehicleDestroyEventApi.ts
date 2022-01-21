import ApiWrapper from "api/ApiWrapper";
import { Loading, Loadable } from "Loading";

import { PsCharacter, CharacterApi } from "api/CharacterApi";
import { PsItem, ItemApi } from "api/ItemApi";
import { PsVehicle, VehicleApi } from "api/VehicleApi";

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

    public static getBySessionID(sessionID: number): Promise<Loading<ExpandedVehicleDestroyEvent[]>> {
        return VehicleDestroyEventApi.get().readList(`/api/vehicle-destroy/session/${sessionID}`, VehicleDestroyEventApi.parseExpanded);
    }

}