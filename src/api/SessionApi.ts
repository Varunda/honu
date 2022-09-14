import { Loading } from "Loading";
import ApiWrapper from "api/ApiWrapper";

import { CharacterApi, PsCharacter } from "api/CharacterApi";

export class Session {
	public id: number = 0;
	public characterID: string = "";
	public start: Date = new Date();
	public end: Date | null = null;
	public outfitID: string | null = null;
	public teamID: number = 0;
}

export class ExpandedSession {
	public session: Session = new Session();
	public character: PsCharacter | null = null;
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

	public static parseExpanded(elem: any): ExpandedSession {
		return {
			session: SessionApi.parse(elem.session),
			character: (elem.character == null) ? null : CharacterApi.parse(elem.character)
		};
    }

	public static async getByCharacterID(charID: string): Promise<Loading<Session[]>> {
		return SessionApi.get().readList(`/api/character/${charID}/sessions`, SessionApi.parse);
	}

	public static async getBySessionID(sessionID: number): Promise<Loading<Session>> {
		return SessionApi.get().readSingle(`/api/session/${sessionID}`, SessionApi.parse);
	}

	public static async getByRange(unixSecondsEpoch: number, worldID?: number): Promise<Loading<ExpandedSession[]>> {
		return SessionApi.get().readList(`/api/session/history/${unixSecondsEpoch}${(worldID != null ? `?worldID=${worldID}` : "")}`, SessionApi.parseExpanded);
    }

	public static async getByCharacterIDAndPeriod(charID: string, start: Date, end: Date): Promise<Loading<Session[]>> {
		return SessionApi.get().readList(`/api/session/character/${charID}/period?start=${start.toISOString()}&end=${end.toISOString()}`, SessionApi.parse);
    }

	public static async getByOutfit(outfitID: string): Promise<Loading<Session[]>> {
		return SessionApi.get().readList(`/api/session/outfit/${outfitID}`, SessionApi.parse);
    }

}
