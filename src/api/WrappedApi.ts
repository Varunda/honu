import { Loading } from "../Loading";
import ApiWrapper from "./ApiWrapper";

import { AchievementEarned } from "api/AchievementEarnedApi";
import { ExperienceType, ExpEvent } from "api/ExpStatApi";
import { ItemAddedEvent } from "api/ItemAddedEventApi";
import { KillEvent } from "api/KillStatApi";
import { Session } from "api/SessionApi";
import { VehicleDestroyEvent } from "api/VehicleDestroyEventApi";
import { PsCharacter } from "api/CharacterApi";
import { PsItem } from "api/ItemApi";
import { PsFacility } from "api/MapApi";
import { Achievement } from "api/AchievementApi";
import { FacilityControlEvent } from "api/FacilityControlEventApi";
import { PsOutfit } from "api/OutfitApi";
import { PsVehicle } from "api/VehicleApi";
import { FireGroupToFireMode } from "./FireGroupToFireModeApi";
import { WrappedExtraData } from "../view/wrapped/common";

export class WrappedEntry {
    public id: string = ""; // guid
    public inputCharacterIDs: string[] = [];
    public timestamp: Date = new Date();
    public createdAt: Date = new Date();

    public status: number = 0;

    public sessions: Session[] = [];
    public kills: KillEvent[] = [];
    public teamkills: KillEvent[] = [];
    public deaths: KillEvent[] = [];
    public teamdeaths: KillEvent[] = [];
    public exp: ExpEvent[] = [];
    public vehicleKill: VehicleDestroyEvent[] = [];
    public vehicleDeath: VehicleDestroyEvent[] = [];
    public controlEvents: FacilityControlEvent[] = [];
    public achievementEarned: AchievementEarned[] = [];
    public itemAdded: ItemAddedEvent[] = [];

    public characters: Map<string, PsCharacter> = new Map();
    public outfits: Map<string, PsOutfit> = new Map();
    public items: Map<number, PsItem> = new Map();
    public vehicles: Map<number, PsVehicle> = new Map();
    public facilities: Map<number, PsFacility> = new Map();
    public achivements: Map<number, Achievement> = new Map();
    public expTypes: Map<number, ExperienceType> = new Map();
    public fireModeXrefs: Map<number, FireGroupToFireMode[]> = new Map();

    public extra: WrappedExtraData = new WrappedExtraData();

    public static getFireModeIndex(wrapped: WrappedEntry, fireModeID: number): number | null {
        const fireModes: FireGroupToFireMode[] | undefined = wrapped.fireModeXrefs.get(fireModeID);
        if (fireModes != undefined && fireModes.length > 0) {
            const fireModeIndex: number = fireModes[0].fireModeIndex;

            return fireModeIndex;
        }

        return null;
    }

}

export class WrappedApi extends ApiWrapper<WrappedEntry> {
    private static _instance: WrappedApi = new WrappedApi();
    public static get(): WrappedApi { return WrappedApi._instance; }

    public static parse(elem: any): WrappedEntry {
        return {
            ...elem
        };
    }

    public static getByID(id: string): Promise<Loading<WrappedEntry>> {
        return WrappedApi.get().readSingle(`/api/wrapped/${id}`, WrappedApi.parse);
    }

    public static insert(input: string[], year?: number): Promise<Loading<string>> {
        const parms: URLSearchParams = new URLSearchParams();
        for (const iter of input) {
            parms.append("IDs", iter);
        }

        if (year != undefined) {
            parms.append("year", year.toString());
        }

        return WrappedApi.get().postReply(`/api/wrapped?${parms.toString()}`, (elem: any) => elem);
    }

    public static isEnabled(): Promise<Loading<boolean>> {
        return WrappedApi.get().readSingle(`/api/wrapped/enabled`, (elem: any) => elem);
    }

}
