import { Loading } from "Loading";
import ApiWrapper from "api/ApiWrapper";

export class CharacterWorldChange {
	public characterID: string = "";
	public worldID: number = 0;
	public timestamp: Date = new Date();
}

export class CharacterWorldChangeApi extends ApiWrapper<CharacterWorldChangeApi> {
	private static _instance: CharacterWorldChangeApi = new CharacterWorldChangeApi();
	public static get(): CharacterWorldChangeApi { return CharacterWorldChangeApi._instance; }

	public static parse(elem: any): CharacterWorldChange {
		return {
			...elem,
			timestamp: new Date(elem.timestamp)
		};
	}

	public static async getByCharacterID(charID: string): Promise<Loading<CharacterWorldChange[]>> {
		return CharacterWorldChangeApi.get().readList(`/api/character/${charID}/world-changes`, CharacterWorldChangeApi.parse);
	}

}
