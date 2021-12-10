import * as axios from "axios";

export class OutfitPopulation {
	public outfitID: string = "";
	public outfitTag: string | null = null;
	public outfitName: string = "";
	public factionID: number = 0;
	public count: number = 0;
}

export class OutfitPopulationApi {
	private static _instance: OutfitPopulationApi = new OutfitPopulationApi();
	public static get(): OutfitPopulationApi { return OutfitPopulationApi._instance; }

	public static async getPopulation(worldID: number, time: Date): Promise<OutfitPopulation[]> {
		const response: axios.AxiosResponse<any> = await axios.default.get(`/api/population/${worldID}/outfits?time=${time.toISOString()}`);

		if (response.status != 200) {
			return [];
		}

		return response.data;
	}
}
