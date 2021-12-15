import { Loading } from "Loading";
import ApiWrapper from "api/ApiWrapper";

export class OutfitPopulation {
	public outfitID: string = "";
	public outfitTag: string | null = null;
	public outfitName: string = "";
	public factionID: number = 0;
	public count: number = 0;
}

export class OutfitPopulationApi extends ApiWrapper<OutfitPopulation> {
	private static _instance: OutfitPopulationApi = new OutfitPopulationApi();
	public static get(): OutfitPopulationApi { return OutfitPopulationApi._instance; }

	public static parse(elem: any): OutfitPopulation {
		return { ...elem };
	}

	public static async getPopulation(worldID: number, time: Date): Promise<Loading<OutfitPopulation[]>> {
		return OutfitPopulationApi.get().readList(`/api/population/${worldID}/outfits?time=${time.toISOString()}`, OutfitPopulationApi.parse);
	}
}
