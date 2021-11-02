import * as axios from "axios";

export class PsOutfit {
	public id: string = "";
	public name: string = "";
	public tag: string | null = null;
	public factionID: number = 0;
	public timestamp: Date = new Date();
}

export class OutfitApi {
	private static _instance: OutfitApi = new OutfitApi();
	public static get(): OutfitApi { return OutfitApi._instance; }

	public static parse(elem: any): PsOutfit {
		return {
			...elem,
			timestamp: new Date(elem.lastUpdated)
		};
	}

	public static async getByID(outfitID: string): Promise<PsOutfit | null> {
        const response: axios.AxiosResponse<any> = await axios.default.get(`/api/outfit/${outfitID}`);

		if (response.status != 200) {
			return null;
		}

		return OutfitApi.parse(response.data);
	}

}