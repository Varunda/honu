import * as axios from "axios";

export class PsCharacter {
	public id: string = "";
	public name: string = "";
	public worldID: number = 0;

	public outfitID: string | null = null;
	public outfitTag: string | null = null;
	public outfitName: string | null = null;

	public factionID: number = 0;
	public battleRank: number = 0;
	public prestige: number = 0;

	public dateCreated: Date = new Date();
	public dateLastLogin: Date = new Date();
	public dateLastSave: Date = new Date();

}

export class CharacterApi {
	private static _instance: CharacterApi = new CharacterApi();
	public static get(): CharacterApi { return this._instance; }

	public static parse(elem: any): PsCharacter {
		return {
			...elem,
			dateCreated: new Date(elem.dateCreated),
			dateLastLogin: new Date(elem.dateLastLogin),
			dateLastSave: new Date(elem.dateLastSave)
		}
	}

	public static async getByID(charID: string): Promise<PsCharacter | null> {
        const response: axios.AxiosResponse<any> = await axios.default.get(`/api/character/${charID}`);

		if (response.status != 200) {
			return null;
		}

		const c: PsCharacter = CharacterApi.parse(response.data);
		return c;
	}

	public static async getByName(name: string): Promise<PsCharacter[]> {
		const response: axios.AxiosResponse<any> = await axios.default.get(`/api/characters/name/${name}`);

		if (response.status != 200) {
			return [];
		}

		if (Array.isArray(response.data) == false) {
			throw `Data from endpoint was not an array as expected`;
		}

		return response.data.map((iter: any) => CharacterApi.parse(iter));
	}

}