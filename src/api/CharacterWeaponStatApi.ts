import * as axios from "axios";
import { Loading } from "Loading";
import ApiWrapper from "api/ApiWrapper";

import { PsItem } from "api/ItemApi";
import { PsCharacter, CharacterApi } from "api/CharacterApi";
import { PsVehicle } from "api/VehicleApi";

export class WeaponStatEntry {
	public weaponID: string = "";
	public characterID: string = "";
	public kills: number = 0;
	public vehicleKills: number = 0;
	public deaths: number = 0;
	public headshots: number = 0;
	public shots: number = 0;
	public shotsHit: number = 0;
	public secondsWith: number = 0;
	public timestamp: Date = new Date();

	public accuracy: number = 0;
	public headshotRatio: number = 0;
	public killDeathRatio: number = 0;
	public killsPerMinute: number = 0;
	public vehicleKillsPerMinute: number = 0;

}

export class CharacterWeaponStatEntry {
	public characterID: string = "";
	public itemID: string = "";
	public vehicleID: number = 0;
	public item: PsItem | null = null;
	public vehicle: PsVehicle | null = null;

	public itemName: string = "";

	public kills: number = 0;
	public vehicleKills: number = 0;
	public deaths: number = 0;
	public headshots: number = 0;
	public shots: number = 0;
	public shotsHit: number = 0;
	public secondsWith: number = 0;
	public timestamp: Date = new Date();

	public killDeathRatio: number = 0;
	public killsPerMinute: number = 0;
	public accuracy: number = 0;
	public headshotRatio: number = 0;
	public vehicleKillsPerMinute: number = 0;

	public kpmPercent: number | null = null;
	public kdPercent: number | null = null;
	public accPercent: number | null = null;
	public hsrPercent: number | null = null;
	public vkpmPercent: number | null = null;
}

export class ExpandedWeaponStatEntry {
	public entry: WeaponStatEntry = new WeaponStatEntry();
	public character: PsCharacter | null = null;
}

export class PercentileCacheType {
	public static KD: number = 1;
	public static KPM: number = 2;
	public static VKPM: number = 3;
	public static ACC: number = 4;
	public static HSR: number = 5;
	public static KILLS: number = 6;
}

export class CharacterWeaponStatApi extends ApiWrapper<CharacterWeaponStatEntry> {
	private static _instance: CharacterWeaponStatApi = new CharacterWeaponStatApi();
	public static get(): CharacterWeaponStatApi { return CharacterWeaponStatApi._instance; }

	public static parseWeaponStat(elem: any): WeaponStatEntry {
		return {
			...elem,
			timestamp: new Date(elem.timestamp)
		};
	}

	public static parse(elem: any): CharacterWeaponStatEntry {
		return {
			characterID: elem.characterID,
			itemID: elem.itemID,
			vehicleID: elem.stat.vehicleID,
			vehicle: (elem.vehicle == null) ? null : { ...elem.vehicle },
			item: (elem.item == null) ? null : { ...elem.item },
			itemName: (elem.item) ? elem.item.name : (elem.itemID == 0) ? "no weapon" : `<missing ${elem.itemID}>`,
			kills: elem.stat.kills,
			deaths: elem.stat.deaths,
			vehicleKills: elem.stat.vehicleKills,
			headshots: elem.stat.headshots,
			shots: elem.stat.shots,
			shotsHit: elem.stat.shotsHit,
			secondsWith: elem.stat.secondsWith,
			timestamp: new Date(elem.stat.timestamp),

			killDeathRatio: elem.stat.killDeathRatio,
			killsPerMinute: elem.stat.killsPerMinute,
			accuracy: elem.stat.accuracy * 100,
			headshotRatio: elem.stat.headshotRatio * 100,
			vehicleKillsPerMinute: elem.stat.vehicleKillsPerMinute,

			kdPercent: elem.killDeathRatioPercentile,
			kpmPercent: elem.killsPerMinutePercentile,
			accPercent: elem.accuracyPercentile,
			hsrPercent: elem.headshotRatioPercentile,
			vkpmPercent: elem.vehicleKillsPerMinutePercentile
		}
	}

	public static parseExpanded(elem: any): ExpandedWeaponStatEntry {
		return {
			entry: CharacterWeaponStatApi.parseWeaponStat(elem.entry),
			character: elem.character == null ? null : CharacterApi.parse(elem.character)
		};
	}

	public static async getByCharacterID(charID: string): Promise<Loading<CharacterWeaponStatEntry[]>> {
		return CharacterWeaponStatApi.get().readList(`/api/character/${charID}/weapon_stats`, CharacterWeaponStatApi.parse);
	}

}
