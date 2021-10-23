import * as axios from "axios";

export class Session {
	public id: number = 0;
	public characterID: string = "";
	public start: Date = new Date();
	public end: Date | null = null;
	public outfitID: string | null = null;
	public teamID: number = 0;
}

export class SessionApi {
	private static _instance: SessionApi = new SessionApi();
	public static get(): SessionApi { return SessionApi._instance; }

	private static _parse(elem: any): Session {
		return {
			...elem,
			start: new Date(elem.start),
			end: elem.end == null ? null : new Date(elem.end)
		};
	}

	public static async getByCharacterID(charID: string): Promise<Session[]> {
        const response: axios.AxiosResponse<any> = await axios.default.get(`/api/character/${charID}/sessions`);

		if (response.status != 200) {
			return [];
		}

		if (Array.isArray(response.data) == false) {
			throw `response.data is not an array`;
		}

		return response.data.map((iter: any) => SessionApi._parse(iter));
	}

}
