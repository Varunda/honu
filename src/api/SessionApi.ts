import { Loading } from "Loading";
import ApiWrapper from "api/ApiWrapper";

export class Session {
	public id: number = 0;
	public characterID: string = "";
	public start: Date = new Date();
	public end: Date | null = null;
	public outfitID: string | null = null;
	public teamID: number = 0;
}

export class SessionApi extends ApiWrapper<Session> {
	private static _instance: SessionApi = new SessionApi();
	public static get(): SessionApi { return SessionApi._instance; }

	public static parse(elem: any): Session {
		return {
			...elem,
			start: new Date(elem.start),
			end: elem.end == null ? null : new Date(elem.end)
		};
	}

	public static async getByCharacterID(charID: string): Promise<Loading<Session[]>> {
		return SessionApi.get().readList(`/api/character/${charID}/sessions`, SessionApi.parse);
	}

	public static async getBySessionID(sessionID: number): Promise<Loading<Session>> {
		return SessionApi.get().readSingle(`/api/session/${sessionID}`, SessionApi.parse);
	}

}
