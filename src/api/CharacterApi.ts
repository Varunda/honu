import * as axios from "axios";
import { Loading } from "Loading";
import ApiWrapper from "api/ApiWrapper";

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

	public lastUpdated: Date = new Date();

	public dateCreated: Date = new Date();
	public dateLastLogin: Date = new Date();
	public dateLastSave: Date = new Date();

}

export class CharacterApi extends ApiWrapper<PsCharacter> {
	private static _instance: CharacterApi = new CharacterApi();
	public static get(): CharacterApi { return this._instance; }

	public parse(elem: any): PsCharacter {
		return {
			...elem,
			lastUpdated: new Date(elem.lastUpdated),
			dateCreated: new Date(elem.dateCreated),
			dateLastLogin: new Date(elem.dateLastLogin),
			dateLastSave: new Date(elem.dateLastSave)
		}
	}

	public static async getByID(charID: string): Promise<Loading<PsCharacter>> {
		return CharacterApi.get().readSingle(`/api/character/${charID}`);
	}

	public static async getByName(name: string): Promise<Loading<PsCharacter[]>> {
		return CharacterApi.get().readList(`/api/character/name/${name}`);
	}

	public static async searchByName(name: string): Promise<Loading<PsCharacter[]>> {
		return CharacterApi.get().readList(`/api/character/search/${name}`);
	}

}