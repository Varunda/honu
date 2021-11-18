import * as axios from "axios";

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

export class CharacterStatApi {

	public static parse(elem: any): CharacterStat {
		return {
			...elem,
			timestamp: new Date(elem.timestamp)
		};
	};

	public static async getByCharacterID(charID: string): Promise<CharacterStat[]> {
		const response: axios.AxiosResponse = await axios.default.get(`/api/character/${charID}/stats`);

		if (response.status != 200) {
			throw response.data;
		}

		return response.data.map((iter: any) => CharacterStatApi.parse(iter));
	}

}
