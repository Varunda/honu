import * as axios from "axios";

export class CharacterHistoryStat {
	public characterID: string = "";
	public type: string = "";
	public lastUpdated: Date = new Date();
	public allTime: number = 0;
	public oneLifeMax: number = 0;

	public day1: number = 0;
	public day2: number = 0;
	public day3: number = 0;
	public day4: number = 0;
	public day5: number = 0;
	public day6: number = 0;
	public day7: number = 0;
	public day8: number = 0;
	public day9: number = 0;
	public day10: number = 0;
	public day11: number = 0;
	public day12: number = 0;
	public day13: number = 0;
	public day14: number = 0;
	public day15: number = 0;
	public day16: number = 0;
	public day17: number = 0;
	public day18: number = 0;
	public day19: number = 0;
	public day20: number = 0;
	public day21: number = 0;
	public day22: number = 0;
	public day23: number = 0;
	public day24: number = 0;
	public day25: number = 0;
	public day26: number = 0;
	public day27: number = 0;
	public day28: number = 0;
	public day29: number = 0;
	public day30: number = 0;
	public day31: number = 0;
	public days: number[] = [];

	public week1: number = 0;
	public week2: number = 0;
	public week3: number = 0;
	public week4: number = 0;
	public week5: number = 0;
	public week6: number = 0;
	public week7: number = 0;
	public week8: number = 0;
	public week9: number = 0;
	public week10: number = 0;
	public week11: number = 0;
	public week12: number = 0;
	public week13: number = 0;
	public weeks: number[] = [];

	public month1: number = 0;
	public month2: number = 0;
	public month3: number = 0;
	public month4: number = 0;
	public month5: number = 0;
	public month6: number = 0;
	public month7: number = 0;
	public month8: number = 0;
	public month9: number = 0;
	public month10: number = 0;
	public month11: number = 0;
	public month12: number = 0;
	public months: number[] = [];
}

export class CharacterHistoryStatApi {
	private static _instance: CharacterHistoryStatApi = new CharacterHistoryStatApi();
	public static get(): CharacterHistoryStatApi { return CharacterHistoryStatApi._instance; }

	public static parse(elem: any): CharacterHistoryStat {
		return {
			...elem,
			lastUpdated: new Date(elem.lastUpdated)
		};
	}

	public static async getByCharacterID(charID: string): Promise<CharacterHistoryStat[]> {
        const response: axios.AxiosResponse<any> = await axios.default.get(`/api/character/${charID}/history_stats`);
		if (response.status != 200) {
			return [];
		}

		if (Array.isArray(response.data) == false) {
			throw `response.data is not an array`;
		}

		return response.data.map((iter: any) => CharacterHistoryStatApi.parse(iter));
	}

}