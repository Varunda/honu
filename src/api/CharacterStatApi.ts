import * as axios from "axios";
import { Loading } from "Loading";
import ApiWrapper from "api/ApiWrapper";

export class CharacterStat {
	public characterID: string = "";
	public statName: string = "";
	public profileID: number = 0;
	public valueForever: number = 0;
	public valueMonthly: number = 0;
	public valueWeekly: number = 0;
	public valueDaily: number = 0;
	public valueMaxOneLife: number = 0;
	public timestamp: Date = new Date();
}

export class CharacterStatApi extends ApiWrapper<CharacterStat> {

	private static _instance: CharacterStatApi = new CharacterStatApi();
	public static get(): CharacterStatApi { return CharacterStatApi._instance; }

	public static parse(elem: any): CharacterStat {
		return {
			...elem,
			timestamp: new Date(elem.timestamp)
		};
	};

	public static async getByCharacterID(charID: string): Promise<Loading<CharacterStat[]>> {
		return CharacterStatApi.get().readList(`/api/character/${charID}/stats`, CharacterStatApi.parse);
	}

}
