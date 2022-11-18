import { Loading, Loadable } from "Loading";
import ApiWrapper from "api/ApiWrapper";

export class CharacterMetadata {
	public id: string = "";
	public lastUpdated: Date = new Date();
	public notFoundCount: number = 0;
}

export class CharacterMetadataApi extends ApiWrapper<CharacterMetadata> {
	private static _instance: CharacterMetadataApi = new CharacterMetadataApi();
	public static get(): CharacterMetadataApi { return this._instance; }

	public static parse(elem: any): CharacterMetadata {
		return {
			...elem,
			lastUpdated: new Date(elem.lastUpdated)
		}
	}

	public static async getByID(charID: string): Promise<Loading<CharacterMetadata>> {
		return CharacterMetadataApi.get().readSingle(`/api/character/${charID}/metadata`, CharacterMetadataApi.parse);
	}

	public static async getByIDs(IDs: string[]): Promise<Loading<CharacterMetadata[]>> {
		const params: URLSearchParams = new URLSearchParams();
		for (const charID of IDs) {
			params.append("IDs", charID);
		}

		return CharacterMetadataApi.get().readList(`/api/character/many/metadata?${params.toString()}`, CharacterMetadataApi.parse);
    }

}

