import { Loading } from "Loading";
import ApiWrapper from "api/ApiWrapper";

import { CharacterApi, PsCharacter } from "api/CharacterApi";
import { PsOutfit, OutfitApi } from "api/OutfitApi";

export class Session {
	public id: number = 0;
	public characterID: string = "";
	public start: Date = new Date();
	public end: Date | null = null;
	public outfitID: string | null = null;
	public teamID: number = 0;

	public summaryCalculated: Date | null = null;
	public kills: number = 0;
	public deaths: number = 0;
	public vehicleKills: number = 0;
	public experienceGained: number = 0;
	public heals: number = 0;
	public revives: number = 0;
	public shieldRepairs: number = 0;
	public resupplies: number = 0;
	public repairs: number = 0;
	public spawns: number = 0;
}

export class SessionBlock {
	public characterID: string = "";
	public sessions: Session[] = [];
	public outfits: Map<string, PsOutfit> = new Map();
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
			end: elem.end == null ? null : new Date(elem.end),
			summaryCalculated: elem.summaryCalculated == null ? null : new Date(elem.summaryCalculated)
		};
	}

	public static parseExpanded(elem: any): ExpandedSession {
		return {
			session: SessionApi.parse(elem.session),
			character: (elem.character == null) ? null : CharacterApi.parse(elem.character)
		};
    }

	public static parseBlock(elem: any): SessionBlock {
		const block: SessionBlock = new SessionBlock();

		block.characterID = elem.characterID;
		block.sessions = elem.sessions.map((iter: any) => SessionApi.parse(iter));

		const outfits: PsOutfit[] = elem.outfits.map((iter: any) => OutfitApi.parse(iter));
		for (const outfit of outfits) {
			block.outfits.set(outfit.id, outfit);
        }

		return block;
    }

	public static async getByCharacterID(charID: string): Promise<Loading<Session[]>> {
		return SessionApi.get().readList(`/api/character/${charID}/sessions`, SessionApi.parse);
	}

	public static async getBlockByCharacterID(charID: string): Promise<Loading<SessionBlock>> {
		return SessionApi.get().readSingle(`/api/character/${charID}/sessions-block`, SessionApi.parseBlock);
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
