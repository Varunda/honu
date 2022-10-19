import ApiWrapper from "api/ApiWrapper";
import { Loading, Loadable } from "Loading";
import { CharacterApi, PsCharacter } from "./CharacterApi";

export class WeaponStatTop {
    public id: number = 0;
    public worldID: number = 0;
    public factionID: number = 0;
    public typeID: number = 0;
    public timestamp: Date = new Date();

    public characterID: string = "";
    public itemID: number = 0;
    public vehicleID: number = 0;

	public kills: number = 0;
	public vehicleKills: number = 0;
	public deaths: number = 0;
	public headshots: number = 0;
	public shots: number = 0;
	public shotsHit: number = 0;
	public secondsWith: number = 0;

	public accuracy: number = 0;
	public headshotRatio: number = 0;
	public killDeathRatio: number = 0;
	public killsPerMinute: number = 0;
	public vehicleKillsPerMinute: number = 0;
}

export class ExpandedWeaponStatTop {
	public entry: WeaponStatTop = new WeaponStatTop();
	public character: PsCharacter | null = null;
}

export class WeaponStatTopApi extends ApiWrapper<WeaponStatTop> {

	private static _instance: WeaponStatTopApi = new WeaponStatTopApi();
	public static get(): WeaponStatTopApi { return WeaponStatTopApi._instance; }

	public static parse(elem: any): WeaponStatTop {
		return {
			...elem,
			timestamp: new Date(elem.timestamp)
		};
    }

	public static parseExpanded(elem: any): ExpandedWeaponStatTop {
		return {
			entry: WeaponStatTopApi.parse(elem.entry),
			character: (elem.character == null) ? null : CharacterApi.parse(elem.character)
		};
    }

	public static async getTopAll(itemID: number): Promise<Loading<ExpandedWeaponStatTop[]>> {
		return WeaponStatTopApi.get().readList(`/api/item/${itemID}/top/all`, WeaponStatTopApi.parseExpanded);
    }

	public static async getQueueItems(): Promise<Loading<number[]>> {
		return WeaponStatTopApi.get().readList(`/api/services/weapon_update_queue`, (iter => iter as number));
    }

}
