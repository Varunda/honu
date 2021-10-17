import * as axios from "axios";

import { PsItem } from "api/ItemApi";

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
}

export class CharacterWeaponStatEntry {
	public characterID: string = "";
	public itemID: string = "";
	public item: PsItem | null = null;

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

	public kpmPercent: number | null = null;
	public kdPercent: number | null = null;
	public accPercent: number | null = null;
	public hsrPercent: number | null = null;
}

export class CharacterWeaponStatApi {
	private static _instance: CharacterWeaponStatApi = new CharacterWeaponStatApi();
	public static get(): CharacterWeaponStatApi { return CharacterWeaponStatApi._instance; }

	private static _parse(elem: any): CharacterWeaponStatEntry {
		return {
			characterID: elem.characterID,
			itemID: elem.itemID,
			item: { ...elem.item },
			itemName: (elem.item) ? elem.item.name : `<missing ${elem.itemID}>`,
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

			kdPercent: elem.killDeathRatioPercentile,
			kpmPercent: elem.killsPerMinutePercentile,
			accPercent: elem.accuracyPercentile,
			hsrPercent: elem.headshotRatioPercentil
		}
	}

	public static async getByCharacterID(charID: string): Promise<CharacterWeaponStatEntry[]> {
        const response: axios.AxiosResponse<any> = await axios.default.get(`/api/character/${charID}/weapon_stats`);

		if (response.status != 200) {
			return [];
		}

		if (Array.isArray(response.data) == false) {
			throw `response.data is not an array`;
		}

		return response.data.map((iter: any) => CharacterWeaponStatApi._parse(iter));
	}

}
